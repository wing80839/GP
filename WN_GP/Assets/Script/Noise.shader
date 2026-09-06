// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Noise Effect"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseXSpeed ("Noise X Speed", Float) = 100.0
        _NoiseYSpeed ("Noise Y Speed", Float) = 100.0
        _Cutoff ("Cutoff Value", Range(0, 1.0)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
         
        Blend SrcAlpha OneMinusSrcAlpha
 
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            fixed _NoiseXSpeed;
            fixed _NoiseYSpeed;
            fixed _Cutoff;
             
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
             
            fixed4 frag (v2f i) : COLOR
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed2 noiseUV = i.uv.xy + fixed2(_NoiseXSpeed, _NoiseYSpeed) * _SinTime.z;
                fixed4 noiseTex = tex2D(_NoiseTex, noiseUV);
                 
                if(noiseTex.r > _Cutoff)
                    noiseTex.a = 0;
                 
                return noiseTex * col;
            }
            ENDCG
        }
    }
}