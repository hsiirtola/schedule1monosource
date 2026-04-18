using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Weather;

public class WeatherVolume : NetworkBehaviour
{
	[Header("Controllers")]
	[SerializeField]
	private List<WeatherEffectController> _effectControllers;

	[Header("Profile")]
	[SerializeField]
	private WeatherProfile _weatherProfile;

	[Header("Debugging & Development")]
	[SerializeField]
	private bool _showGizmos = true;

	private Vector3 _weatherBounds;

	private Vector3 _volumeSize;

	private Vector3 _blendSize;

	private Vector3 _anchorPosition;

	private float _blendAmount;

	private bool _isInitialized;

	private Vector3 _velocity;

	private bool NetworkInitialize___EarlyScheduleOne_002EWeather_002EWeatherVolumeAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EWeather_002EWeatherVolumeAssembly_002DCSharp_002Edll_Excuted;

	public float BlendAmount => _blendAmount;

	public Vector3 WeatherBounds => _weatherBounds;

	public Vector3 BlendSize => _blendSize;

	public Vector3 VolumeSize => _volumeSize;

	public Vector3 Center => ((Component)this).transform.position;

	public Vector3 MinBounds => Center - VolumeSize / 2f;

	public Vector3 MaxBounds => Center + VolumeSize / 2f;

	public List<WeatherEffectController> EffectControllers => _effectControllers;

	public WeatherProfile WeatherProfile => _weatherProfile;

	protected Vector3 TopRightBlendCorner => ((Component)this).transform.position + ((Component)this).transform.right * (_blendSize.x / 2f) + ((Component)this).transform.forward * (_blendSize.z / 2f);

	protected Vector3 BottomRightBlendCorner => ((Component)this).transform.position + ((Component)this).transform.right * (_blendSize.x / 2f) - ((Component)this).transform.forward * (_blendSize.z / 2f);

	protected Vector3 TopLeftBlendCorner => ((Component)this).transform.position - ((Component)this).transform.right * (_blendSize.x / 2f) + ((Component)this).transform.forward * (_blendSize.z / 2f);

	protected Vector3 BottomLeftBlendCorner => ((Component)this).transform.position - ((Component)this).transform.right * (_blendSize.x / 2f) - ((Component)this).transform.forward * (_blendSize.z / 2f);

	[ObserversRpc(BufferLast = true, RunLocally = true)]
	public void Initialise(Vector3 weatherBounds, Vector3 volumeSize, Vector3 blendSize, float blendAmount, Vector3 anchorPosition, float heightMapWorldSize)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		RpcWriter___Observers_Initialise_1999361799(weatherBounds, volumeSize, blendSize, blendAmount, anchorPosition, heightMapWorldSize);
		RpcLogic___Initialise_1999361799(weatherBounds, volumeSize, blendSize, blendAmount, anchorPosition, heightMapWorldSize);
	}

	private void Update()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (InstanceFinder.IsServer)
		{
			((Component)this).transform.localPosition = Vector3.SmoothDamp(((Component)this).transform.localPosition, _anchorPosition, ref _velocity, 1f);
		}
	}

	public void SetAnchor(Vector3 anchorPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_anchorPosition = anchorPosition;
	}

	public void SetNeighbourVolume(WeatherVolume neighbourVolume)
	{
		_effectControllers.ForEach(delegate(WeatherEffectController effectController)
		{
			effectController.SetNeighbourVolume(neighbourVolume);
		});
	}

	public void BlendEffects(float blend, AnimationCurve blendCurve)
	{
		foreach (WeatherEffectController effectController in _effectControllers)
		{
			if (blend <= 0f && effectController.IsActive)
			{
				effectController.Deactivate();
			}
			else if (blend > 0f && !effectController.IsActive)
			{
				effectController.Activate();
			}
			effectController.BlendEffects(blend, blendCurve);
			effectController.UpdateAudio();
		}
	}

	public void SetShaderNumericParameter(string paramater, float value)
	{
		foreach (WeatherEffectController effectController in _effectControllers)
		{
			effectController.SetShaderNumericParameter(paramater, value);
		}
	}

	public void SetShaderColorParameter(string paramater, Color value)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		foreach (WeatherEffectController effectController in _effectControllers)
		{
			effectController.SetShaderColorParameter(paramater, value);
		}
	}

	public void SetVisualEffectNumericParameter(string paramater, float value)
	{
		foreach (WeatherEffectController effectController in _effectControllers)
		{
			effectController.SetVisualEffectNumericParameter(paramater, value);
		}
	}

	public void UpdateVolume(Vector3 playerPosition, float enclosureBlend)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(((Component)this).transform.position.x, 0f, ((Component)this).transform.position.z);
		float sqrDistanceToPlayer = MathUtility.SqrDistance(playerPosition, val);
		foreach (WeatherEffectController effectController in _effectControllers)
		{
			effectController.UpdateProperties(val, playerPosition, sqrDistanceToPlayer, enclosureBlend);
		}
	}

	public bool IsInRightHalf(Vector3 point)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.InverseTransformPoint(point).x >= 0f;
	}

	public Vector2 GetClosestPointOnLeft(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return MathUtility.ClosestPointOnSegment(point.XZ(), TopLeftBlendCorner.XZ(), BottomLeftBlendCorner.XZ());
	}

	public Vector2 GetClosestPointOnRight(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return MathUtility.ClosestPointOnSegment(point.XZ(), TopRightBlendCorner.XZ(), BottomRightBlendCorner.XZ());
	}

	private void OnDrawGizmos()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (_showGizmos)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(TopRightBlendCorner, 0.5f);
			Gizmos.DrawSphere(BottomRightBlendCorner, 0.5f);
			Gizmos.DrawSphere(TopLeftBlendCorner, 0.5f);
			Gizmos.DrawSphere(BottomLeftBlendCorner, 0.5f);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EWeather_002EWeatherVolumeAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EWeather_002EWeatherVolumeAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_Initialise_1999361799));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EWeather_002EWeatherVolumeAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EWeather_002EWeatherVolumeAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_Initialise_1999361799(Vector3 weatherBounds, Vector3 volumeSize, Vector3 blendSize, float blendAmount, Vector3 anchorPosition, float heightMapWorldSize)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteVector3(weatherBounds);
			((Writer)writer).WriteVector3(volumeSize);
			((Writer)writer).WriteVector3(blendSize);
			((Writer)writer).WriteSingle(blendAmount, (AutoPackType)0);
			((Writer)writer).WriteVector3(anchorPosition);
			((Writer)writer).WriteSingle(heightMapWorldSize, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, true, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___Initialise_1999361799(Vector3 weatherBounds, Vector3 volumeSize, Vector3 blendSize, float blendAmount, Vector3 anchorPosition, float heightMapWorldSize)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (_isInitialized)
		{
			return;
		}
		_weatherBounds = weatherBounds;
		_volumeSize = volumeSize;
		_blendSize = blendSize;
		_blendAmount = blendAmount;
		_anchorPosition = anchorPosition;
		_isInitialized = true;
		foreach (WeatherEffectController effectController in _effectControllers)
		{
			effectController.Initialise(this);
			effectController.Deactivate();
		}
		SetVisualEffectNumericParameter("HeightMapWorldSize", heightMapWorldSize);
	}

	private void RpcReader___Observers_Initialise_1999361799(PooledReader PooledReader0, Channel channel)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Vector3 weatherBounds = ((Reader)PooledReader0).ReadVector3();
		Vector3 volumeSize = ((Reader)PooledReader0).ReadVector3();
		Vector3 blendSize = ((Reader)PooledReader0).ReadVector3();
		float blendAmount = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		Vector3 anchorPosition = ((Reader)PooledReader0).ReadVector3();
		float heightMapWorldSize = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Initialise_1999361799(weatherBounds, volumeSize, blendSize, blendAmount, anchorPosition, heightMapWorldSize);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}
}
