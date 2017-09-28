Shader "Unlit/DustyColors"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Offset ("Offset", Range(0,1)) = 0.0
		_Color1 ("Color1", Color) = (1,1,1,1)
		_Color2 ("Color2", Color) = (0.3,0.3,0.35,1)
		_DarkColor ("DarkColor", Color) = (0.9,0.1,0.05,1)
        _DarkSteps("DarkSteps", Range(0,100)) = 12
        _ColorSteps("ColorSteps", Range(0,100)) = 3
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 1000

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
		    float _Offset;
		    fixed4 _Color1;
		    fixed4 _Color2;
		    fixed4 _DarkColor;
		    float _DarkSteps;
		    float _ColorSteps;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

            float GetValueFromStep (float value, float steps) {
                float scaledValue = value * steps;
                return ceil (scaledValue) - scaledValue;
            }

            float4 MixColors(float4 color1, float4 color2, float ratio)
            {
                return (color1 * ratio) + (color2 * (1.0 - ratio));
            }
			
			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);

                float value = col.r + _Offset;

                float darkLevel = GetValueFromStep(value, _DarkSteps);
                float colorLevel = GetValueFromStep(value, _ColorSteps);

                float4 resultColor = MixColors(_Color1, _Color2, colorLevel);
				return MixColors(resultColor, _DarkColor, darkLevel);
			}
			ENDCG
		}
	}
}
