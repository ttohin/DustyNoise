Shader "Unlit/DustyColors"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AdditionalTex ("AdditionalTexture", 2D) = "white" {}
		_DrawingTex ("DrawingTexture", 2D) = "white" {}
		_Offset ("Offset", Range(0,1)) = 0.0
		_MainAdditionalTexRatio("MainAdditionalTexRatio", Range(0,1)) = 1.0
		_Color1 ("Color1", Color) = (1,1,1,1)
		_Color2 ("Color2", Color) = (0.3,0.3,0.35,1)
		_Color3 ("Color3", Color) = (0.3,0.3,0.35,1)
		_DarkColor ("DarkColor", Range(0,1)) = 0.3
        _DarkSteps("DarkSteps", Range(0,100)) = 12
        _ColorSteps("ColorSteps", Range(0,100)) = 3
        _LensEffect("LensEvvect", Range(-0.1,0)) = 0
        _VerticalMirror1("VerticalMirror1", Range(0,1)) = 0.1
        _VerticalMirror2("VerticalMirror2", Range(0,1)) = 0.9
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
			sampler2D _AdditionalTex;
			sampler2D _DrawingTex;
			float4 _MainTex_ST;
		    float _Offset;
		    float _MainAdditionalTexRatio;
		    fixed4 _Color1;
		    fixed4 _Color2;
		    fixed4 _Color3;
		    float _DarkColor;
		    float _DarkSteps;
		    float _ColorSteps;
		    float _LensEffect;
			float _VerticalMirror1;
			float _VerticalMirror2;
			
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

			float2 mirror(float2 textureCoordinate)
			{
				if (textureCoordinate.x < _VerticalMirror1)
					textureCoordinate.x = _VerticalMirror1 + (_VerticalMirror1 - textureCoordinate.x);
				if (textureCoordinate.x > _VerticalMirror2)
					textureCoordinate.x = _VerticalMirror2 - (textureCoordinate.x - _VerticalMirror2);
				return textureCoordinate;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				float2 textureCoordinate = i.uv;

				float2 offset = 2.0 * (textureCoordinate + float2(-0.5, -0.5));
				float distanceFromCenter = length(offset);
				offset *= distanceFromCenter * 2;
				textureCoordinate += offset * _LensEffect;

				textureCoordinate = mirror(textureCoordinate);

				float4 col = tex2D(_MainTex, textureCoordinate);
				float4 additionalTextureColor = tex2D(_AdditionalTex, textureCoordinate);

                float value = MixColors(col, additionalTextureColor, _MainAdditionalTexRatio) + _Offset;
                fixed4 drawingTextureColor = tex2D(_DrawingTex, textureCoordinate);
                value = value + drawingTextureColor.r;

                float darkLevel = GetValueFromStep(value, _DarkSteps);
                float colorLevel = GetValueFromStep(value, _ColorSteps);

                fixed4 resultColor = MixColors(_Color1, _Color2, colorLevel * 2.0) * step(colorLevel, 0.5);
                resultColor += MixColors(_Color2, _Color3, (colorLevel - 0.5) * 2.0) * step(0.5, colorLevel);
				//resultColor = MixColors(_Color1, _Color2, colorLevel);
				return MixColors(resultColor, resultColor * _DarkColor, darkLevel);
			}
			ENDCG
		}
	}
}
