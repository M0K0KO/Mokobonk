#ifndef VAT_COMMON_INCLUDED
#define VAT_COMMON_INCLUDED

TEXTURE2D(_PositionTex); SAMPLER(sampler_PositionTex);
TEXTURE2D(_NormalTex);   SAMPLER(sampler_NormalTex);

void SampleVAT(float2 vatTexel, float time, out float3 positionOS, out float3 normalOS)
{
    float clipTime = time + _ClipTimeOffset;
    float clipFrame = fmod(clipTime * _FPS, _ClipFrameCount);

    float f0 = floor(clipFrame);
    float f1 = fmod(f0 + 1.0, _ClipFrameCount);
    float blend = clipFrame - f0;

    float frameOriginY0 = (_ClipStartFrame + f0) * _RowsPerFrame;
    float frameOriginY1 = (_ClipStartFrame + f1) * _RowsPerFrame;

    float u = vatTexel.x / _TextureWidth;
    float v0 = (frameOriginY0 + vatTexel.y) / _TextureHeight;
    float v1 = (frameOriginY1 + vatTexel.y) / _TextureHeight;

    float3 p0 = SAMPLE_TEXTURE2D_LOD(_PositionTex, sampler_PositionTex, float2(u, v0), 0).xyz;
    float3 p1 = SAMPLE_TEXTURE2D_LOD(_PositionTex, sampler_PositionTex, float2(u, v1), 0).xyz;
    positionOS = lerp(p0, p1, blend);

    float3 n0 = SAMPLE_TEXTURE2D_LOD(_NormalTex, sampler_NormalTex, float2(u, v0), 0).xyz;
    float3 n1 = SAMPLE_TEXTURE2D_LOD(_NormalTex, sampler_NormalTex, float2(u, v1), 0).xyz;
    normalOS = normalize(lerp(n0, n1, blend));
}
#endif