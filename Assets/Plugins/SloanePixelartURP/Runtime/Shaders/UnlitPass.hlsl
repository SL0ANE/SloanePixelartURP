#include "Generic/PixelartShared.hlsl"

void UnlitPassFragment(Varyings input, out float4 outAlbedoProperty : BUFFER_ALBEDO_PROPERTY)
{
    outAlbedoProperties = float4(1, 1, 1, 1);
}