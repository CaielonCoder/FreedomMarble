#ifndef FUNCTION_COLLECTION
#define FUNCTION_COLLECTION

void FloorCheckered_float(float3 worldPos, float4 color1, float4 color2, float thickness, out float4 color)
{
    float value = (floor(worldPos.x) + floor(worldPos.z)) % 2;
    color = color1 * value + color2 * (1 - value);
    float frame = abs(worldPos.x - round(worldPos.x)) < thickness;
    frame += abs(worldPos.z - round(worldPos.z)) < thickness;
    color *= 1 - frame > 0;
}

void FloorCheckered_half(half3 worldPos, half4 color1, half4 color2, half thickness, out half4 color)
{
    half value = (floor(worldPos.x) + floor(worldPos.z)) % 2;
    color = color1 * value + color2 * (1 - value);
    half frame = abs(worldPos.x - round(worldPos.x)) < thickness;
    frame += abs(worldPos.z - round(worldPos.z)) < thickness;
    color *= 1 - frame > 0;
}

#endif