using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Vehicles.Modification;
using UnityEngine;

namespace ScheduleOne.Vehicles;

public class VehicleColor : MonoBehaviour
{
	[Serializable]
	public class BodyMesh
	{
		public MeshRenderer Renderer;

		public int MaterialIndex;
	}

	public BodyMesh[] BodyMeshes;

	public EVehicleColor DefaultColor = EVehicleColor.White;

	private EVehicleColor displayedColor = EVehicleColor.White;

	private bool initialColorApplied;

	private void Start()
	{
		if (!initialColorApplied)
		{
			ApplyColor(DefaultColor);
		}
	}

	public virtual void ApplyColor(EVehicleColor col)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (col == EVehicleColor.Custom)
		{
			displayedColor = col;
			return;
		}
		initialColorApplied = true;
		displayedColor = col;
		Color materialColor = Singleton<VehicleColors>.Instance.colorLibrary.Find((VehicleColors.VehicleColorData x) => x.color == displayedColor).MaterialColor;
		for (int num = 0; num < BodyMeshes.Length; num++)
		{
			List<Material> list = new List<Material>();
			((Renderer)BodyMeshes[num].Renderer).GetSharedMaterials(list);
			list[BodyMeshes[num].MaterialIndex] = new Material(list[BodyMeshes[num].MaterialIndex]);
			list[BodyMeshes[num].MaterialIndex].color = materialColor;
			((Renderer)BodyMeshes[num].Renderer).SetMaterials(list);
		}
	}
}
