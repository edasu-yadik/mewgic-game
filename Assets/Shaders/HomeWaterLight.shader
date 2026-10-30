Shader "Mewgic/HomeWaterLight"
{
    Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) _EffectMask("Water R / Windows G",2D)="black"{} [HideInInspector] _EnvironmentTime("Time",Float)=0 _FlowSpeed("Downward speed",Float)=.022 _FlowBrightness("Water light",Range(0,.1))=.045 _FlowOpacity("Water motion",Range(0,1))=.8 }
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
            sampler2D _MainTex,_EffectMask;fixed4 _Color;
            float _EnvironmentTime,_FlowSpeed,_FlowBrightness,_FlowOpacity;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv);
                // R: su, G: pencere. Mimari veya tüm resim UV'si kaydırılmaz.
                half2 mask=tex2D(_EffectMask,i.uv).rg;
                float t=_EnvironmentTime;
                // Increasing y phase travels towards lower UV y: water flows down.
                float phase=i.uv.y*180+t*max(0,_FlowSpeed)*180;
                float2 ripple=float2(sin(phase*.61+i.uv.x*37)*.00035,sin(phase*.37)*.0012);
                fixed3 movingWater=tex2D(_MainTex,i.uv+ripple*mask.r).rgb;
                c.rgb=lerp(c.rgb,movingWater,mask.r*.32*_FlowOpacity);
                half flow=sin(phase+sin(i.uv.x*350)*.7)*_FlowBrightness;
                c.rgb+=mask.r*_FlowOpacity*flow*half3(.45,.85,1);
                c.rgb*=1+mask.g*sin(t*1.15+i.uv.x*37+i.uv.y*13)*.018;
                return c*i.color;
            }
            ENDCG
        }
    }
}
