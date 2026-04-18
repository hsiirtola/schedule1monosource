using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.Packaging;

public class FunctionalJar : FunctionalPackaging
{
	[Header("References")]
	public Draggable Lid;

	public Transform LidStartPoint;

	public Collider LidSensor;

	public Collider LidCollider;

	public GameObject FullyPackedBlocker;

	private GameObject LidObject;

	private Vector3 lidPosition = Vector3.zero;

	public override CursorManager.ECursorType HoveredCursor { get; protected set; } = CursorManager.ECursorType.Finger;

	public override void Initialize(PackagingStation _station, Transform alignment, bool align = false)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		base.Initialize(_station, alignment, align);
		lidPosition = ((Component)this).transform.InverseTransformPoint(((Component)Lid).transform.position);
		LidObject = ((Component)Lid).gameObject;
		((Component)Lid).transform.SetParent(_station.Container);
		((Component)Lid).transform.position = LidStartPoint.position;
		((Component)Lid).transform.rotation = LidStartPoint.rotation;
		LidSensor.enabled = false;
	}

	public override void Destroy()
	{
		Object.Destroy((Object)(object)LidObject);
		base.Destroy();
	}

	protected override void EnableSealing()
	{
		base.EnableSealing();
		((Behaviour)Lid).enabled = true;
		Lid.ClickableEnabled = true;
		Lid.Rb.isKinematic = false;
		LidSensor.enabled = true;
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
	}

	protected override void OnTriggerStay(Collider other)
	{
		base.OnTriggerStay(other);
		if ((Object)(object)Lid != (Object)null && ((Behaviour)Lid).enabled && ((Object)((Component)other).gameObject).name == "LidTrigger")
		{
			Seal();
		}
	}

	public override void Seal()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		base.Seal();
		((Behaviour)Lid).enabled = false;
		Lid.ClickableEnabled = false;
		((Component)Lid).transform.SetParent(((Component)this).transform);
		Object.Destroy((Object)(object)Lid.Rb);
		Object.Destroy((Object)(object)Lid);
		Object.Destroy((Object)(object)LidCollider);
		((Component)Lid).transform.position = ((Component)this).transform.TransformPoint(lidPosition);
		LidSensor.enabled = false;
	}

	protected override void FullyPacked()
	{
		base.FullyPacked();
		FullyPackedBlocker.SetActive(true);
	}
}
