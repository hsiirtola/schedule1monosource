using System.Collections.Generic;
using GameKit.Utilities;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Weather;

public class PuddleVolume : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private List<GameObject> _puddleObjs;

	[Header("Settings")]
	[SerializeField]
	private Vector2Int _minMaxPuddlesInVolume = new Vector2Int(0, 3);

	[SerializeField]
	private Vector2 _minMaxPuddleDecay = new Vector2(0.1f, 0.5f);

	[SerializeField]
	private Vector2 _minMaxGrowthRate = new Vector2(0.1f, 0.5f);

	private float _decayRate;

	private float _growthRate;

	private void Start()
	{
		NetworkSingleton<EnvironmentManager>.Instance?.RegisterPuddleVolume(this);
		RandomiseActivePuddles();
	}

	private void RandomiseActivePuddles()
	{
		Arrays.Shuffle<GameObject>(_puddleObjs);
		for (int i = 0; i < _puddleObjs.Count; i++)
		{
			_puddleObjs[i].SetActive(i < ((Vector2Int)(ref _minMaxPuddlesInVolume)).x);
		}
	}

	private void Update()
	{
	}

	public void UpdateRates(WeatherConditions weatherConditions)
	{
		_decayRate = Mathf.Lerp(_minMaxPuddleDecay.x, _minMaxPuddleDecay.y, weatherConditions.Sunny);
		_growthRate = Mathf.Lerp(_minMaxGrowthRate.x, _minMaxGrowthRate.y, weatherConditions.Rainy);
	}
}
