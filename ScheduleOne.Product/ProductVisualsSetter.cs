using System;
using UnityEngine;

namespace ScheduleOne.Product;

public abstract class ProductVisualsSetter : MonoBehaviour
{
	[Serializable]
	protected class MeshRendererInt
	{
		public MeshRenderer Renderer;

		public int MaterialIndex;
	}

	public Transform VisualsContainer;

	public abstract void ApplyVisuals(ProductDefinition productDefinition);

	public void ApplyVisuals(ProductItemInstance productInstance)
	{
		if (productInstance == null)
		{
			Debug.LogError((object)"Tried to apply visuals for null product instance.");
		}
		else
		{
			ApplyVisuals(productInstance.Definition as ProductDefinition);
		}
	}

	public void ResetVisuals()
	{
		((Component)VisualsContainer).gameObject.SetActive(false);
	}

	protected bool TryCastProductDefinition<T>(ProductDefinition definition, out T castedDefinition) where T : ProductDefinition
	{
		if ((Object)(object)definition == (Object)null)
		{
			castedDefinition = null;
			Debug.LogError((object)"Tried to apply visuals for null product definition.");
			return false;
		}
		if (definition is T)
		{
			castedDefinition = definition as T;
			return true;
		}
		castedDefinition = null;
		Debug.LogError((object)("Tried to apply visuals for product definition of type " + ((object)definition).GetType().Name + " but expected type " + typeof(T).Name + "."));
		return false;
	}
}
