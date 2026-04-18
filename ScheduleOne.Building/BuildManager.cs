using System;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.ItemFramework;
using ScheduleOne.Tiles;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace ScheduleOne.Building;

public class BuildManager : NetworkSingleton<BuildManager>
{
	[Serializable]
	public class BuildSound
	{
		public BuildableItemDefinition.EBuildSoundType Type;

		public AudioSourceController Sound;
	}

	public List<BuildSound> PlaceSounds = new List<BuildSound>();

	[Header("Materials")]
	public Material ghostMaterial_White;

	public Material ghostMaterial_Red;

	private bool NetworkInitialize___EarlyScheduleOne_002EBuilding_002EBuildManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EBuilding_002EBuildManagerAssembly_002DCSharp_002Edll_Excuted;

	public bool isBuilding { get; protected set; }

	public GameObject currentBuildHandler { get; protected set; }

	public void StartBuilding(ItemInstance item)
	{
		if (!(item.Definition is BuildableItemDefinition))
		{
			Console.LogError("StartBuilding called but not passed BuildableItemDefinition");
			return;
		}
		if (isBuilding)
		{
			Console.LogWarning("StartBuilding called but building is already happening!");
			StopBuilding();
		}
		BuildableItem builtItem = (item.Definition as BuildableItemDefinition).BuiltItem;
		if ((Object)(object)builtItem == (Object)null)
		{
			Console.LogWarning("itemToBuild is null!");
			return;
		}
		isBuilding = true;
		currentBuildHandler = Object.Instantiate<GameObject>(builtItem.BuildHandler, NetworkSingleton<GameManager>.Instance.Temp);
		currentBuildHandler.GetComponent<BuildStart_Base>().StartBuilding(item);
	}

	public void StopBuilding()
	{
		isBuilding = false;
		currentBuildHandler.GetComponent<BuildStop_Base>().Stop_Building();
	}

	public void PlayBuildSound(BuildableItemDefinition.EBuildSoundType type, Vector3 point)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		BuildSound buildSound = PlaceSounds.Find((BuildSound s) => s.Type == type);
		if (buildSound != null)
		{
			((Component)buildSound.Sound).transform.position = point;
			buildSound.Sound.Play();
		}
	}

	public void DisableColliders(GameObject obj)
	{
		Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	public void DisableLights(GameObject obj)
	{
		OptimizedLight[] componentsInChildren = obj.GetComponentsInChildren<OptimizedLight>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Enabled = false;
		}
		Light[] componentsInChildren2 = obj.GetComponentsInChildren<Light>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			((Behaviour)componentsInChildren2[j]).enabled = false;
		}
	}

	public void DisableNetworking(GameObject obj)
	{
		NetworkObject[] componentsInChildren = obj.GetComponentsInChildren<NetworkObject>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy((Object)(object)componentsInChildren[i]);
		}
	}

	public void DisableSpriteRenderers(GameObject obj)
	{
		SpriteRenderer[] componentsInChildren = obj.GetComponentsInChildren<SpriteRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).enabled = false;
		}
	}

	public void ApplyMaterial(GameObject obj, Material mat, bool allMaterials = true)
	{
		MeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (Object.op_Implicit((Object)(object)((Component)componentsInChildren[i]).gameObject.GetComponentInParent<OverrideGhostMaterial>()))
			{
				continue;
			}
			if (allMaterials)
			{
				Material[] materials = ((Renderer)componentsInChildren[i]).materials;
				for (int j = 0; j < materials.Length; j++)
				{
					materials[j] = mat;
				}
				((Renderer)componentsInChildren[i]).materials = materials;
			}
			else
			{
				((Renderer)componentsInChildren[i]).material = mat;
			}
		}
	}

	public void DisableNavigation(GameObject obj)
	{
		NavMeshObstacle[] componentsInChildren = obj.GetComponentsInChildren<NavMeshObstacle>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Behaviour)componentsInChildren[i]).enabled = false;
		}
		NavMeshSurface[] componentsInChildren2 = obj.GetComponentsInChildren<NavMeshSurface>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			((Behaviour)componentsInChildren2[j]).enabled = false;
		}
		NavMeshLink[] componentsInChildren3 = obj.GetComponentsInChildren<NavMeshLink>();
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			((Behaviour)componentsInChildren3[k]).enabled = false;
		}
	}

	public void DisableCanvases(GameObject obj)
	{
		Canvas[] componentsInChildren = obj.GetComponentsInChildren<Canvas>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Behaviour)componentsInChildren[i]).enabled = false;
		}
	}

	public GridItem CreateGridItem(ItemInstance item, Grid grid, Vector2 originCoordinate, int rotation, string guid = "", Action<GridItem> onBeforeSpawn = null)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		BuildableItemDefinition buildableItemDefinition = item.Definition as BuildableItemDefinition;
		if ((Object)(object)buildableItemDefinition == (Object)null)
		{
			Console.LogError("BuildGridItem called but could not find BuildableItemDefinition");
			return null;
		}
		if ((Object)(object)grid == (Object)null)
		{
			Console.LogError("BuildGridItem called and passed null grid");
			return null;
		}
		string gUID = (string.IsNullOrEmpty(guid) ? GUIDManager.GenerateUniqueGUID().ToString() : guid);
		GridItem component = Object.Instantiate<GameObject>(((Component)buildableItemDefinition.BuiltItem).gameObject, (Transform)null).GetComponent<GridItem>();
		component.SetLocallyBuilt();
		component.InitializeGridItem(item, grid, originCoordinate, rotation, gUID);
		onBeforeSpawn?.Invoke(component);
		((NetworkBehaviour)this).NetworkObject.Spawn(((Component)component).gameObject, (NetworkConnection)null, default(Scene));
		return component;
	}

	public ProceduralGridItem CreateProceduralGridItem(ItemInstance item, int rotationAngle, List<CoordinateProceduralTilePair> matches, string guid = "")
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		BuildableItemDefinition buildableItemDefinition = item.Definition as BuildableItemDefinition;
		if ((Object)(object)buildableItemDefinition == (Object)null)
		{
			Console.LogError("BuildProceduralGridItem called but could not find BuildableItemDefinition");
			return null;
		}
		string gUID = (string.IsNullOrEmpty(guid) ? GUIDManager.GenerateUniqueGUID().ToString() : guid);
		ProceduralGridItem component = Object.Instantiate<GameObject>(((Component)buildableItemDefinition.BuiltItem).gameObject, (Transform)null).GetComponent<ProceduralGridItem>();
		component.SetLocallyBuilt();
		component.InitializeProceduralGridItem(item, rotationAngle, matches, gUID);
		((NetworkBehaviour)this).NetworkObject.Spawn(((Component)component).gameObject, (NetworkConnection)null, default(Scene));
		return component;
	}

	public SurfaceItem CreateSurfaceItem(ItemInstance item, Surface parentSurface, Vector3 relativePosition, Quaternion relativeRotation, string guid = "")
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		BuildableItemDefinition buildableItemDefinition = item.Definition as BuildableItemDefinition;
		if ((Object)(object)buildableItemDefinition == (Object)null)
		{
			Console.LogError("CreateSurfaceItem called but could not find BuildableItemDefinition");
			return null;
		}
		string gUID = (string.IsNullOrEmpty(guid) ? GUIDManager.GenerateUniqueGUID().ToString() : guid);
		SurfaceItem component = Object.Instantiate<GameObject>(((Component)buildableItemDefinition.BuiltItem).gameObject, (Transform)null).GetComponent<SurfaceItem>();
		component.SetLocallyBuilt();
		component.InitializeSurfaceItem(item, gUID, parentSurface.GUID.ToString(), relativePosition, relativeRotation);
		((NetworkBehaviour)this).NetworkObject.Spawn(((Component)component).gameObject, (NetworkConnection)null, default(Scene));
		return component;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EBuilding_002EBuildManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EBuilding_002EBuildManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EBuilding_002EBuildManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EBuilding_002EBuildManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
