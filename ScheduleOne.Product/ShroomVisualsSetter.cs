using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using UnityEngine;

namespace ScheduleOne.Product;

public class ShroomVisualsSetter : ProductVisualsSetter
{
	protected enum EShroomMaterialType
	{
		Mushroom,
		Bulk
	}

	[Serializable]
	protected class MeshMaterialSettings
	{
		public MeshRenderer Mesh;

		public List<EShroomMaterialType> Materials;
	}

	[SerializeField]
	private MeshMaterialSettings[] _meshes;

	public override void ApplyVisuals(ProductDefinition definition)
	{
		if (!TryCastProductDefinition<ShroomDefinition>(definition, out var castedDefinition))
		{
			Console.LogError("ShroomVisualsSetter applied to non-Shroom product definition: " + ((BaseItemDefinition)definition).ID);
			return;
		}
		for (int i = 0; i < _meshes.Length; i++)
		{
			Material[] array = (Material[])(object)new Material[_meshes[i].Materials.Count];
			for (int j = 0; j < array.Length; j++)
			{
				switch (_meshes[i].Materials[j])
				{
				case EShroomMaterialType.Mushroom:
					array[j] = castedDefinition.ShroomMaterial;
					break;
				case EShroomMaterialType.Bulk:
					array[j] = castedDefinition.BulkMaterial;
					break;
				default:
					Console.LogError("Unhandled shroom material type: " + _meshes[i].Materials[j]);
					break;
				}
			}
			((Renderer)_meshes[i].Mesh).materials = array;
		}
		((Component)VisualsContainer).gameObject.SetActive(true);
	}
}
