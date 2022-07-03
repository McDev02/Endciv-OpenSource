Shader "Endciv/Terrain/Terrain_Simple" {
	Properties{
		_LayerMask("Grid R, Layer A", 2D) = "black" {}
		_WetColor("WetColor", Color) = (0,0,0,0.5)
		_MaskVal("Mask", float) = 0.9
	}
		SubShader{
			Tags { "Queue" = "Geometry-100" "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf StandardSpecular fullforwardshadows vertex:vert

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0
			// needs up to 16 texcoords - gles is up to 8
			#pragma exclude_renderers gles

			//#pragma multi_compile __ BLEND_ON			What is it even?
			#pragma multi_compile QUALITY_HIGH QUALITY_MEDIUM QUALITY_LOW	
			#pragma multi_compile __ GRID_ON
			#pragma multi_compile __ OVERLAY_DEBUG OVERLAY_LAYER

			struct Input {
				float4 wUV;
				float2 layerUV;
			};


			uniform half _G_WorldMapFactor;

		  void vert(inout appdata_full v, out Input o) {
			  UNITY_INITIALIZE_OUTPUT(Input, o);
			  o.layerUV = v.texcoord.xy;

				half3 wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.wUV.xy = wPos.xz;
				o.wUV.zw = wPos.xz * _G_WorldMapFactor;
		  }

		  //Fragment Programm
		  uniform sampler2D _TerrainSurface0, _TerrainSurface1, _TerrainSurface2, _TerrainSurface3;
		  uniform sampler2D _TerrainNormal0, _TerrainNormal1, _TerrainNormal2, _TerrainNormal3;
		  uniform sampler2D _TerrainSnowTex, _TerrainSnowNormal;
		  uniform sampler2D _BlendTex, _BlendNormal;
		  uniform sampler2D _LayerMask;
		  uniform half _TerrainSnowTiling, _BlendTiling, _BlendEdge;
		  //half _TerrainEdge[4];	Is this required? Is currently hard coded
		  uniform half4 _TerrainTiling;
		  uniform half _GlobalGrassGrowth, _GlobalSnow;
		  uniform fixed4 _GlobalGrassColor;

		  uniform half _MaskVal;

		  //Splatmap calculation
		  void SplatmapMix(half2 uv, half2 uv2, inout half4 splat_control, out fixed4 mixedDiffuse, out fixed4 mixedNormal)
		  {
			  half4 nnh0 = tex2D(_TerrainNormal0, uv* _TerrainTiling.x);
			  half4 nnh1 = tex2D(_TerrainNormal1, uv* _TerrainTiling.y);
#if QUALITY_HIGH
				half4 nnh2 = tex2D(_TerrainNormal2, uv* _TerrainTiling.z);
#endif
				half4 nnh3 = tex2D(_TerrainNormal3, uv* _TerrainTiling.w);
#if BLEND_ON
				half4 nnhblend = tex2D(_BlendNormal, uv2* _BlendTiling);
#endif
				half4 nnhsnow = tex2D(_TerrainSnowNormal, uv * _TerrainSnowTiling);

				half mask;
				//Grass
				mask = saturate(splat_control.r + _GlobalGrassGrowth);
				splat_control.r = pow(saturate(mask + (nnh1.b - (1 - mask)) / (mask + 0.001)), 2);
#if QUALITY_HIGH
				mask = splat_control.g;
				splat_control.g = pow(saturate(mask + (nnh2.b - (1 - mask)) / (mask + 0.001)), 2);
#endif
				mask = splat_control.b;
				splat_control.b = pow(saturate(mask + (nnh3.b - (1 - mask)) / (mask + 0.001)), 3);

#if BLEND_ON
				mask = saturate(_MaskVal * ((1 - splat_control.a)*1.05 - 0.03));
				half blendSplat = pow(saturate(mask + (nnhblend.b - (1 - mask)) / (mask + 0.001)), _BlendEdge);
#endif

				//Soil and Grass blend
				half4 nnh = lerp(nnh0, nnh1, splat_control.r);
#if BLEND_ON
				//BlendLayer
				nnh = lerp(nnh, nnhblend, blendSplat);
#endif

#if QUALITY_HIGH
				//Dirt layer
				nnh = lerp(nnh, nnh2, splat_control.g);
#endif
				//Trash layer
				nnh = lerp(nnh, nnh3, splat_control.b);
				//Snow layer
#if QUALITY_LOW
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
#if BLEND_ON
				//BlendLayer
				mixedDiffuse = lerp(mixedDiffuse, tex2D(_BlendTex, uv2 * _BlendTiling), blendSplat);
#endif	

#if QUALITY_HIGH
				//Dirt layer
				mixedDiffuse = lerp(mixedDiffuse, tex2D(_TerrainSurface2, uv * _TerrainTiling.z), splat_control.g);
#endif
				//Trash layer
				mixedDiffuse = lerp(mixedDiffuse, tex2D(_TerrainSurface3, uv* _TerrainTiling.w), splat_control.b);
				//Snow layer
				mixedDiffuse = lerp(mixedDiffuse, tex2D(_TerrainSnowTex, uv* _TerrainSnowTiling), snowBlend);

				mixedNormal = nnh;
			}


		  // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		  // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		  // #pragma instancing_options assumeuniformscaling
		  UNITY_INSTANCING_BUFFER_START(Props)
			  // put more per-instance properties here
		  UNITY_INSTANCING_BUFFER_END(Props)


		  uniform sampler2D _GTEX_TerrainSplatMap;
		  uniform sampler2D _GTEX_TerrainDebugMap;
		  uniform half _GlobalWetness;
		  uniform fixed4 _WetColor;

		  void surf(Input i, inout SurfaceOutputStandardSpecular o) {

			  half4 splat_control = tex2D(_GTEX_TerrainSplatMap, i.wUV.zw);

			  half4 diffuse;
			  half3 specular; half smoothness;
			  fixed4 normHeight;
			  fixed3 normals = float3(0, 0, 1);
			  fixed3 emission = float3(0, 0, 0);

			  SplatmapMix(i.wUV.xy, i.layerUV, splat_control, diffuse, normHeight);


#if GRID_ON || OVERLAY_LAYER
			  fixed4 layerMask = tex2D(_LayerMask, i.wUV.xy - float2(0.25, 0.25));
#endif

			  //Grid
			  #if GRID_ON
			  diffuse.rgb = saturate(diffuse.rgb + layerMask.r * 0.2);
			  emission += layerMask.r * 0.02;
			  #endif

			  //Convert normal		
			  normals.rg = normHeight.rg;
			  half height = normHeight.b;
			  normals = normals * 2 - 1;	//_NormalPower
			  normals.b = sqrt(1 - normals.r*normals.r - normals.g * normals.g);
			  //World space: normals = mul(float3x3(float3(1, 0, 0), float3(0, 0, 1), float3(0, 1, 0)), normals);	//Workaround

			  fixed puddleMask = _GlobalWetness;
			  fixed puddleMask2 = _GlobalWetness;
			  height += normHeight.a - 0.5;
			  //Reduce specular
			  //diffuse.a*=0;
  #if QUALITY_LOW
  #else
				  puddleMask = saturate((_GlobalWetness - height) * 3);
				  normals = lerp(normals, float3(0, 0, 1), puddleMask);
				  puddleMask2 = pow(puddleMask,5);
  #endif
				  specular = lerp(diffuse.a, 0.1, puddleMask2);
				  smoothness = lerp(diffuse.a, 0.98, pow(puddleMask2,80));
				  smoothness = max(smoothness , lerp(diffuse.a, 0.36, _GlobalWetness*0.4 + puddleMask));
				  diffuse.rgb = lerp(diffuse.rgb, _WetColor.rgb, saturate(puddleMask + _GlobalWetness * 2)* _WetColor.a);

				  //DebugMap
	  #if OVERLAY_DEBUG
	  			fixed4 debugMap = tex2D(_GTEX_TerrainDebugMap, i.wUV.zw);
			  	diffuse.rgb = lerp(diffuse.rgb, debugMap.rgb, debugMap.a);
	  #elif OVERLAY_LAYER
				  fixed4 map = tex2D(_GTEX_TerrainDebugMap, i.wUV.zw);
				  half mask = saturate((map.a + layerMask.a - 1) * 10);
				  diffuse.rgb = lerp(diffuse.rgb, map.rgb, mask);
				// specular = lerp(specular, 0, mask);
				// smoothness = lerp(smoothness, 0, mask);
				 normals =  lerp(normals, float3(0, 0, 1), mask);
				  emission += map.rgb * mask * 0.05;
	  #endif

				  //Output
				  o.Albedo = diffuse.rgb;
				  o.Specular = specular;
				  o.Smoothness = smoothness;

				  o.Normal = normals;//UnpackNormal(tex2D(_TerrainNormal1, i.wUV.xy* _TerrainTiling.y));
				  o.Emission = emission;

				  o.Alpha = diffuse.a;
			  }
			  ENDCG
		}
			FallBack "Diffuse"
}
