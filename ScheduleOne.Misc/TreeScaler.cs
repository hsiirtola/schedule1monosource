using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Misc;

public class TreeScaler : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	protected List<Transform> branchMeshes = new List<Transform>();

	public float minScale = 1f;

	public float maxScale = 1.3f;

	public float minScaleDistance = 20f;

	public float maxScaleDistance = 100f;

	protected virtual void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(UpdateScale);
	}

	private void UpdateScale()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(Vector3.Distance(((Component)this).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position), minScaleDistance, maxScaleDistance) / (maxScaleDistance - minScaleDistance);
		float num2 = minScale + (maxScale - minScale) * num;
		foreach (Transform branchMesh in branchMeshes)
		{
			branchMesh.localScale = new Vector3(num2, 1f, num2);
		}
	}
}
