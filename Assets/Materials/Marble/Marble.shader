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
            Tags
            {
                "LightMode" = "UniversalForward"
            }
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

			#pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF	
			#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
                float3 viewDirWS : TEXCOORD2;
                float3 normalWS : NORMAL0;
                float3 positionWS : TEXCOORD3;
                float3 viewNormal : NORMAL1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            sampler2D _CameraOpaqueTexture;

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
                OUT.screenPos = float2((OUT.screenPos.x + 1.0) / 2.0, (OUT.screenPos.y + 1.0) / 2.0);

                OUT.normalWS = GetVertexNormalInputs(IN.normalOS).normalWS;
                OUT.positionWS = GetVertexPositionInputs(IN.positionOS).positionWS;
                OUT.viewDirWS = GetWorldSpaceNormalizeViewDir(OUT.positionWS);

                OUT.viewNormal = mul((float3x3)UNITY_MATRIX_V, OUT.normalWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float2 uv = float2(IN.screenPos.x, 1.0 - IN.screenPos.y) - IN.viewNormal.xy * (1-IN.viewNormal.z) * 0.025;
                half4 screenColor = tex2D(_CameraOpaqueTexture, uv);

				float3 reflectVector = reflect(-IN.viewDirWS, IN.normalWS);
                half3 reflection = GlossyEnvironmentReflection(reflectVector, IN.positionWS, 0.1, 1.0, GetNormalizedScreenSpaceUV(IN.positionHCS.xy));

                float fresnelFactor = IN.viewNormal.z * 0.5;
                float invFresnel =  1 - fresnelFactor;
                color.rgb = color.rgb * clamp(invFresnel, 0.5, 1) + reflection * 2 * invFresnel + screenColor.rgb * fresnelFactor;
                color.a = 1;
                return color;
            }
            ENDHLSL
        }
    }
}
