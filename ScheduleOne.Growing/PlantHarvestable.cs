using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Growing;

public class PlantHarvestable : MonoBehaviour
{
	public StorableItemDefinition Product;

	public int ProductQuantity = 1;

	private void Awake()
	{
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Task"));
	}

	public virtual void Harvest(bool giveProduct = true)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		Plant componentInParent = ((Component)this).GetComponentInParent<Plant>();
		if (giveProduct)
		{
			ItemInstance harvestedProduct = componentInParent.GetHarvestedProduct(ProductQuantity);
			if (Product is ProductDefinition productDefinition && !ProductManager.DiscoveredProducts.Contains(productDefinition))
			{
				NetworkSingleton<ProductManager>.Instance.DiscoverProduct(((BaseItemDefinition)productDefinition).ID);
			}
			PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(harvestedProduct);
		}
		((Component)this).GetComponentInParent<Pot>().SetHarvestableActive_Server(ArrayExt.IndexOf<Transform>(componentInParent.FinalGrowthStage.GrowthSites, ((Component)this).transform.parent), active: false);
		GameObject obj = Object.Instantiate<GameObject>(((Component)this).gameObject, GameObject.Find("_Temp").transform);
		obj.transform.position = ((Component)this).transform.position;
		obj.transform.rotation = ((Component)this).transform.rotation;
		obj.transform.localScale = ((Component)this).transform.lossyScale;
		Object.Destroy((Object)(object)obj.GetComponent<PlantHarvestable>());
		Object.Destroy((Object)(object)obj.GetComponentInChildren<Collider>());
		obj.AddComponent(typeof(Rigidbody));
		Rigidbody component = obj.GetComponent<Rigidbody>();
		component.AddForce(Vector3.up * 1.5f, (ForceMode)2);
		component.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 1f), Random.Range(-1f, 1f)) * 4f, (ForceMode)2);
		Object.Destroy((Object)(object)obj, 2f);
	}
}
