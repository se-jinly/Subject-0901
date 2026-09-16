// Outline.hlsl — 한쪽 방향 엣지 검출 (Fullscreen Shader Graph Custom Function용)

#ifndef OUTLINE_INCLUDED
#define OUTLINE_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

// 원근/직교 모두 "카메라로부터의 월드 단위 거리"로 변환
float Outline_EyeDepth(float2 uv)
{
    float raw = SampleSceneDepth(uv);
    if (unity_OrthoParams.w > 0.5)
    {
    #if UNITY_REVERSED_Z
        raw = 1.0 - raw;
    #endif
        return lerp(_ProjectionParams.y, _ProjectionParams.z, raw);
    }
    return LinearEyeDepth(raw, _ZBufferParams);
}
#endif

void Outline_float(float2 UV, float2 Texel, float DepthThreshold, float NormalThreshold, float3 NormalBias,
                   out float DepthEdge, out float NormalEdge)
{
#if SHADERGRAPH_PREVIEW
    DepthEdge = 0;
    NormalEdge = 0;
#else
    float2 offsets[4] =
    {
        float2( Texel.x, 0),
        float2(-Texel.x, 0),
        float2(0,  Texel.y),
        float2(0, -Texel.y)
    };

    float  depth  = Outline_EyeDepth(UV);
    float3 normal = SampleSceneNormals(UV);

    float depthSum  = 0;
    float normalSum = 0;

    for (int i = 0; i < 4; i++)
    {
        float2 nUV     = UV + offsets[i];
        float  nDepth  = Outline_EyeDepth(nUV);
        float3 nNormal = SampleSceneNormals(nUV);

        // 이웃이 나보다 멀면 + → 나는 앞 물체의 가장자리
        float dDiff = nDepth - depth;
        depthSum += saturate(dDiff);

        // 노멀 차이. 주름 양쪽 중 한쪽만 채택
        float3 nDiff = normal - nNormal;
        float oneSide = step(0.0, dot(nDiff, NormalBias));
        // 이웃이 확실히 더 앞이면 그쪽 담당
        float notBehind = step(-DepthThreshold, dDiff);
        normalSum += dot(nDiff, nDiff) * oneSide * notBehind;
    }

    DepthEdge  = step(DepthThreshold, depthSum);
    NormalEdge = step(NormalThreshold, normalSum);
#endif
}

#endif