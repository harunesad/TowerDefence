Shader "Custom/StylizedToon"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline width", Range (.002, 0.03)) = .005
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        // OUTLINE PASS (Basit bir teknikle dış hat çizimi)
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite On
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            float _Outline;
            float4 _OutlineColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                float3 norm   = normalize(mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float2 offset = TransformViewToProjection(norm.xy);

                o.pos.xy += offset * o.pos.z * _Outline;
                o.color = _OutlineColor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }

        // MAIN TOON PASS
        CGPROGRAM
        #pragma surface surf ToonRamp

        sampler2D _Ramp;

        // Custom lighting function that uses a texture ramp based
        // on angle between light direction and normal
        #pragma lighting ToonRamp exclude_path:prepass
        inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
        {
            #ifndef USING_DIRECTIONAL_LIGHT
            lightDir = normalize(lightDir);
            #endif

            half d = dot (s.Normal, lightDir)*0.5 + 0.5;
            half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;

            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
            c.a = 0;
            return c;
        }

        sampler2D _MainTex;
        float4 _Color;

        struct Input
        {
            float2 uv_MainTex : TEXCOORD0;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }

    Fallback "Diffuse"
}
