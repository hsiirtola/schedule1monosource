using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Packaging;

public class FunctionalPackaging : Draggable
{
	[Header("Settings")]
	public string SealInstruction = "Seal packaging";

	public bool AutoEnableSealing = true;

	public float ProductContactTime = 0.1f;

	public float ProductContactMaxVelocity = 0.3f;

	[Header("References")]
	public PackagingDefinition Definition;

	public Transform AlignmentPoint;

	public Transform[] ProductAlignmentPoints;

	public AudioSourceController SealSound;

	protected List<FunctionalProduct> PackedProducts = new List<FunctionalProduct>();

	public Action onFullyPacked;

	public Action onSealed;

	public Action onReachOutput;

	private PackagingStation station;

	private Dictionary<FunctionalProduct, float> productContactTime = new Dictionary<FunctionalProduct, float>();

	private SmoothedVelocityCalculator VelocityCalculator;

	public bool IsSealed { get; protected set; }

	public bool IsFull { get; protected set; }

	public bool ReachedOutput { get; protected set; }

	public virtual void Initialize(PackagingStation _station, Transform alignment, bool align = true)
	{
		station = _station;
		if (align)
		{
			AlignTo(alignment);
		}
		ClickableEnabled = false;
		base.Rb.isKinematic = true;
		if ((Object)(object)VelocityCalculator == (Object)null)
		{
			VelocityCalculator = ((Component)this).gameObject.AddComponent<SmoothedVelocityCalculator>();
			VelocityCalculator.MaxReasonableVelocity = 2f;
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
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.rotation = alignment.rotation * (Quaternion.Inverse(AlignmentPoint.rotation) * ((Component)this).transform.rotation);
		Vector3 val = ((Component)this).transform.position - AlignmentPoint.position;
		((Component)this).transform.position = alignment.position + val;
		if ((Object)(object)base.Rb == (Object)null)
		{
			base.Rb = ((Component)this).GetComponent<Rigidbody>();
		}
		if ((Object)(object)base.Rb != (Object)null)
		{
			base.Rb.position = ((Component)this).transform.position;
			base.Rb.rotation = ((Component)this).transform.rotation;
		}
	}

	public virtual void Destroy()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (IsFull)
		{
			return;
		}
		foreach (FunctionalProduct item in productContactTime.Keys.ToList())
		{
			if (!((Object)(object)item.Rb == (Object)null) && productContactTime[item] > ProductContactTime && !PackedProducts.Contains(item) && !item.IsHeld)
			{
				PackProduct(item);
			}
		}
	}

	protected virtual void PackProduct(FunctionalProduct product)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		product.ClickableEnabled = false;
		product.ClampZ = false;
		Object.Destroy((Object)(object)product.Rb);
		((Component)product).transform.SetParent(((Component)this).transform);
		if (ProductAlignmentPoints.Length > PackedProducts.Count)
		{
			((Component)product).transform.position = ProductAlignmentPoints[PackedProducts.Count].position;
			((Component)product).transform.rotation = ProductAlignmentPoints[PackedProducts.Count].rotation;
		}
		PackedProducts.Add(product);
		if (PackedProducts.Count >= Definition.Quantity && !IsFull)
		{
			FullyPacked();
		}
	}

	protected virtual void FullyPacked()
	{
		IsFull = true;
		if (onFullyPacked != null)
		{
			onFullyPacked();
		}
		foreach (FunctionalProduct packedProduct in PackedProducts)
		{
			Object.Destroy((Object)(object)packedProduct.Rb);
		}
		if (AutoEnableSealing)
		{
			EnableSealing();
		}
	}

	protected virtual void OnTriggerStay(Collider other)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)station == (Object)null)
		{
			return;
		}
		FunctionalProduct componentInParent = ((Component)other).GetComponentInParent<FunctionalProduct>();
		if ((Object)(object)componentInParent != (Object)null && componentInParent.IsHeld)
		{
			return;
		}
		if ((Object)(object)componentInParent != (Object)null)
		{
			if (!productContactTime.ContainsKey(componentInParent))
			{
				productContactTime.Add(componentInParent, 0f);
			}
			Vector3 velocity = componentInParent.VelocityCalculator.Velocity;
			Vector3 velocity2 = VelocityCalculator.Velocity;
			Vector3 val = velocity - velocity2;
			Debug.DrawRay(((Component)componentInParent).transform.position, velocity, Color.red);
			Debug.DrawRay(((Component)this).transform.position, velocity2, Color.blue);
			if (((Vector3)(ref val)).magnitude < ProductContactMaxVelocity)
			{
				productContactTime[componentInParent] += Time.fixedDeltaTime;
			}
		}
		if (((Object)((Component)other).gameObject).name == ((Object)station.OutputCollider).name && !ReachedOutput && IsSealed && !base.IsHeld)
		{
			ReachedOutput = true;
			if (onReachOutput != null)
			{
				onReachOutput();
			}
		}
	}

	protected virtual void EnableSealing()
	{
		ClickableEnabled = true;
	}

	public virtual void Seal()
	{
		IsSealed = true;
		foreach (FunctionalProduct packedProduct in PackedProducts)
		{
			Collider[] componentsInChildren = ((Component)packedProduct).GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
		if ((Object)(object)SealSound != (Object)null)
		{
			SealSound.Play();
		}
		HoveredCursor = CursorManager.ECursorType.OpenHand;
		ClickableEnabled = true;
		base.Rb.isKinematic = false;
		if (onSealed != null)
		{
			onSealed();
		}
	}
}
