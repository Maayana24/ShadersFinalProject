#ifndef TESSELLATION_INCLUDED
#define TESSELLATION_INCLUDED

// Requires Core.hlsl and _TessMin/_TessMax/_TessDistMin/_TessDistMax
// to be declared before including this file.

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
};

struct TessControlPoint
{
    float4 positionOS : INTERNALTESSPOS;
    float3 normalOS : NORMAL;
    float2 uv : TEXCOORD0;
};

struct TessFactors
{
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};

// _TessMin, _TessMax, _TessDistMin, _TessDistMax must be declared
// by the consuming shader's CBUFFER before including this file.

float CalcTessFactor(float3 positionOS)
{
    float3 positionWS = TransformObjectToWorld(positionOS);
    float dist = distance(positionWS, _WorldSpaceCameraPos);
    return clamp(lerp(_TessMax, _TessMin, (dist - _TessDistMin) / (_TessDistMax - _TessDistMin)), _TessMin, _TessMax);
}

TessFactors SharedPatchConstant(InputPatch<TessControlPoint, 3> patch)
{
    TessFactors f;
    f.edge[0] = CalcTessFactor(0.5 * (patch[1].positionOS.xyz + patch[2].positionOS.xyz));
    f.edge[1] = CalcTessFactor(0.5 * (patch[0].positionOS.xyz + patch[2].positionOS.xyz));
    f.edge[2] = CalcTessFactor(0.5 * (patch[0].positionOS.xyz + patch[1].positionOS.xyz));
    f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) / 3.0;
    return f;
}

void InterpolateBarycentrics(OutputPatch<TessControlPoint, 3> patch, float3 bary,
    out float4 positionOS, out float3 normalOS, out float2 uv)
{
    positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
    normalOS = normalize(patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z);
    uv = patch[0].uv * bary.x + patch[1].uv * bary.y + patch[2].uv * bary.z;
}

// Hull shader attributes can't be inherited across #pragma hull boundaries,
// so each pass must define its own hull function. Use this macro to stamp it out.
#define DECLARE_HULL(funcName)                                                              \
    [domain("tri")]                                                                         \
    [partitioning("fractional_odd")]                                                        \
    [outputtopology("triangle_cw")]                                                         \
    [patchconstantfunc("SharedPatchConstant")]                                              \
    [outputcontrolpoints(3)]                                                                \
    TessControlPoint funcName(InputPatch<TessControlPoint, 3> patch, uint id : SV_OutputControlPointID) \
    {                                                                                       \
        return patch[id];                                                                   \
    }

#endif
