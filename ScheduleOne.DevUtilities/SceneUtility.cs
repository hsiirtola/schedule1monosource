using System.Collections.Generic;
using ScheduleOne.Core;
using ScheduleOne.Economy;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public class SceneUtility : MonoBehaviour
{
	[Header("Afinity Settings")]
	public EDrugType DrugAffinityToAdd;

	public Vector2 MinMaxAffinityRange;

	public bool UseCurrentHighestAffinityAsMax;

	[Header("Objects to Modify")]
	public List<Transform> SceneObjects;

	[Header("Finding Shaders")]
	[SerializeField]
	private Transform _rootObject;

	[SerializeField]
	private bool _showCountOnly = true;

	[Button]
	public void ScanSceneForShaders()
	{
		HashSet<Material> hashSet = new HashSet<Material>();
		List<string> list = new List<string>();
		Renderer[] componentsInChildren = ((Component)_rootObject).GetComponentsInChildren<Renderer>(true);
		foreach (Renderer val in componentsInChildren)
		{
			if (val.sharedMaterials == null)
			{
				continue;
			}
			Material[] sharedMaterials = val.sharedMaterials;
			foreach (Material val2 in sharedMaterials)
			{
				if (!((Object)(object)val2 == (Object)null) && !list.Contains(((Object)val2.shader).name))
				{
					hashSet.Add(val2);
					list.Add(((Object)val2.shader).name);
				}
			}
		}
		Debug.Log((object)$"Found {hashSet.Count} unique shaders:");
		if (_showCountOnly)
		{
			Debug.Log((object)$"Unique shaders found: {hashSet.Count}");
			return;
		}
		foreach (string item in list)
		{
			Debug.Log((object)item);
		}
	}

	[Button]
	public void AddAffinityAndRandomise()
	{
		foreach (Transform sceneObject in SceneObjects)
		{
			Customer[] componentsInChildren = ((Component)sceneObject).GetComponentsInChildren<Customer>();
			foreach (Customer customer in componentsInChildren)
			{
				float num = MinMaxAffinityRange.y;
				if (UseCurrentHighestAffinityAsMax)
				{
					float num2 = -1f;
					foreach (ProductTypeAffinity productAffinity in customer.CustomerData.DefaultAffinityData.ProductAffinities)
					{
						if (productAffinity.Affinity > num2)
						{
							num2 = productAffinity.Affinity;
						}
					}
					num = num2;
				}
				float affinity = Random.Range(MinMaxAffinityRange.x, num);
				if (!customer.CustomerData.DefaultAffinityData.ProductAffinities.Exists((ProductTypeAffinity x) => x.DrugType == DrugAffinityToAdd))
				{
					customer.CustomerData.DefaultAffinityData.ProductAffinities.Add(new ProductTypeAffinity
					{
						DrugType = DrugAffinityToAdd,
						Affinity = affinity
					});
					Debug.Log((object)("Set affinity of " + ((Object)customer).name + " to " + DrugAffinityToAdd.ToString() + " to " + affinity));
				}
			}
		}
	}

	[Button]
	public void RemoveAffinity()
	{
		foreach (Transform sceneObject in SceneObjects)
		{
			Customer[] componentsInChildren = ((Component)sceneObject).GetComponentsInChildren<Customer>();
			foreach (Customer customer in componentsInChildren)
			{
				if (customer.CustomerData.DefaultAffinityData.ProductAffinities.Exists((ProductTypeAffinity x) => x.DrugType == DrugAffinityToAdd))
				{
					customer.CustomerData.DefaultAffinityData.ProductAffinities.RemoveAll((ProductTypeAffinity x) => x.DrugType == DrugAffinityToAdd);
					Debug.Log((object)("Removed affinity of " + ((Object)customer).name + " to " + DrugAffinityToAdd));
				}
			}
		}
	}
}
