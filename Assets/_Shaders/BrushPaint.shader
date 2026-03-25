Shader "Hidden/BrushPaint"
{
    Properties
    {
        [MainTexture] _BaseTexture("Base Map", 2D) = "white" {}
        _BrushCenter("Brush Center (UV)", Vector) = (0.5, 0.5, 0, 0)
        _BrushRadius("Brush Radius (UV)", Float) = 0.1
        _BrushStrength("Brush Strength", Float) = 1.0
        _BrushColor("Brush Color", Color) = (1, 0, 0, 1)
        _PaintMode("Paint Mode (0=growth-add, 1=growth-subtract, 2=color)", Int) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseTexture);
            SAMPLER(sampler_BaseTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseTexture_ST;
                float4 _BrushCenter;
                float _BrushRadius;
                float _BrushStrength;
                float4 _BrushColor;
                int _PaintMode;
            CBUFFER_END

            #define PAINT_MODE_GROWTH_ADD 0
            #define PAINT_MODE_GROWTH_SUBTRACT 1
            #define PAINT_MODE_COLOR 2

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseTexture);
                return OUT;
            }

            // behind the scenes it converts to a ternary operator, but IMO this is more expressive
            #define IF_TRUE_THEN(condition, valueIfTrue) (condition) * (valueIfTrue)

            float4 frag(Varyings IN) : SV_Target
            {
                float falloff = saturate(1.0 - length(IN.uv - _BrushCenter.xy) / _BrushRadius);
                falloff = falloff * falloff;

                float brushIntensity = falloff * _BrushStrength;

                float4 sampledColor = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, IN.uv);
                float4 result = sampledColor + IF_TRUE_THEN(_PaintMode == PAINT_MODE_GROWTH_ADD,      float4(0, 0, 0,  brushIntensity))
                                             + IF_TRUE_THEN(_PaintMode == PAINT_MODE_GROWTH_SUBTRACT, float4(0, 0, 0, -brushIntensity))
                                             + IF_TRUE_THEN(_PaintMode == PAINT_MODE_COLOR,            float4((_BrushColor.rgb - sampledColor.rgb) * brushIntensity, 0));
                return saturate(result);
            }
            ENDHLSL
        }
    }
}
