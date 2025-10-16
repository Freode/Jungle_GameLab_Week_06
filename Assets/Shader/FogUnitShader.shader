Shader "Custom/FogUnitShader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _FogTex ("Fog of War Texture", 2D) = "white" {}
        _FogScale ("Fog World Scale", Float) = 1000.0
        _FogOffset ("Fog World Offset", Vector) = (0,0,0,0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        sampler2D _MainTex;
        sampler2D _FogTex;
        float _FogScale;
        float4 _FogOffset;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 기본 텍스처 색상
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

            // 월드 좌표 기반 FoW 텍스처 샘플링 좌표 계산
            float2 fogUV = (IN.worldPos.xz - _FogOffset.xz) / _FogScale;
            float fog = tex2D(_FogTex, fogUV).r;

            // 안개 영향으로 알파 조절 (1이면 완전 보임, 0이면 완전 투명)
            c.a *= saturate(fog);

            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }

    FallBack "Transparent"
}
