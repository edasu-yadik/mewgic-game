Shader "Mewgic/MapWaterLight"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite",2D)="white"{}
        _Color("Tint",Color)=(1,1,1,1)
        _EnvironmentTime("Environment time",Float)=0
        _TerraceFeather("Terrace edge feather",Float)=.024
        _JourneyWater("Journey water amount",Float)=0
        _StencilComp("Stencil Comparison",Float)=8
        _Stencil("Stencil ID",Float)=0
        _StencilOp("Stencil Operation",Float)=0
        _StencilWriteMask("Stencil Write Mask",Float)=255
        _StencilReadMask("Stencil Read Mask",Float)=255
        _ColorMask("Color Mask",Float)=15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Alpha Clip",Float)=0
    }
    SubShader
    {
        Tags{"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
        Stencil{Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma multi_compile_local _ MEWGIC_CRYSTAL_FLOW
            #pragma multi_compile_local _ MEWGIC_CRYSTAL_FLOW_LITE
            #pragma multi_compile_local _ MEWGIC_JOURNEY_PLATE MEWGIC_JOURNEY_CAT MEWGIC_JOURNEY_STONE MEWGIC_CRYSTAL_REFERENCE
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct Input{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct Output{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;float4 mask:TEXCOORD1;};
            sampler2D _MainTex;fixed4 _Color;float4 _ClipRect;float _EnvironmentTime;float _TerraceFeather;float _JourneyWater;
            float _UIMaskSoftnessX, _UIMaskSoftnessY;
            Output vert(Input v)
            {
                Output o;
                o.vertex=UnityObjectToClipPos(v.vertex);
                o.uv=v.uv;
                o.color=v.color*_Color;
                float2 pixelSize=o.vertex.w/abs(mul((float2x2)UNITY_MATRIX_P,_ScreenParams.xy));
                float4 clipRect=clamp(_ClipRect,-2e10,2e10);
                o.mask=float4(v.vertex.xy*2-clipRect.xy-clipRect.zw,
                    .25/(.25*float2(_UIMaskSoftnessX,_UIMaskSoftnessY)+abs(pixelSize)));
                return o;
            }
            fixed4 frag(Output i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv);
                #ifdef MEWGIC_JOURNEY_STONE
                c=lerp(fixed4(1,1,1,1),c,step(0,i.uv.x));
                #else
                half water=saturate((min(c.g,c.b)-c.r-.12)*4)*smoothstep(.4,.8,c.b);
                half window=saturate((c.r-c.b-.25)*3)*smoothstep(.6,1,c.r);
                c.rgb+=water*sin(i.uv.y*250+_EnvironmentTime*1.8+sin(i.uv.x*60))*half3(.008,.014,.018);
                c.rgb*=1+window*sin(_EnvironmentTime*1.1+i.uv.x*31+i.uv.y*17)*.012;
                #ifdef MEWGIC_CRYSTAL_REFERENCE
                half cascade=smoothstep(.035,.24,min(c.g,c.b)-c.r)*smoothstep(.22,.63,c.g);
                float basin=(1-smoothstep(.086,.134,i.uv.y))*smoothstep(.52,.68,i.uv.x);
                float fallPhase=i.uv.y*175+_EnvironmentTime*11.5+sin(i.uv.x*112)*1.7;
                float poolPhase=i.uv.x*107+i.uv.y*74-_EnvironmentTime*5.2;
                float flowPhase=lerp(fallPhase,poolPhase,basin);
                float2 waterOffset=lerp(float2(.0009*sin(fallPhase*.51),.006*sin(fallPhase*.57)),
                    float2(.004*sin(poolPhase),.0011*sin(poolPhase*.61)),basin);
                fixed3 current=tex2D(_MainTex,i.uv+waterOffset*cascade).rgb;
                half flash=smoothstep(.10,.93,sin(flowPhase)*.64+sin(flowPhase*.53)*.36);
                c.rgb=lerp(c.rgb,current,cascade*.68)+cascade*(flash-.21)*half3(.14,.22,.25);
                #elif defined(MEWGIC_JOURNEY_PLATE)
                half plateCyan=smoothstep(.08,.31,min(c.g,c.b)-c.r)*smoothstep(.29,.69,c.g)*_JourneyWater;
                float pool=1-smoothstep(.105,.20,i.uv.y);
                float vertical=i.uv.y*192+i.uv.x*18+_EnvironmentTime*8.4;
                float horizontal=i.uv.x*125+i.uv.y*106-_EnvironmentTime*3.5;
                float phase=lerp(vertical,horizontal,pool);
                float2 drift=lerp(float2(.0008*sin(vertical*.6),.0025*sin(vertical)),float2(.004*sin(horizontal),.0008*sin(horizontal*.4)),pool);
                fixed3 shifted=tex2D(_MainTex,i.uv+drift*plateCyan).rgb;
                half ripple=smoothstep(.42,.95,sin(phase+sin(i.uv.x*230)*.68));
                c.rgb=lerp(c.rgb,shifted,plateCyan*.55)+plateCyan*ripple*half3(.035,.084,.10);
                #elif defined(MEWGIC_CRYSTAL_FLOW)
                half cyan=smoothstep(.04,.26,min(c.g,c.b)-c.r)*smoothstep(.16,.62,c.g);
                float phase=i.uv.y*108+i.uv.x*20+_EnvironmentTime*4.2;
                #ifndef MEWGIC_CRYSTAL_FLOW_LITE
                float2 drift=float2(sin(phase*.54)*.0016,sin(phase)*.0035);
                half leaves=smoothstep(.13,.39,max(c.g-c.b,min(c.r,c.b)-c.g));
                float leafSway=sin(i.uv.y*44+i.uv.x*51+_EnvironmentTime*1.65);
                fixed3 flow=tex2D(_MainTex,i.uv+drift*cyan+float2(leafSway*.0014,0)*leaves).rgb;
                c.rgb=lerp(c.rgb,flow,max(cyan*.46,leaves*.37));
                #endif
                half stream=smoothstep(.38,.96,sin(phase+sin(i.uv.x*171)*.85));
                half crystal=smoothstep(.40,.88,c.b)*smoothstep(.08,.3,c.b-c.g);
                c.rgb*=1+window*.035*sin(_EnvironmentTime*2.8+i.uv.x*117+i.uv.y*43);
                c.rgb+=cyan*stream*half3(.036,.092,.104);
                c.rgb+=crystal*(.028+.024*sin(_EnvironmentTime*1.7+i.uv.x*32))*half3(.65,.55,1);
                c.a*=smoothstep(0,_TerraceFeather,1-i.uv.y);
                #endif
                #ifdef MEWGIC_JOURNEY_CAT
                c.a=smoothstep(.075,.87,c.a);
                #endif
                #endif
                c*=i.color;
                #ifdef UNITY_UI_CLIP_RECT
                float2 mask=saturate((_ClipRect.zw-_ClipRect.xy-abs(i.mask.xy))*i.mask.zw);
                c.a*=mask.x*mask.y;
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(c.a-.001);
                #endif
                return c;
            }
            ENDCG
        }
    }
}
