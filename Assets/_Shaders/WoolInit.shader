Shader "Hidden/WoolInit"
{
    Properties
    {
        _UVMask("UV Mask", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _OffsetX("Noise Offset X", Float) = 0
        _OffsetY("Noise Offset Y", Float) = 0
        _NoiseScale("Noise Scale", Float) = 8
        _NoiseMin("Noise Min", Float) = 0.4
        _NoiseMax("Noise Max", Float) = 1.0
        _EdgeFadeStart("Edge Fade Start (UV)", Float) = 0.9
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D(_UVMask);
            SAMPLER(sampler_UVMask);

            CBUFFER_START(UnityPerMaterial)
                float4 _UVMask_ST;
                float4 _BaseColor;
                float  _OffsetX;
                float  _OffsetY;
                float  _NoiseScale;
                float  _NoiseMin;
                float  _NoiseMax;
                float  _EdgeFadeStart;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float2 GradientHash(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }

            // Classic gradient noise, output remapped to [0, 1]
            float GradientNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = dot(GradientHash(i + float2(0, 0)) * 2.0 - 1.0, f - float2(0, 0));
                float b = dot(GradientHash(i + float2(1, 0)) * 2.0 - 1.0, f - float2(1, 0));
                float c = dot(GradientHash(i + float2(0, 1)) * 2.0 - 1.0, f - float2(0, 1));
                float d = dot(GradientHash(i + float2(1, 1)) * 2.0 - 1.0, f - float2(1, 1));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y) * 0.5 + 0.5;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                float noise  = GradientNoise(float2(uv.x + _OffsetX, uv.y + _OffsetY) * _NoiseScale);
                float growth = lerp(_NoiseMin, _NoiseMax, noise);
                float edge   = saturate((1.0 - uv.y) / (1.0 - _EdgeFadeStart));
                float mask   = SAMPLE_TEXTURE2D(_UVMask, sampler_UVMask, uv).r;

                return float4(_BaseColor.rgb, growth * edge * mask);
            }

            ENDHLSL
        }
    }
}
