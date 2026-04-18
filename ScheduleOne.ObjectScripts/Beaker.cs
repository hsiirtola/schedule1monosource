using ScheduleOne.PlayerTasks;
using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.ObjectScripts;

public class Beaker : StationItem
{
	public float ClampAngle_MaxLiquid = 50f;

	public float ClampAngle_MinLiquid = 100f;

	public float AngleToPour_MaxLiquid = 95f;

	public float AngleToPour_MinLiquid = 140f;

	[Header("References")]
	public Draggable Draggable;

	public DraggableConstraint Constraint;

	public Collider ConcaveCollider;

	public Collider ConvexCollider;

	public Transform CenterOfMass;

	public ConfigurableJoint Joint;

	public Rigidbody Anchor;

	public LiquidContainer Container;

	public Fillable Fillable;

	public PourableModule Pourable;

	public GameObject FilterPaper;

	private void Start()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		((Joint)Joint).connectedBody = Anchor;
		Draggable.Rb.centerOfMass = ((Component)Draggable.Rb).transform.InverseTransformPoint(CenterOfMass.position);
	}

	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		SoftJointLimit angularZLimit = Joint.angularZLimit;
		((SoftJointLimit)(ref angularZLimit)).limit = Mathf.Lerp(ClampAngle_MinLiquid, ClampAngle_MaxLiquid, Container.CurrentLiquidLevel);
		Joint.angularZLimit = angularZLimit;
		Pourable.AngleFromUpToPour = Mathf.Lerp(AngleToPour_MinLiquid, AngleToPour_MaxLiquid, Container.CurrentLiquidLevel);
	}

	public void SetStatic(bool stat)
	{
		Draggable.ClickableEnabled = !stat;
		ConvexCollider.enabled = !stat;
		ConcaveCollider.enabled = stat;
		Draggable.Rb.isKinematic = stat;
	}
}
