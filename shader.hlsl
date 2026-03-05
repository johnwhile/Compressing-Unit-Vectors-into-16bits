float3 DecodeUnitVector16(min16uint encode)
{
    int n = encode & 0x1FFF;
    int i = (sqrt(1 + 8 * n) - 1) / 2;
    int j = n - (i + 1) * i / 2;
    
    float phi = i * 1.5707963267 / 126;
    float theta = i > 0 ? j * 1.5707963267 / i : 0;

    float3 normal = float3(
        cos(theta) * sin(phi),
        cos(phi),
        sin(theta) * sin(phi));
    
    if ((encode & 0x8000) != 0) normal.x *= -1;
    if ((encode & 0x4000) != 0) normal.y *= -1;
    if ((encode & 0x2000) != 0) normal.z *= -1;
    
    return normal;
}
