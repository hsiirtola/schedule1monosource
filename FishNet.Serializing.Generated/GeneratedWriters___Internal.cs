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
public static class GeneratedWriters___Internal
{
	[RuntimeInitializeOnLoadMethod]
	private static void InitializeOnce()
	{
		GenericWriter<ItemInstance>.Write = ItemSerializers.WriteItemInstance;
		GenericWriter<ProductItemInstance>.Write = ItemSerializers.WriteProductItemInstance;
		GenericWriter<EquippableData>.Write = EquippableDataSerializer.WriteEquippableData;
		GenericWriter<INetworkedEquippableUser>.Write = INetworkedEquippableUserSerializer.WriteINetworkedEquippableUser;
		GenericWriter<EVehicleColor>.Write = Write___ScheduleOne_002EVehicles_002EModification_002EEVehicleColorFishNet_002ESerializing_002EGenerated;
		GenericWriter<ParkData>.Write = Write___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerated;
		GenericWriter<EParkingAlignment>.Write = Write___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerated;
		GenericWriter<AirConditioner.EMode>.Write = Write___ScheduleOne_002ETemperature_002EAirConditioner_002FEModeFishNet_002ESerializing_002EGenerated;
		GenericWriter<ContractInfo>.Write = Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated;
		GenericWriter<ProductList>.Write = Write___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerated;
		GenericWriter<ProductList.Entry>.Write = Write___ScheduleOne_002EProduct_002EProductList_002FEntryFishNet_002ESerializing_002EGenerated;
		GenericWriter<EQuality>.Write = Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<ProductList.Entry>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<QuestWindowConfig>.Write = Write___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerated;
		GenericWriter<GameDateTime>.Write = Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated;
		GenericWriter<QuestManager.EQuestAction>.Write = Write___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerated;
		GenericWriter<EQuestState>.Write = Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated;
		GenericWriter<Manor.EManorState>.Write = Write___ScheduleOne_002EProperty_002EManor_002FEManorStateFishNet_002ESerializing_002EGenerated;
		GenericWriter<EVisualState>.Write = Write___ScheduleOne_002EVision_002EEVisualStateFishNet_002ESerializing_002EGenerated;
		GenericWriter<VisionEventReceipt>.Write = Write___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerated;
		GenericWriter<VisionCone.EEventLevel>.Write = Write___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerated;
		GenericWriter<Message>.Write = Write___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerated;
		GenericWriter<Message.ESenderType>.Write = Write___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerated;
		GenericWriter<MessageChain>.Write = Write___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<string>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<MSGConversationData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerated;
		GenericWriter<TextMessageData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002ETextMessageDataFishNet_002ESerializing_002EGenerated;
		GenericWriter<TextMessageData[]>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<TextResponseData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002ETextResponseDataFishNet_002ESerializing_002EGenerated;
		GenericWriter<TextResponseData[]>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<Response>.Write = Write___ScheduleOne_002EMessaging_002EResponseFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<Response>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<NetworkObject>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<AdvancedTransitRouteData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteDataFishNet_002ESerializing_002EGenerated;
		GenericWriter<ManagementItemFilter.EMode>.Write = Write___ScheduleOne_002EManagement_002EManagementItemFilter_002FEModeFishNet_002ESerializing_002EGenerated;
		GenericWriter<AdvancedTransitRouteData[]>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteData_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<ERank>.Write = Write___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerated;
		GenericWriter<EMapRegion>.Write = Write___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<EMapRegion>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMap_002EEMapRegion_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<FullRank>.Write = Write___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerated;
		GenericWriter<PlayerData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerated;
		GenericWriter<VariableData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EVariableDataFishNet_002ESerializing_002EGenerated;
		GenericWriter<VariableData[]>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<AvatarSettings>.Write = Write___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerated;
		GenericWriter<Eye.EyeLidConfiguration>.Write = Write___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerated;
		GenericWriter<AvatarSettings.LayerSetting>.Write = Write___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSettingFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<AvatarSettings.LayerSetting>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<AvatarSettings.AccessorySetting>.Write = Write___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySettingFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<AvatarSettings.AccessorySetting>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<BasicAvatarSettings>.Write = Write___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerated;
		GenericWriter<Impact>.Write = Write___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerated;
		GenericWriter<EImpactType>.Write = Write___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerated;
		GenericWriter<EExplosionType>.Write = Write___ScheduleOne_002ECombat_002EEExplosionTypeFishNet_002ESerializing_002EGenerated;
		GenericWriter<SlotFilter>.Write = Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated;
		GenericWriter<SlotFilter.EType>.Write = Write___ScheduleOne_002EItemFramework_002ESlotFilter_002FETypeFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<EQuality>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EEQuality_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<PlayerCrimeData.EPursuitLevel>.Write = Write___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerated;
		GenericWriter<SprayStroke>.Write = Write___ScheduleOne_002EGraffiti_002ESprayStrokeFishNet_002ESerializing_002EGenerated;
		GenericWriter<UShort2>.Write = Write___ScheduleOne_002EGraffiti_002EUShort2FishNet_002ESerializing_002EGenerated;
		GenericWriter<ESprayColor>.Write = Write___ScheduleOne_002EGraffiti_002EESprayColorFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<SprayStroke>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EGraffiti_002ESprayStroke_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<SprayStroke[]>.Write = Write___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<LandVehicle>.Write = Write___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerated;
		GenericWriter<EVOLineType>.Write = Write___ScheduleOne_002EVoiceOver_002EEVOLineTypeFishNet_002ESerializing_002EGenerated;
		GenericWriter<Property>.Write = Write___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerated;
		GenericWriter<EEmployeeType>.Write = Write___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerated;
		GenericWriter<EDealWindow>.Write = Write___ScheduleOne_002EEconomy_002EEDealWindowFishNet_002ESerializing_002EGenerated;
		GenericWriter<HandoverScreen.EHandoverOutcome>.Write = Write___ScheduleOne_002EUI_002EHandover_002EHandoverScreen_002FEHandoverOutcomeFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<ItemInstance>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EItemInstance_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<ScheduleOne.Persistence.Datas.CustomerData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002ECustomerDataFishNet_002ESerializing_002EGenerated;
		GenericWriter<float[]>.Write = Write___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<EDrugType>.Write = Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated;
		GenericWriter<EDoorSide>.Write = Write___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerated;
		GenericWriter<DeliveryInstance>.Write = Write___ScheduleOne_002EDelivery_002EDeliveryInstanceFishNet_002ESerializing_002EGenerated;
		GenericWriter<StringIntPair>.Write = Write___ScheduleOne_002EDevUtilities_002EStringIntPairFishNet_002ESerializing_002EGenerated;
		GenericWriter<StringIntPair[]>.Write = Write___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<EDeliveryStatus>.Write = Write___ScheduleOne_002EDelivery_002EEDeliveryStatusFishNet_002ESerializing_002EGenerated;
		GenericWriter<DeliveryReceipt>.Write = Write___ScheduleOne_002EDelivery_002EDeliveryReceiptFishNet_002ESerializing_002EGenerated;
		GenericWriter<PlayingCard.ECardSuit>.Write = Write___ScheduleOne_002ECasino_002EPlayingCard_002FECardSuitFishNet_002ESerializing_002EGenerated;
		GenericWriter<PlayingCard.ECardValue>.Write = Write___ScheduleOne_002ECasino_002EPlayingCard_002FECardValueFishNet_002ESerializing_002EGenerated;
		GenericWriter<NetworkObject[]>.Write = Write___FishNet_002EObject_002ENetworkObject_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<RTBGameController.EStage>.Write = Write___ScheduleOne_002ECasino_002ERTBGameController_002FEStageFishNet_002ESerializing_002EGenerated;
		GenericWriter<SlotMachine.ESymbol>.Write = Write___ScheduleOne_002ECasino_002ESlotMachine_002FESymbolFishNet_002ESerializing_002EGenerated;
		GenericWriter<SlotMachine.ESymbol[]>.Write = Write___ScheduleOne_002ECasino_002ESlotMachine_002FESymbol_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<CheckpointManager.ECheckpointLocation>.Write = Write___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerated;
		GenericWriter<ECartelStatus>.Write = Write___ECartelStatusFishNet_002ESerializing_002EGenerated;
		GenericWriter<CartelGoonAppearance>.Write = Write___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerated;
		GenericWriter<CartelDealInfo>.Write = Write___ScheduleOne_002ECartel_002ECartelDealInfoFishNet_002ESerializing_002EGenerated;
		GenericWriter<CartelDealInfo.EStatus>.Write = Write___ScheduleOne_002ECartel_002ECartelDealInfo_002FEStatusFishNet_002ESerializing_002EGenerated;
		GenericWriter<TrashContentData>.Write = Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated;
		GenericWriter<string[]>.Write = Write___System_002EString_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<int[]>.Write = Write___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerated;
		GenericWriter<ContractReceipt>.Write = Write___ScheduleOne_002EEconomy_002EContractReceiptFishNet_002ESerializing_002EGenerated;
		GenericWriter<EContractParty>.Write = Write___ScheduleOne_002EEconomy_002EEContractPartyFishNet_002ESerializing_002EGenerated;
		GenericWriter<WeedAppearanceSettings>.Write = Write___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerated;
		GenericWriter<CocaineAppearanceSettings>.Write = Write___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerated;
		GenericWriter<MethAppearanceSettings>.Write = Write___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerated;
		GenericWriter<ShroomAppearanceSettings>.Write = Write___ScheduleOne_002EProduct_002EShroomAppearanceSettingsFishNet_002ESerializing_002EGenerated;
		GenericWriter<NewMixOperation>.Write = Write___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerated;
		GenericWriter<EquippedItemHandler>.Write = Write___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerated;
		GenericWriter<Jukebox.JukeboxState>.Write = Write___ScheduleOne_002EObjectScripts_002EJukebox_002FJukeboxStateFishNet_002ESerializing_002EGenerated;
		GenericWriter<Jukebox.ERepeatMode>.Write = Write___ScheduleOne_002EObjectScripts_002EJukebox_002FERepeatModeFishNet_002ESerializing_002EGenerated;
		GenericWriter<ChemistryCookOperation>.Write = Write___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerated;
		GenericWriter<DryingOperation>.Write = Write___ScheduleOne_002EObjectScripts_002EDryingOperationFishNet_002ESerializing_002EGenerated;
		GenericWriter<OvenCookOperation>.Write = Write___ScheduleOne_002EObjectScripts_002EOvenCookOperationFishNet_002ESerializing_002EGenerated;
		GenericWriter<MixOperation>.Write = Write___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerated;
		GenericWriter<CoordinateProceduralTilePair>.Write = Write___ScheduleOne_002ETiles_002ECoordinateProceduralTilePairFishNet_002ESerializing_002EGenerated;
		GenericWriter<Coordinate>.Write = Write___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerated;
		GenericWriter<List<CoordinateProceduralTilePair>>.Write = Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerated;
		GenericWriter<Recycler.EState>.Write = Write___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerated;
		GenericWriter<GameData>.Write = Write___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerated;
		GenericWriter<GameSettings>.Write = Write___ScheduleOne_002EDevUtilities_002EGameSettingsFishNet_002ESerializing_002EGenerated;
		GenericWriter<WeatherVolume>.Write = Write___ScheduleOne_002EWeather_002EWeatherVolumeFishNet_002ESerializing_002EGenerated;
		GenericWriter<ExplosionData>.Write = Write___ScheduleOne_002ECombat_002EExplosionDataFishNet_002ESerializing_002EGenerated;
	}

	public static void Write___ScheduleOne_002EVehicles_002EModification_002EEVehicleColorFishNet_002ESerializing_002EGenerated(this Writer writer, EVehicleColor value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EVehicles_002EParkDataFishNet_002ESerializing_002EGenerated(this Writer writer, ParkData value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteGuidAllocated(value.lotGUID);
		writer.WriteInt32(value.spotIndex, (AutoPackType)1);
		Write___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerated(writer, value.alignment);
	}

	public static void Write___ScheduleOne_002EVehicles_002EEParkingAlignmentFishNet_002ESerializing_002EGenerated(this Writer writer, EParkingAlignment value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002ETemperature_002EAirConditioner_002FEModeFishNet_002ESerializing_002EGenerated(this Writer writer, AirConditioner.EMode value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated(this Writer writer, ContractInfo value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteSingle(value.Payment, (AutoPackType)0);
		Write___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerated(writer, value.Products);
		writer.WriteString(value.DeliveryLocationGUID);
		Write___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerated(writer, value.DeliveryWindow);
		writer.WriteBoolean(value.Expires);
		writer.WriteInt32(value.ExpiresAfter, (AutoPackType)1);
		writer.WriteInt32(value.PickupScheduleIndex, (AutoPackType)1);
		writer.WriteBoolean(value.IsCounterOffer);
	}

	public static void Write___ScheduleOne_002EProduct_002EProductListFishNet_002ESerializing_002EGenerated(this Writer writer, ProductList value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerated(writer, value.entries);
	}

	public static void Write___ScheduleOne_002EProduct_002EProductList_002FEntryFishNet_002ESerializing_002EGenerated(this Writer writer, ProductList.Entry value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.ProductID);
		Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(writer, value.Quality);
		writer.WriteInt32(value.Quantity, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(this Writer writer, EQuality value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EProduct_002EProductList_002FEntry_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<ProductList.Entry> value)
	{
		writer.WriteList<ProductList.Entry>(value);
	}

	public static void Write___ScheduleOne_002EQuests_002EQuestWindowConfigFishNet_002ESerializing_002EGenerated(this Writer writer, QuestWindowConfig value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteBoolean(value.IsEnabled);
		writer.WriteInt32(value.WindowStartTime, (AutoPackType)1);
		writer.WriteInt32(value.WindowEndTime, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated(this Writer writer, GameDateTime value)
	{
		writer.WriteInt32(value.elapsedDays, (AutoPackType)1);
		writer.WriteInt32(value.time, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EQuests_002EQuestManager_002FEQuestActionFishNet_002ESerializing_002EGenerated(this Writer writer, QuestManager.EQuestAction value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EQuests_002EEQuestStateFishNet_002ESerializing_002EGenerated(this Writer writer, EQuestState value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EProperty_002EManor_002FEManorStateFishNet_002ESerializing_002EGenerated(this Writer writer, Manor.EManorState value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EVision_002EEVisualStateFishNet_002ESerializing_002EGenerated(this Writer writer, EVisualState value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EVision_002EVisionEventReceiptFishNet_002ESerializing_002EGenerated(this Writer writer, VisionEventReceipt value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteNetworkObject(value.Target);
		Write___ScheduleOne_002EVision_002EEVisualStateFishNet_002ESerializing_002EGenerated(writer, value.State);
	}

	public static void Write___ScheduleOne_002EVision_002EVisionCone_002FEEventLevelFishNet_002ESerializing_002EGenerated(this Writer writer, VisionCone.EEventLevel value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EMessaging_002EMessageFishNet_002ESerializing_002EGenerated(this Writer writer, Message value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteInt32(value.messageId, (AutoPackType)1);
		writer.WriteString(value.text);
		Write___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerated(writer, value.sender);
		writer.WriteBoolean(value.endOfGroup);
	}

	public static void Write___ScheduleOne_002EMessaging_002EMessage_002FESenderTypeFishNet_002ESerializing_002EGenerated(this Writer writer, Message.ESenderType value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EUI_002EPhone_002EMessages_002EMessageChainFishNet_002ESerializing_002EGenerated(this Writer writer, MessageChain value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated(writer, value.Messages);
		writer.WriteInt32(value.id, (AutoPackType)1);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<string> value)
	{
		writer.WriteList<string>(value);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002EMSGConversationDataFishNet_002ESerializing_002EGenerated(this Writer writer, MSGConversationData value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteInt32(value.ConversationIndex, (AutoPackType)1);
		writer.WriteBoolean(value.Read);
		Write___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.MessageHistory);
		Write___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.ActiveResponses);
		writer.WriteBoolean(value.IsHidden);
		writer.WriteString(value.DataType);
		writer.WriteInt32(value.DataVersion, (AutoPackType)1);
		writer.WriteString(value.GameVersion);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002ETextMessageDataFishNet_002ESerializing_002EGenerated(this Writer writer, TextMessageData value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteInt32(value.Sender, (AutoPackType)1);
		writer.WriteInt32(value.MessageID, (AutoPackType)1);
		writer.WriteString(value.Text);
		writer.WriteBoolean(value.EndOfChain);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002ETextMessageData_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, TextMessageData[] value)
	{
		writer.WriteArray<TextMessageData>(value);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002ETextResponseDataFishNet_002ESerializing_002EGenerated(this Writer writer, TextResponseData value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.Text);
		writer.WriteString(value.Label);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002ETextResponseData_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, TextResponseData[] value)
	{
		writer.WriteArray<TextResponseData>(value);
	}

	public static void Write___ScheduleOne_002EMessaging_002EResponseFishNet_002ESerializing_002EGenerated(this Writer writer, Response value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.text);
		writer.WriteString(value.label);
		writer.WriteBoolean(value.disableDefaultResponseBehaviour);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMessaging_002EResponse_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<Response> value)
	{
		writer.WriteList<Response>(value);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CFishNet_002EObject_002ENetworkObject_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<NetworkObject> value)
	{
		writer.WriteList<NetworkObject>(value);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteDataFishNet_002ESerializing_002EGenerated(this Writer writer, AdvancedTransitRouteData value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.SourceGUID);
		writer.WriteString(value.DestinationGUID);
		Write___ScheduleOne_002EManagement_002EManagementItemFilter_002FEModeFishNet_002ESerializing_002EGenerated(writer, value.FilterMode);
		Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated(writer, value.FilterItemIDs);
	}

	public static void Write___ScheduleOne_002EManagement_002EManagementItemFilter_002FEModeFishNet_002ESerializing_002EGenerated(this Writer writer, ManagementItemFilter.EMode value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002EAdvancedTransitRouteData_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, AdvancedTransitRouteData[] value)
	{
		writer.WriteArray<AdvancedTransitRouteData>(value);
	}

	public static void Write___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerated(this Writer writer, ERank value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EMap_002EEMapRegionFishNet_002ESerializing_002EGenerated(this Writer writer, EMapRegion value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EMap_002EEMapRegion_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<EMapRegion> value)
	{
		writer.WriteList<EMapRegion>(value);
	}

	public static void Write___ScheduleOne_002ELevelling_002EFullRankFishNet_002ESerializing_002EGenerated(this Writer writer, FullRank value)
	{
		Write___ScheduleOne_002ELevelling_002EERankFishNet_002ESerializing_002EGenerated(writer, value.Rank);
		writer.WriteInt32(value.Tier, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002EPlayerDataFishNet_002ESerializing_002EGenerated(this Writer writer, PlayerData value)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.PlayerCode);
		writer.WriteVector3(value.Position);
		writer.WriteSingle(value.Rotation, (AutoPackType)0);
		writer.WriteBoolean(value.IntroCompleted);
		writer.WriteString(value.DataType);
		writer.WriteInt32(value.DataVersion, (AutoPackType)1);
		writer.WriteString(value.GameVersion);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002EVariableDataFishNet_002ESerializing_002EGenerated(this Writer writer, VariableData value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.Name);
		writer.WriteString(value.Value);
		writer.WriteString(value.DataType);
		writer.WriteInt32(value.DataVersion, (AutoPackType)1);
		writer.WriteString(value.GameVersion);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002EVariableData_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, VariableData[] value)
	{
		writer.WriteArray<VariableData>(value);
	}

	public static void Write___ScheduleOne_002EAvatarFramework_002EAvatarSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, AvatarSettings value)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteColor(value.SkinColor, (AutoPackType)1);
		writer.WriteSingle(value.Height, (AutoPackType)0);
		writer.WriteSingle(value.Gender, (AutoPackType)0);
		writer.WriteSingle(value.Weight, (AutoPackType)0);
		writer.WriteString(value.HairPath);
		writer.WriteColor(value.HairColor, (AutoPackType)1);
		writer.WriteSingle(value.EyebrowScale, (AutoPackType)0);
		writer.WriteSingle(value.EyebrowThickness, (AutoPackType)0);
		writer.WriteSingle(value.EyebrowRestingHeight, (AutoPackType)0);
		writer.WriteSingle(value.EyebrowRestingAngle, (AutoPackType)0);
		writer.WriteColor(value.LeftEyeLidColor, (AutoPackType)1);
		writer.WriteColor(value.RightEyeLidColor, (AutoPackType)1);
		Write___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerated(writer, value.LeftEyeRestingState);
		Write___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerated(writer, value.RightEyeRestingState);
		writer.WriteString(value.EyeballMaterialIdentifier);
		writer.WriteColor(value.EyeBallTint, (AutoPackType)1);
		writer.WriteSingle(value.PupilDilation, (AutoPackType)0);
		Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerated(writer, value.FaceLayerSettings);
		Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerated(writer, value.BodyLayerSettings);
		Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerated(writer, value.AccessorySettings);
	}

	public static void Write___ScheduleOne_002EAvatarFramework_002EEye_002FEyeLidConfigurationFishNet_002ESerializing_002EGenerated(this Writer writer, Eye.EyeLidConfiguration value)
	{
		writer.WriteSingle(value.topLidOpen, (AutoPackType)0);
		writer.WriteSingle(value.bottomLidOpen, (AutoPackType)0);
	}

	public static void Write___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSettingFishNet_002ESerializing_002EGenerated(this Writer writer, AvatarSettings.LayerSetting value)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		writer.WriteString(value.layerPath);
		writer.WriteColor(value.layerTint, (AutoPackType)1);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FLayerSetting_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<AvatarSettings.LayerSetting> value)
	{
		writer.WriteList<AvatarSettings.LayerSetting>(value);
	}

	public static void Write___ScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySettingFishNet_002ESerializing_002EGenerated(this Writer writer, AvatarSettings.AccessorySetting value)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.path);
		writer.WriteColor(value.color, (AutoPackType)1);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EAvatarFramework_002EAvatarSettings_002FAccessorySetting_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<AvatarSettings.AccessorySetting> value)
	{
		writer.WriteList<AvatarSettings.AccessorySetting>(value);
	}

	public static void Write___ScheduleOne_002EAvatarFramework_002ECustomization_002EBasicAvatarSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, BasicAvatarSettings value)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteInt32(value.Gender, (AutoPackType)1);
		writer.WriteSingle(value.Weight, (AutoPackType)0);
		writer.WriteColor(value.SkinColor, (AutoPackType)1);
		writer.WriteString(value.HairStyle);
		writer.WriteColor(value.HairColor, (AutoPackType)1);
		writer.WriteString(value.Mouth);
		writer.WriteString(value.FacialHair);
		writer.WriteString(value.FacialDetails);
		writer.WriteSingle(value.FacialDetailsIntensity, (AutoPackType)0);
		writer.WriteColor(value.EyeballColor, (AutoPackType)1);
		writer.WriteSingle(value.UpperEyeLidRestingPosition, (AutoPackType)0);
		writer.WriteSingle(value.LowerEyeLidRestingPosition, (AutoPackType)0);
		writer.WriteSingle(value.PupilDilation, (AutoPackType)0);
		writer.WriteSingle(value.EyebrowScale, (AutoPackType)0);
		writer.WriteSingle(value.EyebrowThickness, (AutoPackType)0);
		writer.WriteSingle(value.EyebrowRestingHeight, (AutoPackType)0);
		writer.WriteSingle(value.EyebrowRestingAngle, (AutoPackType)0);
		writer.WriteString(value.Top);
		writer.WriteColor(value.TopColor, (AutoPackType)1);
		writer.WriteString(value.Bottom);
		writer.WriteColor(value.BottomColor, (AutoPackType)1);
		writer.WriteString(value.Shoes);
		writer.WriteColor(value.ShoesColor, (AutoPackType)1);
		writer.WriteString(value.Headwear);
		writer.WriteColor(value.HeadwearColor, (AutoPackType)1);
		writer.WriteString(value.Eyewear);
		writer.WriteColor(value.EyewearColor, (AutoPackType)1);
		Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated(writer, value.Tattoos);
	}

	public static void Write___ScheduleOne_002ECombat_002EImpactFishNet_002ESerializing_002EGenerated(this Writer writer, Impact value)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteVector3(value.HitPoint);
		writer.WriteVector3(value.ImpactForceDirection);
		writer.WriteSingle(value.ImpactForce, (AutoPackType)0);
		writer.WriteSingle(value.ImpactDamage, (AutoPackType)0);
		Write___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerated(writer, value.ImpactType);
		writer.WriteNetworkObject(value.ImpactSource);
		writer.WriteInt32(value.ImpactID, (AutoPackType)1);
		Write___ScheduleOne_002ECombat_002EEExplosionTypeFishNet_002ESerializing_002EGenerated(writer, value.ExplosionType);
	}

	public static void Write___ScheduleOne_002ECombat_002EEImpactTypeFishNet_002ESerializing_002EGenerated(this Writer writer, EImpactType value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002ECombat_002EEExplosionTypeFishNet_002ESerializing_002EGenerated(this Writer writer, EExplosionType value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EItemFramework_002ESlotFilterFishNet_002ESerializing_002EGenerated(this Writer writer, SlotFilter value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		Write___ScheduleOne_002EItemFramework_002ESlotFilter_002FETypeFishNet_002ESerializing_002EGenerated(writer, value.Type);
		Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated(writer, value.ItemIDs);
		Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EEQuality_003EFishNet_002ESerializing_002EGenerated(writer, value.AllowedQualities);
	}

	public static void Write___ScheduleOne_002EItemFramework_002ESlotFilter_002FETypeFishNet_002ESerializing_002EGenerated(this Writer writer, SlotFilter.EType value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EEQuality_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<EQuality> value)
	{
		writer.WriteList<EQuality>(value);
	}

	public static void Write___ScheduleOne_002EPlayerScripts_002EPlayerCrimeData_002FEPursuitLevelFishNet_002ESerializing_002EGenerated(this Writer writer, PlayerCrimeData.EPursuitLevel value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EGraffiti_002ESprayStrokeFishNet_002ESerializing_002EGenerated(this Writer writer, SprayStroke value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		Write___ScheduleOne_002EGraffiti_002EUShort2FishNet_002ESerializing_002EGenerated(writer, value.Start);
		Write___ScheduleOne_002EGraffiti_002EUShort2FishNet_002ESerializing_002EGenerated(writer, value.End);
		Write___ScheduleOne_002EGraffiti_002EESprayColorFishNet_002ESerializing_002EGenerated(writer, value.Color);
		writer.WriteByte(value.StrokeSize);
	}

	public static void Write___ScheduleOne_002EGraffiti_002EUShort2FishNet_002ESerializing_002EGenerated(this Writer writer, UShort2 value)
	{
		writer.WriteUInt16(value.X);
		writer.WriteUInt16(value.Y);
	}

	public static void Write___ScheduleOne_002EGraffiti_002EESprayColorFishNet_002ESerializing_002EGenerated(this Writer writer, ESprayColor value)
	{
		writer.WriteByte((byte)value);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EGraffiti_002ESprayStroke_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<SprayStroke> value)
	{
		writer.WriteList<SprayStroke>(value);
	}

	public static void Write___ScheduleOne_002EGraffiti_002ESprayStroke_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, SprayStroke[] value)
	{
		writer.WriteArray<SprayStroke>(value);
	}

	public static void Write___ScheduleOne_002EVehicles_002ELandVehicleFishNet_002ESerializing_002EGenerated(this Writer writer, LandVehicle value)
	{
		writer.WriteNetworkBehaviour((NetworkBehaviour)(object)value);
	}

	public static void Write___ScheduleOne_002EVoiceOver_002EEVOLineTypeFishNet_002ESerializing_002EGenerated(this Writer writer, EVOLineType value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EProperty_002EPropertyFishNet_002ESerializing_002EGenerated(this Writer writer, Property value)
	{
		writer.WriteNetworkBehaviour((NetworkBehaviour)(object)value);
	}

	public static void Write___ScheduleOne_002EEmployees_002EEEmployeeTypeFishNet_002ESerializing_002EGenerated(this Writer writer, EEmployeeType value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EEconomy_002EEDealWindowFishNet_002ESerializing_002EGenerated(this Writer writer, EDealWindow value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EUI_002EHandover_002EHandoverScreen_002FEHandoverOutcomeFishNet_002ESerializing_002EGenerated(this Writer writer, HandoverScreen.EHandoverOutcome value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002EItemFramework_002EItemInstance_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<ItemInstance> value)
	{
		writer.WriteList<ItemInstance>(value);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002ECustomerDataFishNet_002ESerializing_002EGenerated(this Writer writer, ScheduleOne.Persistence.Datas.CustomerData value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteSingle(value.Dependence, (AutoPackType)0);
		Write___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.ProductAffinities);
		writer.WriteInt32(value.TimeSinceLastDealCompleted, (AutoPackType)1);
		writer.WriteInt32(value.TimeSinceLastDealOffered, (AutoPackType)1);
		writer.WriteInt32(value.OfferedDeals, (AutoPackType)1);
		writer.WriteInt32(value.CompletedDeals, (AutoPackType)1);
		writer.WriteBoolean(value.IsContractOffered);
		Write___ScheduleOne_002EQuests_002EContractInfoFishNet_002ESerializing_002EGenerated(writer, value.OfferedContract);
		Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated(writer, value.OfferedContractTime);
		writer.WriteInt32(value.TimeSincePlayerApproached, (AutoPackType)1);
		writer.WriteInt32(value.TimeSinceInstantDealOffered, (AutoPackType)1);
		writer.WriteBoolean(value.HasBeenRecommended);
		writer.WriteString(value.DataType);
		writer.WriteInt32(value.DataVersion, (AutoPackType)1);
		writer.WriteString(value.GameVersion);
	}

	public static void Write___System_002ESingle_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, float[] value)
	{
		writer.WriteArray<float>(value);
	}

	public static void Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated(this Writer writer, EDrugType value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EDoors_002EEDoorSideFishNet_002ESerializing_002EGenerated(this Writer writer, EDoorSide value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EDelivery_002EDeliveryInstanceFishNet_002ESerializing_002EGenerated(this Writer writer, DeliveryInstance value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.DeliveryID);
		writer.WriteString(value.StoreName);
		writer.WriteString(value.DestinationCode);
		writer.WriteInt32(value.LoadingDockIndex, (AutoPackType)1);
		Write___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.Items);
		Write___ScheduleOne_002EDelivery_002EEDeliveryStatusFishNet_002ESerializing_002EGenerated(writer, value.Status);
		writer.WriteInt32(value.TimeUntilArrival, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EDevUtilities_002EStringIntPairFishNet_002ESerializing_002EGenerated(this Writer writer, StringIntPair value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.String);
		writer.WriteInt32(value.Int, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, StringIntPair[] value)
	{
		writer.WriteArray<StringIntPair>(value);
	}

	public static void Write___ScheduleOne_002EDelivery_002EEDeliveryStatusFishNet_002ESerializing_002EGenerated(this Writer writer, EDeliveryStatus value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EDelivery_002EDeliveryReceiptFishNet_002ESerializing_002EGenerated(this Writer writer, DeliveryReceipt value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.DeliveryID);
		writer.WriteString(value.StoreName);
		writer.WriteString(value.DestinationCode);
		writer.WriteInt32(value.LoadingDockIndex, (AutoPackType)1);
		Write___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.Items);
	}

	public static void Write___ScheduleOne_002ECasino_002EPlayingCard_002FECardSuitFishNet_002ESerializing_002EGenerated(this Writer writer, PlayingCard.ECardSuit value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002ECasino_002EPlayingCard_002FECardValueFishNet_002ESerializing_002EGenerated(this Writer writer, PlayingCard.ECardValue value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___FishNet_002EObject_002ENetworkObject_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, NetworkObject[] value)
	{
		writer.WriteArray<NetworkObject>(value);
	}

	public static void Write___ScheduleOne_002ECasino_002ERTBGameController_002FEStageFishNet_002ESerializing_002EGenerated(this Writer writer, RTBGameController.EStage value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002ECasino_002ESlotMachine_002FESymbolFishNet_002ESerializing_002EGenerated(this Writer writer, SlotMachine.ESymbol value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002ECasino_002ESlotMachine_002FESymbol_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, SlotMachine.ESymbol[] value)
	{
		writer.WriteArray<SlotMachine.ESymbol>(value);
	}

	public static void Write___ScheduleOne_002ELaw_002ECheckpointManager_002FECheckpointLocationFishNet_002ESerializing_002EGenerated(this Writer writer, CheckpointManager.ECheckpointLocation value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ECartelStatusFishNet_002ESerializing_002EGenerated(this Writer writer, ECartelStatus value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerated(this Writer writer, CartelGoonAppearance value)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteBoolean(value.IsMale);
		writer.WriteInt32(value.BaseAppearanceIndex, (AutoPackType)1);
		writer.WriteColor(value.SkinColor, (AutoPackType)1);
		writer.WriteColor(value.HairColor, (AutoPackType)1);
		writer.WriteInt32(value.ClothingIndex, (AutoPackType)1);
		writer.WriteInt32(value.VoiceIndex, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002ECartel_002ECartelDealInfoFishNet_002ESerializing_002EGenerated(this Writer writer, CartelDealInfo value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.RequestedProductID);
		writer.WriteInt32(value.RequestedProductQuantity, (AutoPackType)1);
		writer.WriteInt32(value.PaymentAmount, (AutoPackType)1);
		Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated(writer, value.DueTime);
		Write___ScheduleOne_002ECartel_002ECartelDealInfo_002FEStatusFishNet_002ESerializing_002EGenerated(writer, value.Status);
	}

	public static void Write___ScheduleOne_002ECartel_002ECartelDealInfo_002FEStatusFishNet_002ESerializing_002EGenerated(this Writer writer, CartelDealInfo.EStatus value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EPersistence_002ETrashContentDataFishNet_002ESerializing_002EGenerated(this Writer writer, TrashContentData value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		Write___System_002EString_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.TrashIDs);
		Write___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.TrashQuantities);
	}

	public static void Write___System_002EString_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, string[] value)
	{
		writer.WriteArray<string>(value);
	}

	public static void Write___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerated(this Writer writer, int[] value)
	{
		writer.WriteArray<int>(value);
	}

	public static void Write___ScheduleOne_002EEconomy_002EContractReceiptFishNet_002ESerializing_002EGenerated(this Writer writer, ContractReceipt value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteInt32(value.ReceiptId, (AutoPackType)1);
		Write___ScheduleOne_002EEconomy_002EEContractPartyFishNet_002ESerializing_002EGenerated(writer, value.CompletedBy);
		writer.WriteString(value.CustomerId);
		Write___ScheduleOne_002EGameTime_002EGameDateTimeFishNet_002ESerializing_002EGenerated(writer, value.CompletionTime);
		Write___ScheduleOne_002EDevUtilities_002EStringIntPair_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.Items);
		writer.WriteSingle(value.AmountPaid, (AutoPackType)0);
	}

	public static void Write___ScheduleOne_002EEconomy_002EEContractPartyFishNet_002ESerializing_002EGenerated(this Writer writer, EContractParty value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, WeedAppearanceSettings value)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteColor32(value.MainColor);
		writer.WriteColor32(value.SecondaryColor);
		writer.WriteColor32(value.LeafColor);
		writer.WriteColor32(value.StemColor);
	}

	public static void Write___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, CocaineAppearanceSettings value)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteColor32(value.MainColor);
		writer.WriteColor32(value.SecondaryColor);
	}

	public static void Write___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, MethAppearanceSettings value)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteColor32(value.MainColor);
		writer.WriteColor32(value.SecondaryColor);
	}

	public static void Write___ScheduleOne_002EProduct_002EShroomAppearanceSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, ShroomAppearanceSettings value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
		}
		else
		{
			writer.WriteBoolean(false);
		}
	}

	public static void Write___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerated(this Writer writer, NewMixOperation value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.ProductID);
		writer.WriteString(value.IngredientID);
	}

	public static void Write___ScheduleOne_002EEquipping_002EEquippedItemHandlerFishNet_002ESerializing_002EGenerated(this Writer writer, EquippedItemHandler value)
	{
		writer.WriteNetworkBehaviour((NetworkBehaviour)(object)value);
	}

	public static void Write___ScheduleOne_002EObjectScripts_002EJukebox_002FJukeboxStateFishNet_002ESerializing_002EGenerated(this Writer writer, Jukebox.JukeboxState value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteInt32(value.CurrentVolume, (AutoPackType)1);
		writer.WriteBoolean(value.IsPlaying);
		writer.WriteSingle(value.CurrentTrackTime, (AutoPackType)0);
		Write___System_002EInt32_005B_005DFishNet_002ESerializing_002EGenerated(writer, value.TrackOrder);
		writer.WriteInt32(value.CurrentTrackOrderIndex, (AutoPackType)1);
		writer.WriteBoolean(value.Shuffle);
		Write___ScheduleOne_002EObjectScripts_002EJukebox_002FERepeatModeFishNet_002ESerializing_002EGenerated(writer, value.RepeatMode);
		writer.WriteBoolean(value.Sync);
	}

	public static void Write___ScheduleOne_002EObjectScripts_002EJukebox_002FERepeatModeFishNet_002ESerializing_002EGenerated(this Writer writer, Jukebox.ERepeatMode value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EObjectScripts_002EChemistryCookOperationFishNet_002ESerializing_002EGenerated(this Writer writer, ChemistryCookOperation value)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.RecipeID);
		Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(writer, value.ProductQuality);
		writer.WriteColor(value.StartLiquidColor, (AutoPackType)1);
		writer.WriteSingle(value.LiquidLevel, (AutoPackType)0);
		writer.WriteInt32(value.CurrentTime, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EObjectScripts_002EDryingOperationFishNet_002ESerializing_002EGenerated(this Writer writer, DryingOperation value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.ItemID);
		writer.WriteInt32(value.Quantity, (AutoPackType)1);
		Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(writer, value.StartQuality);
		writer.WriteSingle(value.Time, (AutoPackType)0);
	}

	public static void Write___ScheduleOne_002EObjectScripts_002EOvenCookOperationFishNet_002ESerializing_002EGenerated(this Writer writer, OvenCookOperation value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.IngredientID);
		Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(writer, value.IngredientQuality);
		writer.WriteInt32(value.IngredientQuantity, (AutoPackType)1);
		writer.WriteString(value.ProductID);
		writer.WriteInt32(value.CookProgress, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EObjectScripts_002EMixOperationFishNet_002ESerializing_002EGenerated(this Writer writer, MixOperation value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.ProductID);
		Write___ScheduleOne_002EItemFramework_002EEQualityFishNet_002ESerializing_002EGenerated(writer, value.ProductQuality);
		writer.WriteString(value.IngredientID);
		writer.WriteInt32(value.Quantity, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002ETiles_002ECoordinateProceduralTilePairFishNet_002ESerializing_002EGenerated(this Writer writer, CoordinateProceduralTilePair value)
	{
		Write___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerated(writer, value.coord);
		writer.WriteNetworkObject(value.tileParent);
		writer.WriteInt32(value.tileIndex, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002ETiles_002ECoordinateFishNet_002ESerializing_002EGenerated(this Writer writer, Coordinate value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteInt32(value.x, (AutoPackType)1);
		writer.WriteInt32(value.y, (AutoPackType)1);
	}

	public static void Write___System_002ECollections_002EGeneric_002EList_00601_003CScheduleOne_002ETiles_002ECoordinateProceduralTilePair_003EFishNet_002ESerializing_002EGenerated(this Writer writer, List<CoordinateProceduralTilePair> value)
	{
		writer.WriteList<CoordinateProceduralTilePair>(value);
	}

	public static void Write___ScheduleOne_002EObjectScripts_002ERecycler_002FEStateFishNet_002ESerializing_002EGenerated(this Writer writer, Recycler.EState value)
	{
		writer.WriteInt32((int)value, (AutoPackType)1);
	}

	public static void Write___ScheduleOne_002EPersistence_002EDatas_002EGameDataFishNet_002ESerializing_002EGenerated(this Writer writer, GameData value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteString(value.OrganisationName);
		writer.WriteInt32(value.Seed, (AutoPackType)1);
		Write___ScheduleOne_002EDevUtilities_002EGameSettingsFishNet_002ESerializing_002EGenerated(writer, value.Settings);
		writer.WriteString(value.DataType);
		writer.WriteInt32(value.DataVersion, (AutoPackType)1);
		writer.WriteString(value.GameVersion);
	}

	public static void Write___ScheduleOne_002EDevUtilities_002EGameSettingsFishNet_002ESerializing_002EGenerated(this Writer writer, GameSettings value)
	{
		if (value == null)
		{
			writer.WriteBoolean(true);
			return;
		}
		writer.WriteBoolean(false);
		writer.WriteBoolean(value.ConsoleEnabled);
		writer.WriteBoolean(value.UseRandomizedMixMaps);
	}

	public static void Write___ScheduleOne_002EWeather_002EWeatherVolumeFishNet_002ESerializing_002EGenerated(this Writer writer, WeatherVolume value)
	{
		writer.WriteNetworkBehaviour((NetworkBehaviour)(object)value);
	}

	public static void Write___ScheduleOne_002ECombat_002EExplosionDataFishNet_002ESerializing_002EGenerated(this Writer writer, ExplosionData value)
	{
		writer.WriteSingle(value.DamageRadius, (AutoPackType)0);
		writer.WriteSingle(value.MaxDamage, (AutoPackType)0);
		writer.WriteSingle(value.PushForceRadius, (AutoPackType)0);
		writer.WriteSingle(value.MaxPushForce, (AutoPackType)0);
		writer.WriteBoolean(value.CheckLoS);
		Write___ScheduleOne_002ECombat_002EEExplosionTypeFishNet_002ESerializing_002EGenerated(writer, value.ExplosionType);
	}
}
