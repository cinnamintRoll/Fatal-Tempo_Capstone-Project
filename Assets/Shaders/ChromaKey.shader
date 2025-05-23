Shader "Custom/ChromaKeyObject"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _thresh ("Threshold", Range(0, 1)) = 0.2
        _slope ("Slope", Range(0, 1)) = 0.1
        _keyingColor ("Key Colour", Color) = (0,1,0,1)
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
            float4 _Color; // This enables SpriteRenderer.color support
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
                float4 texColor = tex2D(_MainTex, i.uv);
                float d = distance(_keyingColor.rgb, texColor.rgb);
                float edge0 = _thresh * (1.0 - _slope);
                float alpha = smoothstep(edge0, _thresh, d);

                // Apply SpriteRenderer tint color
                float4 finalColor = texColor * _Color;
                finalColor.a *= alpha;

                return finalColor;
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}
