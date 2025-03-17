#include "Generic/PixelartShared.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

void UnlitPassFragment(Varyings input,
    out float4 outAlbedoProperty : BUFFER_ALBEDO_PROPERTY,
    out float4 outSpecularProperty : BUFFER_SPECULAR_PROPERTY,
    out float4 outLightingProperty : BUFFER_LIGHTING_PROPERTY,
    out float4 outMiscProperty : BUFFER_MISC_PROPERTY,
    out float4 outNormal : BUFFER_NORMAL)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half2 uv = input.uv;
    half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    half3 color = texColor.rgb * _BaseColor.rgb;
    half alpha = texColor.a * _BaseColor.a;

    alpha = AlphaDiscard(alpha, _Cutoff);

    outAlbedoProperty = float4(0.0, 0.0, 0.0, 0.0);
    outSpecularProperty = float4(0.0, 0.0, 0.0, 0.0);
    outLightingProperty = float4(color, PackFloatInt16bit(0.0, SHADING_INDEX_STANDARD, 65535));
    outMiscProperty = float4(0.0, 0.0, 0.0, 0.0);
    outNormal = float4(normalize(input.normalWS) * 0.5 + 0.5, 1.0);
}