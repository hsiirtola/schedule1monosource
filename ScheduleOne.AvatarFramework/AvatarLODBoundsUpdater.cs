using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework;

public class AvatarLODBoundsUpdater : MonoBehaviour
{
	public const float CHECK_RATE_SECONDS = 1f;

	public const float HIP_OFFSET_THRESHOLD = 5f;

	public Avatar Avatar;

	private List<LODGroup> lodGroups;

	private Vector3 hipOffsetOnLastRefresh = Vector3.zero;

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		GetLODGroups();
		Avatar.onSettingsLoaded.AddListener(new UnityAction(GetLODGroups));
		((MonoBehaviour)this).InvokeRepeating("InfrequentUpdate", 0f, 1f);
	}

	private void InfrequentUpdate()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(((Component)this).transform.InverseTransformPoint(Avatar.HipBone.position), hipOffsetOnLastRefresh) > 5f)
		{
			Recalculate();
		}
	}

	private void GetLODGroups()
	{
		lodGroups = ((Component)Avatar).GetComponentsInChildren<LODGroup>().ToList();
	}

	private void Recalculate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		hipOffsetOnLastRefresh = ((Component)this).transform.InverseTransformPoint(Avatar.HipBone.position);
		foreach (LODGroup lodGroup in lodGroups)
		{
			if (!((Object)(object)lodGroup == (Object)null))
			{
				float size = lodGroup.size;
				lodGroup.RecalculateBounds();
				lodGroup.size = size;
			}
		}
	}
}
