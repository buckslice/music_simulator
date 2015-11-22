Shader "Custom/Terrain" {
	Properties {
		//_Color ("Color", Color) = (1,1,1,1)
        _PlayerPos("Player Position", Vector) = (0.0,0.0,0.0,1.0)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "bump" {}
        _NoiseAmp("Noise Amplitude", Range(0,1)) = 0.0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

        #include "UnityCG.cginc"

		sampler2D _MainTex;
        sampler2D _NoiseTex;
        half _NoiseAmp;
        float4 _PlayerPos;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

		struct Input {
			float2 uv_MainTex : TEXCOORD0;
            //float2 uv2 : TEXCOORD1;
            float3 color : Color;
		};

        //void vert(inout appdata_full v, out Input o) {
        //    //UNITY_INITIALIZE_OUTPUT(Input, o);
        //    //o.color = abs(v.normal);
        //}

        void vert(inout appdata_full v) {
            float ripple = sin(-_Time.y + distance(v.vertex.xz, _PlayerPos.xz) / 10.0)*10.0;

            //float newy = distance(v.vertex.xz, _PlayerPos.xz) / 100.0 + 1.0;
            //newy = 1.0 / newy;
            //newy = 1.0 - newy;
            float xx = distance(v.vertex.xz, _PlayerPos.xz) / 100.0;
            xx = xx * xx * (3 - 2 * xx);

            v.vertex.y += xx * ripple;
        }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
            float2 uvs = IN.uv_MainTex;
            fixed4 uvx = tex2D(_NoiseTex, uvs);
            uvs.x += .5;
            fixed4 uvy = tex2D(_NoiseTex, uvs);
            float2 offset = float2(uvx.r - 0.5, uvy.r - 0.5);
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex + offset*_NoiseAmp);
			o.Albedo = IN.color.rgb * c * 10.0;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
            o.Alpha = 1.0f;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
