using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Product;

public class FunctionalProduct : Draggable
{
	public bool ClampZ = true;

	[Header("References")]
	public Transform AlignmentPoint;

	public ProductVisualsSetter Visuals;

	private Vector3 startLocalPos;

	private float lowestMaxZ = 500f;

	public SmoothedVelocityCalculator VelocityCalculator { get; private set; }

	public virtual void Initialize(PackagingStation station, ItemInstance item, Transform alignment, bool align = true)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (align)
		{
			AlignTo(alignment);
		}
		startLocalPos = ((Component)this).transform.localPosition;
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Task"));
		InitializeVisuals(item);
		base.Rb.collisionDetectionMode = (CollisionDetectionMode)2;
		if ((Object)(object)VelocityCalculator == (Object)null)
		{
			VelocityCalculator = ((Component)this).gameObject.AddComponent<SmoothedVelocityCalculator>();
			VelocityCalculator.MaxReasonableVelocity = 2f;
		}
	}

	public virtual void Initialize(ItemInstance item)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		startLocalPos = ((Component)this).transform.localPosition;
		LayerUtility.SetLayerRecursively(((Component)this).gameObject, LayerMask.NameToLayer("Task"));
		InitializeVisuals(item);
		base.Rb.collisionDetectionMode = (CollisionDetectionMode)2;
		if ((Object)(object)VelocityCalculator == (Object)null)
		{
			VelocityCalculator = ((Component)this).gameObject.AddComponent<SmoothedVelocityCalculator>();
			VelocityCalculator.MaxReasonableVelocity = 2f;
		}
	}

	public virtual void InitializeVisuals(ItemInstance item)
	{
		if (!(item is ProductItemInstance productInstance))
		{
			Console.LogError("Item instance is not a product instance!");
		}
		else
		{
			Visuals.ApplyVisuals(productInstance);
		}
	}

	public void AlignTo(Transform alignment)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.rotation = alignment.rotation * (Quaternion.Inverse(AlignmentPoint.rotation) * ((Component)this).transform.rotation);
		((Component)this).transform.position = alignment.position + (((Component)this).transform.position - AlignmentPoint.position);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (ClampZ)
		{
			Clamp();
		}
	}

	private void Clamp()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(Mathf.Abs(((Component)this).transform.localPosition.x / startLocalPos.x), 0f, 1f);
		float num2 = (lowestMaxZ = Mathf.Min(Mathf.Abs(startLocalPos.z) * num, lowestMaxZ));
		Vector3 val = ((Component)this).transform.parent.InverseTransformPoint(base.originalHitPoint);
		val.z = Mathf.Clamp(val.z, 0f - num2, num2);
		Vector3 val2 = ((Component)this).transform.parent.TransformPoint(val);
		SetOriginalHitPoint(val2);
	}
}
