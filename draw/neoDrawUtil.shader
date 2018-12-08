//读取GrabTexture信息
Shader "neo/drawUtil"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		//0.draw grab texture
		Pass
		{
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
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			sampler2D neoGrabTexture;
			float4 neoGrabTexture_ST;

			float Width;
			float Height;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, neoGrabTexture);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(neoGrabTexture, i.uv);
				if (col.a > 0.5) {
					return col;
				}
				return float4(0, 0, 0, 0);
			}
			ENDCG
		}
	}
}
