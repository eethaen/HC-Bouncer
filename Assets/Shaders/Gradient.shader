// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Gradient"
{
	Properties{
		 _ColorA("Color A", Color) = (1,1,1,1)
		 _ColorB("Color B", Color) = (1,1,1,1)
		 _MainTex("Main Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _ColorA;
			fixed4 _ColorB;

			struct v2f {
				float4 position : SV_POSITION;
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.color = 1;
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 color;
				float4 texColor = tex2D(_MainTex, i.uv);
				color.rgb = lerp(_ColorA, _ColorB, texColor.r);
				color.a = texColor.a * i.color.a;
				return color;
			}
			ENDCG
		}
	}
    FallBack "Diffuse"
}
