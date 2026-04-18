using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Vehicles.Modification;

public class VehicleColors : Singleton<VehicleColors>
{
	[Serializable]
	public class VehicleColorData
	{
		public EVehicleColor color;

		public string colorName;

		public Color MaterialColor = Color.white;

		public Color32 UIColor = Color32.op_Implicit(Color.white);
	}

	public List<VehicleColorData> colorLibrary = new List<VehicleColorData>();

	public string GetColorName(EVehicleColor c)
	{
		return colorLibrary.Find((VehicleColorData x) => x.color == c).colorName;
	}

	public Color32 GetColorUIColor(EVehicleColor c)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return colorLibrary.Find((VehicleColorData x) => x.color == c).UIColor;
	}
}
