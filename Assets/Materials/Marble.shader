Shader "Custom/Marble"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

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
                float3 normalOS : NORMAL0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 screenPos : TEXCOORD1;
                float3 viewNormal : NORMAL0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            sampler2D _CameraOpaqueTexture;
            sampler2D _CameraNormalsTexture;

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.screenPos = OUT.positionHCS.xy / OUT.positionHCS.w;
                OUT.screenPos = float2((OUT.screenPos.x + 1.0) / 2.0, (OUT.screenPos.y + 1.0) / 2.0);//(OUT.screenPos.y - 1.0) / 2.0);

				float3 worldNormal = TransformObjectToWorldDir(IN.normalOS);
                OUT.viewNormal = mul((float3x3)UNITY_MATRIX_V, worldNormal);
                //OUT.screenPos = GetNormalizedScreenSpaceUV(GetVertexPositionInputs(IN.positionOS).positionCS.xy);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float2 uv = float2(IN.screenPos.x, 1.0 - IN.screenPos.y) - IN.viewNormal.xy * (1-IN.viewNormal.z) * 0.025;
                half4 screenColor = tex2D(_CameraOpaqueTexture, uv);
                //return float4(0, 0, IN.viewNormal.z, 1.0);
                //return float4(IN.viewNormal.xy, IN.viewNormal.z, 1.0);
                float fresnelFactor = IN.viewNormal.z * 0.9;
                return color * (1 - fresnelFactor) + screenColor * fresnelFactor;
            }
            ENDHLSL
        }
    }
}
