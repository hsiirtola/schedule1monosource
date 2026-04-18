using UnityEngine;

namespace ScheduleOne.Management;

public class TransitRouteMaterial : MonoBehaviour
{
	private void Awake()
	{
		Material material = ((Renderer)((Component)this).GetComponent<MeshRenderer>()).material;
		material.SetInt("unity_GUIZTestMode", 8);
		material.renderQueue = 3000;
	}
}
