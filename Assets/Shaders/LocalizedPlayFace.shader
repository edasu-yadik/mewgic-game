Shader "Mewgic/LocalizedPlayFace"
{
 Properties{[PerRendererData]_MainTex("Sprite",2D)="white"{} _Color("Tint",Color)=(1,1,1,1)}
 SubShader
 {
  Tags{"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="False"}
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
  Pass
  {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct vary{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};sampler2D _MainTex;fixed4 _Color;
   vary vert(app v){vary o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
   fixed4 frag(vary i):SV_Target
   {
    fixed4 original=tex2D(_MainTex,i.uv);
    // Keep the approved gold frame intact. Extend its own blank green face under the new TMP caption.
    float mask=smoothstep(.212,.235,i.uv.x)*(1-smoothstep(.815,.842,i.uv.x))*smoothstep(.255,.285,i.uv.y)*(1-smoothstep(.63,.66,i.uv.y));
    fixed4 blank=tex2D(_MainTex,float2(.20,i.uv.y));return lerp(original,blank,mask)*i.color;
   }
   ENDCG
  }
 }
}
