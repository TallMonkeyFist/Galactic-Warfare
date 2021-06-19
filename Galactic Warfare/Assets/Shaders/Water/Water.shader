Shader "Custom/Water"
{
    Properties
    {
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)
        _DepthMaxDistance("Depth Maximum Distance", Float) = 3

        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0, 1)) = 0.777
        _SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)

        _FoamMaxDistance("Foam Max Distance", Float) = 0.4
        _FoamMinDistance("Foam Min Distance", Float) = 0.04

        _SurfaceDistortion("Surface Distortion", 2D) = "white" {}
        _SurfaceDistortionStrength("Surface Distortion Strength", Range(0, 1)) = 0.27

        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _SmoothStep("SmoothstepAA", Range(0, .5)) = 0.1

        _Displacement("Wave Displacement", Float) = 2.0
        _DisplacementSpeed("Wave Displacement Speed", Range(1, 100)) = 2.0

        _WaveA ("Wave A (dir, steepness, wavelength)", Vector) = (1, 0, 0.5, 10)
        _WaveB ("Wave B", Vector) = (0, 1, .25, 20)
    }

    SubShader
    {
        Tags {"Queue"="Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM

            #define SMOOTHSTEP_AA 0.1

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float3 normal : NORMAL;
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewNormal : NORMAL;
                float2 noiseUV : TEXCOORD0;
                float2 distortUV : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            sampler2D _SurfaceNoise;
            float4 _SurfaceNoise_ST;

            sampler2D _SurfaceDistortion;
            float4 _SurfaceDistortion_ST;

            float _SurfaceDistortionStrength;

            float2 unity_gradientNoise_dir(float2 p)
            {
                p = p % 289;
                float x = (34 * p.x + 1) * p.x % 289 + p.y;
                x = (34 * x + 1) * x % 289;
                x = frac(x / 41) * 2 - 1;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float unity_gradientNoise(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float d00 = dot(unity_gradientNoise_dir(ip), fp);
                float d01 = dot(unity_gradientNoise_dir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(unity_gradientNoise_dir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(unity_gradientNoise_dir(ip + float2(1, 1)), fp - float2(1, 1));
                fp = fp * fp * fp * (fp * (fp * 6 - 15) + 10);
                return lerp(lerp(d00, d01, fp.y), lerp(d10, d11, fp.y), fp.x);
            }

            void Unity_GradientNoise_float(float2 UV, float Scale, out float Out)
            {
                Out = unity_gradientNoise(UV * Scale) + 0.5;
            }

            void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
            {
                Out = UV * Tiling + Offset;
            }

            float _DisplacementSpeed;

            void getNoise(float2 UV, out float Out)
            {
                Out = 0;
                float time = _Time.y / (100 / _DisplacementSpeed);
                float2 uv = 0;
                Unity_TilingAndOffset_float(UV, float2(1, 1), float2(time, time), uv);
                float value = 0;
                Unity_GradientNoise_float(uv, 20, Out);
            }

            float4 _WaveA, _WaveB;

            float3 GerstnerWave(float4 wave, float3 p, inout float3 tangent, inout float3 binormal)
            {
                float steepness = wave.z;
                float waveLength = wave.w;
                float k = 2 * UNITY_PI / waveLength;
                float c = sqrt(9.8 / k);
                float2 d = normalize(wave.xy);
                float f = k * (dot(d, p.xz) - c * _Time.y);
                float a = steepness / k;

                tangent += float3(
                    -d.x * d.x * (steepness * sin(f)),
                    d.x * (steepness * cos(f)),
                    -d.x * d.y * (steepness * sin(f))
                    );

                binormal += float3(
                    -d.x * d.y * (steepness * sin(f)),
                    d.y * (steepness * cos(f)),
                    -d.y * d.y * (steepness * sin(f))
                    );

                return float3(
                    d.x * (a * cos(f)),
                    a * sin(f),
                    d.y * (a * cos(f))
                    );
            }

            float _Displacement;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.viewNormal = COMPUTE_VIEW_NORMAL;
                o.noiseUV = TRANSFORM_TEX(v.uv, _SurfaceNoise);
                o.distortUV = TRANSFORM_TEX(v.uv, _SurfaceDistortion);

                float noiseValue = 0;
                getNoise(o.noiseUV, noiseValue);
                noiseValue = noiseValue * _Displacement;
                float3 position = float3(v.vertex.x, noiseValue, v.vertex.z);
                o.vertex = UnityObjectToClipPos(float4(position, v.vertex.w));

                return o;
            }

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;
            float _DepthMaxDistance;

            sampler2D _CameraDepthTexture;
            float _SurfaceNoiseCutoff;

            float _FoamMaxDistance;
            float _FoamMinDistance;
            float2 _SurfaceNoiseScroll;

            sampler2D _CameraNormalsTexture;

            float4 _FoamColor;

            float _SmoothStep;

            float4 alphaBlend(float4 top, float4 bottom)
            {
                float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
                float alpha = top.a + bottom.a * (1 - top.a);
                return float4(color, alpha);
            }

            float3 FlowUVW(float2 uv, float2 flowVector, float time, bool flowB) {
                float phaseOffset = flowB ? 0.5 : 0;
                float progress = frac(time + phaseOffset);
                float3 uvw;
                uvw.xy = uv - flowVector * progress;
                uvw.z = 1 - abs(1 - 2 * progress);
                return uvw;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //Get the depth from the 2D screen space
                float depth01 = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)).r;

                //Convert 2D screen space into 3D world space depth
                float depthLinear = LinearEyeDepth(depth01);
                //get the depth difference from the plane and everything beneath it
                float depthDifference = depthLinear - i.screenPos.w;

                float waterDepthDifference = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepthDifference);

                float2 distortSample = (tex2D(_SurfaceDistortion, i.distortUV).xy * 2 - 1) * _SurfaceDistortionStrength;

                float2 noiseUV = float2((i.noiseUV.x + _Time.y * _SurfaceNoiseScroll.x) + distortSample.x, (i.noiseUV.y + _Time.y * _SurfaceNoiseScroll.y) + distortSample.y);

                float surfaceNoiseSample = tex2D(_SurfaceNoise, noiseUV).r;

                float3 existingNormal = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(i.screenPos));
                float3 normalDot = saturate(dot(existingNormal, i.viewNormal));

                float foamDistance = lerp(_FoamMaxDistance, _FoamMinDistance, normalDot);
                float foamDepthDifference01 = saturate(depthDifference / foamDistance);

                float surfaceNoiseCutoff = _SurfaceNoiseCutoff * foamDepthDifference01;
                float surfaceNoise = smoothstep(surfaceNoiseCutoff - _SmoothStep, surfaceNoiseCutoff + _SmoothStep, surfaceNoiseSample);

                float4 surfaceNoiseColor = _FoamColor;
                surfaceNoiseColor.a = surfaceNoise;

                return alphaBlend(surfaceNoiseColor, waterColor);
            }
            ENDCG
        }
    }
}
