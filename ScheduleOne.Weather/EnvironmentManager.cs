using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Transporting;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Weather;

public class EnvironmentManager : NetworkSingleton<EnvironmentManager>
{
	[Header("General Components")]
	[SerializeField]
	private Transform _playerObj;

	[Header("Controllers")]
	[SerializeField]
	private DayNightController _dayNightController;

	[SerializeField]
	private MaskController _maskController;

	[Header("Weather Components")]
	[SerializeField]
	private Transform _weatherBoundsAnchor;

	[SerializeField]
	private Transform _weatherVolumeContainer;

	[Header("Weather Profiles")]
	[SerializeField]
	private List<WeatherSequence> _weatherSequences;

	[SerializeField]
	private List<WeightedWeatherSequence> _dailyWeatherSequences;

	[Header("Weather Settings")]
	[SerializeField]
	private float _defaultWeatherVolumeMoveSpeed = 1f;

	[SerializeField]
	[Range(1f, 6f)]
	private int _weatherVolumeCount = 3;

	[SerializeField]
	private Vector3 _weatherBounds = new Vector3(50f, 50f, 50f);

	[SerializeField]
	[Range(0f, 1f)]
	private float _weatherVolumeBlendSize = 0.1f;

	[SerializeField]
	private AnimationCurve _blendCurve;

	[Header("Lighting Settings")]
	[SerializeField]
	private LensFlareSettings _lensFlareSettings;

	[Header("Debugging & Development")]
	[SerializeField]
	private UniversalRendererData _rendererData;

	[SerializeField]
	private bool _debugControlWeatherSpeedWithSlider;

	[SerializeField]
	[Range(0f, 1f)]
	private float _debugWeatherSliderValue;

	private List<WeatherEnclosure> _weatherEnclosures = new List<WeatherEnclosure>();

	private List<SkyOverrideEnclosure> _overrideEnclosures = new List<SkyOverrideEnclosure>();

	private List<PuddleVolume> _puddleVolumes = new List<PuddleVolume>();

	[SyncObject]
	private readonly SyncList<WeatherVolume> _activeWeatherVolumes = new SyncList<WeatherVolume>();

	private WeatherSequence _currentWeatherSequence;

	private WeatherVolume _targetWeatherVolume;

	private Vector3 _weatherVolumeBounds;

	private Vector3 _weatherBoundsCenter;

	private SkySettings _skyOverrideSettings;

	private float _skyOverrideBlendValue;

	private bool _doWeatherBlending;

	private bool _hasWeatherVolumeNeighbour;

	private bool _withinBounds;

	private int _targetWeatherVolumeIndex = -1;

	private int _neighbourWeatherVolumeIndex;

	private float _targetWeatherBlendValue;

	private float _weatherVolumeMoveSpeed = 1f;

	private float _neighbourWeatherBlendValue;

	private Vector2 _closestPointInTargetVolume;

	private Vector2 _closestPointInNeighbourVolume;

	private float _wetUpdateTimer;

	private int _sequenceVolumeStartIndex;

	private Vector3[] _weatherVolumePositions;

	private WeatherConditions _currentWeatherConditions;

	private SkyState _currentSkyState;

	protected ScheduleOneFogFeature _fogFeature;

	private List<IWeatherEntity> _registeredWeatherEntities = new List<IWeatherEntity>();

	private bool NetworkInitialize___EarlyScheduleOne_002EWeather_002EEnvironmentManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EWeather_002EEnvironmentManagerAssembly_002DCSharp_002Edll_Excuted;

	public WeatherConditions CurrentWeatherConditions => _currentWeatherConditions;

	public SkyState CurrentSkyState => _currentSkyState;

	protected Transform Player => GetPlayer();

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EWeather_002EEnvironmentManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		Singleton<LoadManager>.Instance.onLoadComplete.AddListener(new UnityAction(SetRandomWeatherSequence));
		float blendAmount = _weatherVolumeBlendSize * _weatherVolumeBounds.x;
		_maskController.Initialise(_weatherVolumeCount, blendAmount, _weatherVolumeBounds);
		_maskController.ConvertHeightToArray();
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		CreateWeatherVolumes();
	}

	private void Update()
	{
		if (_activeWeatherVolumes.Count != 0)
		{
			DetermineWeatherVolumeWithTarget();
			CalculateWeatherBlendsFromVolumes();
			BlendWeatherProfiles();
			UpdateVolumes();
			UpdateWeatherEntities();
			if (!_dayNightController.EnableDebugTimeControl)
			{
				_dayNightController.UpdateTime(NetworkSingleton<TimeManager>.Instance.NormalizedTimeOfDay);
			}
			UpdateWeather();
			_maskController.RunWetMaskShader(((IEnumerable<WeatherVolume>)_activeWeatherVolumes).ToList());
		}
	}

	private void InitialiseControllers()
	{
	}

	private void InitialiseSky()
	{
		if ((Object)(object)NetworkSingleton<TimeManager>.Instance != (Object)null)
		{
			NetworkSingleton<TimeManager>.Instance.onTick += new Action(OnTick);
			NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(OnMinutePass);
			TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
			timeManager.onTimeSet = (Action)Delegate.Combine(timeManager.onTimeSet, new Action(OnTimeSet));
			TimeManager timeManager2 = NetworkSingleton<TimeManager>.Instance;
			timeManager2.onSleepEnd = (Action)Delegate.Combine(timeManager2.onSleepEnd, new Action(OnSleepEnd));
		}
	}

	private void InitialiseWeather()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		_currentWeatherSequence = _weatherSequences[0];
		_weatherVolumeMoveSpeed = _defaultWeatherVolumeMoveSpeed;
		_weatherVolumeBounds = GetWeatherVolumeBounds();
		_weatherBoundsCenter = GetWeatherBoundsCenter();
		Vector3 weatherVolumeInitialPosition = GetWeatherVolumeInitialPosition();
		_weatherVolumePositions = (Vector3[])(object)new Vector3[_weatherVolumeCount + 1];
		_weatherVolumePositions[0] = weatherVolumeInitialPosition - ((Component)this).transform.right * _weatherVolumeBounds.x;
		for (int i = 0; i < _weatherVolumeCount; i++)
		{
			_weatherVolumePositions[i + 1] = weatherVolumeInitialPosition + new Vector3(_weatherVolumeBounds.x * (float)i, 0f, 0f);
		}
	}

	private void InitialiseGlobalVariables()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		float num = _weatherVolumeBlendSize * _weatherVolumeBounds.x * 2f;
		Shader.SetGlobalFloat("_WeatherBlendAmount", num);
		Shader.SetGlobalVector("_WeatherVolumeSize", Vector4.op_Implicit(_weatherVolumeBounds));
		Shader.SetGlobalVector("_WeatherBounds", Vector4.op_Implicit(_weatherBounds));
	}

	private void CreateWeatherVolumesAtStartIndex(int sequenceVolumeIndex)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer)
		{
			float num = _weatherVolumeBlendSize * _weatherVolumeBounds.x * 2f;
			_ = _weatherVolumeBounds - new Vector3(num, 0f, 0f);
			Vector3 weatherVolumeInitialPosition = GetWeatherVolumeInitialPosition();
			_activeWeatherVolumes.Clear();
			_sequenceVolumeStartIndex = sequenceVolumeIndex;
			for (int i = 0; i < _weatherVolumeCount; i++)
			{
				int wrappedIndex = MathUtility.GetWrappedIndex(_sequenceVolumeStartIndex, i, _currentWeatherSequence.WeatherVolumes.Count);
				CreateVolume(_currentWeatherSequence.WeatherVolumes[wrappedIndex].Volume, weatherVolumeInitialPosition + new Vector3(_weatherVolumeBounds.x * (float)i, 0f, 0f));
			}
		}
	}

	private void CreateVolume(WeatherVolume volume, Vector3 position, int insertIndex = -1)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer)
		{
			float num = _weatherVolumeBlendSize * _weatherVolumeBounds.x * 2f;
			Vector3 blendSize = _weatherVolumeBounds - new Vector3(num, 0f, 0f);
			WeatherVolume weatherVolume = Object.Instantiate<WeatherVolume>(volume, _weatherVolumeContainer);
			((Component)weatherVolume).transform.localPosition = position;
			((NetworkBehaviour)this).Spawn(((NetworkBehaviour)weatherVolume).NetworkObject, (NetworkConnection)null, default(Scene));
			weatherVolume.Initialise(_weatherBounds, _weatherVolumeBounds, blendSize, num, position, _maskController.WorldSize);
			if (insertIndex != -1)
			{
				_activeWeatherVolumes.Insert(insertIndex, weatherVolume);
			}
			else
			{
				_activeWeatherVolumes.Add(weatherVolume);
			}
		}
	}

	private WeatherProfile GetNextWeatherProfile(WeatherProfile currentProfile)
	{
		return null;
	}

	private void DetermineWeatherVolumeWithTarget()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)GetWeatherAnchor()).transform.InverseTransformPoint(Player.position);
		Vector3 val2 = _weatherBounds * 0.5f;
		_withinBounds = Mathf.Abs(val.x) <= val2.x && Mathf.Abs(val.z) <= val2.z;
		if (!_withinBounds)
		{
			ClearWeather();
			return;
		}
		for (int i = 0; i < _activeWeatherVolumes.Count; i++)
		{
			WeatherVolume weatherVolume = _activeWeatherVolumes[i];
			Vector3 val3 = ((Component)weatherVolume).transform.InverseTransformPoint(Player.position);
			Vector3 val4 = weatherVolume.VolumeSize * 0.5f;
			if (Mathf.Abs(val3.x) <= val4.x && Mathf.Abs(val3.z) <= val4.z)
			{
				_targetWeatherVolumeIndex = i;
				_targetWeatherVolume = weatherVolume;
				SetWeatherConditions(weatherVolume.WeatherProfile.Conditions);
				break;
			}
		}
	}

	private void CalculateWeatherBlendsFromVolumes()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_targetWeatherVolume == (Object)null))
		{
			bool flag = _targetWeatherVolume.IsInRightHalf(Player.position);
			_closestPointInTargetVolume = (flag ? _targetWeatherVolume.GetClosestPointOnRight(Player.position) : _targetWeatherVolume.GetClosestPointOnLeft(Player.position));
			_neighbourWeatherVolumeIndex = (flag ? (_targetWeatherVolumeIndex + 1) : (_targetWeatherVolumeIndex - 1));
			_hasWeatherVolumeNeighbour = _neighbourWeatherVolumeIndex >= 0 && _neighbourWeatherVolumeIndex < _activeWeatherVolumes.Count;
			if (_hasWeatherVolumeNeighbour)
			{
				WeatherVolume weatherVolume = _activeWeatherVolumes[_neighbourWeatherVolumeIndex];
				_closestPointInNeighbourVolume = (flag ? weatherVolume.GetClosestPointOnLeft(Player.position) : weatherVolume.GetClosestPointOnRight(Player.position));
				float normalizedPositionAlongSegment = MathUtility.GetNormalizedPositionAlongSegment(_closestPointInTargetVolume, _closestPointInNeighbourVolume, Player.position.XZ());
				float num = 1f - normalizedPositionAlongSegment;
				_doWeatherBlending = num != _targetWeatherBlendValue;
				_targetWeatherBlendValue = num;
				_neighbourWeatherBlendValue = normalizedPositionAlongSegment;
			}
		}
	}

	private void BlendWeatherProfiles()
	{
		if (_doWeatherBlending)
		{
			_activeWeatherVolumes[_targetWeatherVolumeIndex].BlendEffects(_targetWeatherBlendValue, _blendCurve);
			_activeWeatherVolumes[_targetWeatherVolumeIndex].SetNeighbourVolume(_hasWeatherVolumeNeighbour ? _activeWeatherVolumes[_neighbourWeatherVolumeIndex] : null);
			if (_hasWeatherVolumeNeighbour)
			{
				_activeWeatherVolumes[_neighbourWeatherVolumeIndex].BlendEffects(_neighbourWeatherBlendValue, _blendCurve);
				_activeWeatherVolumes[_neighbourWeatherVolumeIndex].SetNeighbourVolume(_activeWeatherVolumes[_targetWeatherVolumeIndex]);
			}
		}
	}

	private void CreateWeatherVolumes()
	{
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		if (_activeWeatherVolumes != null)
		{
			for (int i = 0; i < _activeWeatherVolumes.Count; i++)
			{
				((NetworkBehaviour)this).Despawn(((Component)_activeWeatherVolumes[i]).gameObject, (DespawnType?)null);
			}
		}
		_targetWeatherBlendValue = 0f;
		_neighbourWeatherBlendValue = 0f;
		List<WeatherSequence.SequenceItem> weatherVolumes = _currentWeatherSequence.WeatherVolumes;
		int num = 0;
		int num2 = NetworkSingleton<TimeManager>.Instance.DailyMinSum - 420;
		if (NetworkSingleton<TimeManager>.Instance.CurrentTime < 700)
		{
			num2 += 1440;
		}
		bool flag = false;
		int num3 = 0;
		int sequenceVolumeIndex = 0;
		for (int j = 0; j < weatherVolumes.Count; j++)
		{
			int num4 = num + weatherVolumes[j].ActiveTime + weatherVolumes[j].TransitionInTime;
			if (!MathUtility.BetweenValues(num2, num, num4, maxInclusive: false, minInclusive: true))
			{
				num += weatherVolumes[j].ActiveTime + weatherVolumes[j].TransitionInTime;
				continue;
			}
			num3 = weatherVolumes[j].TransitionInTime;
			flag = MathUtility.BetweenValues(num2, num, num + num3, maxInclusive: false, minInclusive: true);
			sequenceVolumeIndex = MathUtility.GetWrappedIndex((flag && num3 > 0) ? (j - 1) : j, 0, weatherVolumes.Count);
			break;
		}
		CreateWeatherVolumesAtStartIndex(sequenceVolumeIndex);
		if (flag)
		{
			float num5 = num + num3;
			float num6 = Mathf.InverseLerp((float)num, num5, (float)num2);
			for (int k = 0; k < _weatherVolumeCount; k++)
			{
				WeatherVolume weatherVolume = _activeWeatherVolumes[k];
				((Component)weatherVolume).transform.localPosition = Vector3.Lerp(_weatherVolumePositions[k + 1], _weatherVolumePositions[k], num6);
				weatherVolume.SetAnchor(((Component)weatherVolume).transform.localPosition);
			}
		}
	}

	private void MoveWeatherVolumes()
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		if (_activeWeatherVolumes.Count == 0 || !InstanceFinder.IsServer)
		{
			return;
		}
		int num = 0;
		int num2 = NetworkSingleton<TimeManager>.Instance.DailyMinSum - 420;
		if (NetworkSingleton<TimeManager>.Instance.CurrentTime < 700)
		{
			num2 += 1440;
		}
		List<WeatherSequence.SequenceItem> weatherVolumes = _currentWeatherSequence.WeatherVolumes;
		bool flag = false;
		int num3 = 0;
		int index = 0;
		for (int i = 0; i < weatherVolumes.Count; i++)
		{
			int num4 = num + weatherVolumes[i].ActiveTime + weatherVolumes[i].TransitionInTime;
			if (!MathUtility.BetweenValues(num2, num, num4, maxInclusive: true))
			{
				num += weatherVolumes[i].ActiveTime + weatherVolumes[i].TransitionInTime;
				continue;
			}
			num3 = weatherVolumes[i].TransitionInTime;
			flag = MathUtility.BetweenValues(num2, num, num + num3, maxInclusive: true);
			index = i;
			break;
		}
		if (flag)
		{
			float num5 = num + num3;
			float num6 = Mathf.InverseLerp((float)num, num5, (float)num2);
			for (int j = 0; j < _weatherVolumeCount; j++)
			{
				WeatherVolume weatherVolume = _activeWeatherVolumes[j];
				Vector3 anchor = Vector3.Lerp(_weatherVolumePositions[j + 1], _weatherVolumePositions[j], num6);
				weatherVolume.SetAnchor(anchor);
			}
			if (num6 == 1f)
			{
				((NetworkBehaviour)this).Despawn(((Component)_activeWeatherVolumes[0]).gameObject, (DespawnType?)null);
				_activeWeatherVolumes.RemoveAt(0);
				int wrappedIndex = MathUtility.GetWrappedIndex(index, 1, _currentWeatherSequence.WeatherVolumes.Count);
				WeatherVolume volume = _currentWeatherSequence.WeatherVolumes[wrappedIndex].Volume;
				CreateVolume(volume, _weatherVolumePositions[_weatherVolumePositions.Length - 1]);
			}
		}
	}

	public int GetSequenceStartTime(WeatherSequence sequence)
	{
		int num = NetworkSingleton<TimeManager>.Instance.DailyMinSum;
		if (NetworkSingleton<TimeManager>.Instance.CurrentTime < 700)
		{
			num += 85980;
		}
		int result = 0;
		switch (sequence.TimeRef)
		{
		case WeatherSequence.TimeReference.StartOfDay:
			return result;
		case WeatherSequence.TimeReference.OnInitialisation:
			return num;
		default:
		{
			int num2 = TimeManager.GetMinSumFrom24HourTime(sequence.StartTime) - 420;
			if (num2 <= num)
			{
				return num2;
			}
			return result;
		}
		}
	}

	private void UpdateVolumes()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		float blend = 0f;
		using (List<WeatherEnclosure>.Enumerator enumerator = _weatherEnclosures.GetEnumerator())
		{
			while (enumerator.MoveNext() && !enumerator.Current.WithinEnclosure(Player.position, out blend))
			{
			}
		}
		if (blend == 0f)
		{
			bool flag = false;
			foreach (SkyOverrideEnclosure overrideEnclosure in _overrideEnclosures)
			{
				if (overrideEnclosure.WithinEnclosure(Player.position, out var blend2))
				{
					_skyOverrideSettings = overrideEnclosure.SkySettings;
					_skyOverrideBlendValue = blend2;
					blend = blend2;
					flag = true;
					break;
				}
			}
			_skyOverrideSettings = (flag ? _skyOverrideSettings : null);
		}
		Enumerator<WeatherVolume> enumerator3 = _activeWeatherVolumes.GetEnumerator();
		try
		{
			while (enumerator3.MoveNext())
			{
				WeatherVolume current2 = enumerator3.Current;
				Vector3 position = Player.position;
				position.y = 0f;
				current2.UpdateVolume(position, blend);
			}
		}
		finally
		{
			((IDisposable)enumerator3/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private void UpdateWeather()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		if (_targetWeatherVolumeIndex != -1)
		{
			WeatherProfile weatherProfile = _activeWeatherVolumes[_targetWeatherVolumeIndex].WeatherProfile;
			SkySettings neighbourSettings = (_hasWeatherVolumeNeighbour ? _activeWeatherVolumes[_neighbourWeatherVolumeIndex].WeatherProfile.SkySettings : null);
			_currentSkyState = _dayNightController.EvaluateSky(weatherProfile.SkySettings, neighbourSettings, _neighbourWeatherBlendValue, _skyOverrideSettings, _skyOverrideBlendValue);
			for (int i = 0; i < _activeWeatherVolumes.Count; i++)
			{
				WeatherVolume weatherVolume = _activeWeatherVolumes[i];
				WeatherProfile weatherProfile2 = _activeWeatherVolumes[i].WeatherProfile;
				float value = _dayNightController.EvaluateFloatByTimeOfDay(weatherProfile2.SkySettings.CloudDensityGradient);
				Color value2 = _dayNightController.EvaluateColorByTimeOfDay(weatherProfile2.SkySettings.CloudColorGradient);
				weatherVolume.SetShaderNumericParameter("CloudDensity", value);
				weatherVolume.SetShaderColorParameter("CloudColor", value2);
				weatherVolume.SetVisualEffectNumericParameter("WindIntensity", _currentSkyState.WindIntensity);
			}
			Shader.SetGlobalFloat("_StartFogHeightFade", _currentSkyState.FogHeightFade.x);
			Shader.SetGlobalFloat("_EndFogHeightFade", _currentSkyState.FogHeightFade.y);
			Shader.SetGlobalColor("_SkyUpperColor", _currentSkyState.SkyUpperColor);
			Shader.SetGlobalColor("_SkyMiddleColor", _currentSkyState.SkyMiddleColor);
			Shader.SetGlobalColor("_SkyLowerColor", _currentSkyState.SkyLowerColor);
			Shader.SetGlobalColor("_FogColor", _currentSkyState.FogColor);
			Shader.SetGlobalFloat("_FogDensity", _currentSkyState.FogDensity);
			Shader.SetGlobalColor("_SunColor", _currentSkyState.SunColor);
			Shader.SetGlobalColor("_MoonColor", _currentSkyState.MoonColor);
			Shader.SetGlobalFloat("_WindIntensity", _currentSkyState.WindIntensity);
			Shader.SetGlobalFloat("_SunSize", _currentSkyState.SunSize);
			Shader.SetGlobalFloat("_MoonSize", _currentSkyState.MoonSize);
			RenderSettings.ambientSkyColor = _currentSkyState.AmbientSkyColor;
			RenderSettings.ambientEquatorColor = _currentSkyState.AmbientEquatorColor;
			RenderSettings.ambientGroundColor = _currentSkyState.AmbientGroundColor;
			LensFlareSettings.LensFlareSettingsGroup[] lensFlareGroups = _lensFlareSettings.GetLensFlareGroups();
			foreach (LensFlareSettings.LensFlareSettingsGroup lensFlareSettingsGroup in lensFlareGroups)
			{
				SetLensFlare(lensFlareSettingsGroup.LensFlare, Mathf.Lerp(0f, lensFlareSettingsGroup.Intensity, _currentSkyState.FogDensity));
			}
		}
	}

	private void UpdateWeatherEntities()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		foreach (IWeatherEntity registeredWeatherEntity in _registeredWeatherEntities)
		{
			registeredWeatherEntity.OnUpdateWeatherEntity();
			Vector3 position = registeredWeatherEntity.Transform.position;
			registeredWeatherEntity.IsUnderCover = IsPositionUnderCover(position);
			WeatherProfile weatherProfileFromPosition = GetWeatherProfileFromPosition(position);
			if (!((Object)(object)weatherProfileFromPosition == (Object)null) && !weatherProfileFromPosition.Id.Equals(registeredWeatherEntity.WeatherVolume))
			{
				registeredWeatherEntity.WeatherVolume = weatherProfileFromPosition.Id;
				registeredWeatherEntity.OnWeatherChange(weatherProfileFromPosition.Conditions);
			}
		}
	}

	private void SetRandomWeatherSequence()
	{
		int seed = GameManager.Seed;
		int elapsedDays = NetworkSingleton<TimeManager>.Instance.ElapsedDays;
		int currentTime = NetworkSingleton<TimeManager>.Instance.CurrentTime;
		elapsedDays = ((currentTime >= 0 && currentTime < 700) ? (elapsedDays - 1) : elapsedDays);
		if (elapsedDays == 0)
		{
			_currentWeatherSequence = _weatherSequences[0];
		}
		else
		{
			float num = (float)new Random(seed + elapsedDays).NextDouble();
			List<float> weights = new List<float>();
			float totalWeight = _dailyWeatherSequences.Sum((WeightedWeatherSequence x) => x.Weight);
			_dailyWeatherSequences.ForEach(delegate(WeightedWeatherSequence s)
			{
				weights.Add(s.Weight / totalWeight);
			});
			float num2 = 0f;
			for (int num3 = 0; num3 < _dailyWeatherSequences.Count; num3++)
			{
				num2 += weights[num3];
				if (num <= num2)
				{
					_currentWeatherSequence = _dailyWeatherSequences[num3].Sequence;
					break;
				}
			}
		}
		CreateWeatherVolumes();
	}

	private void SetLensFlare(LensFlareDataSRP flare, float intensity)
	{
		LensFlareDataElementSRP[] elements = flare.elements;
		for (int i = 0; i < elements.Length; i++)
		{
			elements[i].localIntensity = intensity;
		}
	}

	private void ClearWeather()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		_targetWeatherBlendValue = 0f;
		_targetWeatherVolumeIndex = -1;
		_targetWeatherVolume = null;
		_doWeatherBlending = false;
		Enumerator<WeatherVolume> enumerator = _activeWeatherVolumes.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				enumerator.Current.BlendEffects(0f, _blendCurve);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
		}
	}

	public void RegisterEnclosure(WorldEnclosure enclosure)
	{
		if (!(enclosure is WeatherEnclosure enclosure2))
		{
			if (enclosure is SkyOverrideEnclosure enclosure3)
			{
				RegisterOverrideEnclosure(enclosure3);
			}
		}
		else
		{
			RegisterWeatherEnclosure(enclosure2);
		}
	}

	private void RegisterWeatherEnclosure(WeatherEnclosure enclosure)
	{
		if (!_weatherEnclosures.Contains(enclosure))
		{
			_weatherEnclosures.Add(enclosure);
		}
	}

	private void RegisterOverrideEnclosure(SkyOverrideEnclosure enclosure)
	{
		if (!_overrideEnclosures.Contains(enclosure))
		{
			_overrideEnclosures.Add(enclosure);
		}
	}

	public void RegisterPuddleVolume(PuddleVolume puddleVolume)
	{
		if (!_puddleVolumes.Contains(puddleVolume))
		{
			_puddleVolumes.Add(puddleVolume);
		}
	}

	private void SetWeatherConditions(WeatherConditions conditions)
	{
		_currentWeatherConditions = conditions;
	}

	protected WeatherProfile GetWeatherProfileFromPosition(Vector3 position)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)GetWeatherAnchor()).transform.InverseTransformPoint(position);
		Vector3 val2 = _weatherBounds * 0.5f;
		_withinBounds = Mathf.Abs(val.x) <= val2.x && Mathf.Abs(val.z) <= val2.z;
		if (!_withinBounds)
		{
			return null;
		}
		for (int i = 0; i < _activeWeatherVolumes.Count; i++)
		{
			WeatherVolume weatherVolume = _activeWeatherVolumes[i];
			Vector3 val3 = ((Component)weatherVolume).transform.InverseTransformPoint(position);
			Vector3 val4 = weatherVolume.VolumeSize * 0.5f;
			if (Mathf.Abs(val3.x) <= val4.x && Mathf.Abs(val3.z) <= val4.z)
			{
				return weatherVolume.WeatherProfile;
			}
		}
		return null;
	}

	public WeatherConditions GetActiveWeatherConditionsFromPosition(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		WeatherProfile weatherProfileFromPosition = GetWeatherProfileFromPosition(position);
		if (!((Object)(object)weatherProfileFromPosition != (Object)null))
		{
			return null;
		}
		return weatherProfileFromPosition.Conditions;
	}

	private Vector3 GetWeatherVolumeBounds()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Max(1, _weatherVolumeCount - 1);
		return new Vector3(_weatherBounds.x / (float)num, _weatherBounds.y / 10f, _weatherBounds.z);
	}

	private Vector3 GetWeatherVolumeInitialPosition()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		float num = (0f - _weatherBounds.x) / 2f + _weatherVolumeBounds.x / 2f;
		float num2 = _weatherBounds.y / 2f - _weatherVolumeBounds.y / 2f;
		return _weatherBoundsCenter + new Vector3(num, num2, 0f);
	}

	private Vector3 GetWeatherBoundsCenter()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		return GetWeatherAnchor().position + new Vector3(0f, _weatherBounds.y / 2f, 0f);
	}

	private Transform GetWeatherAnchor()
	{
		if (!((Object)(object)_weatherBoundsAnchor != (Object)null))
		{
			return ((Component)this).transform;
		}
		return _weatherBoundsAnchor;
	}

	private void OnMinutePass()
	{
		MoveWeatherVolumes();
	}

	private void OnTick()
	{
		if (!_dayNightController.EnableDebugTimeControl)
		{
			_dayNightController.OnTick();
		}
	}

	public void OnTimeSet()
	{
		if (!_dayNightController.EnableDebugTimeControl)
		{
			_dayNightController.OnTimeSet(NetworkSingleton<TimeManager>.Instance.NormalizedTimeOfDay);
			CreateWeatherVolumes();
		}
	}

	public void OnSleepEnd()
	{
		SetRandomWeatherSequence();
	}

	private Transform GetPlayer()
	{
		if (!((Object)(object)Camera.main != (Object)null))
		{
			return _playerObj;
		}
		return ((Component)Camera.main).transform;
	}

	public bool IsPositionUnderCover(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = (position.XZ() + Vector2.one * _maskController.WorldSize / 2f) / _maskController.WorldSize * (float)_maskController.HeightMapResolution;
		int num = (int)val.y * _maskController.HeightMapResolution + (int)val.x;
		num = Mathf.Clamp(num, 0, _maskController.HeightMap.Length - 1);
		float num2 = _maskController.HeightMap[num];
		float num3 = Mathf.Lerp(_maskController.MinMaxHeight.x, _maskController.MinMaxHeight.y, num2);
		return position.y < num3;
	}

	public void OnWeatherEntityRegistered(IWeatherEntity entity)
	{
		if (!_registeredWeatherEntities.Contains(entity))
		{
			_registeredWeatherEntities.Add(entity);
		}
	}

	public void OnWeatherEntityUnregistered(IWeatherEntity entity)
	{
		if (_registeredWeatherEntities.Contains(entity))
		{
			_registeredWeatherEntities.Remove(entity);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		EnvironmentHandler.UnsubscribeFromOnRegisterWeatherEntity(OnWeatherEntityRegistered);
		EnvironmentHandler.UnsubscribeFromOnUnregisterWeatherEntity(OnWeatherEntityUnregistered);
		if ((Object)(object)_fogFeature != (Object)null && ((ScriptableRendererFeature)_fogFeature).isActive)
		{
			((ScriptableRendererFeature)_fogFeature).SetActive(false);
		}
	}

	[Button]
	public void SetDebugSequence()
	{
		SetWeatherSequence("Debug");
	}

	[Button]
	public void SetWeather(string type)
	{
		SetWeatherSequence(type);
	}

	private void SetWeatherSequence(string sequenceId)
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		WeatherSequence weatherSequence = _weatherSequences.Find((WeatherSequence s) => s.Id.Equals(sequenceId));
		if ((Object)(object)weatherSequence == (Object)null)
		{
			Debug.LogError((object)("No weather sequence found with id " + sequenceId));
			return;
		}
		for (int num = 0; num < _weatherVolumeCount; num++)
		{
			((NetworkBehaviour)this).Despawn(((Component)_activeWeatherVolumes[num]).gameObject, (DespawnType?)null);
		}
		_currentWeatherSequence = weatherSequence;
		CreateWeatherVolumes();
	}

	public void StopVolumeMovement()
	{
		_weatherVolumeMoveSpeed = 0f;
	}

	public void StartVolumeMovement()
	{
		_weatherVolumeMoveSpeed = _defaultWeatherVolumeMoveSpeed;
	}

	public void SetVolumeMoveSpeed(float speed)
	{
		_weatherVolumeMoveSpeed = speed;
	}

	public void TriggerLightningEvent()
	{
		ThunderController activeThunderController = GetActiveThunderController();
		if ((Object)(object)activeThunderController == (Object)null)
		{
			Debug.LogWarning((object)"No active ThunderController found in weather volumes. Cannot trigger lightning event.");
		}
		else
		{
			activeThunderController.TriggerRandomLightningStrike();
		}
	}

	public void TriggerPlayerLightningEvent(Player player)
	{
		ThunderController activeThunderController = GetActiveThunderController();
		if (!((Object)(object)activeThunderController == (Object)null))
		{
			activeThunderController.TriggerPlayerLightningStrike(player);
		}
	}

	public void TriggerNpcLightningEvent(NPC npc)
	{
		ThunderController activeThunderController = GetActiveThunderController();
		if (!((Object)(object)activeThunderController == (Object)null))
		{
			activeThunderController.TriggerNPCLightningStrike(npc);
		}
	}

	public void TriggerDistantThunder()
	{
		ThunderController activeThunderController = GetActiveThunderController();
		if ((Object)(object)activeThunderController == (Object)null)
		{
			Debug.LogWarning((object)"No active ThunderController found in weather volumes. Cannot trigger thunder event.");
		}
		else
		{
			activeThunderController.TriggerDistantThunder();
		}
	}

	private ThunderController GetActiveThunderController()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<WeatherVolume> enumerator = _activeWeatherVolumes.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				foreach (WeatherEffectController effectController in enumerator.Current.EffectControllers)
				{
					if (effectController is ThunderController result)
					{
						return result;
					}
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
		}
		return null;
	}

	private void SetWeather_Client()
	{
	}

	private void SetWeatherSpeed_Client()
	{
	}

	private void TriggerThunder_Client()
	{
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EWeather_002EEnvironmentManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EWeather_002EEnvironmentManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((SyncBase)_activeWeatherVolumes).InitializeInstance((NetworkBehaviour)(object)this, 0u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, true);
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EWeather_002EEnvironmentManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EWeather_002EEnvironmentManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)_activeWeatherVolumes).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002EWeather_002EEnvironmentManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		EnvironmentHandler.SubscribeToOnRegisterWeatherEntity(OnWeatherEntityRegistered);
		EnvironmentHandler.SubscribeToOnUnregisterWeatherEntity(OnWeatherEntityUnregistered);
		_fogFeature = ((ScriptableRendererData)_rendererData).rendererFeatures.Find((ScriptableRendererFeature x) => ((Object)x).name == "ScheduleOneFog") as ScheduleOneFogFeature;
		if ((Object)(object)_fogFeature != (Object)null && !((ScriptableRendererFeature)_fogFeature).isActive)
		{
			((ScriptableRendererFeature)_fogFeature).SetActive(true);
		}
		InitialiseSky();
		InitialiseWeather();
		InitialiseControllers();
		InitialiseGlobalVariables();
	}
}
