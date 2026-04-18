using UnityEngine;

namespace ScheduleOne.Product;

public class MultiTypeVisualsSetter : MonoBehaviour
{
	public WeedVisualsSetter WeedVisuals;

	public MethVisualsSetter MethVisuals;

	public CocaineVisualsSetter CocaineVisuals;

	public ShroomVisualsSetter ShroomVisuals;

	private void Awake()
	{
	}

	public void ApplyVisuals(ProductItemInstance itemInstance)
	{
		if (itemInstance == null)
		{
			Debug.LogError((object)"Tried to apply visuals for null product item instance.");
		}
		else
		{
			ApplyVisuals(itemInstance.Definition as ProductDefinition);
		}
	}

	public void ApplyVisuals(ProductDefinition product)
	{
		ResetVisuals();
		if ((Object)(object)product == (Object)null)
		{
			Debug.LogError((object)"Tried to apply visuals for null product definition.");
			return;
		}
		switch (product.DrugType)
		{
		case EDrugType.Marijuana:
			WeedVisuals.ApplyVisuals(product);
			break;
		case EDrugType.Methamphetamine:
			MethVisuals.ApplyVisuals(product);
			break;
		case EDrugType.Cocaine:
			CocaineVisuals.ApplyVisuals(product);
			break;
		case EDrugType.Shrooms:
			ShroomVisuals.ApplyVisuals(product);
			break;
		default:
			Debug.LogError((object)$"Tried to apply visuals for product type {product.DrugType} but no visuals are set up for this type.");
			break;
		}
	}

	private void ResetVisuals()
	{
		WeedVisuals.ResetVisuals();
		MethVisuals.ResetVisuals();
		CocaineVisuals.ResetVisuals();
		ShroomVisuals.ResetVisuals();
	}
}
