Shader "Mewgic/WoodsIslandEffects"
{
 Properties
 {
  [PerRendererData] _MainTex("Sprite",2D)="white"{}
  _Color("Tint",Color)=(1,1,1,1)
  _EnvironmentTime("Environment time",Float)=0
  _EffectMode("Water 0 / Golden lights 1",Float)=0
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
   struct Input {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct Output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;float4 local:TEXCOORD1;};
   sampler2D _MainTex; fixed4 _Color; float4 _ClipRect; float _EnvironmentTime, _EffectMode;
   Output vert(Input v) {Output o;o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
   fixed4 frag(Output i):SV_Target
   {
    fixed4 src=tex2D(_MainTex,i.uv);
    half water=saturate((min(src.g,src.b)-src.r-.09)*5)*smoothstep(.4,.8,src.b);
    // Mavi ağaç taçları ve taşlar su efekti almaz; yalnız alt şelale/akarsu alanı.
    water*=1-smoothstep(.47,.55,i.uv.y);
    half band=.5+.5*sin(i.uv.y*190+_EnvironmentTime*5+sin(i.uv.x*70));
    half gold=saturate((src.r-src.b-.24)*4)*smoothstep(.65,1,src.r);
    fixed4 c;
    if (_EffectMode < .5) c=fixed4(.42,.95,1,src.a*water*(.07+.24*band));
    else c=fixed4(1,.83,.42,src.a*gold*(.02+.09*(.5+.5*sin(_EnvironmentTime*1.2+i.uv.x*19))));
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
