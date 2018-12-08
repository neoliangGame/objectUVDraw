//显示绘制结果
Shader "neo/draw"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		
		_Point("Point", Vector) = (0,0,0,0)
		_PointColor("Point Color", Color) = (0,1,1,1)
		_RadiusSquare("Point radius square", Range(0,1000)) = 1
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 100

		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma multi_compile_fwdbase

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				half3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 light : COLOR0;
				float4 worldPos : COLOR1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 _Point;
			float4 _PointColor;
			float _RadiusSquare;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				half3 lightDir = normalize(ObjSpaceLightDir(v.vertex));
				half3 normalDir = normalize(v.normal);
				o.light = float4(0,0,0,0);
				o.light.x = dot(normalDir, lightDir) * 0.9 + 0.2;
				if (o.light.x < 0) {
					o.light.x = -o.light.x * 0.2;
				}

				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float4 diff = i.worldPos - _Point;
				float square = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;
				if (square < _RadiusSquare)
					col *= _PointColor;
				return col * i.light.x;
			}
			ENDCG
		}
	}
}
