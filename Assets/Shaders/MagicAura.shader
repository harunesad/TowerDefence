Shader "Custom/MagicAura"
{
    Properties
    {
        _Color ("Aura Color", Color) = (0.2, 1.0, 0.2, 0.5)
        _PulseSpeed ("Pulse Speed", Range(0.5, 5.0)) = 1.5
        _RingCount ("Ring Count", Range(1, 8)) = 3
        _RingWidth ("Ring Width", Range(0.01, 0.3)) = 0.08
        _InnerRadius ("Inner Radius", Range(0.0, 0.5)) = 0.15
        _RotationSpeed ("Rotation Speed", Range(0, 5)) = 1.0
        _NoiseScale ("Noise Scale", Range(0, 10)) = 3.0
        _NoiseStrength ("Noise Strength", Range(0, 0.5)) = 0.1
        _GlowIntensity ("Glow Intensity", Range(1, 5)) = 2.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Name "MagicAura"
            Blend One One // Additive
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _PulseSpeed;
                float _RingCount;
                float _RingWidth;
                float _InnerRadius;
                float _RotationSpeed;
                float _NoiseScale;
                float _NoiseStrength;
                float _GlowIntensity;
            CBUFFER_END

            // Simple hash-based noise (no texture needed)
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f); // smoothstep

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Center UV to -0.5..0.5
                float2 centeredUV = input.uv - 0.5;

                // Rotate UV over time
                float angle = _Time.y * _RotationSpeed;
                float cosA = cos(angle);
                float sinA = sin(angle);
                float2 rotatedUV = float2(
                    centeredUV.x * cosA - centeredUV.y * sinA,
                    centeredUV.x * sinA + centeredUV.y * cosA
                );

                // Distance from center
                float dist = length(rotatedUV);

                // Add noise distortion to the distance
                float noiseVal = noise(rotatedUV * _NoiseScale + _Time.y * 0.5);
                float distortedDist = dist + (noiseVal - 0.5) * _NoiseStrength;

                // Create concentric rings
                float ringPattern = 0.0;
                float adjustedDist = (distortedDist - _InnerRadius) / (0.5 - _InnerRadius);

                for (int r = 0; r < (int)_RingCount; r++)
                {
                    float ringPos = ((float)r + 0.5) / _RingCount;
                    float ringDist = abs(adjustedDist - ringPos);
                    float ring = smoothstep(_RingWidth, 0.0, ringDist);
                    ringPattern += ring;
                }

                // Pulse
                float pulse = 0.7 + 0.3 * sin(_Time.y * _PulseSpeed);

                // Soft edge falloff (fade at outer boundary)
                float edgeFade = smoothstep(0.5, 0.35, dist);

                // Inner fade (transparent at very center)
                float innerFade = smoothstep(_InnerRadius * 0.5, _InnerRadius, dist);

                // Combine everything
                float finalAlpha = ringPattern * pulse * edgeFade * innerFade;
                finalAlpha = saturate(finalAlpha);

                half4 col = _Color * _GlowIntensity * finalAlpha;
                return col;
            }
            ENDHLSL
        }
    }

    // Built-in fallback for non-URP
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100

        Pass
        {
            Blend One One
            ZWrite Off
            Cull Off

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
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _PulseSpeed;
            float _RingCount;
            float _RingWidth;
            float _InnerRadius;
            float _RotationSpeed;
            float _NoiseScale;
            float _NoiseStrength;
            float _GlowIntensity;

            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centeredUV = i.uv - 0.5;
                float angle = _Time.y * _RotationSpeed;
                float cosA = cos(angle);
                float sinA = sin(angle);
                float2 rotatedUV = float2(
                    centeredUV.x * cosA - centeredUV.y * sinA,
                    centeredUV.x * sinA + centeredUV.y * cosA
                );

                float dist = length(rotatedUV);
                float noiseVal = noise(rotatedUV * _NoiseScale + _Time.y * 0.5);
                float distortedDist = dist + (noiseVal - 0.5) * _NoiseStrength;
                float ringPattern = 0.0;
                float adjustedDist = (distortedDist - _InnerRadius) / (0.5 - _InnerRadius);

                for (int r = 0; r < (int)_RingCount; r++)
                {
                    float ringPos = ((float)r + 0.5) / _RingCount;
                    float ringDist = abs(adjustedDist - ringPos);
                    float ring = smoothstep(_RingWidth, 0.0, ringDist);
                    ringPattern += ring;
                }

                float pulse = 0.7 + 0.3 * sin(_Time.y * _PulseSpeed);
                float edgeFade = smoothstep(0.5, 0.35, dist);
                float innerFade = smoothstep(_InnerRadius * 0.5, _InnerRadius, dist);
                float finalAlpha = ringPattern * pulse * edgeFade * innerFade;
                finalAlpha = saturate(finalAlpha);

                fixed4 col = _Color * _GlowIntensity * finalAlpha;
                return col;
            }
            ENDCG
        }
    }

    Fallback Off
}
