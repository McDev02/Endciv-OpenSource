using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Endciv
{
	public class CharacterMeshFactory : ModularMeshFactory
	{
		public static CharacterMeshFactory Instance { get; private set; }
		[SerializeField] CombineCharacterSet[] maleCharacterSet;
		[SerializeField] CombineCharacterSet[] femaleCharacterSet;
		Texture2D lastTexture;

		Stopwatch watch;
		[SerializeField] string textureName = "_MainTex";

		const int TextureWidth = 512;
		const int TextureHeight = 512;

		[SerializeField] MeshTextureSetting bodyTextureSetting;
		[SerializeField] MeshTextureSetting headTextureSetting;
		[SerializeField] MeshTextureSetting hairTextureSetting;
		[SerializeField] MeshTextureSetting miscTextureSetting;

		[SerializeField] bool useFirstTexture;

		bool addHairMesh;

		public int totalProcesses;
		public int currentProcess;
		public int activeProcesses;

		[SerializeField]
		HumanColorSettings humanColorSettings;

		private void Awake()
		{
			if (Instance != null)
			{
				Destroy(this);
				return;
			}
			Instance = this;
			watch = new Stopwatch();
			bodyTextureSetting.Setup(TextureWidth, TextureHeight);
			headTextureSetting.Setup(TextureWidth, TextureHeight);
			hairTextureSetting.Setup(TextureWidth, TextureHeight);
			miscTextureSetting.Setup(TextureWidth, TextureHeight);
		}

		public SkinnedMeshRenderer GenerateModel(ModularCharacterView unitView, ELivingBeingGender gender, ELivingBeingAge age)
		{
			CombineCharacterSet set = CivRandom.SelectRandom(gender == ELivingBeingGender.Male ? maleCharacterSet : femaleCharacterSet);
			var combined = GenerateCombinedMesh(set);

			var mat = unitView.SkinnedMesh.material;
			if (mat == null) Debug.LogError("Material for Human is missing. Define it in the prefabs SkinnedMeshRenderer");
			else
				mat.SetTexture(textureName, combined.texture);

			unitView.SkinnedMesh.sharedMesh = combined.mesh;
			unitView.MeshFilter.sharedMesh = combined.mesh;
			unitView.MeshRenderer.sharedMaterial = mat;
			return unitView.SkinnedMesh;
		}
		public SkinnedMeshRenderer GenerateMesh(SkinnedMeshRenderer skinnedMeshRenderer, ELivingBeingGender gender, ELivingBeingAge age)
		{
			CombineCharacterSet set = CivRandom.SelectRandom(gender == ELivingBeingGender.Male ? maleCharacterSet : femaleCharacterSet);
			var combined = GenerateCombinedMesh(set);

			var mat = skinnedMeshRenderer.material;
			if (mat == null) Debug.LogError("Material for Human is missing. Define it in the prefabs SkinnedMeshRenderer");
			else
				skinnedMeshRenderer.sharedMaterial.SetTexture(textureName, combined.texture);

			skinnedMeshRenderer.sharedMesh = combined.mesh;

			return skinnedMeshRenderer;
		}


		IEnumerator CombineTextures(CombineCharacterSet set, Texture2D texture)
		{
			int myID = totalProcesses++;
			while (currentProcess != myID)
				yield return null;
			activeProcesses++;
			float downsampleFactor = 1f / Mathf.Max(1, TextureDownsampling + 1);
			Color32[] cols = new Color32[(int)(TextureWidth * downsampleFactor) * (int)(TextureHeight * downsampleFactor)];

			int downsampledWidth = (int)(TextureWidth * downsampleFactor);
			Color skinColor = humanColorSettings.GetRandomSkinTone();
			Color hairColor = humanColorSettings.GetRandomHairTone();

			yield return AddTexture(cols, bodyTextureSetting, set.bodyTexture, downsampledWidth, EOverlayMode.Overlay, skinColor, EMaskMode.None);
			yield return AddTexture(cols, headTextureSetting, set.headTexture, downsampledWidth, EOverlayMode.Overlay, skinColor, EMaskMode.MaskOverlay);

			//Head Hair
			if (!(set.MaleOrFemale && CivRandom.Range(0, 5) == 1))
			{
				yield return AddTexture(cols, headTextureSetting, CivRandom.SelectRandom(set.headHairTextures), downsampledWidth, EOverlayMode.Overlay, hairColor, EMaskMode.MaskApplication);
			}
			//Clothing
			yield return AddTexture(cols, bodyTextureSetting, CivRandom.SelectRandom(set.shirtsTexture), downsampledWidth, EOverlayMode.Multiply, GetRandomClothColor(), EMaskMode.MaskApplication);
			yield return AddTexture(cols, bodyTextureSetting, CivRandom.SelectRandom(set.pantsTexture), downsampledWidth, EMaskMode.MaskApplication);

			if (addHairMesh)
			{
				AddTexture(cols, hairTextureSetting, set.hairTexture, downsampledWidth, EOverlayMode.Overlay, hairColor, EMaskMode.MaskApplication);
			}
			//yield return AddTexture(cols, miscTextureSetting, set.miscTexture,  downsampledWidth);

			//Create Texture
			texture.SetPixels32(cols);
			texture.Apply(true);
			texture.Compress(false);

			//Next process
			activeProcesses--;
			currentProcess++;
		}

		private Color GetRandomClothColor()
		{
			HSBColor col;
			col.H = CivRandom.Range(0f, 1f);
			col.S = CivMath.fQuadraticInverse(CivRandom.Range(0f, 1f));
			col.B = CivRandom.Range(0f, 1f);
			col.A = 1f;
			return col.ToColor();

		}
		Mesh GetCombinedMesh(CombineCharacterSet set)
		{
			Mesh mesh = new Mesh();
			mesh.name = "Combined Character";
			if (set.bodyMesh == null) { Debug.LogError("No body mesh found, escape."); return mesh; }
			if (set.headMesh == null) { Debug.LogError("No head mesh found, escape."); return mesh; }

			ApplySkinnedData(mesh, set.bodyMesh);

			AddSkinnedMesh(mesh, set.bodyMesh, bodyTextureSetting);
			AddSkinnedMesh(mesh, set.headMesh, headTextureSetting);
			if (addHairMesh && set.hairMesh != null) AddSkinnedMesh(mesh, set.hairMesh, hairTextureSetting);
			//if (set.miscMesh != null) AddSkinnedMesh(mesh, set.miscMesh, miscTextureSetting);

			return mesh;
		}


		public CombinedMesh GenerateCombinedMesh(CombineCharacterSet set)
		{
			int chance = 2;
			if (set.MaleOrFemale) chance += 4;

			addHairMesh = CivRandom.Range(0, chance) == 1;
			latestCombinedMesh = new CombinedMesh();

			// watch.Reset();
			// watch.Start();
			latestCombinedMesh.mesh = GetCombinedMesh(set);
			//watch.LogRound("GetCombinedMesh");
			if (!useFirstTexture || lastTexture == null)
				lastTexture = GetCombinedTexture(set);
			latestCombinedMesh.texture = lastTexture;
			//watch.LogRound("GetCombinedTexture");

			return latestCombinedMesh;
		}

		Texture2D GetCombinedTexture(CombineCharacterSet set)
		{
			float downsampleFactor = 1f / Mathf.Max(1, TextureDownsampling + 1);
			Texture2D tex = new Texture2D((int)(TextureWidth * downsampleFactor), (int)(TextureHeight * downsampleFactor), TextureFormat.RGB24, true, false);
			tex.name = "character_diff";

			StartCoroutine(CombineTextures(set, tex));

			return tex;
		}

		private void OnDestroy()
		{
			Instance = null;
		}
	}
}