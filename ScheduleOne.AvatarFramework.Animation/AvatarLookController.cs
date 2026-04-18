using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using RootMotion.FinalIK;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework.Animation;

public class AvatarLookController : MonoBehaviour
{
	public const float LookAtPlayerRange = 4f;

	public const float EyeContractRange = 10f;

	public static Transform TempContainer;

	public bool DEBUG;

	[Header("References")]
	public AimIK Aim;

	public Transform HeadBone;

	public Transform LookForwardTarget;

	public Transform LookOrigin;

	public EyeController Eyes;

	[Header("Optional NPC reference")]
	public NPC NPC;

	[Header("Settings")]
	public bool AutoLookAtPlayer = true;

	public float LookLerpSpeed = 1f;

	public float AimIKWeight = 0.6f;

	public float BodyRotationSpeed = 1f;

	private Avatar avatar;

	private Vector3 lookAtPos = Vector3.zero;

	private Transform lookAtTarget;

	private Vector3 lastFrameOffset = Vector3.zero;

	private bool overrideLookAt;

	private Vector3 overriddenLookTarget = Vector3.zero;

	private int overrideLookPriority;

	private bool overrideRotateBody;

	private bool blockLookOverrides;

	private Vector3 lastFrameLookOriginPos;

	private Vector3 lastFrameLookOriginForward;

	public Transform ForceLookTarget;

	public bool ForceLookRotateBody;

	private float defaultIKWeight = 0.6f;

	private Player nearestPlayer;

	private float nearestPlayerDist;

	private float localPlayerDist;

	private float cullRange = 100f;

	public float BodyRotationSpeedMultiplier { get; set; } = 1f;

	private void Awake()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		avatar = ((Component)this).GetComponent<Avatar>();
		avatar.onRagdollChange.AddListener((UnityAction<bool, bool, bool>)RagdollChange);
		defaultIKWeight = ((IKSolver)Aim.solver).GetIKPositionWeight();
		lookAtTarget = new GameObject("LookAtTarget (" + ((Object)((Component)this).gameObject).name + ")").transform;
		if ((Object)(object)TempContainer == (Object)null)
		{
			GameObject obj = GameObject.Find("_Temp");
			TempContainer = ((obj != null) ? obj.transform : null);
		}
		lookAtTarget.SetParent(TempContainer);
		LookForward();
		((Component)lookAtTarget).transform.position = lookAtPos;
		lastFrameOffset = LookOrigin.InverseTransformPoint(lookAtTarget.position);
		NPC = ((Component)this).GetComponentInParent<NPC>();
		((MonoBehaviour)this).InvokeRepeating("UpdateNearestPlayer", 0f, 0.5f);
	}

	private void UpdateLook()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ForceLookTarget != (Object)null && CanLookAt(ForceLookTarget.position))
		{
			OverrideLookTarget(ForceLookTarget.position, 100, ForceLookRotateBody);
			return;
		}
		if (AutoLookAtPlayer && (Object)(object)Player.Local != (Object)null && (Player.Local.Paranoid || Player.Local.Schizophrenic))
		{
			OverrideLookTarget(Player.Local.MimicCamera.position, 200);
			((Behaviour)Aim).enabled = !avatar.Animation.IsAvatarCulled;
			Aim.solver.clampWeight = Mathf.MoveTowards(Aim.solver.clampWeight, AimIKWeight, Time.deltaTime * 2f);
			return;
		}
		if (DEBUG)
		{
			Console.Log("Nearest player: " + (((Object)(object)nearestPlayer != (Object)null) ? ((Object)nearestPlayer).name : "null") + " dist: " + nearestPlayerDist);
			Console.Log("Visibility: " + NPC.Awareness.VisionCone.GetPlayerVisibility(nearestPlayer));
			Console.Log("AutoLookAtPlayer: " + AutoLookAtPlayer);
			Console.Log("CanLookAt: " + CanLookAt(nearestPlayer.EyePosition));
			Console.Log("NearestPlayerDist: " + nearestPlayerDist);
		}
		if ((Object)(object)nearestPlayer != (Object)null && AutoLookAtPlayer && CanLookAt(nearestPlayer.EyePosition) && ((Object)(object)NPC == (Object)null || NPC.Awareness.VisionCone.GetPlayerVisibility(nearestPlayer) > 0.075f))
		{
			Vector3 val = nearestPlayer.EyePosition;
			if (((NetworkBehaviour)nearestPlayer).IsOwner)
			{
				val = nearestPlayer.MimicCamera.position;
			}
			if (nearestPlayerDist < 4f)
			{
				lookAtPos = val;
				if (DEBUG)
				{
					Console.Log("Looking at player: " + ((Object)nearestPlayer).name);
				}
			}
			else if (nearestPlayerDist < 10f && Vector3.Angle(val - HeadBone.position, HeadBone.forward) < 45f)
			{
				Transform mimicCamera = nearestPlayer.MimicCamera;
				Vector3 forward = mimicCamera.forward;
				Vector3 val2 = HeadBone.position - mimicCamera.position;
				if (Vector3.Angle(forward, ((Vector3)(ref val2)).normalized) < 15f)
				{
					lookAtPos = val;
					if (DEBUG)
					{
						Console.Log("Looking at player: " + ((Object)nearestPlayer).name);
					}
				}
				else
				{
					LookForward();
				}
			}
			else
			{
				LookForward();
			}
		}
		else
		{
			LookForward();
		}
		if (!((Object)(object)Aim != (Object)null))
		{
			return;
		}
		if (avatar.Ragdolled || avatar.Animation.StandUpAnimationPlaying)
		{
			Aim.solver.clampWeight = 0f;
			((Behaviour)Aim).enabled = false;
			if (DEBUG)
			{
				Console.Log("Aim disabled " + avatar.Ragdolled + " " + avatar.Animation.StandUpAnimationPlaying);
			}
		}
		else
		{
			((Behaviour)Aim).enabled = !avatar.Animation.IsAvatarCulled;
			Aim.solver.clampWeight = Mathf.MoveTowards(Aim.solver.clampWeight, AimIKWeight, Time.deltaTime * 2f);
		}
	}

	private void UpdateNearestPlayer()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.Local == (Object)null)
		{
			return;
		}
		localPlayerDist = Vector3.Distance(Player.Local.Avatar.CenterPoint, ((Component)this).transform.position);
		if (localPlayerDist > cullRange)
		{
			return;
		}
		List<Player> list = new List<Player>();
		foreach (Player player in Player.PlayerList)
		{
			if ((Object)(object)player.Avatar.LookController == (Object)(object)this)
			{
				list.Add(player);
			}
		}
		nearestPlayer = Player.GetClosestPlayer(((Component)this).transform.position, out nearestPlayerDist, list);
	}

	private void LateUpdate()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		if (localPlayerDist > cullRange)
		{
			if ((Object)(object)Aim != (Object)null && ((Behaviour)Aim).enabled)
			{
				((Behaviour)Aim).enabled = false;
				if (DEBUG)
				{
					Console.Log("Aim disabled");
				}
			}
			lastFrameLookOriginPos = LookOrigin.position;
			lastFrameLookOriginForward = LookOrigin.forward;
			return;
		}
		UpdateLook();
		if (blockLookOverrides)
		{
			overrideLookAt = false;
		}
		if (overrideLookAt)
		{
			lookAtPos = overriddenLookTarget;
		}
		if (!avatar.Ragdolled)
		{
			if (overrideLookAt && overrideRotateBody)
			{
				Vector3 val = lookAtPos - ((Component)this).transform.position;
				val.y = 0f;
				((Vector3)(ref val)).Normalize();
				float num = Vector3.SignedAngle(((Component)this).transform.parent.forward, val, Vector3.up);
				if (DEBUG)
				{
					Console.Log("Body rotation: " + num);
				}
				((Component)avatar).transform.localRotation = Quaternion.Lerp(((Component)avatar).transform.localRotation, Quaternion.Euler(0f, num, 0f), Time.deltaTime * BodyRotationSpeed * BodyRotationSpeedMultiplier);
			}
			else if ((Object)(object)((Component)avatar).transform.parent != (Object)null)
			{
				((Component)avatar).transform.localRotation = Quaternion.Lerp(((Component)avatar).transform.localRotation, Quaternion.identity, Time.deltaTime * BodyRotationSpeed * BodyRotationSpeedMultiplier);
			}
		}
		LerpTargetTransform();
		Eyes.LookAt(lookAtPos);
		overrideLookAt = false;
		overriddenLookTarget = Vector3.zero;
		overrideLookPriority = 0;
		overrideRotateBody = false;
		blockLookOverrides = false;
		lastFrameLookOriginPos = LookOrigin.position;
		lastFrameLookOriginForward = LookOrigin.forward;
	}

	public unsafe void OverrideLookTarget(Vector3 targetPosition, int priority, bool rotateBody = false)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!overrideLookAt || priority >= overrideLookPriority)
		{
			if (DEBUG)
			{
				Debug.DrawLine(((Component)this).transform.position, targetPosition, Color.red, 0.1f);
				Vector3 val = targetPosition;
				Console.Log("Overriding look target to: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString() + " priority: " + priority);
			}
			overrideLookAt = true;
			overriddenLookTarget = targetPosition;
			overrideLookPriority = priority;
			overrideRotateBody = rotateBody;
		}
	}

	public void BlockLookTargetOverrides()
	{
		blockLookOverrides = true;
	}

	private void LookForward()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (DEBUG)
		{
			Console.Log("Looking forward");
		}
		LookForwardTarget.position = HeadBone.position + ((Component)this).transform.forward * 1f;
		lookAtPos = LookForwardTarget.position;
	}

	private void LerpTargetTransform()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		lookAtTarget.position = LookOrigin.TransformPoint(lastFrameOffset);
		Vector3 val = lookAtTarget.position - LookOrigin.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		val = lookAtPos - LookOrigin.position;
		Vector3 normalized2 = ((Vector3)(ref val)).normalized;
		Vector3 val2 = Vector3.Lerp(normalized, normalized2, Time.deltaTime * LookLerpSpeed);
		lookAtTarget.position = LookOrigin.position + val2;
		if ((Object)(object)Aim != (Object)null)
		{
			((IKSolverHeuristic)Aim.solver).target = lookAtTarget;
		}
		lastFrameOffset = LookOrigin.InverseTransformPoint(lookAtTarget.position);
	}

	private Player GetNearestPlayer()
	{
		List<Player> playerList = Player.PlayerList;
		if (playerList.Count <= 0)
		{
			return null;
		}
		return playerList.OrderBy((Player p) => Vector3.Distance(((Component)p).transform.position, ((Component)this).transform.position)).First();
	}

	private bool CanLookAt(Vector3 position)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)avatar).transform.forward;
		Vector3 val = position - ((Component)avatar).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		return Vector3.SignedAngle(forward, normalized, Vector3.up) < 90f;
	}

	protected void RagdollChange(bool oldValue, bool ragdoll, bool playStandUpAnim)
	{
	}

	public void OverrideIKWeight(float weight)
	{
		((IKSolver)Aim.solver).SetIKPositionWeight(weight);
	}

	public void ResetIKWeight()
	{
		((IKSolver)Aim.solver).SetIKPositionWeight(defaultIKWeight);
	}
}
