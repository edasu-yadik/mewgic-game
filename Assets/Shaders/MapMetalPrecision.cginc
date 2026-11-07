// The Windows Metal compiler translates through D3D bytecode. Use full
// precision for these layer masks to keep its comparison operands valid.
#if defined(SHADER_API_METAL)
    #undef fixed4
    #undef fixed3
    #undef half
    #define fixed4 float4
    #define fixed3 float3
    #define half float
#endif
