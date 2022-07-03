// --- ENDCIV ---
// 2016 by Crowbox

Shader "ENDCIV/Burning Camprfire" {
	Properties{
		_Spec("Specular Power", Range(0.00, 1)) = 0.3
		_Shininess("Glossiness", Range(0.001, 1)) = 0.078125
		_MainTex("Base (RGB) Spec (A)", 2D) = "white" {}
		_BurnTex("Burnt (RGB) Spec (A)", 2D) = "white" {}
		_BumpMap("Normalmap", 2D) = "bump" {}

		_Edge("Edge", Range(0.5, 5)) = 1
		_BurnRate("Burn Rate", Range(0, 1)) = 0.5
		_GlowRate("Glow Rate", Range(0, 1)) = 0.5
		[HDR] _GlowColor("Glow Color", color) = (1,0.5,0,1)
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 400

		CGPROGRAM
#include "UnityPBSLighting.cginc"
#pragma surface surf StandardSpecular vertex:vert
#pragma target 3.0

			fixed4 _GlowColor;
	sampler2D _MainTex;
	sampler2D _BurnTex;
	sampler2D _BumpMap;
	half _Spec;
	half _FadeB;
	half _Shininess;

	half _Edge, _BurnRate, _GlowRate;

	struct Input {
		float2 uv_MainTex;
		float yDirection;
		INTERNAL_DATA
	};

	void vert(inout appdata_full v, out Input o)
	{
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.yDirection = 1-((v.normal.y+1)*0.5);	// mul(_Object2World, v.normal).xyz;
	}

	void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
		
		half mask = pow(saturate((IN.yDirection  + _BurnRate)*_BurnRate), 2);
		fixed4 idle = tex2D(_MainTex, IN.uv_MainTex);
		fixed4 burnt = tex2D(_BurnTex, IN.uv_MainTex);
		fixed3 tex = lerp(idle.rgb, burnt.rgb, mask);

		half3 glowMask = pow(burnt.a*saturate((IN.yDirection + _GlowRate)*_GlowRate),2);
		glowMask = ( glowMask) * _GlowColor.rgb;


		// Output
		o.Alpha = 1;//idle.a;

		o.Specular = _Shininess;
		o.Smoothness = _Spec;

		o.Albedo = tex;
		o.Emission = glowMask;
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
	}
	ENDCG
	}

		FallBack "Bumped Specular"
}

// 2016 by Crowbox