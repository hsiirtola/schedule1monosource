using System;
using System.Collections.Generic;
using EPOOutline;
using FluffyUnderware.DevTools.Extensions;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.Map;
using ScheduleOne.Property;
using ScheduleOne.Vehicles;
using UnityEngine;

namespace ScheduleOne.Delivery;

public class LoadingDock : MonoBehaviour, IGUIDRegisterable, ITransitEntity
{
	[SerializeField]
	protected string BakedGUID = string.Empty;

	public ScheduleOne.Property.Property ParentProperty;

	public VehicleDetector VehicleDetector;

	public ParkingLot Parking;

	public Transform uiPoint;

	public Transform[] accessPoints;

	public GameObject[] OutlineRenderers;

	private Outlinable OutlineEffect;

	public LandVehicle DynamicOccupant { get; private set; }

	public LandVehicle StaticOccupant { get; private set; }

	public bool IsInUse
	{
		get
		{
			if (!((Object)(object)DynamicOccupant != (Object)null))
			{
				return (Object)(object)StaticOccupant != (Object)null;
			}
			return true;
		}
	}

	public Guid GUID { get; protected set; }

	public string Name => "Loading Dock " + (ArrayExt.IndexOf<LoadingDock>(ParentProperty.LoadingDocks, this) + 1);

	public List<ItemSlot> InputSlots { get; set; } = new List<ItemSlot>();

	public List<ItemSlot> OutputSlots { get; set; } = new List<ItemSlot>();

	public Transform LinkOrigin => uiPoint;

	public Transform[] AccessPoints => accessPoints;

	public bool Selectable { get; } = true;

	public bool IsAcceptingItems { get; set; }

	public bool IsDestroyed { get; set; }

	[Button]
	public void RegenerateGUID()
	{
		BakedGUID = Guid.NewGuid().ToString();
	}

	private void Awake()
	{
		GUID = new Guid(BakedGUID);
		GUIDManager.RegisterObject(this);
	}

	private void Start()
	{
		((MonoBehaviour)this).InvokeRepeating("RefreshOccupant", Random.Range(0f, 1f), 1f);
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	private void RefreshOccupant()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		LandVehicle closestVehicle = VehicleDetector.closestVehicle;
		if ((Object)(object)closestVehicle != (Object)null && closestVehicle.Speed_Kmh < 2f)
		{
			SetOccupant(VehicleDetector.closestVehicle);
		}
		else
		{
			SetOccupant(null);
		}
		if ((Object)(object)StaticOccupant != (Object)null && !StaticOccupant.IsVisible)
		{
			SetStaticOccupant(null);
		}
		if ((Object)(object)DynamicOccupant != (Object)null)
		{
			Vector3 position = ((Component)DynamicOccupant).transform.position - ((Component)DynamicOccupant).transform.forward * (DynamicOccupant.BoundingBoxDimensions.z / 2f + 0.6f);
			((Component)accessPoints[0]).transform.position = position;
			((Component)accessPoints[0]).transform.rotation = Quaternion.LookRotation(((Component)DynamicOccupant).transform.forward, Vector3.up);
			((Component)accessPoints[0]).transform.localPosition = new Vector3(((Component)accessPoints[0]).transform.localPosition.x, 0f, ((Component)accessPoints[0]).transform.localPosition.z);
		}
	}

	private void SetOccupant(LandVehicle occupant)
	{
		if (!((Object)(object)occupant == (Object)(object)DynamicOccupant))
		{
			Console.Log("Loading dock " + ((Object)this).name + " is " + (((Object)(object)occupant == (Object)null) ? "empty" : "occupied") + ".");
			DynamicOccupant = occupant;
			InputSlots.Clear();
			OutputSlots.Clear();
			if ((Object)(object)DynamicOccupant != (Object)null)
			{
				OutputSlots.AddRange(DynamicOccupant.Storage.ItemSlots);
			}
		}
	}

	public void SetStaticOccupant(LandVehicle vehicle)
	{
		StaticOccupant = vehicle;
	}

	public virtual void ShowOutline(Color color)
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		if ((Object)(object)OutlineEffect == (Object)null)
		{
			OutlineEffect = ((Component)this).gameObject.AddComponent<Outlinable>();
			OutlineEffect.OutlineParameters.BlurShift = 0f;
			OutlineEffect.OutlineParameters.DilateShift = 0.5f;
			OutlineEffect.OutlineParameters.FillPass.Shader = Resources.Load<Shader>("Easy performant outline/Shaders/Fills/ColorFill");
			GameObject[] outlineRenderers = OutlineRenderers;
			foreach (GameObject val in outlineRenderers)
			{
				MeshRenderer[] array = (MeshRenderer[])(object)new MeshRenderer[0];
				array = (MeshRenderer[])(object)new MeshRenderer[1] { val.GetComponent<MeshRenderer>() };
				for (int j = 0; j < array.Length; j++)
				{
					OutlineTarget val2 = new OutlineTarget((Renderer)(object)array[j], 0);
					OutlineEffect.TryAddTarget(val2);
				}
			}
		}
		OutlineEffect.OutlineParameters.Color = color;
		Color32 val3 = Color32.op_Implicit(color);
		val3.a = 9;
		OutlineEffect.OutlineParameters.FillPass.SetColor("_PublicColor", Color32.op_Implicit(val3));
		((Behaviour)OutlineEffect).enabled = true;
	}

	public virtual void HideOutline()
	{
		if ((Object)(object)OutlineEffect != (Object)null)
		{
			((Behaviour)OutlineEffect).enabled = false;
		}
	}
}
