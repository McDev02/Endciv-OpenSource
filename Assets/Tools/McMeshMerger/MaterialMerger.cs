using System;
using System.Collections.Generic;
using UnityEngine;

namespace McMeshMerger
{
	public class MaterialMerger
	{

		public static List<Material> GetUniqueMaterials(Material[] sharedMaterials)
		{
			List<Material> returnMats = new List<Material>(sharedMaterials.Length);
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				if (!returnMats.Contains(sharedMaterials[i]))
					returnMats.Add(sharedMaterials[i]);
			}
			return returnMats;
		}
	}
}