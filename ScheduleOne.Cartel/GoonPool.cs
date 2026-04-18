using System.Collections.Generic;
using ScheduleOne.AvatarFramework;
using ScheduleOne.Map;
using ScheduleOne.VoiceOver;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class GoonPool : MonoBehaviour
{
	public const float MALE_CHANCE = 0.7f;

	[Header("References")]
	[SerializeField]
	private CartelGoon[] goons;

	[SerializeField]
	private NPCEnterableBuilding[] exitBuildings;

	[Header("Appearance Settings")]
	public AvatarSettings[] MaleBaseAppearances;

	public AvatarSettings[] FemaleBaseAppearances;

	public AvatarSettings[] MaleClothing;

	public AvatarSettings[] FemaleClothing;

	public VODatabase[] MaleVoices;

	public VODatabase[] FemaleVoices;

	public Color[] SkinTones;

	public Color[] HairColors;

	private List<CartelGoon> spawnedGoons = new List<CartelGoon>();

	private List<CartelGoon> unspawnedGoons = new List<CartelGoon>();

	public int UnspawnedGoonCount => unspawnedGoons.Count;

	protected virtual void Awake()
	{
		CartelGoon[] array = goons;
		foreach (CartelGoon cartelGoon in array)
		{
			if ((Object)(object)cartelGoon != (Object)null)
			{
				unspawnedGoons.Add(cartelGoon);
			}
		}
		NPCEnterableBuilding[] array2 = exitBuildings;
		for (int i = 0; i < array2.Length; i++)
		{
			if ((Object)(object)array2[i] == (Object)null)
			{
				Debug.LogError((object)"Exit building is null in GoonPool. Please assign valid buildings.");
			}
		}
	}

	private void Update()
	{
	}

	public List<CartelGoon> SpawnMultipleGoons(Vector3 spawnPoint, int requestedAmount, bool setAsGoonMates = true)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		List<CartelGoon> list = new List<CartelGoon>();
		if (requestedAmount <= 0)
		{
			Debug.LogWarning((object)"Requested amount must be greater than zero.");
			return list;
		}
		while (requestedAmount > 0 && unspawnedGoons.Count > 0)
		{
			requestedAmount--;
			CartelGoon item = SpawnGoon(spawnPoint);
			list.Add(item);
		}
		if (setAsGoonMates)
		{
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (i != j)
					{
						list[i].AddGoonMate(list[j]);
					}
				}
			}
		}
		return list;
	}

	public CartelGoonAppearance GetRandomAppearance()
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		CartelGoonAppearance cartelGoonAppearance = new CartelGoonAppearance();
		cartelGoonAppearance.IsMale = Random.Range(0f, 1f) < 0.7f;
		if (cartelGoonAppearance.IsMale)
		{
			cartelGoonAppearance.BaseAppearanceIndex = Random.Range(0, MaleBaseAppearances.Length);
			cartelGoonAppearance.SkinColor = SkinTones[Random.Range(0, SkinTones.Length)];
			cartelGoonAppearance.HairColor = HairColors[Random.Range(0, HairColors.Length)];
			cartelGoonAppearance.ClothingIndex = Random.Range(0, MaleClothing.Length);
			cartelGoonAppearance.VoiceIndex = Random.Range(0, MaleVoices.Length);
		}
		else
		{
			cartelGoonAppearance.BaseAppearanceIndex = Random.Range(0, FemaleBaseAppearances.Length);
			cartelGoonAppearance.SkinColor = SkinTones[Random.Range(0, SkinTones.Length)];
			cartelGoonAppearance.HairColor = HairColors[Random.Range(0, HairColors.Length)];
			cartelGoonAppearance.ClothingIndex = Random.Range(0, FemaleClothing.Length);
			cartelGoonAppearance.VoiceIndex = Random.Range(0, FemaleVoices.Length);
		}
		return cartelGoonAppearance;
	}

	public CartelGoon SpawnGoon(Vector3 spawnPoint)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (unspawnedGoons.Count == 0)
		{
			Debug.LogWarning((object)"No unspawned goons available to spawn.");
			return null;
		}
		CartelGoon cartelGoon = unspawnedGoons[0];
		unspawnedGoons.RemoveAt(0);
		spawnedGoons.Add(cartelGoon);
		cartelGoon.Spawn(this, spawnPoint);
		return cartelGoon;
	}

	public void ReturnToPool(CartelGoon goon)
	{
		if ((Object)(object)goon == (Object)null)
		{
			Debug.LogWarning((object)"Attempted to return a null goon to the pool.");
		}
		else if (spawnedGoons.Contains(goon))
		{
			spawnedGoons.Remove(goon);
			unspawnedGoons.Add(goon);
		}
		else
		{
			Debug.LogWarning((object)"Attempted to return a goon that is not in the spawned list.");
		}
	}

	public NPCEnterableBuilding GetNearestExitBuilding(Vector3 position)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (exitBuildings == null || exitBuildings.Length == 0)
		{
			Debug.LogWarning((object)"No exit buildings defined in GoonPool.");
			return null;
		}
		NPCEnterableBuilding result = null;
		float num = float.MaxValue;
		NPCEnterableBuilding[] array = exitBuildings;
		foreach (NPCEnterableBuilding nPCEnterableBuilding in array)
		{
			if (!((Object)(object)nPCEnterableBuilding == (Object)null))
			{
				float num2 = Vector3.Distance(position, nPCEnterableBuilding.GetClosestDoor(position, useableOnly: true).AccessPoint.position);
				if (num2 < num)
				{
					num = num2;
					result = nPCEnterableBuilding;
				}
			}
		}
		return result;
	}
}
