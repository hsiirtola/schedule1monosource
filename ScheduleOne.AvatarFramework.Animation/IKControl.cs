using UnityEngine;

namespace ScheduleOne.AvatarFramework.Animation;

[RequireComponent(typeof(Animator))]
public class IKControl : MonoBehaviour
{
	protected Animator animator;

	public bool ikActive;

	public Transform rightHandObj;

	public Transform lookObj;

	private void Start()
	{
		animator = ((Component)this).GetComponent<Animator>();
	}

	private void OnAnimatorIK()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)animator))
		{
			return;
		}
		if (ikActive)
		{
			if ((Object)(object)lookObj != (Object)null)
			{
				animator.SetLookAtWeight(1f);
				animator.SetLookAtPosition(lookObj.position);
			}
			if ((Object)(object)rightHandObj != (Object)null)
			{
				animator.SetIKPositionWeight((AvatarIKGoal)3, 1f);
				animator.SetIKRotationWeight((AvatarIKGoal)3, 1f);
				animator.SetIKPosition((AvatarIKGoal)3, rightHandObj.position);
				animator.SetIKRotation((AvatarIKGoal)3, rightHandObj.rotation);
			}
		}
		else
		{
			animator.SetIKPositionWeight((AvatarIKGoal)3, 0f);
			animator.SetIKRotationWeight((AvatarIKGoal)3, 0f);
			animator.SetLookAtWeight(0f);
		}
	}
}
