Shader "Custom/ImpostorGrass"
{
    Properties
    {
		_BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _BaseTex ("Base Texture", 2D) = "white" {}
		_CaptureResolution("Capture Resolution", Float) = 1
		_ViewVector("View Vector", Vector) = (1, 0, 0, 0)
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
				float _CaptureResolution;
				float3 _ViewVector;
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

			float2 directionToUV(float3 direction)
			{
				float3 octant = sign(direction);
				float sum = dot(direction, octant);
				float3 octahedron = direction / sum;

				return 0.5f * float2(
					1.0f + octahedron.x + octahedron.z,
					1.0f + octahedron.z - octahedron.x
					);
			}

			v2f vert(appdata v)
			{
				v2f o;

				o.positionCS = TransformObjectToHClip(v.positionOS);

				float2 uv = directionToUV(_ViewVector);
				o.uv = (floor(uv * _CaptureResolution) + v.uv) / _CaptureResolution;

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
