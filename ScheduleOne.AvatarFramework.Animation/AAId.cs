using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Animation;

public static class AAId
{
	public static readonly int DIRECTION = Animator.StringToHash("Direction");

	public static readonly int STRAFE = Animator.StringToHash("Strafe");

	public static readonly int TIME_AIRBORNE = Animator.StringToHash("TimeAirborne");

	public static readonly int IS_CROUCHED = Animator.StringToHash("isCrouched");

	public static readonly int IS_GROUNDED = Animator.StringToHash("isGrounded");

	public static readonly int JUMP = Animator.StringToHash("Jump");

	public static readonly int FLINCH_FORWARD = Animator.StringToHash("Flinch_Forward");

	public static readonly int FLINCH_BACKWARD = Animator.StringToHash("Flinch_Backward");

	public static readonly int FLINCH_LEFT = Animator.StringToHash("Flinch_Left");

	public static readonly int FLINCH_RIGHT = Animator.StringToHash("Flinch_Right");

	public static readonly int FLINCH_HEAVY_FORWARD = Animator.StringToHash("Flinch_Heavy_Forward");

	public static readonly int FLINCH_HEAVY_BACKWARD = Animator.StringToHash("Flinch_Heavy_Backward");

	public static readonly int FLINCH_HEAVY_LEFT = Animator.StringToHash("Flinch_Heavy_Left");

	public static readonly int FLINCH_HEAVY_RIGHT = Animator.StringToHash("Flinch_Heavy_Right");

	public static readonly int STANDUP_BACK = Animator.StringToHash("StandUp_Back");

	public static readonly int STANDUP_FRONT = Animator.StringToHash("StandUp_Front");

	public static readonly int SITTING = Animator.StringToHash("Sitting");

	private static Dictionary<string, int> s_CustomHashes;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		s_CustomHashes = new Dictionary<string, int>();
	}

	public static int Get(string id)
	{
		if (!s_CustomHashes.TryGetValue(id, out var value))
		{
			return s_CustomHashes[id] = Animator.StringToHash(id);
		}
		return value;
	}
}
