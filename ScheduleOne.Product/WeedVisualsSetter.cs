using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using UnityEngine;

namespace ScheduleOne.Product;

public class WeedVisualsSetter : ProductVisualsSetter
{
	[Serializable]
	public class MeshMaterialSettings
	{
		public MeshRenderer Mesh;

		public List<WeedAppearanceSettings.EWeedAppearanceType> Materials;
	}

	public MeshMaterialSettings[] Meshes;

	public override void ApplyVisuals(ProductDefinition definition)
	{
		if (!TryCastProductDefinition<WeedDefinition>(definition, out var castedDefinition))
		{
			Console.LogError("WeedVisualsSetter applied to non-weed product definition: " + ((BaseItemDefinition)definition).ID);
			return;
		}
		for (int i = 0; i < Meshes.Length; i++)
		{
			List<Material> list = new List<Material>();
			((Renderer)Meshes[i].Mesh).GetMaterials(list);
			if (list.Count != Meshes[i].Materials.Count)
			{
				Console.LogError("Mesh materials count does not match settings count for mesh: " + ((Object)Meshes[i].Mesh).name);
				continue;
			}
			for (int j = 0; j < list.Count; j++)
			{
				list[j] = castedDefinition.GetMaterial(Meshes[i].Materials[j]);
			}
			((Renderer)Meshes[i].Mesh).SetMaterials(list.ToList());
		}
		((Component)VisualsContainer).gameObject.SetActive(true);
	}

	private void OnValidate()
	{
		for (int i = 0; i < Meshes.Length; i++)
		{
			List<Material> list = new List<Material>();
			((Renderer)Meshes[i].Mesh).GetSharedMaterials(list);
			if (list.Count != Meshes[i].Materials.Count)
			{
				Console.LogError("Mesh materials count does not match settings count for mesh: " + ((Object)Meshes[i].Mesh).name);
			}
		}
	}
}
