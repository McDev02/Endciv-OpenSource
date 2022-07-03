Shader "Endciv/Standard"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _SurfaceMap ("Surface M(R) AO(G) H(B) S(A)", 2D) = "grey" {}
        _BumpMap("Normal Map", 2D) = "bump" {}       
        _EmissionMap ("Glow (RGB) Power(A)", 2D) = "white" {}
		_GlowPower("Glow Power", Range(0, 1)) = 0.0

		//Debug
		//_Blend ("Blend Power", Range(0,1)) = 0

    }
    SubShader
    {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:vert alphatest:_Cutoff addshadow fullforwardshadows nometa nolightmap nolppv

		#pragma multi_compile __ CONSTRUCTION
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _SurfaceMap;
        sampler2D _BumpMap;
        sampler2D  _EmissionMap;
        half _GlowPower;
        //fixed _Cutoff;

		//Terrain
		uniform half _G_WorldMapFactor, _GlobalSnow,_TerrainSnowTiling;
		uniform sampler2D _TerrainSnowTex, _TerrainSnowNormal;

#ifdef CONSTRUCTION
		uniform half _ConstructionProgress;
#endif
		//Debug
		//half _Blend;

        struct Input
        {
            float2 uv_MainTex;
			float4 wPosUp;
            //float4 vCol;
		};

	void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			float4 w;
            w = mul(unity_ObjectToWorld, v.vertex);	   

#ifdef CONSTRUCTION
            //half dissolve = (1-v.vertex.y/v.color.g) - saturate(( _ConstructionProgress - v.color.r ) * v.color.g) + 0.5 ;            
            //o.vCol.a = dissolve;
            w.y -= saturate((_ConstructionProgress - v.color.r ) * v.color.g) * v.color.b;
            v.vertex = mul(unity_WorldToObject, w);	
#endif
	        w.w = normalize( mul(unity_ObjectToWorld, float4( v.normal, 0.0 ))).y;
			o.wPosUp = w;  
    }

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) ;
            fixed4 surf = tex2D (_SurfaceMap, IN.uv_MainTex) ;
			fixed3 n = UnpackNormal(tex2D (_BumpMap, IN.uv_MainTex));
			
			half2 wUV = IN.wPosUp.xz * _TerrainSnowTiling;
			//Snow
			fixed4 snowD =  tex2D(_TerrainSnowTex, wUV);
			//half4 nnhsnow = tex2D(_TerrainSnowNormal, wUV );
			fixed4 emission = tex2D (_EmissionMap, IN.uv_MainTex);
            emission.rgb *= emission.a* _GlowPower;

			half m = saturate( (_GlobalSnow * IN.wPosUp.w - surf.b) * 1.2);

			//Blend Surface
			c.rgb = lerp(c, snowD, m).rgb;
			n = lerp(n, float3(0,0,1), m);
			surf.ra = lerp(surf.ra, float2(snowD.a,snowD.a), m);
			emission.rgb *= 1-m;

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = surf.r;
            o.Smoothness = surf.a;
			o.Normal = n;
			o.Occlusion = surf.g;//pow(surf.g, _Occlusion);
            o.Emission = emission.rgb ;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Legacy Shaders/Transparent/Cutout/VertexLit"
}
