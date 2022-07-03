Shader "Skybox/Gradient" {
Properties {
    [HDR]_SkyColor ("Sky Color", Color) = (.5, .5, .5, .5)
    [HDR]_EquatorColor ("Equator Color", Color) = (.5, .5, .5, .5)
    [HDR]_GroundColor ("Ground Color", Color) = (.5, .5, .5, .5)
    _Intensity ("Intensity", Float) = 1.0
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    Pass {

        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        #include "UnityCG.cginc"

        half4 _SkyColor, _EquatorColor ,_GroundColor;
        half  _Intensity;

        float3 TransformSkybox (float3 vertex)
        {
            float2x2 m = float2x2(1,0,0,1);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }
		
        struct appdata_t {
            float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert (appdata_t v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = TransformSkybox(v.vertex);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.vertex.xyz;
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            half3 c = _EquatorColor;
			half fade = asin(i.texcoord.y)*0.63661977236; //1/ Pi over 2
			half m1 = saturate(fade);
			half m2 = saturate(-fade);
			c= lerp(_EquatorColor,_SkyColor,m1);
			c=lerp(c,_GroundColor,m2);
            c *= _Intensity;
            return half4(c, 1);
        }
        ENDCG
    }
}


Fallback Off

}
