using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.NPCs;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Vision;

public class EntityVisibility : NetworkBehaviour
{
	public const float MAX_VISIBLITY = 100f;

	public List<VisibilityAttribute> ActiveAttributes = new List<VisibilityAttribute>();

	[Header("Settings")]
	public LayerMask VisibilityCheckMask;

	[Header("References")]
	public Transform CentralVisibilityPoint;

	public List<Transform> VisibilityPoints = new List<Transform>();

	private VisibilityAttribute environmentalVisibility;

	private Dictionary<string, Coroutine> removalRoutinesDict = new Dictionary<string, Coroutine>();

	private Dictionary<string, float> maxPointsChangesByUniquenessCode = new Dictionary<string, float>();

	private List<RaycastHit> hits;

	private bool NetworkInitialize___EarlyScheduleOne_002EVision_002EEntityVisibilityAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EVision_002EEntityVisibilityAssembly_002DCSharp_002Edll_Excuted;

	public virtual float CurrentVisibility => CalculateVisibility();

	public virtual float Suspiciousness => 0f;

	public List<EntityVisualState> VisualStates { get; protected set; } = new List<EntityVisualState>();

	public Vector3 CenterPoint
	{
		get
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			if (VisibilityPoints.Count <= 0 || !((Object)(object)VisibilityPoints[0] != (Object)null))
			{
				return Vector3.zero;
			}
			return VisibilityPoints[0].position;
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EVision_002EEntityVisibility_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		if (((NetworkBehaviour)this).IsOwner)
		{
			environmentalVisibility = new VisibilityAttribute("Environmental Brightess", 0f);
		}
	}

	private float CalculateVisibility()
	{
		UpdateEnvironmentalVisibilityAttribute();
		maxPointsChangesByUniquenessCode.Clear();
		foreach (VisibilityAttribute activeAttribute in ActiveAttributes)
		{
			if (activeAttribute is UniqueVisibilityAttribute uniqueVisibilityAttribute)
			{
				maxPointsChangesByUniquenessCode.TryGetValue(uniqueVisibilityAttribute.uniquenessCode, out var value);
				maxPointsChangesByUniquenessCode[uniqueVisibilityAttribute.uniquenessCode] = Mathf.Max(value, uniqueVisibilityAttribute.pointsChange);
			}
		}
		float num = 0f;
		foreach (VisibilityAttribute activeAttribute2 in ActiveAttributes)
		{
			if (!(activeAttribute2 is UniqueVisibilityAttribute uniqueVisibilityAttribute2) || uniqueVisibilityAttribute2.pointsChange >= maxPointsChangesByUniquenessCode[uniqueVisibilityAttribute2.uniquenessCode])
			{
				num += activeAttribute2.pointsChange;
				num *= activeAttribute2.multiplier;
			}
		}
		return Mathf.Clamp(num, 0f, 100f);
	}

	public VisibilityAttribute GetAttribute(string name)
	{
		return ActiveAttributes.Find((VisibilityAttribute x) => x.name.ToLower() == name.ToLower());
	}

	private void UpdateEnvironmentalVisibilityAttribute()
	{
		if (environmentalVisibility != null)
		{
			environmentalVisibility.multiplier = Singleton<EnvironmentFX>.Instance.normalizedEnvironmentalBrightness;
		}
	}

	public float CalculateExposureToPoint(Vector3 point, float checkRange = 50f, NPC checkingNPC = null)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if ((Object)(object)this == (Object)null || (Object)(object)((Component)this).gameObject == (Object)null || (Object)(object)((Component)this).transform == (Object)null)
		{
			return 0f;
		}
		if (Vector3.Distance(point, ((Component)this).transform.position) > checkRange + 1f)
		{
			return 0f;
		}
		List<VisionObscurer> list = new List<VisionObscurer>();
		foreach (Transform visibilityPoint in VisibilityPoints)
		{
			float num2 = Vector3.Distance(point, visibilityPoint.position);
			if (num2 > checkRange)
			{
				continue;
			}
			Vector3 val = visibilityPoint.position - point;
			hits = Physics.RaycastAll(point, ((Vector3)(ref val)).normalized, Mathf.Min(checkRange, num2), LayerMask.op_Implicit(VisibilityCheckMask), (QueryTriggerInteraction)2).ToList();
			RaycastHit val2;
			for (int i = 0; i < hits.Count; i++)
			{
				val2 = hits[i];
				LandVehicle componentInParent = ((Component)((RaycastHit)(ref val2)).collider).GetComponentInParent<LandVehicle>();
				if ((Object)(object)checkingNPC != (Object)null && (Object)(object)componentInParent != (Object)null)
				{
					if ((Object)(object)checkingNPC.CurrentVehicle == (Object)(object)componentInParent)
					{
						hits.RemoveAt(i);
						i--;
					}
					continue;
				}
				if ((Object)(object)checkingNPC != (Object)null)
				{
					val2 = hits[i];
					if ((Object)(object)((Component)((RaycastHit)(ref val2)).collider).GetComponentInParent<NPC>() == (Object)(object)checkingNPC)
					{
						hits.RemoveAt(i);
						i--;
						continue;
					}
				}
				val2 = hits[i];
				if ((Object)(object)((Component)((RaycastHit)(ref val2)).collider).GetComponentInParent<EntityVisibility>() == (Object)(object)this)
				{
					hits.RemoveAt(i);
					i--;
					continue;
				}
				val2 = hits[i];
				VisionObscurer componentInParent2 = ((Component)((RaycastHit)(ref val2)).collider).GetComponentInParent<VisionObscurer>();
				if ((Object)(object)componentInParent2 != (Object)null)
				{
					if ((Object)(object)visibilityPoint == (Object)(object)CentralVisibilityPoint && !list.Contains(componentInParent2))
					{
						list.Add(componentInParent2);
					}
					hits.RemoveAt(i);
					i--;
				}
				else
				{
					val2 = hits[i];
					if (((RaycastHit)(ref val2)).collider.isTrigger)
					{
						hits.RemoveAt(i);
						i--;
					}
				}
			}
			if (hits.Count > 0)
			{
				val2 = hits[0];
				Debug.DrawRay(point, ((RaycastHit)(ref val2)).point - point, Color.red, 0.1f);
			}
			else
			{
				val = visibilityPoint.position - point;
				Debug.DrawRay(point, ((Vector3)(ref val)).normalized * num2, Color.green, 0.1f);
				num += 1f / (float)VisibilityPoints.Count;
			}
		}
		float num3 = 1f;
		for (int j = 0; j < list.Count; j++)
		{
			num3 *= 1f - list[j].ObscuranceAmount;
		}
		_ = 1f;
		return num * num3;
	}

	[ServerRpc(RunLocally = true)]
	public void ApplyState(string label, EVisualState state, float autoRemoveAfter = 0f)
	{
		RpcWriter___Server_ApplyState_2910447583(label, state, autoRemoveAfter);
		RpcLogic___ApplyState_2910447583(label, state, autoRemoveAfter);
	}

	[ServerRpc(RunLocally = true)]
	public void RemoveState(string label, float delay = 0f)
	{
		RpcWriter___Server_RemoveState_606697822(label, delay);
		RpcLogic___RemoveState_606697822(label, delay);
	}

	public EntityVisualState GetState(string label)
	{
		return VisualStates.Find((EntityVisualState x) => x.label == label);
	}

	public void ClearStates()
	{
		EntityVisualState[] array = VisualStates.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i].label == "Visible"))
			{
				RemoveState(array[i].label);
			}
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EVision_002EEntityVisibilityAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EVision_002EEntityVisibilityAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_ApplyState_2910447583));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_RemoveState_606697822));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EVision_002EEntityVisibilityAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EVision_002EEntityVisibilityAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_ApplyState_2910447583(string label, EVisualState state, float autoRemoveAfter = 0f)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteString(label);
			GeneratedWriters___Internal.Write___ScheduleOne_002EVision_002EEVisualStateFishNet_002ESerializing_002EGenerated((Writer)(object)writer, state);
			((Writer)writer).WriteSingle(autoRemoveAfter, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___ApplyState_2910447583(string label, EVisualState state, float autoRemoveAfter = 0f)
	{
		EntityVisualState entityVisualState = GetState(label);
		if (entityVisualState == null)
		{
			entityVisualState = new EntityVisualState();
			entityVisualState.label = label;
			VisualStates.Add(entityVisualState);
		}
		entityVisualState.state = state;
		if (removalRoutinesDict.ContainsKey(label))
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(removalRoutinesDict[label]);
			removalRoutinesDict.Remove(label);
		}
		if (autoRemoveAfter > 0f)
		{
			RemoveState(label, autoRemoveAfter);
		}
	}

	private void RpcReader___Server_ApplyState_2910447583(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string label = ((Reader)PooledReader0).ReadString();
		EVisualState state = GeneratedReaders___Internal.Read___ScheduleOne_002EVision_002EEVisualStateFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float autoRemoveAfter = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___ApplyState_2910447583(label, state, autoRemoveAfter);
		}
	}

	private void RpcWriter___Server_RemoveState_606697822(string label, float delay = 0f)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
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
		else if (!((NetworkBehaviour)this).IsOwner)
		{
			NetworkManager networkManager2 = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager2 == null)
			{
				networkManager2 = InstanceFinder.NetworkManager;
			}
			if (networkManager2 != null)
			{
				networkManager2.LogWarning("Cannot complete action because you are not the owner of this object. .");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because you are not the owner of this object. .");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((Writer)writer).WriteString(label);
			((Writer)writer).WriteSingle(delay, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___RemoveState_606697822(string label, float delay = 0f)
	{
		EntityVisualState newState = GetState(label);
		if (newState == null)
		{
			return;
		}
		if (delay > 0f)
		{
			if (removalRoutinesDict.ContainsKey(label))
			{
				((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(removalRoutinesDict[label]);
				removalRoutinesDict.Remove(label);
			}
			removalRoutinesDict.Add(label, ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(DelayedRemove()));
		}
		else
		{
			Destroy();
		}
		IEnumerator DelayedRemove()
		{
			yield return (object)new WaitForSeconds(delay);
			Destroy();
			removalRoutinesDict.Remove(label);
		}
		void Destroy()
		{
			if (newState.stateDestroyed != null)
			{
				newState.stateDestroyed();
			}
			VisualStates.Remove(newState);
		}
	}

	private void RpcReader___Server_RemoveState_606697822(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string label = ((Reader)PooledReader0).ReadString();
		float delay = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && ((NetworkBehaviour)this).OwnerMatches(conn) && !conn.IsLocalClient)
		{
			RpcLogic___RemoveState_606697822(label, delay);
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EVision_002EEntityVisibility_Assembly_002DCSharp_002Edll()
	{
	}
}
