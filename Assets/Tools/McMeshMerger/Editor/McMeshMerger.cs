using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using Endciv;

namespace McMeshMerger
{
	public class McMeshMergerEditor : EditorWindow
	{
		GameObject[] currentSelection;
		MeshMergeGroupRoot groupRoot;
		List<MeshMergeRoot> rootObjects = new List<MeshMergeRoot>();

		const string mergedAssetsPath = "Assets/Content/Models/Structures/MergedModels/";

		int totalModels;
		string[] tabNames = new string[] { "Materials", "Objects" };
		int selectedTab;
		float objectsConstructionProgress;

		bool foldoutMaterials = true;
		int foldoutObject = 0;
		Vector2 scrollposMaterials;
		Vector2 scrollposObjects;
		float globalTextureScale = 1;

		public enum ETextureType { Diffuse, Surface, Normal, Glow }
		public enum ETextureSuffix { diff, surf, norm, glow }

		// Add menu named "My Window" to the Window menu
		[MenuItem("Endciv/Tools/Mesh Merger")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
			McMeshMergerEditor window = (McMeshMergerEditor)EditorWindow.GetWindow(typeof(McMeshMergerEditor));
			window.Show();
		}

		void OnGUI()
		{
			GUILayout.Label("Material Mesh Merger", EditorStyles.boldLabel);

			GUILayout.Space(5);
			GuiHead();
			GUILayout.Space(5);

			GuiMeshMaterialMerge();
		}

		void GuiHead()
		{
		}

		void GuiMeshMaterialMerge()
		{
			bool valid = true;

			if (currentSelection == null || currentSelection.Length <= 0)
				GUILayout.Label("Select at least one object");

			else
			{
				if (GUILayout.Button("Update Selection"))
					ApplySelectionRoots();
			}
			if (!valid)
			{
				return;
			}

			else
			{
				int amount = 0;
				GUILayout.Label((rootObjects == null ? "No" : rootObjects.Count.ToString()) + " Objects selected");
				GUILayout.Label(totalModels + " Models selected");
				if (groupRoot != null && groupRoot.uniqueMaterials != null)
					amount = groupRoot.uniqueMaterials.Count;
				GUILayout.Label($"{amount} unique {(amount == 1 ? " Materials" : " Material")}");
			}

			if (groupRoot != null)
			{
				GUILayout.Space(12);

				GUILayout.Label("Texture Settings");
				groupRoot.mergerToolSettings.minTexturCellSize = EditorGUILayout.IntField("Min Cell Size", groupRoot.mergerToolSettings.minTexturCellSize);

				groupRoot.mergerToolSettings.targetWidth = Mathf.Clamp(EditorGUILayout.IntField("Target Width", groupRoot.mergerToolSettings.targetWidth), 1, 8192);
				groupRoot.mergerToolSettings.targetHeight = Mathf.Clamp(EditorGUILayout.IntField("Target Height", groupRoot.mergerToolSettings.targetHeight), 1, 8192);
				string areaError;

				bool texturesReadable = true;// AreTexturesReadable(out areaError);
											 //if (!texturesReadable) EditorGUILayout.HelpBox(areaError, MessageType.Error);

				bool textureFit = DoTexturesFit(out areaError);
				EditorGUILayout.HelpBox(areaError, textureFit ? MessageType.Info : MessageType.Error);

				GUILayout.Space(6);

				GUILayout.BeginHorizontal();
				groupRoot.mergerToolSettings.createDiffuseTexture = GUILayout.Toggle(groupRoot.mergerToolSettings.createDiffuseTexture, "Diffuse Texture", GUILayout.Width(120));
				groupRoot.mergerToolSettings.includeAlphaChannel = GUILayout.Toggle(groupRoot.mergerToolSettings.includeAlphaChannel, "Include Alpha Channel", GUILayout.Width(120));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				groupRoot.mergerToolSettings.createSpecularTexture = GUILayout.Toggle(groupRoot.mergerToolSettings.createSpecularTexture, "Specular Texture", GUILayout.Width(120));
				groupRoot.mergerToolSettings.createNormalTexture = GUILayout.Toggle(groupRoot.mergerToolSettings.createNormalTexture, "Normal Texture", GUILayout.Width(120));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				groupRoot.mergerToolSettings.createGlowTexture = GUILayout.Toggle(groupRoot.mergerToolSettings.createGlowTexture, "Glow Texture", GUILayout.Width(120));
				GUILayout.EndHorizontal();

				GUILayout.Space(12);

				using (new EditorGUI.DisabledGroupScope(!textureFit || !texturesReadable))
				{
					var textureName = groupRoot.groupName;
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Preview Textures"))
						groupRoot.mergedDiffuse = CombineTextures(groupRoot.uniqueMaterials, groupRoot.mergedDiffuse, ETextureType.Diffuse, textureName);
					if (GUILayout.Button("Merge Models"))
						MergeModels(textureName);

					GUILayout.EndHorizontal();
					if (groupRoot.mergedDiffuse != null)
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label("Models will be saved at:\n" + mergedAssetsPath + groupRoot.GetPath());
						if (GUILayout.Button("Save Data"))
							SaveCurrentModelData();
						GUILayout.EndHorizontal();
					}
				}

				GUILayout.Space(12);

				selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
				switch (selectedTab)
				{
					case 0: //Materials
						DrawMaterialsTab();
						break;
					case 1: //Objects
						DrawObjectsTab();
						break;
					default:
						break;
				}
			}
		}

		void MergeModels(string textureName)
		{
			if (groupRoot.mergerToolSettings.createDiffuseTexture) groupRoot.mergedDiffuse = CombineTextures(groupRoot.uniqueMaterials, groupRoot.mergedDiffuse, ETextureType.Diffuse, textureName);
			else groupRoot.mergedDiffuse = null;
			if (groupRoot.mergerToolSettings.createSpecularTexture) groupRoot.mergedSurface = CombineTextures(groupRoot.uniqueMaterials, groupRoot.mergedSurface, ETextureType.Surface, textureName);
			else groupRoot.mergedSurface = null;
			if (groupRoot.mergerToolSettings.createNormalTexture) groupRoot.mergedNormal = CombineTextures(groupRoot.uniqueMaterials, groupRoot.mergedNormal, ETextureType.Normal, textureName);
			else groupRoot.mergedNormal = null;
			if (groupRoot.mergerToolSettings.createGlowTexture) groupRoot.mergedGlow = CombineTextures(groupRoot.uniqueMaterials, groupRoot.mergedGlow, ETextureType.Glow, textureName);
			else groupRoot.mergedGlow = null;

			CheckAndCreateMaterial(groupRoot);
			for (int i = 0; i < rootObjects.Count; i++)
			{
				if (rootObjects[i] == null) continue;
				CheckAndCreateNewMesh(rootObjects[i]);
				MeshMerger.MergeMeshes(rootObjects[i], groupRoot.uniqueMaterials);

				CreateNewMeshObject(rootObjects[i], groupRoot);   //appliedSelectionRoots[i].transform
				rootObjects[i].gameObject.SetActive(false);
			}
		}

		private void CheckAndCreateNewTexture()
		{

		}

		private void CheckAndCreateNewMesh(MeshMergeRoot meshMergeRoot)
		{
			if (meshMergeRoot.mergedMesh != null) return;

			string path = mergedAssetsPath + groupRoot.GetPath();
			string filename = meshMergeRoot.meshName + ".asset";

			meshMergeRoot.mergedMesh = new Mesh();
			meshMergeRoot.mergedMesh.name = meshMergeRoot.meshName;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			try
			{
				AssetDatabase.CreateAsset(meshMergeRoot.mergedMesh, path + filename);
				AssetDatabase.SaveAssets();
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}
		private void CheckAndCreateMaterial(MeshMergeGroupRoot root)
		{
			if (root.mergedMatetrial != null) return;

			string path = mergedAssetsPath + groupRoot.GetPath();
			string filename = root.groupName + "_material.asset";

			var s = Shader.Find("Endciv/Standard");
			root.mergedMatetrial = new Material(s);
			root.mergedMatetrial.name = root.groupName;

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);
			try
			{
				AssetDatabase.CreateAsset(root.mergedMatetrial, path + filename);
				AssetDatabase.SaveAssets();
			}
			catch (Exception e)
			{
				Debug.LogError(e.ToString());
			}
		}

		void SaveCurrentModelData()
		{
			AssetDatabase.SaveAssets();
		}

		string GetTextureName(string name, ETextureType type)
		{
			string suffix = "";
			switch (type)
			{
				case ETextureType.Diffuse:
					suffix = "diff";
					break;
				case ETextureType.Surface:
					suffix = "surf";
					break;
				case ETextureType.Normal:
					suffix = "norm";
					break;
			}
			return $"{name}_{suffix}";
		}

		Texture2D CombineTextures(List<MaterialData> materials, Texture2D tex, ETextureType type, string texName)
		{
			bool linear = true;
			TextureFormat format = TextureFormat.RGBA32;
			bool compressHQ = false;
			switch (type)
			{
				case ETextureType.Diffuse:
					compressHQ = false;
					linear = false;
					break;
				case ETextureType.Surface:
					compressHQ = false;
					linear = false;
					break;
				case ETextureType.Normal:
					compressHQ = true;
					linear = false;
					break;
				case ETextureType.Glow:
					compressHQ = false;
					linear = false;
					break;
			}

			texName = GetTextureName(texName, type);
			string path = mergedAssetsPath + groupRoot.GetPath() + "Textures/";
			string filename = texName + ".asset";

			if (tex == null || tex.width != groupRoot.mergerToolSettings.targetWidth || tex.height != groupRoot.mergerToolSettings.targetHeight)
			{
				tex = new Texture2D(groupRoot.mergerToolSettings.targetWidth, groupRoot.mergerToolSettings.targetHeight, format, true, linear);

				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				try
				{
					AssetDatabase.CreateAsset(tex, path + filename);
					AssetDatabase.SaveAssets();
				}
				catch (Exception e)
				{
					Debug.LogError(e.ToString());
				}
			}

			PackMaterialsAndPreview();
			int id = 0;
			Color32[] cols = new Color32[tex.width * tex.height];

			Debug.Log("Start Texture generation");
			for (int i = 0; i < materials.Count; i++)
			{
				var mat = materials[i];
				var colorMultiplicator = Color.white;
				Texture2D matTex = null;
				switch (type)
				{
					case ETextureType.Diffuse:
						//colorMultiplicator = mat.material.GetColor("_Color");
						matTex = materials[i].diffuseTex;
						break;
					case ETextureType.Surface:
						matTex = materials[i].surfaceTex;
						if (matTex == null)
						{
							colorMultiplicator.r = 0;       //Metallic
							colorMultiplicator.g = 1;       //AO
							colorMultiplicator.b = 0.5f;    //Height
							colorMultiplicator.a = 0.2f;    //Smoothness
						}
						break;
					case ETextureType.Normal:
						matTex = materials[i].normalTex;
						if (matTex == null)
						{
							colorMultiplicator.r = colorMultiplicator.g = 0.5f;
							colorMultiplicator.b = 0f;
						}
						break;
					case ETextureType.Glow:
						matTex = materials[i].glowTex;
						if (matTex == null)
						{
							colorMultiplicator = Color.black;
							colorMultiplicator.a = 0;
						}
						break;
				}
				var rect = materials[i].rect;
				var min = rect.min;
				//var relativeRect = materials[i].rectRelative;
				float xFactor = 1f / materials[i].bakeWidth;
				float yFactor = 1f / materials[i].bakeHeight;
				Color col;
				for (int y = (int)rect.min.y; y < rect.max.y; y++)
				{
					for (int x = (int)rect.min.x; x < rect.max.x; x++)
					{
						if (matTex != null) col = matTex.GetPixelBilinear((x - min.x) * xFactor, (y - min.y) * yFactor) * colorMultiplicator;
						else col = colorMultiplicator;
						if (type == ETextureType.Diffuse && !groupRoot.mergerToolSettings.includeAlphaChannel)
							col.a = 1;
						if (type == ETextureType.Normal)
							col.g = Mathf.Pow(col.g, 1f / 2.2f);
						cols[y * tex.width + x] = col;
					}
				}
			}
			tex.name = texName + "_" + ((ETextureSuffix)type).ToString();

			tex.SetPixels32(cols);
			tex.Apply(true);
			Debug.Log("Finished Texture generation");

			//	tex.Compress(compressHQ);
			return tex;
		}

		private bool AreTexturesReadable(out string areaError)
		{
			bool success = true;
			int nonReadableCount = 0;

			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < groupRoot.uniqueMaterials.Count; i++)
			{
				var mat = groupRoot.uniqueMaterials[i];
				if (!mat.hasTexture) continue;
				if (mat.diffuseTex != null)
				{
					try
					{
						mat.diffuseTex.SetPixel(0, 0, mat.diffuseTex.GetPixel(0, 0));
					}
					catch (Exception)
					{
						builder.AppendLine($"Diffuse texture is not readable: {mat.diffuseTex.name}");
						success = false;
						nonReadableCount++;
					}
				}
				if (mat.surfaceTex != null)
				{
					try
					{
						mat.surfaceTex.SetPixel(0, 0, mat.surfaceTex.GetPixel(0, 0));
					}
					catch (Exception)
					{
						builder.AppendLine($"Specular texture is not readable: {mat.surfaceTex.name}");
						success = false;
						nonReadableCount++;
					}
				}
				if (mat.normalTex != null)
				{
					try
					{
						mat.normalTex.SetPixel(0, 0, mat.normalTex.GetPixel(0, 0));
					}
					catch (Exception)
					{
						builder.AppendLine($"Normal texture is not readable: {mat.normalTex.name}");
						success = false;
						nonReadableCount++;
					}
				}
				if (mat.glowTex != null)
				{
					try
					{
						mat.glowTex.SetPixel(0, 0, mat.glowTex.GetPixel(0, 0));
					}
					catch (Exception)
					{
						builder.AppendLine($"Glow texture is not readable: {mat.glowTex.name}");
						success = false;
						nonReadableCount++;
					}
				}
			}

			if (nonReadableCount == 1)
				areaError = $"One Texture is not readable\n{builder.ToString()}";
			else if (nonReadableCount > 1)
				areaError = $"{nonReadableCount} Textures are not readable\n{builder.ToString()}";
			else
				areaError = "";
			return success;
		}

		bool DoTexturesFit(out string areaError)
		{
			int area = 0;
			int maxWidth = 0;
			int maxHeight = 0;
			for (int i = 0; i < groupRoot.uniqueMaterials.Count; i++)
			{
				area += groupRoot.uniqueMaterials[i].bakeWidth * groupRoot.uniqueMaterials[i].bakeHeight;
				if (maxWidth < groupRoot.uniqueMaterials[i].bakeWidth) maxWidth = groupRoot.uniqueMaterials[i].bakeWidth;
				if (maxHeight < groupRoot.uniqueMaterials[i].bakeHeight) maxHeight = groupRoot.uniqueMaterials[i].bakeHeight;
			}

			var dimArea = Mathf.CeilToInt(Mathf.Sqrt(area));
			areaError = $"Texture area sum: {dimArea}x{dimArea} - Minsize: {maxWidth}x{maxHeight}";
			if (maxWidth > groupRoot.mergerToolSettings.targetWidth || maxHeight > groupRoot.mergerToolSettings.targetHeight)
				return false;
			return area <= groupRoot.mergerToolSettings.targetWidth * groupRoot.mergerToolSettings.targetHeight;
		}

		void DrawObjectsTab()
		{
			bool updateMesh = false;

			GUILayout.BeginHorizontal();
			var tmp = objectsConstructionProgress;
			GUILayout.Label($"Construction {tmp.ToString("0.00")}", GUILayout.Width(120));
			objectsConstructionProgress = GUILayout.HorizontalSlider(objectsConstructionProgress, 0, 1);
			if (!Mathf.Approximately(tmp, objectsConstructionProgress))
				updateMesh = true;
			GUILayout.EndHorizontal();

			bool cancelFoldout = false;
			if (rootObjects == null) return;
			scrollposObjects = EditorGUILayout.BeginScrollView(scrollposObjects);
			for (int i = 0; i < rootObjects.Count; i++)
			{
				var root = rootObjects[i];
				var models = rootObjects[i].models;
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Name: {root.name}", EditorStyles.boldLabel);
				GUILayout.Label($"Meshes: {models.Count}");
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				tmp = root.constructionFade;
				GUILayout.Label($"Fade {tmp.ToString("0.00")}", GUILayout.Width(80));
				root.constructionFade = GUILayout.HorizontalSlider(root.constructionFade, 0.01f, 2);
				if (!Mathf.Approximately(tmp, root.constructionFade))
					updateMesh = true;

				tmp = root.constructionHeight;
				GUILayout.Label($"Height {tmp.ToString("0.00")}", GUILayout.Width(80));
				root.constructionHeight = GUILayout.HorizontalSlider(root.constructionHeight, 0.0f, 4);
				if (!Mathf.Approximately(tmp, root.constructionHeight))
					updateMesh = true;
				GUILayout.EndHorizontal();

				if (GUILayout.Button("Randomize"))
				{
					RandomizeModels(root.models);
					updateMesh = true;
				}

				var foldout = EditorGUILayout.Foldout(foldoutObject == i, "Objects");

				if (!foldout && foldoutObject == i)
					cancelFoldout = true;
				if (foldout)
				{
					if (foldoutObject != i)
						foldoutObject = i;

					for (int j = 0; j < models.Count; j++)
					{
						var model = models[j];
						GUILayout.Label($"Name: {model.transform.name}");
						GUILayout.BeginHorizontal();
						tmp = model.constructionBegin;
						model.constructionBegin = GUILayout.HorizontalSlider(model.constructionBegin, 0, 1);
						if (!Mathf.Approximately(tmp, model.constructionBegin))
							updateMesh = true;

						model.constructionEnd = Mathf.Clamp01(model.constructionBegin + root.constructionFade);
						model.constructionHeight = root.constructionHeight;
						//tmp = model.constructionEnd;
						//model.constructionEnd = GUILayout.HorizontalSlider(model.constructionEnd,0, 1);
						//if (!Mathf.Approximately(tmp, model.constructionEnd))
						//	updateMesh = true;
						GUILayout.EndHorizontal();
					}
				}
				if (i < rootObjects.Count - 1)
					GUILayout.Space(10);
			}
			EditorGUILayout.EndScrollView();
			if (cancelFoldout)
				foldoutObject = -1;

			if (updateMesh)
				UpdateConstructionPreview();
		}

		void RandomizeModels(List<ModelData> models)
		{
			for (int i = 0; i < models.Count; i++)
			{
				var model = models[i];
				model.constructionBegin = CivRandom.Range(model.randomMin, model.randomMax);
			}
		}

		void DrawMaterialsTab()
		{
			GUILayout.BeginHorizontal();
			globalTextureScale = Mathf.Clamp(EditorGUILayout.FloatField("Global Scale", globalTextureScale), 0.01f, 4);
			int tmpScaleMode = 0;
			if (GUILayout.Button("Set all to global scale"))
				tmpScaleMode = 1;
			if (GUILayout.Button("Scale all relatively"))
				tmpScaleMode = 2;

			GUILayout.EndHorizontal();

			foldoutMaterials = EditorGUILayout.Foldout(foldoutMaterials, "Materials");
			if (foldoutMaterials)
			{
				scrollposMaterials = EditorGUILayout.BeginScrollView(scrollposMaterials);
				for (int i = 0; i < groupRoot.uniqueMaterials.Count; i++)
				{
					var mat = groupRoot.uniqueMaterials[i];
					GUILayout.BeginHorizontal();
					GUILayout.BeginVertical();
					GUILayout.Label($"Name: {mat.material.name}");
					GUILayout.Label($"Diffuse: {(mat.diffuseTex == null ? "No" : "Yes")}");
					GUILayout.Label($"Specular: {(mat.surfaceTex == null ? "No" : "Yes")}");
					GUILayout.Label($"Normal: {(mat.normalTex == null ? "No" : "Yes")}");
					GUILayout.Label($"Glow: {(mat.glowTex == null ? "No" : "Yes")}");

					GUILayout.EndVertical();

					GUILayout.BeginVertical();
					if (tmpScaleMode == 1) mat.textureScale = globalTextureScale;
					else if (tmpScaleMode == 2) mat.textureScale *= globalTextureScale;
					mat.textureScale = EditorGUILayout.FloatField("Texture Scale", mat.textureScale);
					GUILayout.Label($"Resolution: {mat.bakeWidth}x{mat.bakeHeight}");
					GUILayout.EndVertical();
					GUILayout.EndHorizontal();
					GUILayout.Space(4);
				}
				EditorGUILayout.EndScrollView();
			}
		}

		void CreateNewMeshObject(MeshMergeRoot root, MeshMergeGroupRoot groupRoot)
		{
			var obj = new GameObject(root.meshName);
			if (root.transform != null)
			{
				obj.transform.position = root.transform.position;
				obj.transform.rotation = root.transform.rotation;
			}
			obj.transform.localScale = Vector3.one;
			var filter = obj.AddComponent<MeshFilter>();
			var render = obj.AddComponent<MeshRenderer>();

			string path = mergedAssetsPath + groupRoot.GetPath();
			string filename = root.meshName + ".asset";

			filter.sharedMesh = root.mergedMesh;
			var mat = groupRoot.mergedMatetrial;
			if (groupRoot.mergedDiffuse != null) mat.SetTexture(Utilities.MAP_DIFF, groupRoot.mergedDiffuse);
			if (groupRoot.mergedSurface != null) mat.SetTexture(Utilities.MAP_SURF, groupRoot.mergedSurface);
			if (groupRoot.mergedNormal != null) mat.SetTexture(Utilities.MAP_NORM, groupRoot.mergedNormal);
			if (groupRoot.mergedGlow != null) mat.SetTexture(Utilities.MAP_GLOW, groupRoot.mergedGlow);
			render.sharedMaterial = mat;
		}

		void PackMaterialsAndPreview()
		{
			var packerTexturePreview = FindObjectOfType<RectPackerTest>();
			if (packerTexturePreview == null) return;

			packerTexturePreview.Recalculate(groupRoot.uniqueMaterials, groupRoot.mergerToolSettings.targetWidth, groupRoot.mergerToolSettings.targetHeight);
		}

		void GenerateGroupRoot()
		{

		}

		void ApplySelectionRoots()
		{
			rootObjects.Clear();

			var selectedRoots = Utilities.SelectRootObjects(currentSelection);
			if (selectedRoots == null || selectedRoots.Count <= 0)
				return;

			groupRoot = selectedRoots[0].GetComponent<MeshMergeGroupRoot>();
			if (groupRoot == null)
				return;

			var savedUniqueMaterials = groupRoot.uniqueMaterials;
			groupRoot.uniqueMaterials = new List<MaterialData>();
			if (savedUniqueMaterials == null) savedUniqueMaterials = new List<MaterialData>();


			var modelRoots = Utilities.SelectChildren(groupRoot.transform);

			totalModels = 0;
			for (int r = 0; r < modelRoots.Count; r++)
			{
				var selected = modelRoots[r].gameObject;
				var meshMerger = selected.GetComponent<MeshMergeRoot>();
				if (meshMerger == null)
					meshMerger = selected.AddComponent<MeshMergeRoot>();

				if (string.IsNullOrEmpty(meshMerger.meshName))
					meshMerger.meshName = selected.name;

				rootObjects.Add(meshMerger);

				var renderers = selected.GetComponentsInChildren<MeshRenderer>();
				if (renderers != null && renderers.Length > 0)
				{
					MeshMergeRoot root = selected.GetComponent<MeshMergeRoot>();
					if (root == null)
					{
						root = selected.AddComponent<MeshMergeRoot>();
						root.meshName = root.name;
					}

					var list = rootObjects[r].models;
					if (list == null)
						list = new List<ModelData>(renderers.Length);
					else list.Clear();

					for (int i = 0; i < renderers.Length; i++)
					{
						var rend = renderers[i];
						ModelData selectionData = rend.gameObject.GetComponent<ModelData>();
						if (selectionData == null)
							selectionData = rend.gameObject.AddComponent<ModelData>();

						selectionData.renderer = rend;
						selectionData.filter = rend.GetComponent<MeshFilter>();

						if (selectionData.renderer == null || selectionData.filter == null) continue;
						selectionData.mesh = selectionData.filter.sharedMesh;

						if (selectionData.mesh == null) continue;
						if (selectionData.renderer.sharedMaterials.Length <= 0) continue;
						var mat = rend.sharedMaterial;
						if (mat == null) continue;

						//Find material
						selectionData.materialID = -1;
						for (int s = 0; s < savedUniqueMaterials.Count; s++)
						{
							var matData = savedUniqueMaterials[s];
							if (mat == matData.material)
							{
								matData.id = groupRoot.uniqueMaterials.Count;
								groupRoot.uniqueMaterials.Add(matData);
								savedUniqueMaterials.RemoveAt(s);
								break;
							}
						}
						for (int m = 0; m < groupRoot.uniqueMaterials.Count; m++)
						{
							if (mat == groupRoot.uniqueMaterials[m].material)
							{
								selectionData.materialID = m;
								break;
							}
						}
						//Add material to list
						if (selectionData.materialID < 0)
						{
							selectionData.materialID = groupRoot.uniqueMaterials.Count;
							var matData = new MaterialData(selectionData.materialID, mat, groupRoot.mergerToolSettings.minTexturCellSize, groupRoot.mergerToolSettings.minTexturCellSize);
							groupRoot.uniqueMaterials.Add(matData);
							if (matData.normalTex != null)
								groupRoot.mergerToolSettings.createNormalTexture = true;
							if (matData.glowTex != null)
								groupRoot.mergerToolSettings.createGlowTexture = true;
						}

						selectionData.id = list.Count;
						list.Add(selectionData);
						totalModels++;
					}
					rootObjects[r].models = list;
				}
				else rootObjects[r] = null;
			}
		}

		void UpdateConstructionPreview()
		{
			if (groupRoot.mergedMatetrial != null)
			{
				groupRoot.mergedMatetrial.EnableKeyword("CONSTRUCTION");
				groupRoot.mergedMatetrial.SetFloat("_ConstructionProgress", 1f - objectsConstructionProgress);
			}

			for (int i = 0; i < rootObjects.Count; i++)
			{
				if (rootObjects[i] != null && rootObjects[i].mergedMesh != null)
					MeshMerger.MeshUpdateVertexColor(rootObjects[i].mergedMesh, rootObjects[i].models);
			}
		}
		void OnSelectionChange()
		{
			currentSelection = Selection.gameObjects;
			Repaint();
		}
	}
}