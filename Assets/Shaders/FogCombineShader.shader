Shader "Unlit/FogCombineShader"
{
    Properties
    {
        _MainTex ("Base Fog Texture", 2D) = "white" {}
        _MaskTex ("Visibility Mask Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha // 일반적인 알파 블렌딩 활성화
        ZWrite Off
        Cull Off

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

            sampler2D _MainTex; // 베이스 안개 텍스처 (회색/검은색)
            sampler2D _MaskTex; // 가시성 마스크 텍스처 (하얀색 메시)
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 베이스 안개 텍스처에서 색상과 알파값을 가져옴
                fixed4 baseColor = tex2D(_MainTex, i.uv);
                
                // 가시성 마스크 텍스처에서 값을 가져옴 (하얀색은 1, 검은색은 0)
                // R 채널 값만 사용해도 충분합니다.
                fixed maskValue = tex2D(_MaskTex, i.uv).r;

                // 핵심 로직:
                // 베이스 안개의 알파값에 마스크 값을 곱합니다.
                // 마스크가 하얀색(1)인 곳 -> baseColor.a * 1 = 원래 알파값 유지
                // 마스크가 검은색(0)인 곳 -> baseColor.a * 0 = 알파가 0이 되어 투명해짐
                baseColor.a *= (1.0 - maskValue);

                return baseColor;
            }
            ENDCG
        }
    }
}
