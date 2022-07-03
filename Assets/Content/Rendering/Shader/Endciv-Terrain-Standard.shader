/*
Written by Kevin Scheitler - Crowbox 2016
ENDCIV Terrain shader
*/

Shader "ENDCIV/Terrain/Standard_Blend" {
	Properties{
		_GridTex("Grid", 2D) = "black" {}
		_LayerMask("Layer Mask", 2D) = "white" {}
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.0
		_Specular("Specular", Range(0.0, 1.0)) = 0.0
			
		//_NormalPower("Normal Power", Float) = 1.0
		_WetColor("WetColor", Color) = (0,0,0,0.5)

		_MaskVal("Mask", float) = 0.9
	}

		SubShader{
			Pass
		{
			Tags{
			"LightMode" = "Deferred"
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
			}

			Name "BaseTerrain"
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 3.0
			// needs up to 16 texcoords - gles is up to 8
			#pragma exclude_renderers gles

			#pragma multi_compile __ BLEND_ON
			#pragma multi_compile QUALITY_HIGH QUALITY_MEDIUM QUALITY_LOW

			#include "UnityCG.cginc"
			#include "UnityPBSLighting.cginc"
			//#include "UnityStandardBRDF.cginc"
			//#include "UnityStandardUtils.cginc"

			//Vertex Programm
			struct v2f {
				float4 pos          : POSITION;
				float4 wUV			: TEXCOORD0;	//World UV tiled (xy) and full (zw)
				float2 layerUV      : TEXCOORD1;
				float3 viewDir		: TEXCOORD2;
				half3 worldNormal	: TEXCOORD3;
				half3 sh			: TEXCOORD4;
				float3x3 tbn		: TEXCOORD5;
			};

			half _G_WorldMapFactor;

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.layerUV = v.texcoord.xy;

				half3 wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.wUV.xy = wPos.xz;
				o.wUV.zw = wPos.xz * _G_WorldMapFactor;

				TANGENT_SPACE_ROTATION;
				o.tbn = transpose(rotation);
				o.viewDir = _WorldSpaceCameraPos.xyz - wPos;

				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldNormal = worldNormal;
				o.sh = 0;
				o.sh = ShadeSHPerVertex(worldNormal, o.sh);

				return o;
			}


			//Fragment Programm
			sampler2D _TerrainSurface0, _TerrainSurface1, _TerrainSurface2, _TerrainSurface3;
			sampler2D _TerrainNormal0, _TerrainNormal1, _TerrainNormal2, _TerrainNormal3;
			sampler2D _TerrainSnowTex, _TerrainSnowNormal;
			sampler2D _BlendTex, _BlendNormal;
			sampler2D _GridTex;
			//half _NormalPower;
			half _TerrainSnowTiling, _BlendTiling, _BlendEdge;
			//half _TerrainEdge[4];	Is this required? Is currently hard coded
			half4 _TerrainTiling;
			half _GlobalGrassGrowth, _GlobalSnow;
			fixed4 _GlobalGrassColor;

			half _MaskVal;

			//Splatmap calculation
			void SplatmapMix(half2 uv, half2 uv2, inout half4 splat_control, out fixed4 mixedDiffuse, out fixed3 mixedNormal)
			{
				half3 nnh0 = tex2D(_TerrainNormal0, uv* _TerrainTiling.x).rgb;
				half3 nnh1 = tex2D(_TerrainNormal1, uv* _TerrainTiling.y).rgb;
#ifdef QUALITY_HIGH
				half3 nnh2 = tex2D(_TerrainNormal2, uv* _TerrainTiling.z).rgb;
#endif
				half3 nnh3 = tex2D(_TerrainNormal3, uv* _TerrainTiling.w).rgb;
#ifdef BLEND_ON
				half3 nnhblend = tex2D(_BlendNormal, uv2* _BlendTiling).rgb;
#endif
				half3 nnhsnow = tex2D(_TerrainSnowNormal, uv * _TerrainSnowTiling).rgb;

				half mask;
				//Grass
				mask = saturate(splat_control.r + _GlobalGrassGrowth);
				splat_control.r = pow(saturate(mask + (nnh1.b - (1 - mask)) / (mask + 0.001)), 2);
#ifdef QUALITY_HIGH
				mask = splat_control.g;
				splat_control.g = pow(saturate(mask + (nnh2.b - (1 - mask)) / (mask + 0.001)), 2);
#endif
				mask = splat_control.b;
				splat_control.b = pow(saturate(mask + (nnh3.b - (1 - mask)) / (mask + 0.001)), 3);

#ifdef BLEND_ON
				mask = saturate(_MaskVal * ((1 - splat_control.a)*1.05-0.03));
				half blendSplat = pow(saturate(mask + (nnhblend.b - (1 - mask)) / (mask + 0.001)), _BlendEdge);
#endif

				//Soil and Grass blend
				half3 nnh = lerp(nnh0, nnh1, splat_control.r);
#ifdef BLEND_ON
				//BlendLayer
				nnh = lerp(nnh, nnhblend, blendSplat);
#endif

#ifdef QUALITY_HIGH
				//Dirt layer
				nnh = lerp(nnh, nnh2, splat_control.g);
#endif
				//Trash layer
				nnh = lerp(nnh, nnh3, splat_control.b);
				//Snow layer
#ifdef QUALITY_LOW
				mask = _GlobalSnow;
#else
				//Use Grass mask to modify snow a bit and enhance at that area
				mask = saturate(_GlobalSnow *  (splat_control.r *0.2 + 1));
#endif
				half snowBlend = pow(saturate(mask + (nnhsnow.b - (1 - mask)) / (mask + 0.001)), 2);
				nnh = lerp(nnh, nnhsnow, snowBlend);

				//Soil base
				mixedDiffuse = tex2D(_TerrainSurface0, uv* _TerrainTiling.x);
				//Grass layer
				mixedDiffuse = lerp(mixedDiffuse, _GlobalGrassColor * tex2D(_TerrainSurface1, uv* _TerrainTiling.y), splat_control.r);
#ifdef BLEND_ON
				//BlendLayer
				mixedDiffuse = lerp(mixedDiffuse, tex2D(_BlendTex, uv2 * _BlendTiling), blendSplat);
#endif	

#ifdef QUALITY_HIGH
				//Dirt layer
				mixedDiffuse = lerp(mixedDiffuse, tex2D(_TerrainSurface2, uv * _TerrainTiling.z), splat_control.g);
#endif
				//Trash layer
				mixedDiffuse = lerp(mixedDiffuse, tex2D(_TerrainSurface3, uv* _TerrainTiling.w), splat_control.b);
				//Snow layer
				mixedDiffuse = lerp(mixedDiffuse, tex2D(_TerrainSnowTex, uv* _TerrainSnowTiling), snowBlend);

				mixedNormal = nnh;
			}

			half _Smoothness, _Specular;

			uniform sampler2D _GTEX_TerrainSplatMap;
			uniform half _GlobalWetness;
			fixed4 _WetColor;
						
			void frag(
				v2f i,
				out half4 outDiffuse : SV_Target0,			// RT0: diffuse color (rgb), occlusion (a)
				out half4 outSpecSmoothness : SV_Target1,	// RT1: spec color (rgb), smoothness (a)
				out half4 outNormal : SV_Target2,			// RT2: normal (rgb), --unused, very low precision-- (a)
				out half4 outEmission : SV_Target3			// RT3: emission (rgb), --unused-- (a)
				)
			{
				half4 splat_control = tex2D(_GTEX_TerrainSplatMap, i.wUV.zw);

				half4 diffuse;
				half3 specular; half smoothness;
				fixed3 normals = float3(0, 0, 1);

				SplatmapMix(i.wUV.xy, i.layerUV, splat_control, diffuse, normals);

				//Convert data
				fixed height = normals.b;
				//UnpackNormals	
				//normals.r = 0;// sin(i.wUV.x);
				//normals.g =  sin(i.wUV.y);
				normals = normals * 2 - 1;	//_NormalPower
				//normals *= 10;
				normals.b = sqrt(1 - normals.r*normals.r - normals.g * normals.g);
				
				//normals = float3(0, 0, 1);
				//normals = mul(i.tbn, normals);
				normals = mul(float3x3(float3(1, 0, 0), float3(0, 0, 1), float3(0, 1, 0)), normals);	//Workaround
				//Output
#ifdef QUALITY_LOW
				smoothness = lerp(diffuse.a * _Smoothness, 0.19, _GlobalWetness);
				specular = lerp(diffuse.a * _Specular, 0.38, _GlobalWetness);
#else
				fixed puddleMask = max(0.4, 1 - height*height)* _GlobalWetness;
				//smoothness = saturate(max(diffuse.a * _Smoothness, max(0.2, 1 - height*height)*0.7* _GlobalWetness));
				smoothness = saturate(max(diffuse.a * _Smoothness, puddleMask*puddleMask*0.8* _GlobalWetness));
				specular =  saturate(max(diffuse.a * _Specular, puddleMask*puddleMask*0.03));
				diffuse.rgb = lerp (diffuse.rgb, _WetColor.rgb, puddleMask * _WetColor.a);
				normals = lerp(normals, float3(0, 1, 0), puddleMask*0.9);
#endif
				outSpecSmoothness = half4(specular, smoothness);
				

				// energy conservation
				half oneMinusReflectivity;
				diffuse.rgb = EnergyConservationBetweenDiffuseAndSpecular(diffuse.rgb, specular, /*out*/ oneMinusReflectivity);

				half3 c = diffuse.rgb * ShadeSHPerPixel(i.worldNormal, i.sh, i.pos);
				outDiffuse = half4(diffuse.rgb, 0);	//Alpha = Occlusion
				outSpecSmoothness = half4(specular, smoothness);
				//normals = float3(0, 0, 1) ;
				outNormal = half4(normals * 0.5 + 0.5, 1);
				outEmission = half4(c, 1);	//Indirect lighting only

//#ifndef UNITY_HDR_ON	Not working
//				outEmission.rgb = exp2(-outEmission.rgb);
//#endif
			}

			//End Pass 1: BaseTerrain
			ENDCG
			}

			Pass
			{
				Tags{
				"LightMode" = "Deferred"
				"Queue" = "Geometry-100"
				"RenderType" = "Opaque"
			}

				Name "OverlayData"
				Blend One OneMinusSrcAlpha
				CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest

#pragma multi_compile __ LAYER_ON
#pragma multi_compile __ GRID_ON
#include "UnityCG.cginc"


			struct v2f {
				float4 pos          : POSITION;
				float4 wUV			: TEXCOORD0;	//World UV tiled (xy) and full (zw)
			};

			half _G_WorldMapFactor;

			v2f vert(appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				half3 wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.wUV.xy = wPos.xz;
				o.wUV.zw = wPos.xz * _G_WorldMapFactor;			

				return o;
			}

			sampler2D _GridTex;
			sampler2D _LayerMask;
			sampler2D _GTEX_GridLayerMap;
			sampler2D _GTEX_GridHighlightMap;

			void frag(
				v2f i,
				out half4 outDiffuse : SV_Target0,            // RT0: diffuse color (rgb), occlusion (a)
				out half4 outSpecSmoothness : SV_Target1,    // RT1: spec color (rgb), smoothness (a)
				out half4 outNormal : SV_Target2,            // RT2: normal (rgb), --unused, very low precision-- (a)
				out half4 outEmission : SV_Target3            // RT3: emission (rgb), --unused-- (a)
				)
			{
				half3 color = half3(0, 0, 0);
				half3 emission = half3(0, 0, 0);
				fixed4 colorData;

				//Layers
				#ifdef LAYER_ON
				colorData = tex2D(_GTEX_GridLayerMap, i.wUV.zw);
				colorData.a *= saturate((colorData.a + tex2D(_LayerMask, i.wUV.xy * 2).r - 1) * 5);
				colorData.a = 0.5 * sqrt(colorData.a);
				color.rgb = lerp(color.rgb, colorData.rgb, colorData.a);
				emission.rgb += colorData.rgb * colorData.a * 0.25;
				#endif

				//Highlight
				colorData = 0.5 * tex2D(_GTEX_GridHighlightMap, i.wUV.zw);
				color.rgb += colorData.rgb;
				emission.rgb += colorData.rgb * 0.25;

				//Grid
				#ifdef GRID_ON
				fixed grid = tex2D(_GridTex, i.wUV.xy * 2).a;
				color += grid * 0.3;
				emission += grid * 0.04;
				#endif

				outDiffuse = float4(color, 0.5);
				outEmission = float4(emission, 0.5);

			}

			ENDCG
				//END OverlayShader
			}

			//END SUBSHADER
		}
			//Fallback - Very low quality shader
				Fallback "Legacy Shaders/VertexLit"
}
/*
Written by Kevin Scheitler - Crowbox 2016
*/