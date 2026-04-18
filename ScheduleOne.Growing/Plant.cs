using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Trash;
using UnityEngine;

namespace ScheduleOne.Growing;

public abstract class Plant : MonoBehaviour
{
	public const float BaseQualityLevel = 0.5f;

	[Header("References")]
	public Transform VisualsContainer;

	public PlantGrowthStage[] GrowthStages;

	public Collider Collider;

	public AudioSourceController SnipSound;

	public AudioSourceController DestroySound;

	public ParticleSystem FullyGrownParticles;

	public Transform HarvestLabelPositionTransform;

	[Header("Settings")]
	public SeedDefinition SeedDefinition;

	public int GrowthTime = 48;

	public int BaseYieldQuantity = 12;

	public string HarvestTarget = "buds";

	public float MinColliderScale = 0.4f;

	public float ColliderScaleThreshold = 0.5f;

	[Header("Trash")]
	public TrashItem PlantScrapPrefab;

	[HideInInspector]
	public List<int> ActiveHarvestables = new List<int>();

	public Action onFullyHarvested;

	public Pot Pot { get; protected set; }

	public float NormalizedGrowthProgress { get; protected set; }

	public bool IsFullyGrown => NormalizedGrowthProgress >= 1f;

	public float YieldMultiplier { get; private set; } = 1f;

	public float QualityLevel { get; private set; } = 0.5f;

	public PlantGrowthStage FinalGrowthStage => GrowthStages[GrowthStages.Length - 1];

	private void Awake()
	{
	}

	public virtual void Initialize(NetworkObject pot, float growthProgress)
	{
		Pot = ((Component)pot).GetComponent<Pot>();
		if ((Object)(object)Pot == (Object)null)
		{
			Console.LogWarning("Plant.Initialize: pot is null");
			return;
		}
		YieldMultiplier = Pot.YieldMultiplier;
		QualityLevel = 0.5f;
		for (int i = 0; i < FinalGrowthStage.GrowthSites.Length; i++)
		{
			SetHarvestableActive(i, active: false);
		}
		SetNormalizedGrowthProgress(growthProgress);
	}

	public virtual void MinPass(int mins)
	{
		if (!(NormalizedGrowthProgress >= 1f) && !NetworkSingleton<TimeManager>.Instance.IsEndOfDay)
		{
			float num = 1f / ((float)GrowthTime * 60f) * (float)mins;
			num *= Pot.GetTemperatureGrowthMultiplier();
			num *= Pot.GetAverageLightExposure(out var growSpeedMultiplier);
			num *= Pot.GrowSpeedMultiplier;
			num *= growSpeedMultiplier;
			if (GameManager.IS_TUTORIAL)
			{
				num *= 0.3f;
			}
			if (Pot.NormalizedMoistureAmount <= 0f)
			{
				num *= 0f;
			}
			SetNormalizedGrowthProgress(NormalizedGrowthProgress + num);
		}
	}

	public void AdditiveApplied(AdditiveDefinition additive, bool isInitialApplication)
	{
		if (additive.QualityChange != 0f)
		{
			QualityLevel += additive.QualityChange;
		}
		if (additive.YieldMultiplier != 0f)
		{
			YieldMultiplier *= additive.YieldMultiplier;
			YieldMultiplier = Mathf.Max(0f, YieldMultiplier);
		}
		if (isInitialApplication && additive.InstantGrowth > 0f)
		{
			SetNormalizedGrowthProgress(NormalizedGrowthProgress + additive.InstantGrowth);
		}
	}

	public virtual void SetNormalizedGrowthProgress(float progress)
	{
		progress = Mathf.Clamp(progress, 0f, 1f);
		float normalizedGrowthProgress = NormalizedGrowthProgress;
		NormalizedGrowthProgress = progress;
		UpdateVisuals();
		ResizeCollider();
		if (NormalizedGrowthProgress >= 1f && normalizedGrowthProgress < 1f)
		{
			GrowthDone();
		}
	}

	protected virtual void UpdateVisuals()
	{
		int num = Mathf.FloorToInt(NormalizedGrowthProgress * (float)GrowthStages.Length);
		for (int i = 0; i < GrowthStages.Length; i++)
		{
			((Component)GrowthStages[i]).gameObject.SetActive(i + 1 == num);
		}
	}

	public virtual void SetHarvestableActive(int index, bool active)
	{
		int count = ActiveHarvestables.Count;
		((Component)FinalGrowthStage.GrowthSites[index]).gameObject.SetActive(active);
		ActiveHarvestables.Remove(index);
		if (active)
		{
			ActiveHarvestables.Add(index);
		}
		if (count > 0 && ActiveHarvestables.Count == 0)
		{
			OnFullyHarvested();
		}
	}

	private void OnFullyHarvested()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (onFullyHarvested != null)
		{
			onFullyHarvested();
		}
		if (InstanceFinder.IsServer && (Object)(object)PlantScrapPrefab != (Object)null)
		{
			int num = Random.Range(1, 2);
			for (int i = 0; i < num; i++)
			{
				Vector3 forward = Pot.LeafDropPoint.forward;
				forward += new Vector3(0f, Random.Range(-0.2f, 0.2f), 0f);
				NetworkSingleton<TrashManager>.Instance.CreateTrashItem(PlantScrapPrefab.ID, Pot.LeafDropPoint.position + forward * 0.2f, Random.rotation, forward * 0.5f);
			}
		}
		((Component)DestroySound).transform.SetParent(((Component)NetworkSingleton<GameManager>.Instance.Temp).transform);
		DestroySound.PlayOneShot();
		Object.Destroy((Object)(object)DestroySound, 1f);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public bool IsHarvestableActive(int index)
	{
		return ActiveHarvestables.Contains(index);
	}

	private void GrowthDone()
	{
		if (InstanceFinder.IsServer)
		{
			if (!((NetworkBehaviour)Pot).IsSpawned)
			{
				Console.LogError("Pot not spawned!");
				return;
			}
			int num = Mathf.RoundToInt((float)BaseYieldQuantity * YieldMultiplier);
			num = Mathf.Clamp(num, 1, FinalGrowthStage.GrowthSites.Length);
			foreach (int item in GenerateUniqueIntegers(0, FinalGrowthStage.GrowthSites.Length - 1, num))
			{
				Pot.SetHarvestableActive_Server(item, active: true);
			}
		}
		if ((Object)(object)FullyGrownParticles != (Object)null)
		{
			FullyGrownParticles.Play();
		}
	}

	private List<int> GenerateUniqueIntegers(int min, int max, int count)
	{
		List<int> list = new List<int>();
		if (max - min + 1 < count)
		{
			Debug.LogWarning((object)"Range is too small to generate the requested number of unique integers.");
			return null;
		}
		List<int> list2 = new List<int>();
		for (int i = min; i <= max; i++)
		{
			list2.Add(i);
		}
		for (int j = 0; j < count; j++)
		{
			int index = Random.Range(0, list2.Count);
			list.Add(list2[index]);
			list2.RemoveAt(index);
		}
		return list;
	}

	public void SetVisible(bool vis)
	{
		((Component)VisualsContainer).gameObject.SetActive(vis);
	}

	private void ResizeCollider()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Collider == (Object)null))
		{
			((Component)Collider).transform.localScale = new Vector3(1f, Mathf.Lerp(MinColliderScale, 1f, Mathf.Clamp01(NormalizedGrowthProgress - ColliderScaleThreshold) / (1f - ColliderScaleThreshold)), 1f);
		}
	}

	public virtual ItemInstance GetHarvestedProduct(int quantity = 1)
	{
		Console.LogError("Plant.GetHarvestedProduct: This method should be overridden by a subclass.");
		return null;
	}

	public PlantData GetPlantData()
	{
		return new PlantData(((BaseItemDefinition)SeedDefinition).ID, NormalizedGrowthProgress, ActiveHarvestables.ToArray());
	}
}
