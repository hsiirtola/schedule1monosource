using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.PlayerTasks.Tasks;

public class PourOntoTargetTask : GrowContainerPourTask
{
	public float SUCCESS_THRESHOLD = 0.12f;

	public float SUCCESS_TIME = 0.4f;

	private float timeOverTarget;

	public PourOntoTargetTask(GrowContainer _growContainer, ItemInstance _itemInstance, Pourable _pourablePrefab)
		: base(_growContainer, _itemInstance, _pourablePrefab)
	{
		_growContainer.RandomizePourTargetPosition();
		_growContainer.SetPourTargetActive(active: true);
	}

	public override void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		Vector3 val = pourable.PourPoint.position - growContainer.GetCurrentTargetPosition();
		val.y = 0f;
		if (((Vector3)(ref val)).magnitude < SUCCESS_THRESHOLD)
		{
			timeOverTarget += Time.deltaTime * pourable.NormalizedPourRate;
			if (timeOverTarget >= SUCCESS_TIME)
			{
				TargetReached();
			}
		}
		else
		{
			timeOverTarget = 0f;
		}
	}

	public override void StopTask()
	{
		growContainer.SetPourTargetActive(active: false);
		base.StopTask();
	}

	public virtual void TargetReached()
	{
		growContainer.RandomizePourTargetPosition();
		timeOverTarget = 0f;
		Singleton<TaskManager>.Instance.PlayTaskCompleteSound();
	}
}
