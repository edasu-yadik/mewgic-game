Shader "Mewgic/HomeWaterLight"
{
    Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) [HideInInspector] _EnvironmentTime("Time",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v2f {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            sampler2D _MainTex;fixed4 _Color;
            float _EnvironmentTime;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv);
                // Color masks affect bright cyan water and amber windows, never deform architecture.
                half water=saturate((min(c.g,c.b)-c.r-.12)*4)*smoothstep(.4,.8,c.b);
                half window=saturate((c.r-c.b-.25)*3)*smoothstep(.6,1,c.r);
                half flow=sin(i.uv.y*260+_EnvironmentTime*1.8+sin(i.uv.x*91)*2)*.018;
                c.rgb+=water*flow*half3(.45,.85,1);
                c.rgb*=1+window*(sin(_EnvironmentTime*1.4+i.uv.x*37+i.uv.y*13)*.014);
                return c*i.color;
            }
            ENDCG
        }
    }
}
