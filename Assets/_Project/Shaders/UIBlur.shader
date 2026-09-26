// Separable Gaussian blur for ScreenBlur (Graphics.Blit, RT to RT — outside the render loop, so a
// plain CG shader is fine under URP). _Direction picks the axis, _Radius scales the taps.
Shader "Dogtor/UIBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Radius ("Radius", Float) = 1
        _Direction ("Direction", Vector) = (1, 0, 0, 0)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Radius;
            float4 _Direction;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 step = _Direction.xy * _MainTex_TexelSize.xy * _Radius;
                fixed4 c = tex2D(_MainTex, i.uv) * 0.227027;
                c += (tex2D(_MainTex, i.uv + step * 1.384615) + tex2D(_MainTex, i.uv - step * 1.384615)) * 0.316216;
                c += (tex2D(_MainTex, i.uv + step * 3.230769) + tex2D(_MainTex, i.uv - step * 3.230769)) * 0.070270;
                return c;
            }
            ENDCG
        }
    }
}
