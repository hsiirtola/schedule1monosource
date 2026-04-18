using ScheduleOne.Core.Items.Framework;
using UnityEngine;

namespace ScheduleOne.Product;

public class MethVisualsSetter : ProductVisualsSetter
{
	public MeshRenderer[] CrystalMaterials;

	public override void ApplyVisuals(ProductDefinition definition)
	{
		if (!TryCastProductDefinition<MethDefinition>(definition, out var castedDefinition))
		{
			Console.LogError("MethVisualsSetter applied to non-meth product definition: " + ((BaseItemDefinition)definition).ID);
			return;
		}
		Material crystalMaterial = castedDefinition.CrystalMaterial;
		MeshRenderer[] crystalMaterials = CrystalMaterials;
		for (int i = 0; i < crystalMaterials.Length; i++)
		{
			((Renderer)crystalMaterials[i]).material = crystalMaterial;
		}
		((Component)VisualsContainer).gameObject.SetActive(true);
	}
}
