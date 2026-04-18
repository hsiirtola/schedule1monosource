using System;
using LiquidVolumeFX;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.GameTime;
using UnityEngine;

namespace ScheduleOne.StationFramework;

public class LiquidContainer : MonoBehaviour
{
	[Header("Settings")]
	[Range(0f, 1f)]
	public float Viscosity = 0.4f;

	public bool AdjustMurkiness = true;

	[Header("References")]
	public LiquidVolume LiquidVolume;

	public LiquidVolumeCollider Collider;

	public Transform ColliderTransform_Min;

	public Transform ColliderTransform_Max;

	[Header("Visuals Settings")]
	public float MaxLevel = 1f;

	private MeshRenderer liquidMesh;

	public float CurrentLiquidLevel { get; private set; }

	public Color LiquidColor { get; private set; } = Color.white;

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		liquidMesh = ((Component)LiquidVolume).GetComponent<MeshRenderer>();
		SetLiquidColor(LiquidVolume.liquidColor1);
	}

	private void Start()
	{
		LiquidVolume.directionalLight = Singleton<EnvironmentFX>.Instance.SunLight;
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		}
	}

	private void MinPass()
	{
		UpdateLighting();
	}

	private void UpdateLighting()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (AdjustMurkiness)
		{
			float num = Mathf.Abs(NetworkSingleton<TimeManager>.Instance.NormalizedTimeOfDay - 0.5f) / 0.5f;
			float num2 = Mathf.Lerp(1f, 0.75f, num);
			SetLiquidColor(LiquidColor * num2, setColorVariable: false, updateLigting: false);
		}
	}

	public void SetLiquidLevel(float level, bool debug = false)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if (debug)
		{
			Console.Log("setting liquid level to: " + level);
		}
		CurrentLiquidLevel = Mathf.Clamp01(level);
		LiquidVolume.level = Mathf.Lerp(0f, MaxLevel, CurrentLiquidLevel);
		if ((Object)(object)liquidMesh != (Object)null)
		{
			((Renderer)liquidMesh).enabled = CurrentLiquidLevel > 0.01f;
		}
		if ((Object)(object)Collider != (Object)null && (Object)(object)ColliderTransform_Min != (Object)null && (Object)(object)ColliderTransform_Max != (Object)null)
		{
			((Component)Collider).transform.localPosition = Vector3.Lerp(ColliderTransform_Min.localPosition, ColliderTransform_Max.localPosition, CurrentLiquidLevel);
			((Component)Collider).transform.localScale = Vector3.Lerp(ColliderTransform_Min.localScale, ColliderTransform_Max.localScale, CurrentLiquidLevel);
		}
	}

	public void SetLiquidColor(Color color, bool setColorVariable = true, bool updateLigting = true)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		if (setColorVariable)
		{
			LiquidColor = color;
		}
		LiquidVolume.liquidColor1 = color;
		LiquidVolume.liquidColor2 = color;
		if (updateLigting)
		{
			UpdateLighting();
		}
	}
}
