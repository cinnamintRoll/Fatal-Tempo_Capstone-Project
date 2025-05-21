Shader "Custom/ChromaKeyObject"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _thresh ("Threshold", Range(0, 1)) = 0.2
        _slope ("Slope", Range(0, 1)) = 0.1
        _keyingColor ("Key Colour", Color) = (0,1,0,1) // Default green screen
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _keyingColor;
            float _thresh;
            float _slope;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 col = tex2D(_MainTex, i.uv).rgb;
                float d = distance(_keyingColor.rgb, col);
                float edge0 = _thresh * (1.0 - _slope);
                float alpha = smoothstep(edge0, _thresh, d);
                return float4(col, alpha);
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}
