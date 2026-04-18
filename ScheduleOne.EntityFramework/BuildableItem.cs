using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EPOOutline;
using FishNet;
using FishNet.Component.Ownership;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Building;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.EntityFramework;

[RequireComponent(typeof(PredictedSpawn))]
public abstract class BuildableItem : NetworkBehaviour, IGUIDRegisterable, ISaveable
{
	public enum EOutlineColor
	{
		White,
		Blue,
		LightBlue
	}

	[HideInInspector]
	public bool isGhost;

	[Header("Build Settings")]
	[SerializeField]
	protected GameObject buildHandler;

	public float HoldDistance = 2.5f;

	public Transform BuildPoint;

	public Transform MidAirCenterPoint;

	public BoxCollider BoundingCollider;

	[Header("Outline settings")]
	[SerializeField]
	protected List<GameObject> OutlineRenderers = new List<GameObject>();

	[SerializeField]
	protected bool IncludeOutlineRendererChildren = true;

	protected Outlinable OutlineEffect;

	[Header("Culling Settings")]
	public GameObject[] GameObjectsToCull;

	public List<MeshRenderer> MeshesToCull;

	[Header("Buildable Events")]
	public UnityEvent onGhostModel;

	public UnityEvent onInitialized;

	public UnityEvent onDestroyed;

	public Action<BuildableItem> onDestroyedWithParameter;

	private bool NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted;

	public ItemInstance ItemInstance { get; protected set; }

	public ScheduleOne.Property.Property ParentProperty { get; protected set; }

	public bool IsDestroyed { get; protected set; }

	public bool Initialized { get; protected set; }

	public Guid GUID { get; protected set; }

	public bool IsCulled { get; protected set; }

	public GameObject BuildHandler => buildHandler;

	protected bool _locallyBuilt { get; set; }

	public string SaveFolderName => ((BaseItemInstance)ItemInstance).ID + "_" + GUID.ToString().Substring(0, 6);

	public string SaveFileName => "Data";

	public Loader Loader => null;

	public bool ShouldSaveUnderFolder => true;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; }

	public void SetLocallyBuilt()
	{
		_locallyBuilt = true;
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EEntityFramework_002EBuildableItem_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected virtual void Start()
	{
		if (!isGhost)
		{
			InitializeSaveable();
			if (GUID == Guid.Empty)
			{
				GUID = GUIDManager.GenerateUniqueGUID();
				GUIDManager.RegisterObject(this);
			}
			ActivateDuringBuild[] componentsInChildren = ((Component)((Component)this).transform).GetComponentsInChildren<ActivateDuringBuild>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Component)componentsInChildren[i]).gameObject.SetActive(false);
			}
		}
	}

	protected virtual ScheduleOne.Property.Property GetProperty(Transform searchTransform = null)
	{
		if ((Object)(object)searchTransform == (Object)null)
		{
			searchTransform = ((Component)this).transform;
		}
		PropertyContentsContainer componentInParent = ((Component)searchTransform).GetComponentInParent<PropertyContentsContainer>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			return componentInParent.Property;
		}
		return ((Component)searchTransform).GetComponentInParent<ScheduleOne.Property.Property>();
	}

	public virtual string GetManagementName()
	{
		return GetDefaultManagementName();
	}

	public virtual string GetDefaultManagementName()
	{
		if (ItemInstance != null)
		{
			return ((BaseItemInstance)ItemInstance).Name;
		}
		return "Unknown";
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsLocalClient && Initialized)
		{
			SendInitializationToClient(connection);
		}
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		if (Initialized && _locallyBuilt)
		{
			((MonoBehaviour)this).StartCoroutine(WaitForDataSend());
		}
		IEnumerator WaitForDataSend()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => ((NetworkBehaviour)this).NetworkObject.IsSpawned));
			SendInitializationToServer();
		}
	}

	protected abstract void SendInitializationToClient(NetworkConnection conn);

	protected abstract void SendInitializationToServer();

	protected void InitializeBuildableItem(ItemInstance instance, string GUID, string parentPropertyCode)
	{
		if (Initialized)
		{
			return;
		}
		if (instance == null)
		{
			Console.LogError("InitializeBuildItem: passed null instance");
		}
		if (((BaseItemInstance)instance).Quantity != 1)
		{
			Console.LogWarning("BuiltadlbeItem initialized with quantity '" + ((BaseItemInstance)instance).Quantity + "'! This should be 1.");
		}
		Initialized = true;
		ItemInstance = instance;
		SetGUID(new Guid(GUID));
		ParentProperty = ScheduleOne.Property.Property.Properties.FirstOrDefault((ScheduleOne.Property.Property p) => p.PropertyCode == parentPropertyCode);
		if ((Object)(object)ParentProperty == (Object)null)
		{
			ParentProperty = Business.Businesses.FirstOrDefault((Business b) => b.PropertyCode == parentPropertyCode);
		}
		if ((Object)(object)ParentProperty != (Object)null)
		{
			ParentProperty.AddBuildableItem(this);
			if (ParentProperty.IsContentCulled)
			{
				SetCulled(culled: true);
			}
		}
		else
		{
			Console.LogError("BuildableItem '" + ((Object)((Component)this).gameObject).name + "' does not have a parent Property!");
		}
		ActivateDuringBuild[] componentsInChildren = ((Component)((Component)this).transform).GetComponentsInChildren<ActivateDuringBuild>();
		for (int num = 0; num < componentsInChildren.Length; num++)
		{
			((Component)componentsInChildren[num]).gameObject.SetActive(false);
		}
		if (onInitialized != null)
		{
			onInitialized.Invoke();
		}
	}

	public bool CanBePickedUp(out string reason)
	{
		if (PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(ItemInstance))
		{
			return CanBeDestroyed(out reason);
		}
		reason = "Won't fit in inventory";
		return false;
	}

	public virtual bool CanBeDestroyed(out string reason)
	{
		reason = string.Empty;
		return true;
	}

	public void PickupItem()
	{
		string reason = string.Empty;
		if (!CanBePickedUp(out reason))
		{
			Console.LogWarning("Item can not be picked up!");
			return;
		}
		PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(ItemInstance);
		Destroy_Server();
	}

	protected virtual void Destroy()
	{
		if (!IsDestroyed)
		{
			IsDestroyed = true;
			if ((Object)(object)ParentProperty != (Object)null)
			{
				ParentProperty.RemoveBuildableItem(this);
			}
			if (onDestroyed != null)
			{
				onDestroyed.Invoke();
			}
			if (onDestroyedWithParameter != null)
			{
				onDestroyedWithParameter(this);
			}
			((Component)this).gameObject.SetActive(false);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void Destroy_Server()
	{
		RpcWriter___Server_Destroy_Server_2166136261();
		RpcLogic___Destroy_Server_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void Destroy_Client()
	{
		RpcWriter___Observers_Destroy_Client_2166136261();
		RpcLogic___Destroy_Client_2166136261();
	}

	public void SetGUID(Guid guid)
	{
		GUID = guid;
		GUIDManager.RegisterObject(this);
	}

	private static Color32 GetColorFromOutlineColorEnum(EOutlineColor col)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		return (Color32)(col switch
		{
			EOutlineColor.White => Color32.op_Implicit(Color.white), 
			EOutlineColor.Blue => new Color32((byte)0, (byte)200, byte.MaxValue, byte.MaxValue), 
			EOutlineColor.LightBlue => new Color32((byte)120, (byte)225, byte.MaxValue, byte.MaxValue), 
			_ => Color32.op_Implicit(Color.white), 
		});
	}

	public virtual void ShowOutline(Color color)
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		if (IsDestroyed || (Object)(object)((Component)this).gameObject == (Object)null)
		{
			return;
		}
		if ((Object)(object)OutlineEffect == (Object)null)
		{
			OutlineEffect = ((Component)this).gameObject.AddComponent<Outlinable>();
			OutlineEffect.OutlineParameters.BlurShift = 0f;
			OutlineEffect.OutlineParameters.DilateShift = 0.5f;
			OutlineEffect.OutlineParameters.FillPass.Shader = Resources.Load<Shader>("Easy performant outline/Shaders/Fills/ColorFill");
			foreach (GameObject outlineRenderer in OutlineRenderers)
			{
				MeshRenderer[] array = (MeshRenderer[])(object)new MeshRenderer[0];
				array = (MeshRenderer[])((!IncludeOutlineRendererChildren) ? ((Array)new MeshRenderer[1] { outlineRenderer.GetComponent<MeshRenderer>() }) : ((Array)outlineRenderer.GetComponentsInChildren<MeshRenderer>()));
				for (int i = 0; i < array.Length; i++)
				{
					((Renderer)array[i]).allowOcclusionWhenDynamic = false;
					OutlineTarget val = new OutlineTarget((Renderer)(object)array[i], 0);
					OutlineEffect.TryAddTarget(val);
				}
			}
		}
		OutlineEffect.OutlineParameters.Color = color;
		Color32 val2 = Color32.op_Implicit(color);
		val2.a = 9;
		OutlineEffect.OutlineParameters.FillPass.SetColor("_PublicColor", Color32.op_Implicit(val2));
		((Behaviour)OutlineEffect).enabled = true;
	}

	public void ShowOutline(EOutlineColor color)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		ShowOutline(Color32.op_Implicit(GetColorFromOutlineColorEnum(color)));
	}

	public virtual void HideOutline()
	{
		if (!IsDestroyed && !((Object)(object)((Component)this).gameObject == (Object)null) && (Object)(object)OutlineEffect != (Object)null)
		{
			((Behaviour)OutlineEffect).enabled = false;
		}
	}

	public bool GetPenetration(out float x, out float z, out float y)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0497: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0513: Unknown result type (might be due to invalid IL or missing references)
		//IL_0518: Unknown result type (might be due to invalid IL or missing references)
		//IL_0468: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0482: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_0529: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)BoundingCollider).transform.TransformPoint(BoundingCollider.center);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(BoundingCollider.size.x * ((Component)BoundingCollider).transform.localScale.x, BoundingCollider.size.y * ((Component)BoundingCollider).transform.localScale.y, BoundingCollider.size.z * ((Component)BoundingCollider).transform.localScale.z);
		float num = val2.x / 2f;
		float num2 = 0f;
		x = 0f;
		z = 0f;
		y = 0f;
		Vector3 val3 = val - ((Component)this).transform.right * num;
		if (HasLoS_IgnoreBuildables(val3) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(val3, ((Component)this).transform.right, val2.x / 2f + num - num2, out var hit, LayerMask.op_Implicit(1 << LayerMask.NameToLayer("Default")), includeTriggers: false, num2, 45f) && Vector3.Angle(((Component)this).transform.right, -((RaycastHit)(ref hit)).normal) < 5f)
		{
			x = val2.x - Vector3.Distance(val3, ((RaycastHit)(ref hit)).point);
			Debug.DrawLine(val3, ((RaycastHit)(ref hit)).point, Color.green);
		}
		val3 = val + ((Component)this).transform.right * num;
		if (HasLoS_IgnoreBuildables(val3) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(val3, -((Component)this).transform.right, val2.x / 2f + num - num2, out hit, LayerMask.op_Implicit(1 << LayerMask.NameToLayer("Default")), includeTriggers: false, num2, 45f) && Vector3.Angle(-((Component)this).transform.right, -((RaycastHit)(ref hit)).normal) < 5f)
		{
			float num3 = 0f - (val2.x - Vector3.Distance(val3, ((RaycastHit)(ref hit)).point));
			x = num3;
			Debug.DrawLine(val3, ((RaycastHit)(ref hit)).point, Color.red);
		}
		num = val2.z / 2f;
		val3 = val - ((Component)this).transform.forward * num;
		if (HasLoS_IgnoreBuildables(val3) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(val3, ((Component)this).transform.forward, val2.z / 2f + num - num2, out hit, LayerMask.op_Implicit(1 << LayerMask.NameToLayer("Default")), includeTriggers: false, num2, 45f) && Vector3.Angle(((Component)this).transform.forward, -((RaycastHit)(ref hit)).normal) < 5f)
		{
			z = val2.z - Vector3.Distance(val3, ((RaycastHit)(ref hit)).point);
			Debug.DrawLine(val3, ((RaycastHit)(ref hit)).point, Color.cyan);
		}
		val3 = val + ((Component)this).transform.forward * num;
		if (HasLoS_IgnoreBuildables(val3) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(val3, -((Component)this).transform.forward, val2.z / 2f + num - num2, out hit, LayerMask.op_Implicit(1 << LayerMask.NameToLayer("Default")), includeTriggers: false, num2, 45f) && Vector3.Angle(-((Component)this).transform.forward, -((RaycastHit)(ref hit)).normal) < 5f)
		{
			float num4 = 0f - (val2.z - Vector3.Distance(val3, ((RaycastHit)(ref hit)).point));
			z = num4;
			Debug.DrawLine(val3, ((RaycastHit)(ref hit)).point, Color.yellow);
		}
		num = val2.y / 2f;
		val3 = val - ((Component)this).transform.up * num;
		if (HasLoS_IgnoreBuildables(val3) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(val3, ((Component)this).transform.up, val2.y / 2f + num - num2, out hit, LayerMask.op_Implicit(1 << LayerMask.NameToLayer("Default")), includeTriggers: false, num2, 45f) && Vector3.Angle(((Component)this).transform.forward, -((RaycastHit)(ref hit)).normal) < 5f)
		{
			y = val2.y - Vector3.Distance(val3, ((RaycastHit)(ref hit)).point);
			Debug.DrawLine(val3, ((RaycastHit)(ref hit)).point, Color.cyan);
		}
		val3 = val + ((Component)this).transform.up * num;
		if (HasLoS_IgnoreBuildables(val3) && PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(val3, -((Component)this).transform.up, val2.y / 2f + num - num2, out hit, LayerMask.op_Implicit(1 << LayerMask.NameToLayer("Default")), includeTriggers: false, num2, 45f) && Vector3.Angle(-((Component)this).transform.up, -((RaycastHit)(ref hit)).normal) < 5f)
		{
			float num5 = 0f - (val2.y - Vector3.Distance(val3, ((RaycastHit)(ref hit)).point));
			y = num5;
			Debug.DrawLine(val3, ((RaycastHit)(ref hit)).point, Color.yellow);
		}
		if (x != 0f || z != 0f || y != 0f)
		{
			return true;
		}
		return false;
	}

	private bool HasLoS_IgnoreBuildables(Vector3 point)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSingleton<PlayerCamera>.Instance.Raycast_ExcludeBuildables(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, point - ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, Vector3.Distance(point, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position) - 0.01f, out var _, LayerMask.op_Implicit(1 << LayerMask.NameToLayer("Default"))))
		{
			return false;
		}
		return true;
	}

	public virtual void SetCulled(bool culled)
	{
		IsCulled = culled;
		foreach (MeshRenderer item in MeshesToCull)
		{
			if (!((Object)(object)item == (Object)null))
			{
				((Renderer)item).enabled = !culled;
			}
		}
		GameObject[] gameObjectsToCull = GameObjectsToCull;
		foreach (GameObject val in gameObjectsToCull)
		{
			if (!((Object)(object)val == (Object)null))
			{
				val.SetActive(!culled);
			}
		}
	}

	public virtual DynamicSaveData GetSaveData()
	{
		return new DynamicSaveData(GetBaseData());
	}

	public virtual BuildableItemData GetBaseData()
	{
		return new BuildableItemData(GUID, ItemInstance, 0);
	}

	public string GetSaveString()
	{
		return GetBaseData().GetJson();
	}

	public virtual List<string> WriteData(string parentFolderPath)
	{
		return new List<string>();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_Destroy_Server_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_Destroy_Client_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEntityFramework_002EBuildableItemAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_Destroy_Server_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsClientInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because client is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___Destroy_Server_2166136261()
	{
		Destroy_Client();
		((NetworkBehaviour)this).Despawn((DespawnType?)(DespawnType)0);
	}

	private void RpcReader___Server_Destroy_Server_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___Destroy_Server_2166136261();
		}
	}

	private void RpcWriter___Observers_Destroy_Client_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___Destroy_Client_2166136261()
	{
		Destroy();
	}

	private void RpcReader___Observers_Destroy_Client_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___Destroy_Client_2166136261();
		}
	}

	protected virtual void Awake_UserLogic_ScheduleOne_002EEntityFramework_002EBuildableItem_Assembly_002DCSharp_002Edll()
	{
		((Collider)BoundingCollider).isTrigger = true;
		LayerUtility.SetLayerRecursively(((Component)BoundingCollider).gameObject, LayerMask.NameToLayer("Invisible"));
	}
}
