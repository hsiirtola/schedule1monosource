using System;
using ScheduleOne.Audio;
using ScheduleOne.Core;
using ScheduleOne.Experimental;
using ScheduleOne.Weather;
using UnityEngine;

namespace ScheduleOne.Vehicles;

public class Wheel : MonoBehaviour
{
	public const float SIDEWAY_SLIP_THRESHOLD = 0.2f;

	public const float FORWARD_SLIP_THRESHOLD = 0.8f;

	public const float DRIFT_AUDIO_THRESHOLD = 0.2f;

	public const float MIN_SPEED_FOR_DRIFT = 8f;

	public const float WHEEL_ANIMATION_DISTANCE = 40f;

	public const float HandbrakeFowardStiffnessMultiplier_Front = 0.9f;

	public const float HandbrakeSidewayStiffnessMultiplier_Front = 0.7f;

	public const float HandbrakeFowardStiffnessMultiplier_Rear = 0.9f;

	public const float HandbrakeSidewayStiffnessMultiplier_Rear = 0.3f;

	public bool DEBUG_MODE;

	[Header("References")]
	public Transform wheelModel;

	public Transform modelContainer;

	public WheelCollider wheelCollider;

	public Transform axleConnectionPoint;

	public Collider staticCollider;

	public ParticleSystem DriftParticles;

	[Header("Data")]
	[SerializeField]
	private WheelData _defaultData;

	[SerializeField]
	private WheelOverrideData _rainOverrideData;

	[Header("Settings")]
	public bool DriftParticlesEnabled = true;

	[Header("Drift Audio")]
	public bool DriftAudioEnabled;

	public AudioSourceController DriftAudioSource;

	private float defaultForwardStiffness = 1f;

	private float defaultSidewaysStiffness = 1f;

	private LandVehicle vehicle;

	private Vector3 lastFixedUpdatePosition = Vector3.zero;

	private WheelHit wheelData;

	private WheelFrictionCurve forwardCurve;

	private WheelFrictionCurve sidewaysCurve;

	private VehicleSettings _settings;

	public bool IsDrifting { get; protected set; }

	public bool IsDrifting_Smoothed => DriftTime > 0.2f;

	public float DriftTime { get; protected set; }

	public float DriftIntensity { get; protected set; }

	public bool IsSteerWheel { get; set; }

	private void Awake()
	{
		vehicle = ((Component)this).GetComponentInParent<LandVehicle>();
		_settings = (((Object)(object)_defaultData != (Object)null) ? _defaultData.Settings : new VehicleSettings());
	}

	protected virtual void Start()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		wheelCollider.ConfigureVehicleSubsteps(5f, 12, 15);
		WheelFrictionCurve val = wheelCollider.forwardFriction;
		defaultForwardStiffness = ((WheelFrictionCurve)(ref val)).stiffness;
		val = wheelCollider.sidewaysFriction;
		defaultSidewaysStiffness = ((WheelFrictionCurve)(ref val)).stiffness;
	}

	public void FixedUpdateWheel()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (((Collider)wheelCollider).enabled)
		{
			Vector3 position = default(Vector3);
			Quaternion rotation = default(Quaternion);
			wheelCollider.GetWorldPose(ref position, ref rotation);
			((Component)modelContainer).transform.localRotation = Quaternion.identity;
			((Component)wheelModel).transform.rotation = rotation;
			((Component)wheelModel).transform.position = position;
		}
		if (vehicle.LocalPlayerIsDriver)
		{
			ApplyFriction();
			CheckDrifting();
			UpdateDriftEffects();
			UpdateDriftAudio();
		}
	}

	public void FakeWheelRotation()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (!((Collider)wheelCollider).enabled)
		{
			Vector3 val = ((Component)this).transform.position - lastFixedUpdatePosition;
			float num = ((Component)this).transform.InverseTransformVector(val).z / ((float)System.Math.PI * 2f * wheelCollider.radius) * 360f;
			((Component)modelContainer).transform.localEulerAngles = new Vector3(0f, wheelCollider.steerAngle, 0f);
			((Component)wheelModel).transform.Rotate(num, 0f, 0f, (Space)1);
			lastFixedUpdatePosition = ((Component)this).transform.position;
		}
	}

	private void CheckDrifting()
	{
		if (!((Collider)wheelCollider).enabled)
		{
			IsDrifting = false;
			DriftTime = 0f;
			DriftIntensity = 0f;
			return;
		}
		if (Mathf.Abs(vehicle.Speed_Kmh) < 8f)
		{
			IsDrifting = false;
			DriftTime = 0f;
			DriftIntensity = 0f;
			return;
		}
		wheelCollider.GetGroundHit(ref wheelData);
		IsDrifting = (Mathf.Abs(((WheelHit)(ref wheelData)).sidewaysSlip) > 0.2f || Mathf.Abs(((WheelHit)(ref wheelData)).forwardSlip) > 0.8f) && Mathf.Abs(vehicle.Speed_Kmh) > 2f;
		float num = Mathf.Clamp01(Mathf.Abs(((WheelHit)(ref wheelData)).sidewaysSlip));
		float num2 = Mathf.Clamp01(Mathf.Abs(((WheelHit)(ref wheelData)).forwardSlip));
		DriftIntensity = Mathf.Max(num, num2);
		if (IsDrifting)
		{
			DriftTime += Time.fixedDeltaTime;
		}
		else
		{
			DriftTime = 0f;
		}
		if (DEBUG_MODE)
		{
			Debug.Log((object)("Sideways slip: " + ((WheelHit)(ref wheelData)).sidewaysSlip + "\nForward slip: " + ((WheelHit)(ref wheelData)).forwardSlip));
			Debug.Log((object)("Drifting: " + IsDrifting));
		}
	}

	private void UpdateDriftEffects()
	{
		if (IsDrifting_Smoothed && DriftParticlesEnabled)
		{
			if (!DriftParticles.isPlaying)
			{
				DriftParticles.Play();
			}
		}
		else if (DriftParticles.isPlaying)
		{
			DriftParticles.Stop();
		}
	}

	private void UpdateDriftAudio()
	{
		if (DriftAudioEnabled)
		{
			if (IsDrifting_Smoothed && DriftIntensity > 0.2f && !DriftAudioSource.IsPlaying)
			{
				DriftAudioSource.Play();
			}
			if (DriftAudioSource.IsPlaying)
			{
				float volumeMultiplier = Mathf.Clamp01(Mathf.InverseLerp(0.2f, 1f, DriftIntensity));
				DriftAudioSource.VolumeMultiplier = volumeMultiplier;
			}
		}
	}

	private void ApplyFriction()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		bool flag = vehicle.HandbrakeApplied || !vehicle.IsOccupied;
		forwardCurve = wheelCollider.forwardFriction;
		sidewaysCurve = wheelCollider.sidewaysFriction;
		if (IsSteerWheel)
		{
			((WheelFrictionCurve)(ref forwardCurve)).stiffness = _settings.ForwardFriction.Stiffness * (flag ? 0.9f : 1f);
			((WheelFrictionCurve)(ref sidewaysCurve)).stiffness = _settings.SidewaysFriction.Stiffness * (flag ? 0.7f : 1f);
		}
		else
		{
			((WheelFrictionCurve)(ref forwardCurve)).stiffness = _settings.ForwardFriction.Stiffness * (flag ? 0.9f : 1f);
			((WheelFrictionCurve)(ref sidewaysCurve)).stiffness = _settings.SidewaysFriction.Stiffness * (flag ? 0.3f : 1f);
		}
		((WheelFrictionCurve)(ref forwardCurve)).extremumSlip = _settings.ForwardFriction.ExtremumSlip;
		((WheelFrictionCurve)(ref forwardCurve)).extremumValue = _settings.ForwardFriction.ExtremumValue;
		((WheelFrictionCurve)(ref forwardCurve)).asymptoteSlip = _settings.ForwardFriction.AsymptoteSlip;
		((WheelFrictionCurve)(ref forwardCurve)).asymptoteValue = _settings.ForwardFriction.AsymptoteValue;
		((WheelFrictionCurve)(ref sidewaysCurve)).extremumSlip = _settings.SidewaysFriction.ExtremumSlip;
		((WheelFrictionCurve)(ref sidewaysCurve)).extremumValue = _settings.SidewaysFriction.ExtremumValue;
		((WheelFrictionCurve)(ref sidewaysCurve)).asymptoteSlip = _settings.SidewaysFriction.AsymptoteSlip;
		((WheelFrictionCurve)(ref sidewaysCurve)).asymptoteValue = _settings.SidewaysFriction.AsymptoteValue;
		wheelCollider.forwardFriction = forwardCurve;
		wheelCollider.sidewaysFriction = sidewaysCurve;
	}

	public virtual void SetPhysicsEnabled(bool enabled)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (enabled)
		{
			((Collider)wheelCollider).enabled = true;
			staticCollider.enabled = false;
			return;
		}
		((Collider)wheelCollider).enabled = false;
		ApplyDefaultWheelModelPosition();
		staticCollider.enabled = true;
		lastFixedUpdatePosition = ((Component)this).transform.position;
		if (DriftParticles.isPlaying)
		{
			DriftParticles.Stop();
		}
		if (DriftAudioSource.IsPlaying)
		{
			DriftAudioSource.Stop();
		}
	}

	public bool IsWheelGrounded()
	{
		WheelHit val = default(WheelHit);
		return wheelCollider.GetGroundHit(ref val);
	}

	public void OnWeatherChange(WeatherConditions newConditions)
	{
		if ((Object)(object)_defaultData == (Object)null)
		{
			Debug.LogWarning((object)("No default wheel data assigned for " + ((Object)((Component)this).gameObject).name));
			return;
		}
		if (_defaultData.Settings == null)
		{
			Debug.LogWarning((object)("Default wheel data for " + ((Object)((Component)this).gameObject).name + " does not contain settings."));
			return;
		}
		VehicleSettings vehicleSettings = _defaultData.Settings.Clone();
		if (newConditions.Rainy > 0f && !vehicle.IsUnderCover)
		{
			vehicleSettings = vehicleSettings.Blend(_rainOverrideData.Settings, newConditions.Rainy);
		}
		_settings = vehicleSettings;
	}

	[Button]
	private void ApplyDefaultWheelModelPosition()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		((Component)wheelModel).transform.localPosition = new Vector3(((Component)wheelModel).transform.localPosition.x, wheelCollider.suspensionDistance * (wheelCollider.suspensionSpring.targetPosition - 1f), ((Component)wheelModel).transform.localPosition.z);
		((Component)wheelModel).transform.localRotation = Quaternion.identity;
	}
}
