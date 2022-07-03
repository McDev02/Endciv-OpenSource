Shader "ENDCIV/StandardMask" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB) Mask(A)", 2D) = "white" {}
		_MaskMap ("Metal(R) AO(G) Smooth(A)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,2)) = 1
		_Metallic ("Metallic", Range(0,2)) = 1
		_BumpMap ("Normal", 2D) = "bump" {}
		_Occlusion ("Occlusion Power", Range(0.01,2)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Back
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _MaskMap;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		half _Occlusion;
		fixed4 _Color, _Color2;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			fixed4 mask = tex2D (_MaskMap, IN.uv_MainTex);
			o.Albedo = saturate(c.rgb+ _Color2*mask.b);
			o.Metallic = saturate(mask.r*_Metallic);
			o.Smoothness = saturate(mask.a*_Glossiness);
			o.Occlusion = pow(mask.g, _Occlusion);
			o.Normal = UnpackNormal(tex2D (_BumpMap, IN.uv_MainTex));
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
