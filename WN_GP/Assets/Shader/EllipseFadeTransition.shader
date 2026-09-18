Shader "UI/EllipseFadeTransition"
{
    // 用於全螢幕 UI Image 的橢圓淡入淡出轉場
    // _Radius 由 0 長到 maxRadius：畫面從全黑 -> 橢圓由中心向外擴散 -> 完全透明(淡入完成)
    // _Radius 由 maxRadius 縮到 0：則是淡出，畫面從透明收合成全黑

    Properties
    {
        _Color ("Overlay Color", Color) = (0,0,0,1)
        _Center ("Center (Viewport 0-1)", Vector) = (0.5,0.5,0,0)
        _Radius ("Radius", Range(0,2)) = 0
        _EllipseScale ("Ellipse Scale (x=width比例,y=height比例)", Vector) = (1.78,1,0,0)
        _Softness ("Edge Softness", Range(0.001,0.5)) = 0.05
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            fixed4 _Color;
            float4 _Center;
            float  _Radius;
            float4 _EllipseScale;
            float  _Softness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - _Center.xy;
                uv *= _EllipseScale.xy; // 依螢幕比例或美術需求拉伸成橢圓
                float dist = length(uv);

                // dist < Radius-Softness => 透明(洞內，看得到場景)
                // dist > Radius          => 不透明(黑幕)
                float alpha = smoothstep(_Radius - _Softness, _Radius, dist);

                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
