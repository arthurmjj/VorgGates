Shader "VorgGates/AnimatedEmissive"
{
    Properties
    {
        _ColorA("Dark", Color) = (0.25, 0.04, 0.01, 1)
        [HDR] _ColorB("Hot", Color) = (3, 1.1, 0.2, 1)
        _Speed("Flow Speed", Float) = 0.25
        _Scale("Noise Scale", Float) = 1.3
        _Pulse("Pulse", Float) = 0.15
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ColorA;
                half4 _ColorB;
                float _Speed;
                float _Scale;
                float _Pulse;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            float Hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), f.x), lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), f.x), f.y);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = (input.positionWS.xz + input.positionWS.yy * 0.6) * _Scale;
                float time = _Time.y * _Speed;
                float flow = Noise(uv + float2(time, time * 0.6));
                flow = flow * 0.65 + Noise(uv * 2.3 - float2(time * 1.4, time)) * 0.35;
                float heat = smoothstep(0.28, 0.78, flow);
                float pulse = 1.0 + sin(_Time.y * 2.2 + input.positionWS.x * 0.7) * _Pulse;
                return half4(lerp(_ColorA.rgb, _ColorB.rgb, heat * heat) * pulse, 1.0);
            }
            ENDHLSL
        }
    }
}
