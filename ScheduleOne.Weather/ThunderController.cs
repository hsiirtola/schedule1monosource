using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Effects;
using ScheduleOne.Employees;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Weather;

public class ThunderController : WeatherEffectController
{
	private const float _npcLightningStrikeDistanceFromPlayer = 40f;

	[Header("Thunder Settings")]
	[SerializeField]
	private float _maxThunderDelay = 3f;

	[SerializeField]
	private Vector2 _timeBetweenThunders = new Vector2(3f, 20f);

	[SerializeField]
	[Range(0f, 1f)]
	private float _chanceForLightingStrike = 0.08f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _chanceForLightingToHitPlayer = 0.01f;

	[SerializeField]
	[Range(0f, 1f)]
	private float _chanceForLightingToHitNPC = 0.01f;

	private float _sqrDistanceToPlayer;

	private float _thundertimer;

	private float _timeUntilNextThunder;

	private float _effectNormalisedDistanceToPlayer;

	private RandomizedAudioSourceController _thunderAudio;

	private RandomizedAudioSourceController _lightningAudio;

	private VFXEffectHandler _lightningEffect;

	private VFXEffectHandler _thunderEffect;

	private Vector3 _debugThunderLocation;

	private bool NetworkInitialize___EarlyScheduleOne_002EWeather_002EThunderControllerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EWeather_002EThunderControllerAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EWeather_002EThunderController_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	private void Start()
	{
		_lightningEffect.Deactivate();
		Random.InitState(DateTime.Now.Millisecond);
	}

	protected override void Update()
	{
		base.Update();
		if (InstanceFinder.IsServer && !(_distanceToPlayerNormalised < 0.1f))
		{
			_thundertimer += Time.deltaTime;
			if (_thundertimer >= _timeUntilNextThunder)
			{
				TriggerThunder();
				RandomiseThunderTimer();
				_thundertimer = 0f;
			}
		}
	}

	[Button]
	private void TriggerThunder()
	{
		if (!InstanceFinder.IsServer)
		{
			Debug.LogWarning((object)"Only the server can trigger thunder");
		}
		else if (!((Object)(object)_thunderAudio == (Object)null) && !((Object)(object)_lightningAudio == (Object)null))
		{
			float value = Random.value;
			if (value < _chanceForLightingToHitPlayer)
			{
				TriggerRandomPlayerLightningStrike();
			}
			else if (value < _chanceForLightingToHitNPC)
			{
				TriggerRandomNPCLightningStrike();
			}
			else if (value < _chanceForLightingStrike)
			{
				TriggerRandomLightningStrike();
			}
			else
			{
				TriggerDistantThunder();
			}
		}
	}

	public void TriggerRandomLightningStrike()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 randomPointInVolume = GetRandomPointInVolume();
		if (MapHeightSampler.TrySample(randomPointInVolume.x, randomPointInVolume.z, out var hitPoint))
		{
			TriggerLightningStrike_Server(hitPoint);
		}
		else
		{
			Debug.LogWarning((object)"Failed to sample height for lightning strike");
		}
	}

	public void TriggerRandomPlayerLightningStrike()
	{
		Player[] array = Player.PlayerList.Where((Player x) => CanBeStruck(x)).ToArray();
		if (array.Length != 0)
		{
			TriggerPlayerLightningStrike(array[Random.Range(0, array.Length)]);
		}
		static bool CanBeStruck(Player player)
		{
			if ((Object)(object)player == (Object)null)
			{
				return false;
			}
			if (!player.Health.IsAlive)
			{
				return false;
			}
			if (player.IsArrested)
			{
				return false;
			}
			return true;
		}
	}

	public void TriggerPlayerLightningStrike(Player player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (MapHeightSampler.TrySample(player.CenterPointTransform.position.x, player.CenterPointTransform.position.z, out var hitPoint))
		{
			TriggerLightningStrike_Server(hitPoint);
		}
	}

	public void TriggerRandomNPCLightningStrike()
	{
		Player plr = Player.GetRandomPlayer();
		if (!((Object)(object)plr == (Object)null))
		{
			float lightningStrikeDistanceSqr = 1600f;
			List<NPC> list = NPCManager.NPCRegistry.Where((NPC x) => CanBeStruck(x) && Vector3.SqrMagnitude(((Component)x).transform.position - ((Component)plr).transform.position) < lightningStrikeDistanceSqr).ToList();
			if (list.Count != 0)
			{
				TriggerNPCLightningStrike(list[Random.Range(0, list.Count)]);
			}
		}
		static bool CanBeStruck(NPC npc)
		{
			if ((Object)(object)npc == (Object)null)
			{
				return false;
			}
			if (!npc.isVisible)
			{
				return false;
			}
			if (npc.Health.IsDead)
			{
				return false;
			}
			if (npc.isInBuilding)
			{
				return false;
			}
			if (npc.IsInVehicle)
			{
				return false;
			}
			if (npc is Employee)
			{
				return false;
			}
			return true;
		}
	}

	public void TriggerNPCLightningStrike(NPC targetNPC)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (MapHeightSampler.TrySample(((Component)targetNPC).transform.position.x, ((Component)targetNPC).transform.position.z, out var hitPoint))
		{
			TriggerLightningStrike_Server(hitPoint);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void TriggerLightningStrike_Server(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Server_TriggerLightningStrike_Server_4276783012(position);
	}

	[ObserversRpc]
	private void TriggerLightningStrike_Client(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_TriggerLightningStrike_Client_4276783012(position);
	}

	public void TriggerDistantThunder()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		TriggerDistantThunder_Client(GetRandomPointInVolume());
	}

	[ObserversRpc]
	private void TriggerDistantThunder_Client(Vector3 location)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_TriggerDistantThunder_Client_4276783012(location);
	}

	private void RandomiseThunderTimer()
	{
		_timeUntilNextThunder = Random.Range(_timeBetweenThunders.x, _timeBetweenThunders.y);
	}

	public override void UpdateAudio()
	{
		foreach (AudioSourceController audioSource in _audioSources)
		{
			bool useEffectDistance = (Object)(object)audioSource == (Object)(object)_lightningAudio;
			UpdateAudio(audioSource, useEffectDistance);
		}
	}

	private Vector3 GetRandomPointInVolume()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 center = _mainVolume.Center;
		center.y -= 50f;
		Bounds val = default(Bounds);
		((Bounds)(ref val))._002Ector(center, _mainVolume.VolumeSize * 0.75f);
		return ((Bounds)(ref val)).center + new Vector3(Random.Range(0f - ((Bounds)(ref val)).extents.x, ((Bounds)(ref val)).extents.x), Random.Range(0f - ((Bounds)(ref val)).extents.y, ((Bounds)(ref val)).extents.y), Random.Range(0f - ((Bounds)(ref val)).extents.z, ((Bounds)(ref val)).extents.z));
	}

	private void UpdateAudio(AudioSourceController audioSource, bool useEffectDistance)
	{
		ScheduleOne.Audio.AudioSettings audioSettings = _audioSettings.Find((ScheduleOne.Audio.AudioSettings s) => s.Id == audioSource.Id + "Outside");
		ScheduleOne.Audio.AudioSettings audioSettings2 = _audioSettings.Find((ScheduleOne.Audio.AudioSettings s) => s.Id == audioSource.Id + "Inside");
		AudioSettingsWrapper audioSettingsWrapper = audioSource.ExtractAudioSettings();
		if (!((Object)(object)audioSettings2 == (Object)null) && !((Object)(object)audioSettings == (Object)null))
		{
			float num = ((useEffectDistance && _effectNormalisedDistanceToPlayer != -1f) ? _effectNormalisedDistanceToPlayer : _distanceToPlayerNormalised);
			float num2 = (_useWeatherBlendForAudio ? _weatherBlend : num);
			audioSettingsWrapper.Volume = Mathf.Lerp(audioSettings.Wrapper.Volume, audioSettings2.Wrapper.Volume, _enclosureCurve.Evaluate(_enclosureBlend)) * _distanceCurve.Evaluate(num2);
			audioSettingsWrapper.VolumeMultiplier = Mathf.Lerp(audioSettings.Wrapper.VolumeMultiplier, audioSettings2.Wrapper.VolumeMultiplier, _enclosureCurve.Evaluate(_enclosureBlend));
			audioSettingsWrapper.PitchMultiplier = Mathf.Lerp(audioSettings.Wrapper.PitchMultiplier, audioSettings2.Wrapper.PitchMultiplier, _enclosureCurve.Evaluate(_enclosureBlend));
			audioSettingsWrapper.LowPassCutoffFrequency = (int)MathUtility.LogLerp(audioSettings.Wrapper.LowPassCutoffFrequency, audioSettings2.Wrapper.LowPassCutoffFrequency, _enclosureBlend);
			audioSource.ApplyAudioSettings(audioSettingsWrapper);
		}
	}

	public override void UpdateProperties(Vector3 anchorPosition, Vector3 playerPosition, float sqrDistanceToPlayer, float enclosureBlend)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateProperties(anchorPosition, playerPosition, sqrDistanceToPlayer, enclosureBlend);
		_sqrDistanceToPlayer = sqrDistanceToPlayer;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EWeather_002EThunderControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EWeather_002EThunderControllerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_TriggerLightningStrike_Server_4276783012));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_TriggerLightningStrike_Client_4276783012));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_TriggerDistantThunder_Client_4276783012));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EWeather_002EThunderControllerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EWeather_002EThunderControllerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_TriggerLightningStrike_Server_4276783012(Vector3 position)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsClientInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteVector3(position);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___TriggerLightningStrike_Server_4276783012(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		TriggerLightningStrike_Client(position);
		NetworkSingleton<CombatManager>.Instance.CreateExplosion(position, ExplosionData.LightningStrike);
	}

	private void RpcReader___Server_TriggerLightningStrike_Server_4276783012(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___TriggerLightningStrike_Server_4276783012(position);
		}
	}

	private void RpcWriter___Observers_TriggerLightningStrike_Client_4276783012(Vector3 position)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteVector3(position);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___TriggerLightningStrike_Client_4276783012(Vector3 position)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"Lightning strike at {position}");
		float num = MathUtility.SqrDistance(position, _playerPosition);
		_effectNormalisedDistanceToPlayer = Mathf.InverseLerp(_minMaxDistanceToPlayer.y * _minMaxDistanceToPlayer.y, _minMaxDistanceToPlayer.x * _minMaxDistanceToPlayer.x, num);
		UpdateAudio();
		_lightningAudio.PlayOneShot();
		_lightningEffect.Activate();
		_lightningEffect.SetPosition(position);
		_lightningEffect.DelayDeactivate(2f, delegate
		{
			_effectNormalisedDistanceToPlayer = -1f;
		});
	}

	private void RpcReader___Observers_TriggerLightningStrike_Client_4276783012(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___TriggerLightningStrike_Client_4276783012(position);
		}
	}

	private void RpcWriter___Observers_TriggerDistantThunder_Client_4276783012(Vector3 location)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteVector3(location);
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___TriggerDistantThunder_Client_4276783012(Vector3 location)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"Distant thunder at {location}");
		Vector3 b = default(Vector3);
		((Vector3)(ref b))._002Ector(_playerPosition.x, 0f, _playerPosition.z);
		Vector3 val = location;
		val.y = 0f;
		_debugThunderLocation = new Vector3(val.x, ((Component)_thunderEffect).transform.position.y, val.z);
		float num = MathUtility.SqrDistance(val, b);
		_effectNormalisedDistanceToPlayer = Mathf.InverseLerp(_minMaxDistanceToPlayer.y * _minMaxDistanceToPlayer.y, _minMaxDistanceToPlayer.x * _minMaxDistanceToPlayer.x, num);
		UpdateAudio();
		float num2 = _effectNormalisedDistanceToPlayer * _maxThunderDelay;
		_thunderEffect.Activate();
		_thunderEffect.SetPosition(_debugThunderLocation);
		_thunderEffect.DelayDeactivate(2f + num2, delegate
		{
			_effectNormalisedDistanceToPlayer = -1f;
		});
		_thunderAudio.PlayOneShotDelayed(num2);
	}

	private void RpcReader___Observers_TriggerDistantThunder_Client_4276783012(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 location = ((Reader)PooledReader0).ReadVector3();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___TriggerDistantThunder_Client_4276783012(location);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EWeather_002EThunderController_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_thunderAudio = _audioSources.Find((AudioSourceController s) => s.Id == "Thunder") as RandomizedAudioSourceController;
		_lightningAudio = _audioSources.Find((AudioSourceController s) => s.Id == "Lightning") as RandomizedAudioSourceController;
		_lightningEffect = visualEffects.Find((VFXEffectHandler e) => e.Id == "Lightning");
		_thunderEffect = visualEffects.Find((VFXEffectHandler e) => e.Id == "Thunder");
		RandomiseThunderTimer();
	}
}
