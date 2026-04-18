using System.Collections.Generic;
using System.Runtime.InteropServices;
using FishNet.Object;
using ScheduleOne.AvatarFramework;
using ScheduleOne.AvatarFramework.Customization;
using ScheduleOne.Cartel;
using ScheduleOne.Casino;
using ScheduleOne.Combat;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.Delivery;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.Economy;
using ScheduleOne.Employees;
using ScheduleOne.Equipping;
using ScheduleOne.Equipping.Framework;
using ScheduleOne.GameTime;
using ScheduleOne.Graffiti;
using ScheduleOne.ItemFramework;
using ScheduleOne.Law;
using ScheduleOne.Levelling;
using ScheduleOne.Management;
using ScheduleOne.Map;
using ScheduleOne.Messaging;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Property;
using ScheduleOne.Quests;
using ScheduleOne.Temperature;
using ScheduleOne.Tiles;
using ScheduleOne.UI.Handover;
using ScheduleOne.UI.Phone.Messages;
using ScheduleOne.Vehicles;
using ScheduleOne.Vehicles.Modification;
using ScheduleOne.Vision;
using ScheduleOne.VoiceOver;
using ScheduleOne.Weather;
using UnityEngine;

namespace FishNet.Serializing.Generated;

[StructLayout(LayoutKind.Auto, CharSet = CharSet.Auto)]
public static class GeneratedReaders___Internal
{
	[RuntimeInitializeOnLoadMethod]
	private static void InitializeOnce()
	{
		GenericReader<ItemInstance>.Read = ItemSerializers.ReadItemInstance;
		GenericReader<ProductItemInstance>.Read = ItemSerializers.ReadProductItemInstance;
		GenericReader<EquippableData>.Read = EquippableDataSerializer.ReadEquippableData;
		GenericReader<INetworkedEquippableUser>.Read = INetworkedEquippableUserSerializer.ReadINetworkedEquippableUser;
		GenericReader<EVehicleColor>.Read = Read___ScheduleOne_002EVehicles_002EModification_002EEVehicleColorFishNet_002ESerializing_002EGenerateds;
		GenericReader<ParkData>.Read = Read___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerateds;
		GenericReader<EParkingAlignment>.Read = Read___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerateds;
		GenericReader<AirConditioner.EMode>.Read = Read___ScheduleOne_002ETemperature_002EAirConditioner_002FEModeFishNet_002ESerializing_002EGenerateds;
		GenericReader<ContractInfo>.Read = Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds;
		GenericReader<ProductList>.Read = Read___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerateds;
		GenericReader<ProductList.Entry>.Read = Read___ScheduleOne_002EProduct_002EProductList_002FEntryFishNet_002ESerializing_002EGenerateds;
		GenericReader<EQuality>.Read = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<ProductList.Entry>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<QuestWindowConfig>.Read = Read___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerateds;
		GenericReader<GameDateTime>.Read = Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds;
		GenericReader<QuestManager.EQuestAction>.Read = Read___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerateds;
		GenericReader<EQuestState>.Read = Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds;
		GenericReader<Manor.EManorState>.Read = Read___ScheduleOne_002EProperty_002EManor_002FEManorStateFishNet_002ESerializing_002EGenerateds;
		GenericReader<EVisualState>.Read = Read___ScheduleOne_002EVision_002EEVisualStateFishNet_002ESerializing_002EGenerateds;
		GenericReader<VisionEventReceipt>.Read = Read___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerateds;
		GenericReader<VisionCone.EEventLevel>.Read = Read___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerateds;
		GenericReader<Message>.Read = Read___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerateds;
		GenericReader<Message.ESenderType>.Read = Read___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerateds;
		GenericReader<MessageChain>.Read = Read___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<string>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<MSGConversationData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerateds;
		GenericReader<TextMessageData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002ETextMessageDataFishNet_002ESerializing_002EGenerateds;
		GenericReader<TextMessageData[]>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<TextResponseData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002ETextResponseDataFishNet_002ESerializing_002EGenerateds;
		GenericReader<TextResponseData[]>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<Response>.Read = Read___ScheduleOne_002EMessaging_002EResponseFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<Response>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<NetworkObject>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<AdvancedTransitRouteData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteDataFishNet_002ESerializing_002EGenerateds;
		GenericReader<ManagementItemFilter.EMode>.Read = Read___ScheduleOne_002EManagement_002EManagementItemFilter_002FEModeFishNet_002ESerializing_002EGenerateds;
		GenericReader<AdvancedTransitRouteData[]>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteData_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<ERank>.Read = Read___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerateds;
		GenericReader<EMapRegion>.Read = Read___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<EMapRegion>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMap_002EEMapRegion_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<FullRank>.Read = Read___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerateds;
		GenericReader<PlayerData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerateds;
		GenericReader<VariableData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EVariableDataFishNet_002ESerializing_002EGenerateds;
		GenericReader<VariableData[]>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<AvatarSettings>.Read = Read___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerateds;
		GenericReader<Eye.EyeLidConfiguration>.Read = Read___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerateds;
		GenericReader<AvatarSettings.LayerSetting>.Read = Read___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSettingFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<AvatarSettings.LayerSetting>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<AvatarSettings.AccessorySetting>.Read = Read___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySettingFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<AvatarSettings.AccessorySetting>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<BasicAvatarSettings>.Read = Read___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerateds;
		GenericReader<Impact>.Read = Read___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerateds;
		GenericReader<EImpactType>.Read = Read___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerateds;
		GenericReader<EExplosionType>.Read = Read___ScheduleOne_002ECombat_002EEExplosionTypeFishNet_002ESerializing_002EGenerateds;
		GenericReader<SlotFilter>.Read = Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds;
		GenericReader<SlotFilter.EType>.Read = Read___ScheduleOne_002EItemFramework_002ESlotFilter_002FETypeFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<EQuality>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EEQuality_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<PlayerCrimeData.EPursuitLevel>.Read = Read___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerateds;
		GenericReader<SprayStroke>.Read = Read___ScheduleOne_002EGraffiti_002ESprayStrokeFishNet_002ESerializing_002EGenerateds;
		GenericReader<UShort2>.Read = Read___ScheduleOne_002EGraffiti_002EUShort2FishNet_002ESerializing_002EGenerateds;
		GenericReader<ESprayColor>.Read = Read___ScheduleOne_002EGraffiti_002EESprayColorFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<SprayStroke>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EGraffiti_002ESprayStroke_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<SprayStroke[]>.Read = Read___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<LandVehicle>.Read = Read___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerateds;
		GenericReader<EVOLineType>.Read = Read___ScheduleOne_002EVoiceOver_002EEVOLineTypeFishNet_002ESerializing_002EGenerateds;
		GenericReader<Property>.Read = Read___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerateds;
		GenericReader<EEmployeeType>.Read = Read___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerateds;
		GenericReader<EDealWindow>.Read = Read___ScheduleOne_002EEconomy_002EEDealWindowFishNet_002ESerializing_002EGenerateds;
		GenericReader<HandoverScreen.EHandoverOutcome>.Read = Read___ScheduleOne_002EUI_002EHandover_002EHandoverScreen_002FEHandoverOutcomeFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<ItemInstance>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EItemInstance_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<ScheduleOne.Persistence.Datas.CustomerData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002ECustomerDataFishNet_002ESerializing_002EGenerateds;
		GenericReader<float[]>.Read = Read___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<EDrugType>.Read = Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds;
		GenericReader<EDoorSide>.Read = Read___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerateds;
		GenericReader<DeliveryInstance>.Read = Read___ScheduleOne_002EDelivery_002EDeliveryInstanceFishNet_002ESerializing_002EGenerateds;
		GenericReader<StringIntPair>.Read = Read___ScheduleOne_002EDevUtilities_002EStringIntPairFishNet_002ESerializing_002EGenerateds;
		GenericReader<StringIntPair[]>.Read = Read___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<EDeliveryStatus>.Read = Read___ScheduleOne_002EDelivery_002EEDeliveryStatusFishNet_002ESerializing_002EGenerateds;
		GenericReader<DeliveryReceipt>.Read = Read___ScheduleOne_002EDelivery_002EDeliveryReceiptFishNet_002ESerializing_002EGenerateds;
		GenericReader<PlayingCard.ECardSuit>.Read = Read___ScheduleOne_002ECasino_002EPlayingCard_002FECardSuitFishNet_002ESerializing_002EGenerateds;
		GenericReader<PlayingCard.ECardValue>.Read = Read___ScheduleOne_002ECasino_002EPlayingCard_002FECardValueFishNet_002ESerializing_002EGenerateds;
		GenericReader<NetworkObject[]>.Read = Read___FishNet_002EObject_002ENetworkObject_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<RTBGameController.EStage>.Read = Read___ScheduleOne_002ECasino_002ERTBGameController_002FEStageFishNet_002ESerializing_002EGenerateds;
		GenericReader<SlotMachine.ESymbol>.Read = Read___ScheduleOne_002ECasino_002ESlotMachine_002FESymbolFishNet_002ESerializing_002EGenerateds;
		GenericReader<SlotMachine.ESymbol[]>.Read = Read___ScheduleOne_002ECasino_002ESlotMachine_002FESymbol_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<CheckpointManager.ECheckpointLocation>.Read = Read___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerateds;
		GenericReader<ECartelStatus>.Read = Read___ECartelStatusFishNet_002ESerializing_002EGenerateds;
		GenericReader<CartelGoonAppearance>.Read = Read___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerateds;
		GenericReader<CartelDealInfo>.Read = Read___ScheduleOne_002ECartel_002ECartelDealInfoFishNet_002ESerializing_002EGenerateds;
		GenericReader<CartelDealInfo.EStatus>.Read = Read___ScheduleOne_002ECartel_002ECartelDealInfo_002FEStatusFishNet_002ESerializing_002EGenerateds;
		GenericReader<TrashContentData>.Read = Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds;
		GenericReader<string[]>.Read = Read___System_002EString_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<int[]>.Read = Read___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerateds;
		GenericReader<ContractReceipt>.Read = Read___ScheduleOne_002EEconomy_002EContractReceiptFishNet_002ESerializing_002EGenerateds;
		GenericReader<EContractParty>.Read = Read___ScheduleOne_002EEconomy_002EEContractPartyFishNet_002ESerializing_002EGenerateds;
		GenericReader<WeedAppearanceSettings>.Read = Read___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerateds;
		GenericReader<CocaineAppearanceSettings>.Read = Read___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerateds;
		GenericReader<MethAppearanceSettings>.Read = Read___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerateds;
		GenericReader<ShroomAppearanceSettings>.Read = Read___ScheduleOne_002EProduct_002EShroomAppearanceSettingsFishNet_002ESerializing_002EGenerateds;
		GenericReader<NewMixOperation>.Read = Read___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerateds;
		GenericReader<EquippedItemHandler>.Read = Read___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerateds;
		GenericReader<Jukebox.JukeboxState>.Read = Read___ScheduleOne_002EObjectScripts_002EJukebox_002FJukeboxStateFishNet_002ESerializing_002EGenerateds;
		GenericReader<Jukebox.ERepeatMode>.Read = Read___ScheduleOne_002EObjectScripts_002EJukebox_002FERepeatModeFishNet_002ESerializing_002EGenerateds;
		GenericReader<ChemistryCookOperation>.Read = Read___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerateds;
		GenericReader<DryingOperation>.Read = Read___ScheduleOne_002EObjectScripts_002EDryingOperationFishNet_002ESerializing_002EGenerateds;
		GenericReader<OvenCookOperation>.Read = Read___ScheduleOne_002EObjectScripts_002EOvenCookOperationFishNet_002ESerializing_002EGenerateds;
		GenericReader<MixOperation>.Read = Read___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerateds;
		GenericReader<CoordinateProceduralTilePair>.Read = Read___ScheduleOne_002ETiles_002ECoordinateProceduralTilePairFishNet_002ESerializing_002EGenerateds;
		GenericReader<Coordinate>.Read = Read___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerateds;
		GenericReader<List<CoordinateProceduralTilePair>>.Read = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds;
		GenericReader<Recycler.EState>.Read = Read___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerateds;
		GenericReader<GameData>.Read = Read___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerateds;
		GenericReader<GameSettings>.Read = Read___ScheduleOne_002EDevUtilities_002EGameSettingsFishNet_002ESerializing_002EGenerateds;
		GenericReader<WeatherVolume>.Read = Read___ScheduleOne_002EWeather_002EWeatherVolumeFishNet_002ESerializing_002EGenerateds;
		GenericReader<ExplosionData>.Read = Read___ScheduleOne_002ECombat_002EExplosionDataFishNet_002ESerializing_002EGenerateds;
	}

	public static EVehicleColor Read___ScheduleOne_002EVehicles_002EModification_002EEVehicleColorFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EVehicleColor)reader.ReadInt32((AutoPackType)1);
	}

	public static ParkData Read___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		ParkData parkData = new ParkData();
		parkData.lotGUID = reader.ReadGuid();
		parkData.spotIndex = reader.ReadInt32((AutoPackType)1);
		parkData.alignment = Read___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerateds(reader);
		return parkData;
	}

	public static EParkingAlignment Read___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EParkingAlignment)reader.ReadInt32((AutoPackType)1);
	}

	public static AirConditioner.EMode Read___ScheduleOne_002ETemperature_002EAirConditioner_002FEModeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (AirConditioner.EMode)reader.ReadInt32((AutoPackType)1);
	}

	public static ContractInfo Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		ContractInfo contractInfo = new ContractInfo();
		contractInfo.Payment = reader.ReadSingle((AutoPackType)0);
		contractInfo.Products = Read___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerateds(reader);
		contractInfo.DeliveryLocationGUID = reader.ReadString();
		contractInfo.DeliveryWindow = Read___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerateds(reader);
		contractInfo.Expires = reader.ReadBoolean();
		contractInfo.ExpiresAfter = reader.ReadInt32((AutoPackType)1);
		contractInfo.PickupScheduleIndex = reader.ReadInt32((AutoPackType)1);
		contractInfo.IsCounterOffer = reader.ReadBoolean();
		return contractInfo;
	}

	public static ProductList Read___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		ProductList productList = new ProductList();
		productList.entries = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerateds(reader);
		return productList;
	}

	public static ProductList.Entry Read___ScheduleOne_002EProduct_002EProductList_002FEntryFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		ProductList.Entry entry = new ProductList.Entry();
		entry.ProductID = reader.ReadString();
		entry.Quality = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(reader);
		entry.Quantity = reader.ReadInt32((AutoPackType)1);
		return entry;
	}

	public static EQuality Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EQuality)reader.ReadInt32((AutoPackType)1);
	}

	public static List<ProductList.Entry> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<ProductList.Entry>();
	}

	public static QuestWindowConfig Read___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		QuestWindowConfig questWindowConfig = new QuestWindowConfig();
		questWindowConfig.IsEnabled = reader.ReadBoolean();
		questWindowConfig.WindowStartTime = reader.ReadInt32((AutoPackType)1);
		questWindowConfig.WindowEndTime = reader.ReadInt32((AutoPackType)1);
		return questWindowConfig;
	}

	public static GameDateTime Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return new GameDateTime
		{
			elapsedDays = reader.ReadInt32((AutoPackType)1),
			time = reader.ReadInt32((AutoPackType)1)
		};
	}

	public static QuestManager.EQuestAction Read___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (QuestManager.EQuestAction)reader.ReadInt32((AutoPackType)1);
	}

	public static EQuestState Read___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EQuestState)reader.ReadInt32((AutoPackType)1);
	}

	public static Manor.EManorState Read___ScheduleOne_002EProperty_002EManor_002FEManorStateFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (Manor.EManorState)reader.ReadInt32((AutoPackType)1);
	}

	public static EVisualState Read___ScheduleOne_002EVision_002EEVisualStateFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EVisualState)reader.ReadInt32((AutoPackType)1);
	}

	public static VisionEventReceipt Read___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		VisionEventReceipt visionEventReceipt = new VisionEventReceipt();
		visionEventReceipt.Target = reader.ReadNetworkObject();
		visionEventReceipt.State = Read___ScheduleOne_002EVision_002EEVisualStateFishNet_002ESerializing_002EGenerateds(reader);
		return visionEventReceipt;
	}

	public static VisionCone.EEventLevel Read___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (VisionCone.EEventLevel)reader.ReadInt32((AutoPackType)1);
	}

	public static Message Read___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		Message message = new Message();
		message.messageId = reader.ReadInt32((AutoPackType)1);
		message.text = reader.ReadString();
		message.sender = Read___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerateds(reader);
		message.endOfGroup = reader.ReadBoolean();
		return message;
	}

	public static Message.ESenderType Read___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (Message.ESenderType)reader.ReadInt32((AutoPackType)1);
	}

	public static MessageChain Read___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		MessageChain messageChain = new MessageChain();
		messageChain.Messages = Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds(reader);
		messageChain.id = reader.ReadInt32((AutoPackType)1);
		return messageChain;
	}

	public static List<string> Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<string>();
	}

	public static MSGConversationData Read___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		MSGConversationData mSGConversationData = new MSGConversationData();
		mSGConversationData.ConversationIndex = reader.ReadInt32((AutoPackType)1);
		mSGConversationData.Read = reader.ReadBoolean();
		mSGConversationData.MessageHistory = Read___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
		mSGConversationData.ActiveResponses = Read___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
		mSGConversationData.IsHidden = reader.ReadBoolean();
		mSGConversationData.DataType = reader.ReadString();
		mSGConversationData.DataVersion = reader.ReadInt32((AutoPackType)1);
		mSGConversationData.GameVersion = reader.ReadString();
		return mSGConversationData;
	}

	public static TextMessageData Read___ScheduleOne_002EPersistence_002EDatas_002ETextMessageDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		TextMessageData textMessageData = new TextMessageData();
		textMessageData.Sender = reader.ReadInt32((AutoPackType)1);
		textMessageData.MessageID = reader.ReadInt32((AutoPackType)1);
		textMessageData.Text = reader.ReadString();
		textMessageData.EndOfChain = reader.ReadBoolean();
		return textMessageData;
	}

	public static TextMessageData[] Read___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<TextMessageData>();
	}

	public static TextResponseData Read___ScheduleOne_002EPersistence_002EDatas_002ETextResponseDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		TextResponseData textResponseData = new TextResponseData();
		textResponseData.Text = reader.ReadString();
		textResponseData.Label = reader.ReadString();
		return textResponseData;
	}

	public static TextResponseData[] Read___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<TextResponseData>();
	}

	public static Response Read___ScheduleOne_002EMessaging_002EResponseFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		Response response = new Response();
		response.text = reader.ReadString();
		response.label = reader.ReadString();
		response.disableDefaultResponseBehaviour = reader.ReadBoolean();
		return response;
	}

	public static List<Response> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<Response>();
	}

	public static List<NetworkObject> Read___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<NetworkObject>();
	}

	public static AdvancedTransitRouteData Read___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		AdvancedTransitRouteData advancedTransitRouteData = new AdvancedTransitRouteData();
		advancedTransitRouteData.SourceGUID = reader.ReadString();
		advancedTransitRouteData.DestinationGUID = reader.ReadString();
		advancedTransitRouteData.FilterMode = Read___ScheduleOne_002EManagement_002EManagementItemFilter_002FEModeFishNet_002ESerializing_002EGenerateds(reader);
		advancedTransitRouteData.FilterItemIDs = Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds(reader);
		return advancedTransitRouteData;
	}

	public static ManagementItemFilter.EMode Read___ScheduleOne_002EManagement_002EManagementItemFilter_002FEModeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (ManagementItemFilter.EMode)reader.ReadInt32((AutoPackType)1);
	}

	public static AdvancedTransitRouteData[] Read___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteData_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<AdvancedTransitRouteData>();
	}

	public static ERank Read___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (ERank)reader.ReadInt32((AutoPackType)1);
	}

	public static EMapRegion Read___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EMapRegion)reader.ReadInt32((AutoPackType)1);
	}

	public static List<EMapRegion> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMap_002EEMapRegion_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<EMapRegion>();
	}

	public static FullRank Read___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return new FullRank
		{
			Rank = Read___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerateds(reader),
			Tier = reader.ReadInt32((AutoPackType)1)
		};
	}

	public static PlayerData Read___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (reader.ReadBoolean())
		{
			return null;
		}
		PlayerData playerData = new PlayerData();
		playerData.PlayerCode = reader.ReadString();
		playerData.Position = reader.ReadVector3();
		playerData.Rotation = reader.ReadSingle((AutoPackType)0);
		playerData.IntroCompleted = reader.ReadBoolean();
		playerData.DataType = reader.ReadString();
		playerData.DataVersion = reader.ReadInt32((AutoPackType)1);
		playerData.GameVersion = reader.ReadString();
		return playerData;
	}

	public static VariableData Read___ScheduleOne_002EPersistence_002EDatas_002EVariableDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		VariableData variableData = new VariableData();
		variableData.Name = reader.ReadString();
		variableData.Value = reader.ReadString();
		variableData.DataType = reader.ReadString();
		variableData.DataVersion = reader.ReadInt32((AutoPackType)1);
		variableData.GameVersion = reader.ReadString();
		return variableData;
	}

	public static VariableData[] Read___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<VariableData>();
	}

	public static AvatarSettings Read___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		if (reader.ReadBoolean())
		{
			return null;
		}
		AvatarSettings avatarSettings = ScriptableObject.CreateInstance<AvatarSettings>();
		avatarSettings.SkinColor = reader.ReadColor((AutoPackType)1);
		avatarSettings.Height = reader.ReadSingle((AutoPackType)0);
		avatarSettings.Gender = reader.ReadSingle((AutoPackType)0);
		avatarSettings.Weight = reader.ReadSingle((AutoPackType)0);
		avatarSettings.HairPath = reader.ReadString();
		avatarSettings.HairColor = reader.ReadColor((AutoPackType)1);
		avatarSettings.EyebrowScale = reader.ReadSingle((AutoPackType)0);
		avatarSettings.EyebrowThickness = reader.ReadSingle((AutoPackType)0);
		avatarSettings.EyebrowRestingHeight = reader.ReadSingle((AutoPackType)0);
		avatarSettings.EyebrowRestingAngle = reader.ReadSingle((AutoPackType)0);
		avatarSettings.LeftEyeLidColor = reader.ReadColor((AutoPackType)1);
		avatarSettings.RightEyeLidColor = reader.ReadColor((AutoPackType)1);
		avatarSettings.LeftEyeRestingState = Read___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerateds(reader);
		avatarSettings.RightEyeRestingState = Read___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerateds(reader);
		avatarSettings.EyeballMaterialIdentifier = reader.ReadString();
		avatarSettings.EyeBallTint = reader.ReadColor((AutoPackType)1);
		avatarSettings.PupilDilation = reader.ReadSingle((AutoPackType)0);
		avatarSettings.FaceLayerSettings = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerateds(reader);
		avatarSettings.BodyLayerSettings = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerateds(reader);
		avatarSettings.AccessorySettings = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerateds(reader);
		return avatarSettings;
	}

	public static Eye.EyeLidConfiguration Read___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return new Eye.EyeLidConfiguration
		{
			topLidOpen = reader.ReadSingle((AutoPackType)0),
			bottomLidOpen = reader.ReadSingle((AutoPackType)0)
		};
	}

	public static AvatarSettings.LayerSetting Read___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSettingFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return new AvatarSettings.LayerSetting
		{
			layerPath = reader.ReadString(),
			layerTint = reader.ReadColor((AutoPackType)1)
		};
	}

	public static List<AvatarSettings.LayerSetting> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<AvatarSettings.LayerSetting>();
	}

	public static AvatarSettings.AccessorySetting Read___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySettingFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (reader.ReadBoolean())
		{
			return null;
		}
		AvatarSettings.AccessorySetting accessorySetting = new AvatarSettings.AccessorySetting();
		accessorySetting.path = reader.ReadString();
		accessorySetting.color = reader.ReadColor((AutoPackType)1);
		return accessorySetting;
	}

	public static List<AvatarSettings.AccessorySetting> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<AvatarSettings.AccessorySetting>();
	}

	public static BasicAvatarSettings Read___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		if (reader.ReadBoolean())
		{
			return null;
		}
		BasicAvatarSettings basicAvatarSettings = ScriptableObject.CreateInstance<BasicAvatarSettings>();
		basicAvatarSettings.Gender = reader.ReadInt32((AutoPackType)1);
		basicAvatarSettings.Weight = reader.ReadSingle((AutoPackType)0);
		basicAvatarSettings.SkinColor = reader.ReadColor((AutoPackType)1);
		basicAvatarSettings.HairStyle = reader.ReadString();
		basicAvatarSettings.HairColor = reader.ReadColor((AutoPackType)1);
		basicAvatarSettings.Mouth = reader.ReadString();
		basicAvatarSettings.FacialHair = reader.ReadString();
		basicAvatarSettings.FacialDetails = reader.ReadString();
		basicAvatarSettings.FacialDetailsIntensity = reader.ReadSingle((AutoPackType)0);
		basicAvatarSettings.EyeballColor = reader.ReadColor((AutoPackType)1);
		basicAvatarSettings.UpperEyeLidRestingPosition = reader.ReadSingle((AutoPackType)0);
		basicAvatarSettings.LowerEyeLidRestingPosition = reader.ReadSingle((AutoPackType)0);
		basicAvatarSettings.PupilDilation = reader.ReadSingle((AutoPackType)0);
		basicAvatarSettings.EyebrowScale = reader.ReadSingle((AutoPackType)0);
		basicAvatarSettings.EyebrowThickness = reader.ReadSingle((AutoPackType)0);
		basicAvatarSettings.EyebrowRestingHeight = reader.ReadSingle((AutoPackType)0);
		basicAvatarSettings.EyebrowRestingAngle = reader.ReadSingle((AutoPackType)0);
		basicAvatarSettings.Top = reader.ReadString();
		basicAvatarSettings.TopColor = reader.ReadColor((AutoPackType)1);
		basicAvatarSettings.Bottom = reader.ReadString();
		basicAvatarSettings.BottomColor = reader.ReadColor((AutoPackType)1);
		basicAvatarSettings.Shoes = reader.ReadString();
		basicAvatarSettings.ShoesColor = reader.ReadColor((AutoPackType)1);
		basicAvatarSettings.Headwear = reader.ReadString();
		basicAvatarSettings.HeadwearColor = reader.ReadColor((AutoPackType)1);
		basicAvatarSettings.Eyewear = reader.ReadString();
		basicAvatarSettings.EyewearColor = reader.ReadColor((AutoPackType)1);
		basicAvatarSettings.Tattoos = Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds(reader);
		return basicAvatarSettings;
	}

	public static Impact Read___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (reader.ReadBoolean())
		{
			return null;
		}
		Impact impact = new Impact();
		impact.HitPoint = reader.ReadVector3();
		impact.ImpactForceDirection = reader.ReadVector3();
		impact.ImpactForce = reader.ReadSingle((AutoPackType)0);
		impact.ImpactDamage = reader.ReadSingle((AutoPackType)0);
		impact.ImpactType = Read___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerateds(reader);
		impact.ImpactSource = reader.ReadNetworkObject();
		impact.ImpactID = reader.ReadInt32((AutoPackType)1);
		impact.ExplosionType = Read___ScheduleOne_002ECombat_002EEExplosionTypeFishNet_002ESerializing_002EGenerateds(reader);
		return impact;
	}

	public static EImpactType Read___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EImpactType)reader.ReadInt32((AutoPackType)1);
	}

	public static EExplosionType Read___ScheduleOne_002ECombat_002EEExplosionTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EExplosionType)reader.ReadInt32((AutoPackType)1);
	}

	public static SlotFilter Read___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		SlotFilter slotFilter = new SlotFilter();
		slotFilter.Type = Read___ScheduleOne_002EItemFramework_002ESlotFilter_002FETypeFishNet_002ESerializing_002EGenerateds(reader);
		slotFilter.ItemIDs = Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds(reader);
		slotFilter.AllowedQualities = Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EEQuality_003EFishNet_002ESerializing_002EGenerateds(reader);
		return slotFilter;
	}

	public static SlotFilter.EType Read___ScheduleOne_002EItemFramework_002ESlotFilter_002FETypeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (SlotFilter.EType)reader.ReadInt32((AutoPackType)1);
	}

	public static List<EQuality> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EEQuality_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<EQuality>();
	}

	public static PlayerCrimeData.EPursuitLevel Read___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (PlayerCrimeData.EPursuitLevel)reader.ReadInt32((AutoPackType)1);
	}

	public static SprayStroke Read___ScheduleOne_002EGraffiti_002ESprayStrokeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		SprayStroke sprayStroke = new SprayStroke();
		sprayStroke.Start = Read___ScheduleOne_002EGraffiti_002EUShort2FishNet_002ESerializing_002EGenerateds(reader);
		sprayStroke.End = Read___ScheduleOne_002EGraffiti_002EUShort2FishNet_002ESerializing_002EGenerateds(reader);
		sprayStroke.Color = Read___ScheduleOne_002EGraffiti_002EESprayColorFishNet_002ESerializing_002EGenerateds(reader);
		sprayStroke.StrokeSize = reader.ReadByte();
		return sprayStroke;
	}

	public static UShort2 Read___ScheduleOne_002EGraffiti_002EUShort2FishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return new UShort2
		{
			X = reader.ReadUInt16(),
			Y = reader.ReadUInt16()
		};
	}

	public static ESprayColor Read___ScheduleOne_002EGraffiti_002EESprayColorFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (ESprayColor)reader.ReadByte();
	}

	public static List<SprayStroke> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EGraffiti_002ESprayStroke_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<SprayStroke>();
	}

	public static SprayStroke[] Read___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<SprayStroke>();
	}

	public static LandVehicle Read___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (LandVehicle)(object)reader.ReadNetworkBehaviour();
	}

	public static EVOLineType Read___ScheduleOne_002EVoiceOver_002EEVOLineTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EVOLineType)reader.ReadInt32((AutoPackType)1);
	}

	public static Property Read___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (Property)(object)reader.ReadNetworkBehaviour();
	}

	public static EEmployeeType Read___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EEmployeeType)reader.ReadInt32((AutoPackType)1);
	}

	public static EDealWindow Read___ScheduleOne_002EEconomy_002EEDealWindowFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EDealWindow)reader.ReadInt32((AutoPackType)1);
	}

	public static HandoverScreen.EHandoverOutcome Read___ScheduleOne_002EUI_002EHandover_002EHandoverScreen_002FEHandoverOutcomeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (HandoverScreen.EHandoverOutcome)reader.ReadInt32((AutoPackType)1);
	}

	public static List<ItemInstance> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EItemInstance_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<ItemInstance>();
	}

	public static ScheduleOne.Persistence.Datas.CustomerData Read___ScheduleOne_002EPersistence_002EDatas_002ECustomerDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		ScheduleOne.Persistence.Datas.CustomerData customerData = new ScheduleOne.Persistence.Datas.CustomerData();
		customerData.Dependence = reader.ReadSingle((AutoPackType)0);
		customerData.ProductAffinities = Read___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
		customerData.TimeSinceLastDealCompleted = reader.ReadInt32((AutoPackType)1);
		customerData.TimeSinceLastDealOffered = reader.ReadInt32((AutoPackType)1);
		customerData.OfferedDeals = reader.ReadInt32((AutoPackType)1);
		customerData.CompletedDeals = reader.ReadInt32((AutoPackType)1);
		customerData.IsContractOffered = reader.ReadBoolean();
		customerData.OfferedContract = Read___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerateds(reader);
		customerData.OfferedContractTime = Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds(reader);
		customerData.TimeSincePlayerApproached = reader.ReadInt32((AutoPackType)1);
		customerData.TimeSinceInstantDealOffered = reader.ReadInt32((AutoPackType)1);
		customerData.HasBeenRecommended = reader.ReadBoolean();
		customerData.DataType = reader.ReadString();
		customerData.DataVersion = reader.ReadInt32((AutoPackType)1);
		customerData.GameVersion = reader.ReadString();
		return customerData;
	}

	public static float[] Read___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<float>();
	}

	public static EDrugType Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EDrugType)reader.ReadInt32((AutoPackType)1);
	}

	public static EDoorSide Read___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EDoorSide)reader.ReadInt32((AutoPackType)1);
	}

	public static DeliveryInstance Read___ScheduleOne_002EDelivery_002EDeliveryInstanceFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		DeliveryInstance deliveryInstance = new DeliveryInstance();
		deliveryInstance.DeliveryID = reader.ReadString();
		deliveryInstance.StoreName = reader.ReadString();
		deliveryInstance.DestinationCode = reader.ReadString();
		deliveryInstance.LoadingDockIndex = reader.ReadInt32((AutoPackType)1);
		deliveryInstance.Items = Read___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
		deliveryInstance.Status = Read___ScheduleOne_002EDelivery_002EEDeliveryStatusFishNet_002ESerializing_002EGenerateds(reader);
		deliveryInstance.TimeUntilArrival = reader.ReadInt32((AutoPackType)1);
		return deliveryInstance;
	}

	public static StringIntPair Read___ScheduleOne_002EDevUtilities_002EStringIntPairFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		StringIntPair stringIntPair = new StringIntPair();
		stringIntPair.String = reader.ReadString();
		stringIntPair.Int = reader.ReadInt32((AutoPackType)1);
		return stringIntPair;
	}

	public static StringIntPair[] Read___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<StringIntPair>();
	}

	public static EDeliveryStatus Read___ScheduleOne_002EDelivery_002EEDeliveryStatusFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EDeliveryStatus)reader.ReadInt32((AutoPackType)1);
	}

	public static DeliveryReceipt Read___ScheduleOne_002EDelivery_002EDeliveryReceiptFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		DeliveryReceipt deliveryReceipt = new DeliveryReceipt();
		deliveryReceipt.DeliveryID = reader.ReadString();
		deliveryReceipt.StoreName = reader.ReadString();
		deliveryReceipt.DestinationCode = reader.ReadString();
		deliveryReceipt.LoadingDockIndex = reader.ReadInt32((AutoPackType)1);
		deliveryReceipt.Items = Read___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
		return deliveryReceipt;
	}

	public static PlayingCard.ECardSuit Read___ScheduleOne_002ECasino_002EPlayingCard_002FECardSuitFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (PlayingCard.ECardSuit)reader.ReadInt32((AutoPackType)1);
	}

	public static PlayingCard.ECardValue Read___ScheduleOne_002ECasino_002EPlayingCard_002FECardValueFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (PlayingCard.ECardValue)reader.ReadInt32((AutoPackType)1);
	}

	public static NetworkObject[] Read___FishNet_002EObject_002ENetworkObject_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<NetworkObject>();
	}

	public static RTBGameController.EStage Read___ScheduleOne_002ECasino_002ERTBGameController_002FEStageFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (RTBGameController.EStage)reader.ReadInt32((AutoPackType)1);
	}

	public static SlotMachine.ESymbol Read___ScheduleOne_002ECasino_002ESlotMachine_002FESymbolFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (SlotMachine.ESymbol)reader.ReadInt32((AutoPackType)1);
	}

	public static SlotMachine.ESymbol[] Read___ScheduleOne_002ECasino_002ESlotMachine_002FESymbol_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<SlotMachine.ESymbol>();
	}

	public static CheckpointManager.ECheckpointLocation Read___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (CheckpointManager.ECheckpointLocation)reader.ReadInt32((AutoPackType)1);
	}

	public static ECartelStatus Read___ECartelStatusFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (ECartelStatus)reader.ReadInt32((AutoPackType)1);
	}

	public static CartelGoonAppearance Read___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (reader.ReadBoolean())
		{
			return null;
		}
		CartelGoonAppearance cartelGoonAppearance = new CartelGoonAppearance();
		cartelGoonAppearance.IsMale = reader.ReadBoolean();
		cartelGoonAppearance.BaseAppearanceIndex = reader.ReadInt32((AutoPackType)1);
		cartelGoonAppearance.SkinColor = reader.ReadColor((AutoPackType)1);
		cartelGoonAppearance.HairColor = reader.ReadColor((AutoPackType)1);
		cartelGoonAppearance.ClothingIndex = reader.ReadInt32((AutoPackType)1);
		cartelGoonAppearance.VoiceIndex = reader.ReadInt32((AutoPackType)1);
		return cartelGoonAppearance;
	}

	public static CartelDealInfo Read___ScheduleOne_002ECartel_002ECartelDealInfoFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		CartelDealInfo cartelDealInfo = new CartelDealInfo();
		cartelDealInfo.RequestedProductID = reader.ReadString();
		cartelDealInfo.RequestedProductQuantity = reader.ReadInt32((AutoPackType)1);
		cartelDealInfo.PaymentAmount = reader.ReadInt32((AutoPackType)1);
		cartelDealInfo.DueTime = Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds(reader);
		cartelDealInfo.Status = Read___ScheduleOne_002ECartel_002ECartelDealInfo_002FEStatusFishNet_002ESerializing_002EGenerateds(reader);
		return cartelDealInfo;
	}

	public static CartelDealInfo.EStatus Read___ScheduleOne_002ECartel_002ECartelDealInfo_002FEStatusFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (CartelDealInfo.EStatus)reader.ReadInt32((AutoPackType)1);
	}

	public static TrashContentData Read___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		TrashContentData trashContentData = new TrashContentData();
		trashContentData.TrashIDs = Read___System_002EString_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
		trashContentData.TrashQuantities = Read___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
		return trashContentData;
	}

	public static string[] Read___System_002EString_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<string>();
	}

	public static int[] Read___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadArrayAllocated<int>();
	}

	public static ContractReceipt Read___ScheduleOne_002EEconomy_002EContractReceiptFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		ContractReceipt contractReceipt = new ContractReceipt();
		contractReceipt.ReceiptId = reader.ReadInt32((AutoPackType)1);
		contractReceipt.CompletedBy = Read___ScheduleOne_002EEconomy_002EEContractPartyFishNet_002ESerializing_002EGenerateds(reader);
		contractReceipt.CustomerId = reader.ReadString();
		contractReceipt.CompletionTime = Read___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerateds(reader);
		contractReceipt.Items = Read___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
		contractReceipt.AmountPaid = reader.ReadSingle((AutoPackType)0);
		return contractReceipt;
	}

	public static EContractParty Read___ScheduleOne_002EEconomy_002EEContractPartyFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EContractParty)reader.ReadInt32((AutoPackType)1);
	}

	public static WeedAppearanceSettings Read___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (reader.ReadBoolean())
		{
			return null;
		}
		WeedAppearanceSettings weedAppearanceSettings = new WeedAppearanceSettings();
		weedAppearanceSettings.MainColor = reader.ReadColor32();
		weedAppearanceSettings.SecondaryColor = reader.ReadColor32();
		weedAppearanceSettings.LeafColor = reader.ReadColor32();
		weedAppearanceSettings.StemColor = reader.ReadColor32();
		return weedAppearanceSettings;
	}

	public static CocaineAppearanceSettings Read___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (reader.ReadBoolean())
		{
			return null;
		}
		CocaineAppearanceSettings cocaineAppearanceSettings = new CocaineAppearanceSettings();
		cocaineAppearanceSettings.MainColor = reader.ReadColor32();
		cocaineAppearanceSettings.SecondaryColor = reader.ReadColor32();
		return cocaineAppearanceSettings;
	}

	public static MethAppearanceSettings Read___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (reader.ReadBoolean())
		{
			return null;
		}
		MethAppearanceSettings methAppearanceSettings = new MethAppearanceSettings();
		methAppearanceSettings.MainColor = reader.ReadColor32();
		methAppearanceSettings.SecondaryColor = reader.ReadColor32();
		return methAppearanceSettings;
	}

	public static ShroomAppearanceSettings Read___ScheduleOne_002EProduct_002EShroomAppearanceSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		return new ShroomAppearanceSettings();
	}

	public static NewMixOperation Read___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		NewMixOperation newMixOperation = new NewMixOperation();
		newMixOperation.ProductID = reader.ReadString();
		newMixOperation.IngredientID = reader.ReadString();
		return newMixOperation;
	}

	public static EquippedItemHandler Read___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (EquippedItemHandler)(object)reader.ReadNetworkBehaviour();
	}

	public static Jukebox.JukeboxState Read___ScheduleOne_002EObjectScripts_002EJukebox_002FJukeboxStateFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		Jukebox.JukeboxState jukeboxState = new Jukebox.JukeboxState();
		jukeboxState.CurrentVolume = reader.ReadInt32((AutoPackType)1);
		jukeboxState.IsPlaying = reader.ReadBoolean();
		jukeboxState.CurrentTrackTime = reader.ReadSingle((AutoPackType)0);
		jukeboxState.TrackOrder = Read___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerateds(reader);
		jukeboxState.CurrentTrackOrderIndex = reader.ReadInt32((AutoPackType)1);
		jukeboxState.Shuffle = reader.ReadBoolean();
		jukeboxState.RepeatMode = Read___ScheduleOne_002EObjectScripts_002EJukebox_002FERepeatModeFishNet_002ESerializing_002EGenerateds(reader);
		jukeboxState.Sync = reader.ReadBoolean();
		return jukeboxState;
	}

	public static Jukebox.ERepeatMode Read___ScheduleOne_002EObjectScripts_002EJukebox_002FERepeatModeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (Jukebox.ERepeatMode)reader.ReadInt32((AutoPackType)1);
	}

	public static ChemistryCookOperation Read___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (reader.ReadBoolean())
		{
			return null;
		}
		ChemistryCookOperation chemistryCookOperation = new ChemistryCookOperation();
		chemistryCookOperation.RecipeID = reader.ReadString();
		chemistryCookOperation.ProductQuality = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(reader);
		chemistryCookOperation.StartLiquidColor = reader.ReadColor((AutoPackType)1);
		chemistryCookOperation.LiquidLevel = reader.ReadSingle((AutoPackType)0);
		chemistryCookOperation.CurrentTime = reader.ReadInt32((AutoPackType)1);
		return chemistryCookOperation;
	}

	public static DryingOperation Read___ScheduleOne_002EObjectScripts_002EDryingOperationFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		DryingOperation dryingOperation = new DryingOperation();
		dryingOperation.ItemID = reader.ReadString();
		dryingOperation.Quantity = reader.ReadInt32((AutoPackType)1);
		dryingOperation.StartQuality = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(reader);
		dryingOperation.Time = reader.ReadSingle((AutoPackType)0);
		return dryingOperation;
	}

	public static OvenCookOperation Read___ScheduleOne_002EObjectScripts_002EOvenCookOperationFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		OvenCookOperation ovenCookOperation = new OvenCookOperation();
		ovenCookOperation.IngredientID = reader.ReadString();
		ovenCookOperation.IngredientQuality = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(reader);
		ovenCookOperation.IngredientQuantity = reader.ReadInt32((AutoPackType)1);
		ovenCookOperation.ProductID = reader.ReadString();
		ovenCookOperation.CookProgress = reader.ReadInt32((AutoPackType)1);
		return ovenCookOperation;
	}

	public static MixOperation Read___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		MixOperation mixOperation = new MixOperation();
		mixOperation.ProductID = reader.ReadString();
		mixOperation.ProductQuality = Read___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerateds(reader);
		mixOperation.IngredientID = reader.ReadString();
		mixOperation.Quantity = reader.ReadInt32((AutoPackType)1);
		return mixOperation;
	}

	public static CoordinateProceduralTilePair Read___ScheduleOne_002ETiles_002ECoordinateProceduralTilePairFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return new CoordinateProceduralTilePair
		{
			coord = Read___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerateds(reader),
			tileParent = reader.ReadNetworkObject(),
			tileIndex = reader.ReadInt32((AutoPackType)1)
		};
	}

	public static Coordinate Read___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		Coordinate coordinate = new Coordinate();
		coordinate.x = reader.ReadInt32((AutoPackType)1);
		coordinate.y = reader.ReadInt32((AutoPackType)1);
		return coordinate;
	}

	public static List<CoordinateProceduralTilePair> Read___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return reader.ReadListAllocated<CoordinateProceduralTilePair>();
	}

	public static Recycler.EState Read___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (Recycler.EState)reader.ReadInt32((AutoPackType)1);
	}

	public static GameData Read___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		GameData gameData = new GameData();
		gameData.OrganisationName = reader.ReadString();
		gameData.Seed = reader.ReadInt32((AutoPackType)1);
		gameData.Settings = Read___ScheduleOne_002EDevUtilities_002EGameSettingsFishNet_002ESerializing_002EGenerateds(reader);
		gameData.DataType = reader.ReadString();
		gameData.DataVersion = reader.ReadInt32((AutoPackType)1);
		gameData.GameVersion = reader.ReadString();
		return gameData;
	}

	public static GameSettings Read___ScheduleOne_002EDevUtilities_002EGameSettingsFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		if (reader.ReadBoolean())
		{
			return null;
		}
		GameSettings gameSettings = new GameSettings();
		gameSettings.ConsoleEnabled = reader.ReadBoolean();
		gameSettings.UseRandomizedMixMaps = reader.ReadBoolean();
		return gameSettings;
	}

	public static WeatherVolume Read___ScheduleOne_002EWeather_002EWeatherVolumeFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return (WeatherVolume)(object)reader.ReadNetworkBehaviour();
	}

	public static ExplosionData Read___ScheduleOne_002ECombat_002EExplosionDataFishNet_002ESerializing_002EGenerateds(Reader reader)
	{
		return new ExplosionData
		{
			DamageRadius = reader.ReadSingle((AutoPackType)0),
			MaxDamage = reader.ReadSingle((AutoPackType)0),
			PushForceRadius = reader.ReadSingle((AutoPackType)0),
			MaxPushForce = reader.ReadSingle((AutoPackType)0),
			CheckLoS = reader.ReadBoolean(),
			ExplosionType = Read___ScheduleOne_002ECombat_002EEExplosionTypeFishNet_002ESerializing_002EGenerateds(reader)
		};
	}
}
