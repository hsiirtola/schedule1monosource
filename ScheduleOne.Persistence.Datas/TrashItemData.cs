using System;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class TrashItemData
{
	public string DataType = string.Empty;

	public string TrashID;

	public string GUID;

	public Vector3 Position;

	public Quaternion Rotation;

	public TrashContentData Contents;

	public TrashItemData(string trashID, string guid, Vector3 position, Quaternion rotation)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		DataType = "TrashItemData";
		TrashID = trashID;
		GUID = guid;
		Position = position;
		Rotation = rotation;
	}
}
