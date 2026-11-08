Shader "Mewgic/ShopCostume"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Costume ("Fabric", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="False" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            sampler2D _MainTex;
            float4 _ClipRect;
            fixed4 _Costume;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct Varying { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; float2 position:TEXCOORD1; };
            Varying Vert(Input input)
            {
                Varying output;
                output.position=input.vertex.xy;
                output.vertex=UnityObjectToClipPos(input.vertex);
                output.uv=input.uv;
                output.color=input.color;
                return output;
            }
            fixed4 Frag(Varying input):SV_Target
            {
                fixed4 c=tex2D(_MainTex,input.uv);
                float violet=smoothstep(.03,.12,c.b-c.g)*smoothstep(.02,.13,c.r-c.g)*smoothstep(.48,.69,c.b/max(.01,c.r));
                float2 face=(input.uv-float2(.54,.525))/float2(.23,.145);
                violet*=smoothstep(.78,1.08,length(face));
                float light=max(c.r,c.b);
                float3 fabric=_Costume.rgb*(.12+.88*light)+.18*smoothstep(.64,1,light);
                c.rgb=lerp(c.rgb,saturate(fabric),violet);
                #ifdef UNITY_UI_CLIP_RECT
                c.a*=UnityGet2DClipping(input.position,_ClipRect);
                #endif
                return c*input.color;
            }
            ENDCG
        }
    }
}
