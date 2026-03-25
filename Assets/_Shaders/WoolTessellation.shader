Shader "Custom/WoolTessellation"
{
    Properties
    {
        _WoolTex("Wool Map (RGB=Color, A=Growth)", 2D) = "white" {}
        _BaseColor("Base Skin Color", Color) = (0.9, 0.8, 0.7, 1)
        _WoolEpsilon("Wool Epsilon", Float) = 0.01
        _MaxDisplacement("Max Displacement", Float) = 0.3
        _TessMin("Min Tessellation", Range(1, 16)) = 1
        _TessMax("Max Tessellation", Range(1, 64)) = 8
        _TessDistMin("Tess Near Distance", Float) = 2
        _TessDistMax("Tess Far Distance", Float) = 15
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        HLSLINCLUDE

        #pragma target 4.6

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_WoolTex);
        SAMPLER(sampler_WoolTex);

        CBUFFER_START(UnityPerMaterial)
            float4 _WoolTex_ST;
            float4 _BaseColor;
            float _WoolEpsilon;
            float _MaxDisplacement;
            float _TessMin;
            float _TessMax;
            float _TessDistMin;
            float _TessDistMax;
        CBUFFER_END

        // Set globally by UIManager each frame — not per-material
        float4 _BrushUV;
        float  _BrushRadius;
        float  _BrushActive;

        #include "Tessellation.hlsl"

        TessControlPoint SharedVert(Attributes IN)
        {
            TessControlPoint OUT;
            OUT.positionOS = IN.positionOS;
            OUT.normalOS = IN.normalOS;
            OUT.uv = TRANSFORM_TEX(IN.uv, _WoolTex);
            return OUT;
        }

        void InterpolatePatch(OutputPatch<TessControlPoint, 3> patch, float3 bary,
        out float4 positionOS, out float3 normalOS, out float2 uv)
        {
            InterpolateBarycentrics(patch, bary, positionOS, normalOS, uv);

            // the "growth" value is stored in the alpha channel of the wool texture
            // which is sampled using the interpolated UV coordinates
            float growth = SAMPLE_TEXTURE2D_LOD(_WoolTex, sampler_WoolTex, uv, 0).a;
            positionOS.xyz += normalOS * growth * _MaxDisplacement;
        }

        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma hull hull
            #pragma domain domain
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                half4 fogFactorAndVertexLight : TEXCOORD3;
                float4 shadowCoord : TEXCOORD4;
            };

            TessControlPoint vert(Attributes IN) { return SharedVert(IN); }
            DECLARE_HULL(hull)

            [domain("tri")]
            Varyings domain(TessFactors factors, OutputPatch<TessControlPoint, 3> patch, float3 bary : SV_DomainLocation)
            {
                Varyings OUT;
                float4 positionOS; float3 normalOS; float2 uv;
                InterpolatePatch(patch, bary, positionOS, normalOS, uv);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(normalOS);

                OUT.positionHCS = vertexInput.positionCS;
                OUT.positionWS = vertexInput.positionWS;
                OUT.normalWS = normalInput.normalWS;
                OUT.uv = uv;
                OUT.fogFactorAndVertexLight = half4(ComputeFogFactor(vertexInput.positionCS.z), VertexLighting(vertexInput.positionWS, normalInput.normalWS));
                OUT.shadowCoord = GetShadowCoord(vertexInput);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 woolSample = SAMPLE_TEXTURE2D(_WoolTex, sampler_WoolTex, IN.uv);
                float growth = woolSample.a;

                if (growth <= _WoolEpsilon)
                return half4(_BaseColor.rgb, 1.0);

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = NormalizeNormalPerPixel(IN.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.shadowCoord = IN.shadowCoord;
                inputData.fogCoord = IN.fogFactorAndVertexLight.x;
                inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
                inputData.bakedGI = SampleSH(inputData.normalWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = woolSample.rgb;
                surfaceData.alpha = 1.0;
                surfaceData.smoothness = 0.3;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.occlusion = 1.0;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);

                // Brush preview rim
                float brushDist = length(IN.uv - _BrushUV.xy);
                float rimWidth = max(_BrushRadius * 0.08, 0.003);
                float rim = smoothstep(_BrushRadius - rimWidth, _BrushRadius, brushDist)
                * (1.0 - smoothstep(_BrushRadius, _BrushRadius + rimWidth, brushDist));
                color.rgb = lerp(color.rgb, half3(1, 1, 1), rim * _BrushActive);

                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM

            #pragma vertex shadowVert
            #pragma hull shadowHull
            #pragma domain shadowDomain
            #pragma fragment shadowFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct ShadowVaryings { float4 positionHCS : SV_POSITION; };

            float3 _LightDirection;

            TessControlPoint shadowVert(Attributes IN) { return SharedVert(IN); }
            DECLARE_HULL(shadowHull)

            [domain("tri")]
            ShadowVaryings shadowDomain(TessFactors factors, OutputPatch<TessControlPoint, 3> patch, float3 bary : SV_DomainLocation)
            {
                ShadowVaryings OUT;
                float4 positionOS; float3 normalOS; float2 uv;
                InterpolatePatch(patch, bary, positionOS, normalOS, uv);

                float3 positionWS = TransformObjectToWorld(positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(normalOS);
                OUT.positionHCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return OUT;
            }

            half4 shadowFrag(ShadowVaryings IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
