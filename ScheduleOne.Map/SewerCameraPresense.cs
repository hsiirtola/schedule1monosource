using Funly.SkyStudio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScheduleOne.Map;

public class SewerCameraPresense : Singleton<SewerCameraPresense>
{
	public Transform FullPresenseVolumesContainer;

	public Transform FadeVolumesContainer;

	public SkyProfileOverride SewerSkyProfileOverride;

	public Volume SewerPPVolume;

	private BoxCollider[] fullPresenceVolumes;

	private FadeVolume[] fadeVolumes;

	public float CameraPresenceInSewerArea { get; private set; }

	public float SmoothedCameraPresenceInSewerArea { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		fullPresenceVolumes = ((Component)FullPresenseVolumesContainer).GetComponentsInChildren<BoxCollider>();
		fadeVolumes = ((Component)FadeVolumesContainer).GetComponentsInChildren<FadeVolume>();
	}

	private void LateUpdate()
	{
		UpdatePresense();
		SmoothedCameraPresenceInSewerArea = Mathf.Lerp(SmoothedCameraPresenceInSewerArea, CameraPresenceInSewerArea, Time.deltaTime * 5f);
		SewerSkyProfileOverride.Strength = SmoothedCameraPresenceInSewerArea;
		SewerPPVolume.weight = SmoothedCameraPresenceInSewerArea;
	}

	private void UpdatePresense()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return;
		}
		CameraPresenceInSewerArea = 0f;
		Transform transform = ((Component)PlayerSingleton<PlayerCamera>.Instance.Camera).transform;
		for (int i = 0; i < fullPresenceVolumes.Length; i++)
		{
			if (fullPresenceVolumes[i].IsPointWithinCollider(transform.position))
			{
				CameraPresenceInSewerArea = 1f;
				return;
			}
		}
		for (int j = 0; j < fadeVolumes.Length; j++)
		{
			float positionScalar = fadeVolumes[j].GetPositionScalar(transform.position);
			if (positionScalar > CameraPresenceInSewerArea)
			{
				CameraPresenceInSewerArea = positionScalar;
			}
		}
	}

	public bool IsPointInSewerArea(Vector3 point)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < fullPresenceVolumes.Length; i++)
		{
			if (fullPresenceVolumes[i].IsPointWithinCollider(point))
			{
				return true;
			}
		}
		return false;
	}
}
