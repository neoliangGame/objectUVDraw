//在摄像机还没渲染任何物体之前
//把需要绘画的物体的点周围的UV信息绘制上去
Shader "neo/drawPreGrab"
{
	Properties
	{
		_MainTex("MainTex",2D) = "white"{}
		_Point("Point", Vector) = (0,0,0,0)
		_RadiusSquare("Point radius square", Range(0,1000)) = 1
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Background-1" }
		LOD 100

		Cull Back
		ZTest Off
		ZWrite Off

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
				float4 worldPos : COLOR0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _Point;
			float _RadiusSquare;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 uvMap = float4(i.uv.x ,i.uv.y ,0,1);

				float4 diff = i.worldPos - _Point;
				float square = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;
				clip(_RadiusSquare - square);

				return uvMap;
			}
			ENDCG
		}

		GrabPass{
			"neoGrabTexture"
		}
	}
}
