using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Object.Synchronizing;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.EntityFramework;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Property;
using ScheduleOne.StationFramework;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Employees;

public class Botanist : Employee, IConfigurable
{
	public const float CriticalWateringThreshold = 0.2f;

	public const float WateringThreshold = 0.3f;

	public const float MoistureLevelRandomMin = 0.9f;

	public const float MoistureLevelRandomMax = 1f;

	public const float SoilPourTime = 10f;

	public const float WaterPourTime = 10f;

	public const float AdditivePourTime = 10f;

	public const float SeedSowTime = 15f;

	public const float IndividualHarvestTime = 1f;

	public const float ApplySpawnTime = 15f;

	[Header("References")]
	public Sprite typeIcon;

	[SerializeField]
	protected ConfigurationReplicator configReplicator;

	[Header("UI")]
	public BotanistUIElement WorldspaceUIPrefab;

	public Transform uiPoint;

	[Header("Settings")]
	public int MaxAssignedPots = 8;

	public DialogueContainer NoAssignedStationsDialogue;

	public DialogueContainer UnspecifiedPotsDialogue;

	public DialogueContainer NullDestinationPotsDialogue;

	public DialogueContainer MissingMaterialsDialogue;

	public DialogueContainer NoPotsRequireWorkDialogue;

	[CompilerGenerated]
	[SyncVar]
	public NetworkObject _003CCurrentPlayerConfigurer_003Ek__BackingField;

	private StartDryingRackBehaviour _startDryingRackBehaviour;

	private StopDryingRackBehaviour _stopDryingRackBehaviour;

	private UseSpawnStationBehaviour _useSpawnStationBehaviour;

	private AddSoilToGrowContainerBehaviour _addSoilToGrowContainerBehaviour;

	private ApplyAdditiveToGrowContainerBehaviour _applyAdditiveToGrowContainerBehaviour;

	private SowSeedInPotBehaviour _sowSeedInPotBehaviour;

	private WaterPotBehaviour _waterPotBehaviour;

	private HarvestPotBehaviour _harvestPotBehaviour;

	private MistMushroomBedBehaviour _mistMushroomBedBehaviour;

	private HarvestMushroomBedBehaviour _harvestMushroomBedBehaviour;

	private ApplySpawnToMushroomBedBehaviour _applySpawnToMushroomBedBehaviour;

	private List<Behaviour> _workBehaviours = new List<Behaviour>();

	public SyncVar<NetworkObject> syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField;

	private bool NetworkInitialize___EarlyScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted;

	public EntityConfiguration Configuration => configuration;

	protected BotanistConfiguration configuration { get; set; }

	public ConfigurationReplicator ConfigReplicator => configReplicator;

	public EConfigurableType ConfigurableType => EConfigurableType.Botanist;

	public WorldspaceUIElement WorldspaceUI { get; set; }

	public NetworkObject CurrentPlayerConfigurer
	{
		[CompilerGenerated]
		get
		{
			return SyncAccessor__003CCurrentPlayerConfigurer_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(value, true);
		}
	}

	public Sprite TypeIcon => typeIcon;

	public Transform Transform => ((Component)this).transform;

	public Transform UIPoint => uiPoint;

	public bool CanBeSelected => true;

	public ScheduleOne.Property.Property ParentProperty => base.AssignedProperty;

	public NetworkObject SyncAccessor__003CCurrentPlayerConfigurer_003Ek__BackingField
	{
		get
		{
			return CurrentPlayerConfigurer;
		}
		set
		{
			if (value || !((NetworkBehaviour)this).IsServerInitialized)
			{
				CurrentPlayerConfigurer = value;
			}
			if (Application.isPlaying)
			{
				syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.SetValue(value, value);
			}
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetConfigurer(NetworkObject player)
	{
		RpcWriter___Server_SetConfigurer_3323014238(player);
		RpcLogic___SetConfigurer_3323014238(player);
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EEmployees_002EBotanist_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override bool IsAnyWorkInProgress()
	{
		return _workBehaviours.Any((Behaviour b) => b.Active);
	}

	protected override void UpdateBehaviour()
	{
		base.UpdateBehaviour();
		if (_workBehaviours.Any((Behaviour b) => b.Active))
		{
			MarkIsWorking();
		}
		else
		{
			if (!InstanceFinder.IsServer)
			{
				return;
			}
			if (base.Fired)
			{
				LeavePropertyAndDespawn();
			}
			else
			{
				if (!CanWork())
				{
					return;
				}
				if (configuration.Assigns.SelectedObjects.Count == 0)
				{
					SubmitNoWorkReason("I haven't been assigned anything", "You can use your management clipboards to assign me pots, growing racks, etc.");
					SetIdle(idle: true);
				}
				else
				{
					if (!InstanceFinder.IsServer)
					{
						return;
					}
					Pot potForWatering = GetPotForWatering(0.2f);
					if ((Object)(object)potForWatering != (Object)null)
					{
						_waterPotBehaviour.AssignAndEnable(potForWatering);
						return;
					}
					MushroomBed mushroomBedForMisting = GetMushroomBedForMisting(0.2f);
					if ((Object)(object)mushroomBedForMisting != (Object)null)
					{
						_mistMushroomBedBehaviour.AssignAndEnable(mushroomBedForMisting);
						return;
					}
					foreach (GrowContainer growContainersForAdditive in GetGrowContainersForAdditives())
					{
						if ((Object)(object)growContainersForAdditive != (Object)null && _applyAdditiveToGrowContainerBehaviour.DoesBotanistHaveAccessToRequiredSupplies(growContainersForAdditive))
						{
							_applyAdditiveToGrowContainerBehaviour.AssignAndEnable(growContainersForAdditive);
							return;
						}
					}
					foreach (GrowContainer item in GetGrowContainersForSoilPour())
					{
						if (_addSoilToGrowContainerBehaviour.DoesBotanistHaveAccessToRequiredSupplies(item))
						{
							if (IsEntityAccessible(item))
							{
								_addSoilToGrowContainerBehaviour.AssignAndEnable(item);
								return;
							}
							continue;
						}
						string fix = "Make sure there's soil in my supplies stash.";
						if ((Object)(object)configuration.Supplies.SelectedObject == (Object)null)
						{
							fix = "Use your management clipboard to assign a supplies stash to me, then make sure there's soil in it.";
						}
						SubmitNoWorkReason("There are empty pots, but I don't have any soil to pour.", fix);
					}
					List<Pot> potsReadyForSeed = GetPotsReadyForSeed();
					bool flag = false;
					foreach (Pot item2 in potsReadyForSeed)
					{
						if (!_sowSeedInPotBehaviour.DoesBotanistHaveAccessToRequiredSupplies(item2))
						{
							if (!flag)
							{
								flag = true;
								string fix2 = "Make sure I have the right seeds in my supplies stash.";
								if ((Object)(object)configuration.Supplies.SelectedObject == (Object)null)
								{
									fix2 = "Use your management clipboards to assign a supplies stash to me, and make sure it contains the right seeds.";
								}
								SubmitNoWorkReason("There is a pot ready for sowing, but I don't have any seeds for it.", fix2, 1);
							}
						}
						else if (IsEntityAccessible(item2))
						{
							_sowSeedInPotBehaviour.AssignAndEnable(item2);
							return;
						}
					}
					List<MushroomBed> bedsReadyForSpawn = GetBedsReadyForSpawn();
					flag = false;
					foreach (MushroomBed item3 in bedsReadyForSpawn)
					{
						if (!_applySpawnToMushroomBedBehaviour.DoesBotanistHaveAccessToRequiredSupplies(item3))
						{
							if (!flag)
							{
								flag = true;
								string fix3 = "Make sure I have shroom spawn my supplies stash.";
								if ((Object)(object)configuration.Supplies.SelectedObject == (Object)null)
								{
									fix3 = "Use your management clipboards to assign a supplies stash to me, and make sure it contains shroom spawn.";
								}
								SubmitNoWorkReason("I don't have any shroom spawn to mix into my assigned mushroom beds.", fix3, 1);
							}
						}
						else if (IsEntityAccessible(item3))
						{
							_applySpawnToMushroomBedBehaviour.AssignAndEnable(item3);
							return;
						}
					}
					List<Pot> potsForHarvest = GetPotsForHarvest();
					if (potsForHarvest != null && potsForHarvest.Count > 0)
					{
						_harvestPotBehaviour.AssignAndEnable(potsForHarvest[0]);
						return;
					}
					List<MushroomBed> mushroomBedsForHarvest = GetMushroomBedsForHarvest();
					if (mushroomBedsForHarvest != null && mushroomBedsForHarvest.Count > 0)
					{
						_harvestMushroomBedBehaviour.AssignAndEnable(mushroomBedsForHarvest[0]);
						return;
					}
					foreach (DryingRack item4 in GetRacksToStop())
					{
						if (IsEntityAccessible(item4))
						{
							StopDryingRack(item4);
							return;
						}
					}
					foreach (DryingRack item5 in GetRacksReadyToMove())
					{
						if (IsEntityAccessible(item5))
						{
							MoveItemBehaviour.Initialize((item5.Configuration as DryingRackConfiguration).DestinationRoute, item5.OutputSlot.ItemInstance);
							MoveItemBehaviour.Enable_Networked();
							return;
						}
					}
					foreach (MushroomSpawnStation item6 in GetSpawnStationsReadyToUse())
					{
						if (IsEntityAccessible(item6))
						{
							_useSpawnStationBehaviour.AssignStation(item6);
							_useSpawnStationBehaviour.Enable_Networked();
							return;
						}
					}
					foreach (MushroomSpawnStation item7 in GetSpawnStationsReadyToMove())
					{
						if (IsEntityAccessible(item7))
						{
							MoveItemBehaviour.Initialize((item7.Configuration as SpawnStationConfiguration).DestinationRoute, item7.OutputSlot.ItemInstance);
							MoveItemBehaviour.Enable_Networked();
							return;
						}
					}
					Pot potForWatering2 = GetPotForWatering(0.3f);
					if ((Object)(object)potForWatering2 != (Object)null)
					{
						_waterPotBehaviour.AssignAndEnable(potForWatering2);
						return;
					}
					MushroomBed mushroomBedForMisting2 = GetMushroomBedForMisting(0.3f);
					if ((Object)(object)mushroomBedForMisting2 != (Object)null)
					{
						_mistMushroomBedBehaviour.AssignAndEnable(mushroomBedForMisting2);
						return;
					}
					if (CanMoveDryableToRack(out var dryable, out var destinationRack, out var moveQuantity))
					{
						TransitRoute route = new TransitRoute(configuration.Supplies.SelectedObject as ITransitEntity, destinationRack);
						if (MoveItemBehaviour.IsTransitRouteValid(route, ((BaseItemInstance)dryable).ID))
						{
							MoveItemBehaviour.Initialize(route, dryable, moveQuantity);
							MoveItemBehaviour.Enable_Networked();
							Console.Log("Moving " + moveQuantity + " " + ((BaseItemInstance)dryable).ID + " to drying rack");
							return;
						}
					}
					foreach (DryingRack item8 in GetRacksToStart())
					{
						if (IsEntityAccessible(item8))
						{
							StartDryingRack(item8);
							return;
						}
					}
					SubmitNoWorkReason("There's nothing for me to do right now.", string.Empty);
					SetIdle(idle: true);
				}
			}
		}
	}

	private bool IsEntityAccessible(ITransitEntity entity)
	{
		return (Object)(object)NavMeshUtility.GetReachableAccessPoint(entity, this) != (Object)null;
	}

	private void StartDryingRack(DryingRack rack)
	{
		_startDryingRackBehaviour.AssignRack(rack);
		_startDryingRackBehaviour.Enable_Networked();
	}

	private void StopDryingRack(DryingRack rack)
	{
		_stopDryingRackBehaviour.AssignRack(rack);
		_stopDryingRackBehaviour.Enable_Networked();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		SendConfigurationToClient(connection);
	}

	public void SendConfigurationToClient(NetworkConnection conn)
	{
		if (!conn.IsHost)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(WaitForConfig());
		}
		IEnumerator WaitForConfig()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => Configuration != null));
			Configuration.ReplicateAllFields(conn);
		}
	}

	protected override void AssignProperty(ScheduleOne.Property.Property prop, bool warp)
	{
		base.AssignProperty(prop, warp);
		prop.AddConfigurable(this);
		configuration = new BotanistConfiguration(configReplicator, this, this);
		CreateWorldspaceUI();
	}

	protected override void UnassignProperty()
	{
		base.AssignedProperty.RemoveConfigurable(this);
		base.UnassignProperty();
	}

	protected override void ResetConfiguration()
	{
		if (configuration != null)
		{
			configuration.Reset();
		}
		base.ResetConfiguration();
	}

	protected override void Fire()
	{
		if (configuration != null)
		{
			configuration.Destroy();
			DestroyWorldspaceUI();
		}
		base.Fire();
	}

	private bool CanMoveDryableToRack(out QualityItemInstance dryable, out DryingRack destinationRack, out int moveQuantity)
	{
		moveQuantity = 0;
		destinationRack = null;
		dryable = GetDryableInSupplies();
		if (dryable == null)
		{
			return false;
		}
		int rackInputCapacity = 0;
		destinationRack = GetAssignedDryingRackFor(dryable, out rackInputCapacity);
		if ((Object)(object)destinationRack == (Object)null)
		{
			return false;
		}
		moveQuantity = Mathf.Min(((BaseItemInstance)dryable).Quantity, rackInputCapacity);
		if (!Movement.CanGetTo(GetSuppliesAsTransitEntity()))
		{
			return false;
		}
		return true;
	}

	public QualityItemInstance GetDryableInSupplies()
	{
		if ((Object)(object)configuration.Supplies.SelectedObject == (Object)null)
		{
			return null;
		}
		List<ItemSlot> list = new List<ItemSlot>();
		BuildableItem selectedObject = configuration.Supplies.SelectedObject;
		if ((Object)(object)selectedObject != (Object)null)
		{
			list.AddRange((selectedObject as ITransitEntity).OutputSlots);
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].Quantity > 0 && ItemFilter_Dryable.IsItemDryable(list[i].ItemInstance))
			{
				return list[i].ItemInstance as QualityItemInstance;
			}
		}
		return null;
	}

	private DryingRack GetAssignedDryingRackFor(QualityItemInstance dryable, out int rackInputCapacity)
	{
		rackInputCapacity = 0;
		foreach (DryingRack assignedRack in configuration.AssignedRacks)
		{
			if ((assignedRack.Configuration as DryingRackConfiguration).TargetQuality.Value > dryable.Quality)
			{
				int inputCapacityForItem = ((ITransitEntity)assignedRack).GetInputCapacityForItem((ItemInstance)dryable, (NPC)this, true);
				if (inputCapacityForItem > 0)
				{
					rackInputCapacity = inputCapacityForItem;
					return assignedRack;
				}
			}
		}
		return null;
	}

	protected override bool ShouldIdle()
	{
		if (configuration.Assigns.SelectedObjects.Count == 0)
		{
			return true;
		}
		return base.ShouldIdle();
	}

	public override EmployeeHome GetHome()
	{
		return configuration.AssignedHome;
	}

	public ITransitEntity GetSuppliesAsTransitEntity()
	{
		if ((Object)(object)configuration.Supplies.SelectedObject == (Object)null)
		{
			return null;
		}
		return configuration.Supplies.SelectedObject as ITransitEntity;
	}

	private Pot GetPotForWatering(float threshold)
	{
		for (int i = 0; i < configuration.AssignedPots.Count; i++)
		{
			if (_waterPotBehaviour.AreTaskConditionsMetForContainer(configuration.AssignedPots[i], threshold) && IsEntityAccessible(configuration.AssignedPots[i]))
			{
				return configuration.AssignedPots[i];
			}
		}
		return null;
	}

	private List<GrowContainer> GetGrowContainersForSoilPour()
	{
		List<GrowContainer> list = new List<GrowContainer>();
		foreach (Pot assignedPot in configuration.AssignedPots)
		{
			if (_addSoilToGrowContainerBehaviour.AreTaskConditionsMetForContainer(assignedPot))
			{
				list.Add(assignedPot);
			}
		}
		foreach (MushroomBed assignedBed in configuration.AssignedBeds)
		{
			if (_addSoilToGrowContainerBehaviour.AreTaskConditionsMetForContainer(assignedBed))
			{
				list.Add(assignedBed);
			}
		}
		return list;
	}

	private List<Pot> GetPotsReadyForSeed()
	{
		List<Pot> list = new List<Pot>();
		for (int i = 0; i < configuration.AssignedPots.Count; i++)
		{
			if (_sowSeedInPotBehaviour.AreTaskConditionsMetForContainer(configuration.AssignedPots[i]))
			{
				list.Add(configuration.AssignedPots[i]);
			}
		}
		return list;
	}

	private List<GrowContainer> GetGrowContainersForAdditives()
	{
		List<GrowContainer> list = new List<GrowContainer>();
		foreach (Pot assignedPot in configuration.AssignedPots)
		{
			if (_applyAdditiveToGrowContainerBehaviour.AreTaskConditionsMetForContainer(assignedPot) && IsEntityAccessible(assignedPot))
			{
				list.Add(assignedPot);
			}
		}
		foreach (MushroomBed assignedBed in configuration.AssignedBeds)
		{
			if (_applyAdditiveToGrowContainerBehaviour.AreTaskConditionsMetForContainer(assignedBed) && IsEntityAccessible(assignedBed))
			{
				list.Add(assignedBed);
			}
		}
		return list;
	}

	private List<Pot> GetPotsForHarvest()
	{
		List<Pot> list = new List<Pot>();
		for (int i = 0; i < configuration.AssignedPots.Count; i++)
		{
			if (_harvestPotBehaviour.AreTaskConditionsMetForContainer(configuration.AssignedPots[i]))
			{
				if (!_harvestPotBehaviour.DoesPotHaveValidDestination(configuration.AssignedPots[i]))
				{
					SubmitNoWorkReason("There is a plant ready for harvest, but it has no destination or it's destination is full.", "Use your management clipboard to assign a destination for each of my pots, and make sure the destination isn't full.");
				}
				else if (IsEntityAccessible(configuration.AssignedPots[i]))
				{
					list.Add(configuration.AssignedPots[i]);
				}
			}
		}
		return list;
	}

	private MushroomBed GetMushroomBedForMisting(float threshold)
	{
		for (int i = 0; i < configuration.AssignedBeds.Count; i++)
		{
			if (_mistMushroomBedBehaviour.AreTaskConditionsMetForContainer(configuration.AssignedBeds[i], threshold) && IsEntityAccessible(configuration.AssignedBeds[i]))
			{
				return configuration.AssignedBeds[i];
			}
		}
		return null;
	}

	private List<MushroomBed> GetMushroomBedsForHarvest()
	{
		List<MushroomBed> list = new List<MushroomBed>();
		for (int i = 0; i < configuration.AssignedBeds.Count; i++)
		{
			if (_harvestMushroomBedBehaviour.AreTaskConditionsMetForContainer(configuration.AssignedBeds[i]))
			{
				if (!_harvestMushroomBedBehaviour.DoesMushroomBedHaveValidDestination(configuration.AssignedBeds[i]))
				{
					SubmitNoWorkReason("There is a mushroom colony ready for harvest, but it has no destination or it's destination is full.", "Use your management clipboard to assign a destination for each of my mushroom beds, and make sure the destination isn't full.");
				}
				else if (IsEntityAccessible(configuration.AssignedBeds[i]))
				{
					list.Add(configuration.AssignedBeds[i]);
				}
			}
		}
		return list;
	}

	private List<MushroomBed> GetBedsReadyForSpawn()
	{
		List<MushroomBed> list = new List<MushroomBed>();
		for (int i = 0; i < configuration.AssignedBeds.Count; i++)
		{
			if (_applySpawnToMushroomBedBehaviour.AreTaskConditionsMetForContainer(configuration.AssignedBeds[i]))
			{
				list.Add(configuration.AssignedBeds[i]);
			}
		}
		return list;
	}

	private List<DryingRack> GetRacksToStart()
	{
		List<DryingRack> list = new List<DryingRack>();
		for (int i = 0; i < configuration.AssignedRacks.Count; i++)
		{
			if (_startDryingRackBehaviour.IsRackReady(configuration.AssignedRacks[i]))
			{
				list.Add(configuration.AssignedRacks[i]);
			}
		}
		return list;
	}

	private List<DryingRack> GetRacksToStop()
	{
		List<DryingRack> list = new List<DryingRack>();
		for (int i = 0; i < configuration.AssignedRacks.Count; i++)
		{
			if (_stopDryingRackBehaviour.IsRackReady(configuration.AssignedRacks[i]))
			{
				list.Add(configuration.AssignedRacks[i]);
			}
		}
		return list;
	}

	private List<DryingRack> GetRacksReadyToMove()
	{
		List<DryingRack> list = new List<DryingRack>();
		for (int i = 0; i < configuration.AssignedRacks.Count; i++)
		{
			ItemSlot outputSlot = configuration.AssignedRacks[i].OutputSlot;
			if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((configuration.AssignedRacks[i].Configuration as DryingRackConfiguration).DestinationRoute, ((BaseItemInstance)outputSlot.ItemInstance).ID))
			{
				list.Add(configuration.AssignedRacks[i]);
			}
		}
		return list;
	}

	private List<MushroomSpawnStation> GetSpawnStationsReadyToUse()
	{
		List<MushroomSpawnStation> list = new List<MushroomSpawnStation>();
		for (int i = 0; i < configuration.AssignedSpawnStations.Count; i++)
		{
			if (_useSpawnStationBehaviour.IsStationReady(configuration.AssignedSpawnStations[i]))
			{
				list.Add(configuration.AssignedSpawnStations[i]);
			}
		}
		return list;
	}

	private List<MushroomSpawnStation> GetSpawnStationsReadyToMove()
	{
		List<MushroomSpawnStation> list = new List<MushroomSpawnStation>();
		for (int i = 0; i < configuration.AssignedSpawnStations.Count; i++)
		{
			ItemSlot outputSlot = configuration.AssignedSpawnStations[i].OutputSlot;
			if (outputSlot.Quantity != 0 && MoveItemBehaviour.IsTransitRouteValid((configuration.AssignedSpawnStations[i].Configuration as SpawnStationConfiguration).DestinationRoute, ((BaseItemInstance)outputSlot.ItemInstance).ID))
			{
				list.Add(configuration.AssignedSpawnStations[i]);
			}
		}
		return list;
	}

	public WorldspaceUIElement CreateWorldspaceUI()
	{
		if ((Object)(object)WorldspaceUI != (Object)null)
		{
			Console.LogWarning(((Object)((Component)this).gameObject).name + " already has a worldspace UI element!");
		}
		ScheduleOne.Property.Property assignedProperty = base.AssignedProperty;
		if ((Object)(object)assignedProperty == (Object)null)
		{
			Console.LogError(((object)assignedProperty)?.ToString() + " is not a child of a property!");
			return null;
		}
		BotanistUIElement component = ((Component)Object.Instantiate<BotanistUIElement>(WorldspaceUIPrefab, (Transform)(object)assignedProperty.WorldspaceUIContainer)).GetComponent<BotanistUIElement>();
		component.Initialize(this);
		WorldspaceUI = component;
		return component;
	}

	public void DestroyWorldspaceUI()
	{
		if ((Object)(object)WorldspaceUI != (Object)null)
		{
			WorldspaceUI.Destroy();
		}
	}

	public override NPCData GetNPCData()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		return new BotanistData(ID, base.AssignedProperty.PropertyCode, FirstName, LastName, base.IsMale, base.AppearanceIndex, ((Component)this).transform.position, ((Component)this).transform.rotation, base.GUID, base.PaidForToday, MoveItemBehaviour.GetSaveData());
	}

	public override DynamicSaveData GetSaveData()
	{
		DynamicSaveData saveData = base.GetSaveData();
		saveData.AddData("Configuration", Configuration.GetSaveString());
		return saveData;
	}

	public override List<string> WriteData(string parentFolderPath)
	{
		return new List<string>();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField = new SyncVar<NetworkObject>((NetworkBehaviour)(object)this, 2u, (WritePermission)0, (ReadPermission)0, -1f, (Channel)0, CurrentPlayerConfigurer);
			((NetworkBehaviour)this).RegisterServerRpc(46u, new ServerRpcDelegate(RpcReader___Server_SetConfigurer_3323014238));
			((NetworkBehaviour)this).RegisterSyncVarRead(new SyncVarReadDelegate(ReadSyncVar___ScheduleOne_002EEmployees_002EBotanist));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EEmployees_002EBotanistAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
			((SyncBase)syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField).SetRegistered();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetConfigurer_3323014238(NetworkObject player)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteNetworkObject(player);
			((NetworkBehaviour)this).SendServerRpc(46u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetConfigurer_3323014238(NetworkObject player)
	{
		CurrentPlayerConfigurer = player;
	}

	private void RpcReader___Server_SetConfigurer_3323014238(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NetworkObject player = ((Reader)PooledReader0).ReadNetworkObject();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetConfigurer_3323014238(player);
		}
	}

	public override bool ReadSyncVar___ScheduleOne_002EEmployees_002EBotanist(PooledReader PooledReader0, uint UInt321, bool Boolean2)
	{
		if (UInt321 == 2)
		{
			if (PooledReader0 == null)
			{
				this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(syncVar____003CCurrentPlayerConfigurer_003Ek__BackingField.GetValue(true), true);
				return true;
			}
			NetworkObject value = ((Reader)PooledReader0).ReadNetworkObject();
			this.sync___set_value__003CCurrentPlayerConfigurer_003Ek__BackingField(value, Boolean2);
			return true;
		}
		return false;
	}

	protected override void Awake_UserLogic_ScheduleOne_002EEmployees_002EBotanist_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		_addSoilToGrowContainerBehaviour = Behaviour.GetBehaviour<AddSoilToGrowContainerBehaviour>();
		_workBehaviours.Add(_addSoilToGrowContainerBehaviour);
		_useSpawnStationBehaviour = Behaviour.GetBehaviour<UseSpawnStationBehaviour>();
		_workBehaviours.Add(_useSpawnStationBehaviour);
		_startDryingRackBehaviour = Behaviour.GetBehaviour<StartDryingRackBehaviour>();
		_workBehaviours.Add(_startDryingRackBehaviour);
		_stopDryingRackBehaviour = Behaviour.GetBehaviour<StopDryingRackBehaviour>();
		_workBehaviours.Add(_stopDryingRackBehaviour);
		_applyAdditiveToGrowContainerBehaviour = Behaviour.GetBehaviour<ApplyAdditiveToGrowContainerBehaviour>();
		_workBehaviours.Add(_applyAdditiveToGrowContainerBehaviour);
		_sowSeedInPotBehaviour = Behaviour.GetBehaviour<SowSeedInPotBehaviour>();
		_workBehaviours.Add(_sowSeedInPotBehaviour);
		_waterPotBehaviour = Behaviour.GetBehaviour<WaterPotBehaviour>();
		_workBehaviours.Add(_waterPotBehaviour);
		_harvestPotBehaviour = Behaviour.GetBehaviour<HarvestPotBehaviour>();
		_workBehaviours.Add(_harvestPotBehaviour);
		_mistMushroomBedBehaviour = Behaviour.GetBehaviour<MistMushroomBedBehaviour>();
		_workBehaviours.Add(_mistMushroomBedBehaviour);
		_harvestMushroomBedBehaviour = Behaviour.GetBehaviour<HarvestMushroomBedBehaviour>();
		_workBehaviours.Add(_harvestMushroomBedBehaviour);
		_applySpawnToMushroomBedBehaviour = Behaviour.GetBehaviour<ApplySpawnToMushroomBedBehaviour>();
		_workBehaviours.Add(_applySpawnToMushroomBedBehaviour);
		_workBehaviours.Add(MoveItemBehaviour);
		for (int i = 0; i < _workBehaviours.Count; i++)
		{
		}
	}
}
