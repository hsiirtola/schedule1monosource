using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.Economy;

[Serializable]
[CreateAssetMenu(fileName = "CustomerData", menuName = "ScriptableObjects/CustomerData", order = 1)]
public class CustomerData : ScriptableObject
{
	public CustomerAffinityData DefaultAffinityData;

	[Header("Preferred Properties - Properties the customer prefers in a product.")]
	public List<Effect> PreferredProperties = new List<Effect>();

	[Header("Spending Behaviour")]
	public float MinWeeklySpend = 200f;

	public float MaxWeeklySpend = 500f;

	[Range(0f, 7f)]
	public int MinOrdersPerWeek = 1;

	[Range(0f, 7f)]
	public int MaxOrdersPerWeek = 5;

	[Header("Timing Settings")]
	public int OrderTime = 1200;

	public EDay PreferredOrderDay;

	[Header("Standards")]
	public ECustomerStandard Standards = ECustomerStandard.Moderate;

	[Header("Direct approaching")]
	public bool CanBeDirectlyApproached = true;

	public bool GuaranteeFirstSampleSuccess;

	[Tooltip("The average relationship of mutual customers to provide a 50% chance of success")]
	[Range(0f, 5f)]
	public float MinMutualRelationRequirement = 3f;

	[Tooltip("The average relationship of mutual customers to provide a 100% chance of success")]
	[Range(0f, 5f)]
	public float MaxMutualRelationRequirement = 5f;

	[Tooltip("If direct approach fails, whats the chance the police will be called?")]
	[Range(0f, 1f)]
	public float CallPoliceChance = 0.5f;

	[Header("Dependence")]
	[Tooltip("How quickly the customer builds dependence")]
	[Range(0f, 2f)]
	public float DependenceMultiplier = 1f;

	[Tooltip("The customer's starting (and lowest possible) dependence level")]
	[Range(0f, 1f)]
	public float BaseAddiction;

	public Action onChanged;

	private void OnValidate()
	{
		if (MinWeeklySpend > MaxWeeklySpend)
		{
			Debug.LogError((object)"Min weekly spend cannot be greater than max weekly spend.", (Object)(object)this);
		}
	}

	public static float GetQualityScalar(EQuality quality)
	{
		return quality switch
		{
			EQuality.Trash => 0f, 
			EQuality.Poor => 0.25f, 
			EQuality.Standard => 0.5f, 
			EQuality.Premium => 0.75f, 
			EQuality.Heavenly => 1f, 
			_ => 0f, 
		};
	}

	public List<EDay> GetOrderDays(float dependence, float normalizedRelationship)
	{
		float num = Mathf.Max(dependence, normalizedRelationship);
		int num2 = Mathf.RoundToInt(Mathf.Lerp((float)MinOrdersPerWeek, (float)MaxOrdersPerWeek, num));
		int preferredOrderDay = (int)PreferredOrderDay;
		int num3 = Mathf.RoundToInt(7f / (float)num2);
		num3 = Mathf.Max(num3, 1);
		List<EDay> list = new List<EDay>();
		for (int i = 0; i < 7; i += num3)
		{
			list.Add((EDay)((i + preferredOrderDay) % 7));
		}
		return list;
	}

	public float GetAdjustedWeeklySpend(float normalizedRelationship)
	{
		return Mathf.Lerp(MinWeeklySpend, MaxWeeklySpend, normalizedRelationship) * LevelManager.GetOrderLimitMultiplier(NetworkSingleton<LevelManager>.Instance.GetFullRank());
	}

	[Button]
	public void RandomizeAffinities()
	{
		DefaultAffinityData = new CustomerAffinityData();
		List<EDrugType> list = Enum.GetValues(typeof(EDrugType)).Cast<EDrugType>().ToList();
		for (int i = 0; i < list.Count; i++)
		{
			DefaultAffinityData.ProductAffinities.Add(new ProductTypeAffinity
			{
				DrugType = list[i],
				Affinity = 0f
			});
		}
		for (int j = 0; j < DefaultAffinityData.ProductAffinities.Count; j++)
		{
			DefaultAffinityData.ProductAffinities[j].Affinity = Random.Range(-1f, 1f);
		}
	}

	[Button]
	public void RandomizeFavouriteEffects()
	{
		string[] obj = new string[5] { "Properties/Tier1", "Properties/Tier2", "Properties/Tier3", "Properties/Tier4", "Properties/Tier5" };
		List<Effect> list = new List<Effect>();
		string[] array = obj;
		foreach (string text in array)
		{
			list.AddRange(Resources.LoadAll<Effect>(text));
		}
		PreferredProperties.Clear();
		for (int j = 0; j < 3; j++)
		{
			int index = Random.Range(0, list.Count);
			PreferredProperties.Add(list[index]);
			list.RemoveAt(index);
		}
	}

	[Button]
	public void RandomizeTiming()
	{
		PreferredOrderDay = (EDay)Random.Range(0, 7);
		int num = Random.Range(420, 1440);
		num = Mathf.RoundToInt((float)num / 15f) * 15;
		OrderTime = TimeManager.Get24HourTimeFromMinSum(num);
	}
}
