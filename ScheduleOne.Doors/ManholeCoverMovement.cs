using UnityEngine;

namespace ScheduleOne.Doors;

public class ManholeCoverMovement : MonoBehaviour
{
	public Animation Anim;

	public void Open()
	{
		Anim["ManholeCover_Open"].speed = 1f;
		if (!Anim.isPlaying)
		{
			Anim[Anim["ManholeCover_Open"].name].time = 0f;
			Anim.Play("ManholeCover_Open");
		}
	}

	public void Close()
	{
		Anim["ManholeCover_Open"].speed = -1f;
		if (!Anim.isPlaying)
		{
			Anim[Anim["ManholeCover_Open"].name].time = Anim["ManholeCover_Open"].length;
			Anim.Play("ManholeCover_Open");
		}
	}
}
