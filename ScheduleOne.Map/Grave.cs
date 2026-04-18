using System;
using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.Map;

public class Grave : MonoBehaviour
{
	[Serializable]
	public class GraveSuface
	{
		public GameObject Object;

		public MeshRenderer Mesh;

		public Material[] Materials;
	}

	[Header("References")]
	public GraveSuface[] Surfaces;

	public GameObject[] HeadstoneObjects;

	public MeshRenderer[] HeadstoneMeshes;

	public Material[] HeadstoneMaterials;

	[Button]
	public void RandomizeGrave()
	{
		int num = Random.Range(0, Surfaces.Length);
		int num2 = Random.Range(0, HeadstoneObjects.Length);
		for (int i = 0; i < Surfaces.Length; i++)
		{
			Surfaces[i].Object.SetActive(i == num);
		}
		for (int j = 0; j < HeadstoneObjects.Length; j++)
		{
			HeadstoneObjects[j].SetActive(j == num2);
		}
		int num3 = Random.Range(0, Surfaces[num].Materials.Length);
		int num4 = Random.Range(0, HeadstoneMaterials.Length);
		((Renderer)Surfaces[num].Mesh).material = Surfaces[num].Materials[num3];
		for (int k = 0; k < HeadstoneMeshes.Length; k++)
		{
			((Renderer)HeadstoneMeshes[k]).material = HeadstoneMaterials[num4];
		}
	}
}
