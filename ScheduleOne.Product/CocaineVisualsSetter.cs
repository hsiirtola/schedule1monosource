using ScheduleOne.Core.Items.Framework;
using UnityEngine;

namespace ScheduleOne.Product;

public class CocaineVisualsSetter : ProductVisualsSetter
{
	public MeshRenderer[] RockMeshes;

	public override void ApplyVisuals(ProductDefinition definition)
	{
		if (!TryCastProductDefinition<CocaineDefinition>(definition, out var castedDefinition))
		{
			Console.LogError("CocaineVisualsSetter applied to non-cocaine product definition: " + ((BaseItemDefinition)definition).ID);
			return;
		}
		Material rockMaterial = castedDefinition.RockMaterial;
		MeshRenderer[] rockMeshes = RockMeshes;
		for (int i = 0; i < rockMeshes.Length; i++)
		{
			((Renderer)rockMeshes[i]).material = rockMaterial;
		}
		((Component)VisualsContainer).gameObject.SetActive(true);
	}
}
