using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScheduleOne.Audio;

public class AudioZoneModifierVolume : MonoBehaviour, IAudioZoneModifier
{
	[FormerlySerializedAs("Zones")]
	[SerializeField]
	private List<AudioZone> _zones = new List<AudioZone>();

	[FormerlySerializedAs("VolumeMultiplier")]
	[SerializeField]
	private float _volumeMultiplier = 0.5f;

	private BoxCollider[] _colliders;

	public float VolumeMultiplier => _volumeMultiplier;

	private void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("Refresh", 0f, 0.25f);
		_colliders = ((Component)this).GetComponentsInChildren<BoxCollider>();
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Invisible"));
	}

	private void Refresh()
	{
		if (IsCameraWithinVolume())
		{
			foreach (AudioZone zone in _zones)
			{
				zone.AddModifier(this);
			}
			return;
		}
		foreach (AudioZone zone2 in _zones)
		{
			zone2.RemoveModifier(this);
		}
	}

	private bool IsCameraWithinVolume()
	{
		if (_colliders != null && PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return _colliders.Any(delegate(BoxCollider c)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				Bounds bounds = ((Collider)c).bounds;
				return ((Bounds)(ref bounds)).Contains(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
			});
		}
		return false;
	}
}
