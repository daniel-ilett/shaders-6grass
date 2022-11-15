Shader "Custom/BillboardGrass"
{
    Properties
    {
		_BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _BaseTex ("Base Texture", 2D) = "white" {}
    }
    SubShader
    {
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "AlphaTest"
			"RenderPipeline" = "UniversalPipeline"
		}

		HLSLINCLUDE
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct appdata
			{
				float4 positionOS: POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 positionCS : SV_Position;
				float2 uv : TEXCOORD0;
			};

			CBUFFER_START(UnityPerMaterial)
				float4 _BaseColor;
				sampler2D _BaseTex;
			CBUFFER_END
		ENDHLSL

        Pass
        {
			Name "GrassPass"
			Tags { "LightMode" = "UniversalForward" }

			Cull Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			v2f vert(appdata v)
			{
				v2f o;

				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.uv = v.uv;

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 color = tex2D(_BaseTex, i.uv);
				clip(color.a - 0.5f);

				return color * _BaseColor;
			}
			ENDHLSL
        }
    }
}
