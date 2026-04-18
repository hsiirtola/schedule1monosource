using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs.Other;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class SmokeBreakBehaviour : Behaviour
{
	[Header("Components")]
	public SmokeCigarette SmokeCigarette;

	[Header("Smoke Break Settings")]
	public Vector2Int MinMaxSmokeBreak;

	public float maxDistanceToSmokeLocation = 50f;

	[Header("Smoking Locations")]
	public List<Transform> SmokeBreakLocations;

	[Header("Debug")]
	[SerializeField]
	private bool _debugMode;

	[SerializeField]
	private int _ocationOverride = -1;

	[SerializeField]
	private bool _showMaxDistance = true;

	[SerializeField]
	private bool _showLocationGizmos = true;

	[SerializeField]
	private bool _showLookAtGizmos = true;

	private int _smokeBreakDuration;

	private Transform _currentSmokeLocation;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESmokeBreakBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESmokeBreakBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private void SetupEvents()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onHourPass = (Action)Delegate.Combine(instance.onHourPass, new Action(OnHourPass));
		TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
		instance2.onTimeSkip = (Action<int>)Delegate.Combine(instance2.onTimeSkip, new Action<int>(OnTimeSkipped));
	}

	private void CleanUp()
	{
		TimeManager instance = NetworkSingleton<TimeManager>.Instance;
		instance.onHourPass = (Action)Delegate.Remove(instance.onHourPass, new Action(OnHourPass));
		TimeManager instance2 = NetworkSingleton<TimeManager>.Instance;
		instance2.onTimeSkip = (Action<int>)Delegate.Remove(instance2.onTimeSkip, new Action<int>(OnTimeSkipped));
	}

	public override void Enable()
	{
		base.Enable();
		_smokeBreakDuration = Random.Range(((Vector2Int)(ref MinMaxSmokeBreak)).x, ((Vector2Int)(ref MinMaxSmokeBreak)).y);
		SetupEvents();
		if (_debugMode)
		{
			Debug.Log((object)$"[NPC Behaviour][Smoke Break] Smoke break enabled for {_smokeBreakDuration} hours");
		}
	}

	public override void Activate()
	{
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		base.Activate();
		if (_debugMode)
		{
			Debug.Log((object)$"[NPC Behaviour][Smoke Break] Smoke break activated, remaining duration: {_smokeBreakDuration} hours");
		}
		List<Transform> list = SmokeBreakLocations.Where((Transform loc) => Vector3.Distance(base.Npc.Movement.FootPosition, loc.position) <= maxDistanceToSmokeLocation).ToList();
		if (list.Count > 0)
		{
			int index = Random.Range(0, list.Count);
			_currentSmokeLocation = list[index];
		}
		else
		{
			if (_debugMode)
			{
				Debug.Log((object)$"[NPC Behaviour][Smoke Break] No smoke break locations within max distance ({maxDistanceToSmokeLocation}) for NPC {((Object)base.Npc).name}. Using closest location.");
			}
			float num = float.MaxValue;
			foreach (Transform smokeBreakLocation in SmokeBreakLocations)
			{
				float num2 = Vector3.Distance(base.Npc.Movement.FootPosition, smokeBreakLocation.position);
				if (num2 < num)
				{
					num = num2;
					_currentSmokeLocation = smokeBreakLocation;
				}
			}
		}
		if (_debugMode && _ocationOverride != -1)
		{
			Debug.Log((object)("[NPC Behaviour][Smoke Break] Location priority override active, using location index: " + _ocationOverride));
			int index2 = Mathf.Clamp(_ocationOverride, 0, SmokeBreakLocations.Count - 1);
			_currentSmokeLocation = SmokeBreakLocations[index2];
		}
		SetDestination(_currentSmokeLocation.position);
	}

	public override void Deactivate()
	{
		base.Deactivate();
		EndSmokeBreak();
		CleanUp();
	}

	private void BeginSmokeBreak()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		SmokeCigarette.Begin();
		Vector3 val = _currentSmokeLocation.GetChild(0).position - ((Component)base.Npc.Movement).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		normalized.y = 0f;
		base.Npc.Movement.FaceDirection(normalized);
		if (_debugMode)
		{
			Debug.Log((object)("[NPC Behaviour][Smoke Break] " + ((Object)base.Npc).name + " arrived at smoking location and has begun smoking"));
		}
	}

	private void EndSmokeBreak()
	{
		SmokeCigarette.End();
		if (_debugMode)
		{
			Debug.Log((object)("[NPC Behaviour][Smoke Break] Smoke break deactivated for " + ((Object)base.Npc).name + ", has stopped smoking"));
		}
	}

	private void CheckSmokeBreakEnd()
	{
		if (_smokeBreakDuration <= 0)
		{
			Disable();
		}
	}

	private void UpdateSmokeBreakDuration(int amount)
	{
		_smokeBreakDuration = Mathf.Max(0, _smokeBreakDuration + amount);
		CheckSmokeBreakEnd();
	}

	protected override void WalkCallback(NPCMovement.WalkResult result)
	{
		base.WalkCallback(result);
		if (result == NPCMovement.WalkResult.Success)
		{
			BeginSmokeBreak();
		}
	}

	private void OnTimeSkipped(int skippedTimeInMintues)
	{
		int num = Mathf.FloorToInt((float)(skippedTimeInMintues / 60));
		UpdateSmokeBreakDuration(-num);
	}

	private void OnHourPass()
	{
		UpdateSmokeBreakDuration(-1);
	}

	[Button]
	public void ChangeLocation()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		SmokeCigarette.End();
		int index = Mathf.Clamp(_ocationOverride, 0, SmokeBreakLocations.Count - 1);
		_currentSmokeLocation = SmokeBreakLocations[index];
		SetDestination(_currentSmokeLocation.position);
	}

	[Button]
	public void ActivateSmokeBreak()
	{
		Enable();
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESmokeBreakBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002ESmokeBreakBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESmokeBreakBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002ESmokeBreakBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
