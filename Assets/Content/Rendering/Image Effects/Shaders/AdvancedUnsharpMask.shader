// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Written 2012 by EyecyArt

Shader "Hidden/Advanced UnsharpMask" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Pixel ("Pixel", Range (0, 5)) = 1
	_Amount ("Amount", Range (-5, 5)) = 1
	_Amount2 ("Second Amount", Range (-5, 5)) = 2
	_Threshold ("Threshold", Range (0, 1)) = 0.5
	//_Contrast ("Contrast", Range (0, 2)) = 1
}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off Fog { Mode off }

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"
#pragma target 3.0

struct v2f {
	float4 pos	: POSITION;
	float2 uv	: TEXCOORD0;
}; 

uniform sampler2D _MainTex;
half _Pixel, _Amount, _Amount2, _Threshold;//, _Contrast;
fixed2 _ScreenSize;

v2f vert (appdata_img v)
{
	v2f o;
	o.pos = UnityObjectToClipPos (v.vertex);
	o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
	return o;
}

fixed4 frag (v2f i) : COLOR
{
	fixed4 col = tex2D(_MainTex, i.uv);
	
	fixed2 uvOff = float2(_Pixel/_ScreenSize.x, _Pixel/_ScreenSize.y);
	fixed2 uvOffyx = float2(_Pixel/_ScreenSize.x, -_Pixel/_ScreenSize.y);
	fixed2 uvOffx = float2(uvOff.x, 0);
	fixed2 uvOffy = float2(0, uvOff.y);
	
	fixed4 blurXY = tex2D(_MainTex, i.uv - uvOff) + tex2D(_MainTex, i.uv + uvOff);
	fixed4 blurYX = tex2D(_MainTex, i.uv - uvOffyx) + tex2D(_MainTex, i.uv + uvOffyx);
	
	fixed4 blurX = tex2D(_MainTex, fmod(10+i.uv - uvOffx,1)) + tex2D(_MainTex, fmod(10+i.uv + uvOffx,1));
	fixed4 blurY = tex2D(_MainTex, fmod(10+i.uv - uvOffy,1)) + tex2D(_MainTex, fmod(10+i.uv + uvOffy,1));
	
	fixed4 blur = (blurXY+blurYX+blurX+blurY+col)/9;
	//fixed4 blur = (blurX+blurY+col)/5;
	
	fixed4 mask = ((col - blur)*_Amount);
	fixed4 mask2 = clamp(mask*_Amount2,-_Threshold,_Threshold);
	//mask = (mask + clamp((mask)*2,0,1));
	
	//col *= mask2;
	//col *= mask+mask2+1;
	mask += mask2;
	
	//mask.rgb = (col.r+col.b+col.g)*0.334;
	//col *= mask+1;

	col += mask;
	
	//fixed4 contrast = (col-0.5)*_Contrast;
	//col += contrast;

	return col;
}
ENDCG
	}
}

Fallback off

}

//Written 2012 by EyecyArt