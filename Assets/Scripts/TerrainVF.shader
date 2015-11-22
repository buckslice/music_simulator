Shader "Custom/TerrainVF"
{
	Properties
	{
        _PlayerPos("Player Position", Vector) = (0.0,0.0,0.0,1.0)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "bump" {}
        _ColorGradient("ColorGradient", 2D) = "white" {}
        _HeightGradient("HeightGradient", 2D) = "black" {}
        _NoiseAmp("Noise Amplitude", Range(0,1)) = 0.0
        _NoiseTexSize("Noise Texture Size", Range(0.015625,1)) = 1.0
        _AlphaBlendMin("Alpha Blend Min", Float) = 200.0
        _AlphaBlendMax("Alpha Blend Max", Float) = 300.0
        _WaveColorStrength("Wave Color Strength", Range(0, 1)) = 0
        _WaveColorMultiplication("Wave Color Multiplication", Range(0, 1)) = 0
        _WaveSizeStrength("Wave Size Strength", Float) = 10
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"}
		LOD 100

		Pass
		{
            Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma target 3.0

			#include "UnityCG.cginc"

			//struct appdata
			//{
			//	float4 vertex : POSITION;
			//	float2 uv : TEXCOORD0;
			//};

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _ColorGradient;
            sampler2D _HeightGradient;
            half _NoiseAmp;
            half _NoiseTexSize;
            half _AlphaBlendMin;
            half _AlphaBlendMax;
            half _WaveColorStrength;
            half _WaveSizeStrength;
            half _WaveColorMultiplication;
            float4 _PlayerPos;
            fixed4 _Color; 
			float4 _MainTex_ST;
            float4 _NoiseTex_ST;

			struct v2f
			{
				float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float2 uv3 :TEXTCOORD2;
				float4 vertex : SV_POSITION;
                float4 color: COLOR;
			};
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.texcoord1, _NoiseTex);

                // create ripple and amplify it based on player distance
                float dist = o.uv3.x = distance(v.vertex.xz, _PlayerPos.xz);
                float ripple = sin(-_Time.y + dist / 10.0)*10.0;
                float xx = dist / 100.0;
                xx = xx * xx * (3 - 2 * xx);

                //float ripplec = sin(-_Time.z*1.0 + dist / 2.0)*10.0;
                float4 uvGradient = float4(o.uv3.x / _AlphaBlendMax, 0.5, 0, 0);

                fixed4 gradientColora = tex2Dlod(_HeightGradient, uvGradient);



                //o.vertex.y += xx * ripple;
                o.vertex.y += xx * _WaveSizeStrength * gradientColora.r;
                o.color = v.color;

                //o.color.b = 1.0 - ripplec;
                //o.color.g = frac(ripple/10.0);
                o.color.a = (dist - _AlphaBlendMin) / (_AlphaBlendMax - _AlphaBlendMin);
                o.color.a = saturate(1.0 - o.color.a);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
                // tweak secondary uvs by noise tex
                float2 uvs = i.uv2/ _NoiseTexSize;
                uvs.x += _Time.gg;
                uvs.y += _Time.gg;
                fixed4 uvx = tex2D(_NoiseTex, uvs);
                uvs.x += .5;
                fixed4 uvy = tex2D(_NoiseTex, uvs);

                float2 uvGradient = float2(i.uv3.x / _AlphaBlendMax, 0.5);
                fixed4 gradientColor = tex2D(_ColorGradient, uvGradient);
                i.color.rgb = lerp(i.color.rgb, gradientColor.rgb, _WaveColorStrength);
                i.color.rgb = lerp(i.color.rgb, i.color.rgb * gradientColor.rgb * 2.0, _WaveColorMultiplication);

                float2 offset = float2(uvx.r - 0.5, uvy.r - 0.5);
                fixed4 c = tex2D(_MainTex, i.uv + offset*_NoiseAmp);
                return c * i.color;
			}
			ENDCG
		}
	}
}
