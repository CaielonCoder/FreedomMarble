Shader "Custom/FailPlane"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _NoiseMap("Base Map", 2D) = "white" {}
        _DepthSize("Depth Size", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"}

		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);
            float _DepthSize;

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _NoiseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = GetVertexPositionInputs(IN.positionOS).positionWS.xz * 0.3;
                OUT.screenPos = ComputeScreenPos(GetVertexPositionInputs(IN.positionOS).positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = GetNormalizedScreenSpaceUV(IN.positionHCS);
                float sceneZ = LinearEyeDepth(SampleSceneDepth(uv), _ZBufferParams);
                float depthOcclusion = 1 - clamp((sceneZ - IN.screenPos.w) / _DepthSize, 0, 1);
                float3 sceneColor = SampleSceneColor(uv);

                half4 noise1 = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, IN.uv + float2(_Time.x * 0.6234324, -_Time.x * 0.309458));
                half4 noise2 = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, IN.uv + float2(-_Time.x * 0.4326774, _Time.x * 0.873542));
                float colorFactor = clamp((noise1.r + noise2.g) * 1, 0, 1);

                sceneColor = sceneColor * depthOcclusion + float3(0.02, 0.2, 0.02) * (1 - depthOcclusion);
                float4 result = float4(0, 0, 0, 1) * (1 - colorFactor) + float4(sceneColor, 1.0) * colorFactor;
                result.a = 1 - clamp((depthOcclusion - 0.8) * 5, 0, 1);
                return result;
            }
            ENDHLSL
        }
    }
}
