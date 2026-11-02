Shader "Mewgic/StarlightIslandLayers"
{
 Properties
 {
  [PerRendererData] _MainTex("Sprite",2D)="white"{}
  _Color("Tint",Color)=(1,1,1,1)
  _EnvironmentTime("Environment time",Float)=0
  _LayerMode("Base 0 / Water 1 / Blossom Glow 2",Float)=0
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
  Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
  Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
  Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
   #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
   #include "UnityCG.cginc"
   #include "UnityUI.cginc"
   struct Input{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct Output{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;float4 local:TEXCOORD1;};
   sampler2D _MainTex;fixed4 _Color;float4 _ClipRect;float _EnvironmentTime,_LayerMode;
   Output vert(Input v){Output o;o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
   fixed4 frag(Output i):SV_Target
   {
    fixed4 s=tex2D(_MainTex,i.uv);fixed4 c=s;
    // Aynı atlas koordinatlarından ikili maske: su pikselleri tabandan çıkar, yalnız su bileşeni çizer.
    half water=step(.09,min(s.g,s.b)-s.r)*step(.55,s.g)*(1-step(.84,i.uv.y));
    if(_LayerMode<.5){c.a*=1-water;}
    else if(_LayerMode<1.5){c.a*=water;c.rgb*=1+.035*sin(i.uv.y*190+_EnvironmentTime*4.5+sin(i.uv.x*53));}
    else{half glow=step(.16,s.r-s.g)*step(.55,s.r);c=fixed4(1,.65,1,s.a*glow*(.008+.018*(.5+.5*sin(_EnvironmentTime*.65+i.uv.x*14))));}
    c*=i.color;
    #ifdef UNITY_UI_CLIP_RECT
     c.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
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
