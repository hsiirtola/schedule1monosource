using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Vehicles;

public class VehicleHumanoidCollider : MonoBehaviour
{
	public LandVehicle Vehicle { get; set; }

	private void Start()
	{
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Ignore Raycast"));
		Collider[] componentsInChildren = ((Component)this).GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].isTrigger = true;
		}
	}

	private void LateUpdate()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Vehicle != (Object)null)
		{
			((Component)this).transform.position = ((Component)Vehicle).transform.position;
			((Component)this).transform.rotation = ((Component)Vehicle).transform.rotation;
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		Debug.Log((object)("Collision Stay: " + ((Object)((Component)collision.collider).gameObject).name));
	}
}
