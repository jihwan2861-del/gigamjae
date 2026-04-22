Shader"UI/Blur"
{
    Properties
    {
        _Size ("Blur Size", Range(0, 10)) = 1
        _Color ("Main Color", Color) = (0,0,0,0.5) // 색상과 투명도 조절용
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        GrabPass { "_BackgroundGrab" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};
struct v2f
{
    float4 vertex : SV_POSITION;
    float4 grabPos : TEXCOORD0;
};

sampler2D _BackgroundGrab;
float4 _BackgroundGrab_TexelSize;
float _Size;
fixed4 _Color;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.grabPos = ComputeGrabScreenPos(o.vertex);
    return o;
}

half4 frag(v2f i) : SV_Target
{
    half4 col = half4(0, 0, 0, 0);
    float dist = _Size;
                
    col += tex2Dproj(_BackgroundGrab, i.grabPos + float4(-dist, -dist, 0, 0) * _BackgroundGrab_TexelSize.xyyy);
    col += tex2Dproj(_BackgroundGrab, i.grabPos + float4(dist, -dist, 0, 0) * _BackgroundGrab_TexelSize.xyyy);
    col += tex2Dproj(_BackgroundGrab, i.grabPos + float4(-dist, dist, 0, 0) * _BackgroundGrab_TexelSize.xyyy);
    col += tex2Dproj(_BackgroundGrab, i.grabPos + float4(dist, dist, 0, 0) * _BackgroundGrab_TexelSize.xyyy);
    col /= 4;

                // 블러된 배경 위에 내가 설정한 색상과 투명도를 섞어줍니다.
    return lerp(col, _Color, _Color.a);
}
            ENDCG
        }
    }
}
