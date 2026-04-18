using UnityEngine;

namespace ScheduleOne.AvatarFramework.Equipping;

[RequireComponent(typeof(AvatarEquippable))]
public class AvatarEquippableLookAt : MonoBehaviour
{
	public int Priority;

	private Avatar avatar;

	private void Start()
	{
		avatar = ((Component)this).GetComponentInParent<Avatar>();
		if ((Object)(object)avatar == (Object)null)
		{
			Debug.LogError((object)"AvatarEquippableLookAt must be a child of an Avatar object.");
		}
	}

	private void LateUpdate()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)avatar == (Object)null))
		{
			avatar.LookController.OverrideLookTarget(((Component)avatar.CurrentEquippable).transform.position, Priority);
		}
	}
}
