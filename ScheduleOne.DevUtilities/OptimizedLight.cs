using System;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace ScheduleOne.DevUtilities;

[RequireComponent(typeof(Light))]
[ExecuteInEditMode]
public class OptimizedLight : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("Enabled")]
	private bool _Enabled = true;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("DisabledForOptimization")]
	private bool _DisabledForOptimization;

	[Range(10f, 500f)]
	public float MaxDistance = 100f;

	public Light _Light;

	[SerializeField]
	private LensFlareComponentSRP _lensFlare;

	private bool culled;

	private float maxDistanceSquared;

	public bool Enabled
	{
		get
		{
			return _Enabled;
		}
		set
		{
			_Enabled = value;
			UpdateLightState();
		}
	}

	public bool DisabledForOptimization
	{
		get
		{
			return _DisabledForOptimization;
		}
		set
		{
			_DisabledForOptimization = value;
			UpdateLightState();
		}
	}

	public virtual void Awake()
	{
		_Light = ((Component)this).GetComponent<Light>();
		_lensFlare = ((Component)this).GetComponent<LensFlareComponentSRP>();
		maxDistanceSquared = MaxDistance * MaxDistance;
		UpdateLightState();
	}

	private void Start()
	{
		if (PlayerSingleton<PlayerMovement>.InstanceExists)
		{
			Register();
		}
		else
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(Register));
		}
		void Register()
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(Register));
			PlayerSingleton<PlayerCamera>.Instance.RegisterMovementEvent(Mathf.RoundToInt(Mathf.Clamp(MaxDistance / 10f, 0.5f, 20f)), UpdateCull);
			UpdateLightState();
		}
	}

	private void OnDestroy()
	{
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			PlayerSingleton<PlayerCamera>.Instance.DeregisterMovementEvent(UpdateCull);
		}
	}

	private void UpdateCull()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)this == (Object)null) && !((Object)(object)((Component)this).gameObject == (Object)null))
		{
			culled = Vector3.SqrMagnitude(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - ((Component)this).transform.position) > maxDistanceSquared * QualitySettings.lodBias;
			UpdateLightState();
		}
	}

	public void SetEnabled(bool enabled)
	{
		if (Enabled != enabled)
		{
			Enabled = enabled;
			UpdateLightState();
		}
	}

	private void UpdateLightState()
	{
		if ((Object)(object)_Light != (Object)null)
		{
			((Behaviour)_Light).enabled = Enabled && !DisabledForOptimization && !culled;
		}
		if ((Object)(object)_lensFlare != (Object)null)
		{
			((Behaviour)_lensFlare).enabled = Enabled && !DisabledForOptimization && !culled;
		}
	}
}
