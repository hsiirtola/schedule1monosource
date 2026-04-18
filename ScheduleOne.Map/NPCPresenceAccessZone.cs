using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.NPCs;
using UnityEngine;

namespace ScheduleOne.Map;

public class NPCPresenceAccessZone : AccessZone
{
	public const float CooldownTime = 0.5f;

	public Collider DetectionZone;

	public NPC TargetNPC;

	private float timeSinceNPCSensed = float.MaxValue;

	protected override void Awake()
	{
		base.Awake();
	}

	protected virtual void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
	}

	protected virtual void MinPass()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)TargetNPC == (Object)null))
		{
			Bounds bounds = DetectionZone.bounds;
			SetIsOpen(((Bounds)(ref bounds)).Contains(TargetNPC.Avatar.CenterPoint));
		}
	}
}
