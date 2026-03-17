Shader "Custom/FailPlane"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _NoiseMap("Base Map", 2D) = "white" {}
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

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : NORMAL0;
            };

            TEXTURE2D(_NoiseMap);
            SAMPLER(sampler_NoiseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _NoiseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _NoiseMap);
                OUT.positionWS = GetVertexPositionInputs(IN.positionOS).positionWS;
                OUT.normalWS = GetVertexNormalInputs(IN.positionOS).normalWS;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 viewDirection = GetWorldSpaceNormalizeViewDir(IN.positionWS); 
                half3 viewProjection = viewDirection - dot(viewDirection, IN.normalWS) * IN.normalWS;

                half4 color = _BaseColor;
                float time = _Time.x + IN.positionWS.y;
                float2 uv1 = float2(IN.positionWS.x + time * 1.2872872, IN.positionWS.z + time * 0.98236634) * 0.25;
                float2 uv2 = float2(IN.positionWS.x + time * 1.0028374, IN.positionWS.z - time * 0.83746376) * 0.25;
                float2 uv3 = float2(IN.positionWS.x - time * 0.9182737, IN.positionWS.z + time * 1.09372346) * 0.25;
                color.a = 0;
               // color.a = SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uv + viewProjection.xz).r;
                color.a += SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uv1 + viewProjection.xz * 0.3).r;
                color.a += SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uv2 + viewProjection.xz * 0.15).g;
                color.a += SAMPLE_TEXTURE2D(_NoiseMap, sampler_NoiseMap, uv3).b;
                return color;
            }
            ENDHLSL
        }
    }
}
