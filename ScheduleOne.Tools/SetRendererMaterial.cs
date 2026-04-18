using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.Tools;

public class SetRendererMaterial : MonoBehaviour
{
	public Material Material;

	[Button]
	public void SetMaterial()
	{
		MeshRenderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer val in componentsInChildren)
		{
			Material[] sharedMaterials = ((Renderer)val).sharedMaterials;
			for (int j = 0; j < sharedMaterials.Length; j++)
			{
				sharedMaterials[j] = Material;
			}
			((Renderer)val).sharedMaterials = sharedMaterials;
		}
	}
}
