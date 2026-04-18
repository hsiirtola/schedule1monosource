using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class Moveable : Clickable
{
	protected Vector3 clickOffset = Vector3.zero;

	protected float clickDist;

	[Header("Bounds")]
	[SerializeField]
	protected float yMax = 10f;

	[SerializeField]
	protected float yMin = -10f;

	public override void StartClick(RaycastHit hit)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		base.StartClick(hit);
		clickDist = Vector3.Distance(((Component)this).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
		clickOffset = ((Component)this).transform.position - PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenToWorldPoint(new Vector3(GameInput.MousePosition.x, GameInput.MousePosition.y, clickDist));
	}

	protected virtual void Update()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (base.IsHeld)
		{
			((Component)this).transform.position = Vector3.Lerp(((Component)this).transform.position, PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenToWorldPoint(new Vector3(GameInput.MousePosition.x, GameInput.MousePosition.y, clickDist)) + clickOffset, Time.deltaTime * 10f);
			((Component)this).transform.localPosition = new Vector3(((Component)this).transform.localPosition.x, Mathf.Clamp(((Component)this).transform.localPosition.y, yMin, yMax), ((Component)this).transform.localPosition.z);
		}
	}

	public override void EndClick()
	{
		base.EndClick();
	}
}
