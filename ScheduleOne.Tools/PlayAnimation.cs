using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.Tools;

public class PlayAnimation : MonoBehaviour
{
	[Button]
	public void Play()
	{
		((Component)this).GetComponent<Animation>().Play();
	}

	public void Play(string animationName)
	{
		((Component)this).GetComponent<Animation>().Play(animationName);
	}
}
