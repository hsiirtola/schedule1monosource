using System;
using System.Collections.Generic;
using ScheduleOne.Vehicles.Modification;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class VehicleData : SaveData
{
	public string GUID;

	public string VehicleCode;

	public Vector3 Position;

	public Quaternion Rotation;

	public string Color;

	public ItemSet VehicleContents;

	public List<SpraySurfaceData> SpraySurfaces = new List<SpraySurfaceData>();

	public VehicleData(Guid guid, string code, Vector3 pos, Quaternion rot, EVehicleColor col, ItemSet vehicleContents, List<SpraySurfaceData> spraySurfaces)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		GUID = guid.ToString();
		VehicleCode = code;
		Position = pos;
		Rotation = rot;
		Color = col.ToString();
		VehicleContents = vehicleContents;
		SpraySurfaces = spraySurfaces;
	}
}
