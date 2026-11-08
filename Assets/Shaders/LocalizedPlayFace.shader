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
    // Keep the approved gold frame intact. Keep the authored highlights and map the emerald face to amethyst.
    float mask=smoothstep(.212,.235,i.uv.x)*(1-smoothstep(.815,.842,i.uv.x))*smoothstep(.255,.285,i.uv.y)*(1-smoothstep(.63,.66,i.uv.y));
        fixed4 blank=tex2D(_MainTex,float2(.20,i.uv.y));
    fixed4 face=lerp(original,blank,mask);
    float emerald=smoothstep(.025,.17,face.g-max(face.r,face.b));
    float3 amethyst=lerp(float3(.105,.013,.24),float3(.70,.16,.92),pow(saturate(face.g),1.48));
    face.rgb=lerp(face.rgb,amethyst,emerald);
    return face*i.color;
   }
   ENDCG
  }
 }
}
