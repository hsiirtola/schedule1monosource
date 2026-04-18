using System;
using System.Collections.Generic;
using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.Map;

public class SewerMushroomLocation : MonoBehaviour
{
	[Serializable]
	public struct MushroomLocationData
	{
		public bool isActive;

		public Vector3 location;

		public Quaternion rotation;

		public float scale;
	}

	[Header("Properties")]
	[SerializeField]
	private List<MushroomLocationData> _data = new List<MushroomLocationData>();

	public void SetMushroomsFromData(GameObject mushroomObject)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (_data.Count <= 0)
		{
			int childCount = mushroomObject.transform.GetChild(0).childCount;
			Transform child = mushroomObject.transform.GetChild(0).GetChild(0);
			SetMushroomFromData(child, new MushroomLocationData
			{
				location = Vector3.zero,
				rotation = Quaternion.identity,
				scale = 1f
			});
			for (int i = 1; i < childCount && i < _data.Count; i++)
			{
				((Component)mushroomObject.transform.GetChild(0).GetChild(i)).gameObject.SetActive(false);
			}
			return;
		}
		int childCount2 = mushroomObject.transform.GetChild(0).childCount;
		for (int j = 0; j < childCount2 && j < _data.Count; j++)
		{
			Transform child2 = mushroomObject.transform.GetChild(0).GetChild(j);
			((Component)child2).gameObject.SetActive(_data[j].isActive);
			if (_data[j].isActive)
			{
				SetMushroomFromData(child2, _data[j]);
			}
		}
	}

	private void SetMushroomFromData(Transform childMushroomObj, MushroomLocationData data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		childMushroomObj.localPosition = data.location;
		childMushroomObj.localRotation = data.rotation;
		childMushroomObj.localScale = Vector3.one * data.scale;
	}

	public void ClearData()
	{
		_data.Clear();
	}

	[Button]
	public void SetMushroomLocationData()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Transform child = ((Component)this).transform.GetChild(0).GetChild(0);
		for (int i = 0; i < child.childCount; i++)
		{
			Transform child2 = child.GetChild(i);
			_data.Add(new MushroomLocationData
			{
				location = child2.localPosition,
				rotation = child2.localRotation,
				scale = child2.localScale.x,
				isActive = ((Component)child2).gameObject.activeSelf
			});
		}
	}
}
