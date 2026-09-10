void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float ShadowAtten)
{
#if SHADERGRAPH_PREVIEW
    Direction = float3(0.5, 0.5, 0);
    Color = 1;
    ShadowAtten = 1;
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    Light light = GetMainLight(shadowCoord);
    Direction = light.direction;
    Color = light.color;
    ShadowAtten = light.shadowAttenuation;
#endif
}