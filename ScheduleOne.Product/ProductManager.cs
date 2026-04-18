using System;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.Effects;
using ScheduleOne.Effects.MixMaps;
using ScheduleOne.GameTime;
using ScheduleOne.ItemFramework;
using ScheduleOne.Levelling;
using ScheduleOne.Map;
using ScheduleOne.Networking;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.StationFramework;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Product;

public class ProductManager : NetworkSingleton<ProductManager>, IBaseSaveable, ISaveable
{
	public const int MIN_PRICE = 1;

	public const int MAX_PRICE = 999;

	public const int CONTRACT_RECEIPT_MAX_COUNT = 500;

	public const int STAGGERED_REPLICATIONS_PER_SECOND = 80;

	public Action<ProductDefinition> onProductDiscovered;

	public static List<ProductDefinition> DiscoveredProducts = new List<ProductDefinition>();

	public static List<ProductDefinition> ListedProducts = new List<ProductDefinition>();

	public static List<ProductDefinition> FavouritedProducts = new List<ProductDefinition>();

	public List<ProductDefinition> AllProducts = new List<ProductDefinition>();

	public List<ProductDefinition> DefaultKnownProducts = new List<ProductDefinition>();

	public List<PropertyItemDefinition> ValidMixIngredients = new List<PropertyItemDefinition>();

	public List<ContractReceipt> ContractReceipts = new List<ContractReceipt>();

	public AnimationCurve SampleSuccessCurve;

	public ProductDefinition[] ListForSaleOnStart;

	[Header("Default Products")]
	public WeedDefinition DefaultWeed;

	public CocaineDefinition DefaultCocaine;

	public MethDefinition DefaultMeth;

	public ShroomDefinition DefaultShroom;

	[Header("Mix Maps")]
	public MixerMap WeedMixMap;

	public MixerMap MethMixMap;

	public MixerMap CokeMixMap;

	public MixerMap ShroomMixMap;

	private List<ProductDefinition> createdProducts = new List<ProductDefinition>();

	public Action<NewMixOperation> onMixCompleted;

	public Action<ProductDefinition> onNewProductCreated;

	public Action<ProductDefinition> onProductListed;

	public Action<ProductDefinition> onProductDelisted;

	public Action<ProductDefinition> onProductFavourited;

	public Action<ProductDefinition> onProductUnfavourited;

	public Action<ContractReceipt> onContractReceiptRecorded;

	public UnityEvent onFirstSampleRejection;

	public UnityEvent onSecondUniqueProductCreated;

	public List<string> ProductNames = new List<string>();

	private List<StationRecipe> mixRecipes = new List<StationRecipe>();

	public Action<StationRecipe> onMixRecipeAdded;

	private Dictionary<ProductDefinition, float> ProductPrices = new Dictionary<ProductDefinition, float>();

	private ProductDefinition highestValueProduct;

	private List<NetworkConnection> productDataSentTo = new List<NetworkConnection>();

	public Action<NetworkConnection> onProductDataSentToConnection;

	private ProductManagerLoader loader = new ProductManagerLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002EProduct_002EProductManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EProduct_002EProductManagerAssembly_002DCSharp_002Edll_Excuted;

	public static bool MethDiscovered => DiscoveredProducts.Any((ProductDefinition p) => p.DrugType == EDrugType.Methamphetamine);

	public static bool CocaineDiscovered => DiscoveredProducts.Any((ProductDefinition p) => p.DrugType == EDrugType.Cocaine);

	public static bool ShroomsDiscovered => DiscoveredProducts.Any((ProductDefinition p) => p.DrugType == EDrugType.Shrooms);

	public static bool IsAcceptingOrders { get; private set; } = true;

	public NewMixOperation CurrentMixOperation { get; private set; }

	public bool IsMixingInProgress => CurrentMixOperation != null;

	public bool IsMixComplete { get; private set; }

	public float TimeSinceProductListingChanged { get; private set; }

	public string SaveFolderName => "Products";

	public string SaveFileName => "Products";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	public int LoadOrder { get; }

	public bool HasSentProductDataToConnection(NetworkConnection conn)
	{
		return productDataSentTo.Contains(conn);
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EProduct_002EProductManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	protected override void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		base.Start();
		Singleton<LoadManager>.Instance.onPreSceneChange.AddListener(new UnityAction(Clean));
		foreach (ProductDefinition defaultKnownProduct in DefaultKnownProducts)
		{
			defaultKnownProduct.OnValidate();
			if ((Object)(object)highestValueProduct == (Object)null || defaultKnownProduct.MarketValue > highestValueProduct.MarketValue)
			{
				highestValueProduct = defaultKnownProduct;
			}
		}
		foreach (ProductDefinition allProduct in AllProducts)
		{
			if (!ProductNames.Contains(((BaseItemDefinition)allProduct).Name))
			{
				ProductNames.Add(((BaseItemDefinition)allProduct).Name);
			}
			if (!ProductPrices.ContainsKey(allProduct))
			{
				ProductPrices.Add(allProduct, allProduct.MarketValue);
			}
		}
		TimeManager timeManager = NetworkSingleton<TimeManager>.Instance;
		timeManager.onSleepEnd = (Action)Delegate.Combine(timeManager.onSleepEnd, new Action(OnNewDay));
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(OnMinPass);
		foreach (PropertyItemDefinition validMixIngredient in ValidMixIngredients)
		{
			for (int i = 0; i < validMixIngredient.Properties.Count; i++)
			{
				if (!Singleton<PropertyUtility>.Instance.AllProperties.Contains(validMixIngredient.Properties[i]))
				{
					Console.LogError("Mixer " + ((BaseItemDefinition)validMixIngredient).Name + " has property " + ((object)validMixIngredient.Properties[i])?.ToString() + " that is not in the valid properties list");
				}
			}
		}
		if (Application.isEditor)
		{
			ProductDefinition[] listForSaleOnStart = ListForSaleOnStart;
			foreach (ProductDefinition productDefinition in listForSaleOnStart)
			{
				SetProductListed(((BaseItemDefinition)productDefinition).ID, listed: true);
			}
		}
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		for (int i = 0; i < DefaultKnownProducts.Count; i++)
		{
			SetProductDiscovered(null, ((BaseItemDefinition)DefaultKnownProducts[i]).ID, autoList: false);
		}
	}

	public override void OnStartClient()
	{
		((NetworkBehaviour)this).OnStartClient();
		RefreshHighestValueProduct();
	}

	private void Update()
	{
		TimeSinceProductListingChanged += Time.deltaTime;
	}

	private void Clean()
	{
		DiscoveredProducts.Clear();
		ListedProducts.Clear();
		FavouritedProducts.Clear();
		IsAcceptingOrders = true;
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetMethDiscovered()
	{
		RpcWriter___Server_SetMethDiscovered_2166136261();
		RpcLogic___SetMethDiscovered_2166136261();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetCocaineDiscovered()
	{
		RpcWriter___Server_SetCocaineDiscovered_2166136261();
		RpcLogic___SetCocaineDiscovered_2166136261();
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetShroomsDiscovered()
	{
		RpcWriter___Server_SetShroomsDiscovered_2166136261();
		RpcLogic___SetShroomsDiscovered_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void RecordContractReceipt(NetworkConnection conn, ContractReceipt receipt)
	{
		if (conn == null)
		{
			RpcWriter___Observers_RecordContractReceipt_691682765(conn, receipt);
			RpcLogic___RecordContractReceipt_691682765(conn, receipt);
		}
		else
		{
			RpcWriter___Target_RecordContractReceipt_691682765(conn, receipt);
		}
	}

	public List<ContractReceipt> GetContractReceipts(EMapRegion region, List<EContractParty> dealCompleterTypes, int maxMinsAgo)
	{
		List<ContractReceipt> list = new List<ContractReceipt>();
		GameDateTime dateTime = NetworkSingleton<TimeManager>.Instance.GetDateTime();
		for (int num = ContractReceipts.Count - 1; num >= 0; num--)
		{
			if (dealCompleterTypes.Contains(ContractReceipts[num].CompletedBy))
			{
				NPC nPC = NPCManager.GetNPC(ContractReceipts[num].CustomerId);
				if (!((Object)(object)nPC == (Object)null) && nPC.Region == region)
				{
					if ((dateTime - ContractReceipts[num].CompletionTime).GetMinSum() > maxMinsAgo)
					{
						break;
					}
					list.Add(ContractReceipts[num]);
				}
			}
		}
		return list;
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	public MixerMap GetMixerMap(EDrugType type)
	{
		switch (type)
		{
		case EDrugType.Marijuana:
			return WeedMixMap;
		case EDrugType.Methamphetamine:
			return MethMixMap;
		case EDrugType.Cocaine:
			return CokeMixMap;
		case EDrugType.Shrooms:
			return ShroomMixMap;
		default:
			Console.LogError("No mixer map found for " + type);
			return null;
		}
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (connection.IsHost)
		{
			return;
		}
		int byteSizeEstimate_Recipes = mixRecipes.Count * 96;
		int byteSizeEstimate_Discovered = DiscoveredProducts.Count * 32;
		int num = ListedProducts.Count * 32;
		int num2 = FavouritedProducts.Count * 32;
		int byteSizeEstimate_Prices = ProductPrices.Count * 64;
		int byteSizeEstimate_Receipts = ContractReceipts.Count * 128;
		for (int i = 0; i < createdProducts.Count; i++)
		{
			ProductDefinition productDefinition = createdProducts[i];
			if (productDefinition is WeedDefinition)
			{
				WeedDefinition weedDefinition = productDefinition as WeedDefinition;
				WeedAppearanceSettings appearance = new WeedAppearanceSettings(Color32.op_Implicit(weedDefinition.MainMat.color), Color32.op_Implicit(weedDefinition.SecondaryMat.color), Color32.op_Implicit(weedDefinition.LeafMat.color), Color32.op_Implicit(weedDefinition.StemMat.color));
				List<string> list = new List<string>();
				foreach (Effect property in weedDefinition.Properties)
				{
					list.Add(property.ID);
				}
				CreateWeed(connection, ((BaseItemDefinition)productDefinition).Name, ((BaseItemDefinition)productDefinition).ID, EDrugType.Marijuana, list, appearance);
			}
			else if (productDefinition is MethDefinition)
			{
				MethDefinition obj = productDefinition as MethDefinition;
				MethAppearanceSettings appearanceSettings = obj.AppearanceSettings;
				List<string> list2 = new List<string>();
				foreach (Effect property2 in obj.Properties)
				{
					list2.Add(property2.ID);
				}
				CreateMeth(connection, ((BaseItemDefinition)productDefinition).Name, ((BaseItemDefinition)productDefinition).ID, EDrugType.Methamphetamine, list2, appearanceSettings);
			}
			else if (productDefinition is CocaineDefinition)
			{
				CocaineDefinition obj2 = productDefinition as CocaineDefinition;
				CocaineAppearanceSettings appearanceSettings2 = obj2.AppearanceSettings;
				List<string> list3 = new List<string>();
				foreach (Effect property3 in obj2.Properties)
				{
					list3.Add(property3.ID);
				}
				CreateCocaine(connection, ((BaseItemDefinition)productDefinition).Name, ((BaseItemDefinition)productDefinition).ID, EDrugType.Cocaine, list3, appearanceSettings2);
			}
			else
			{
				if (!(productDefinition is ShroomDefinition))
				{
					continue;
				}
				ShroomDefinition obj3 = productDefinition as ShroomDefinition;
				ShroomAppearanceSettings appearanceSettings3 = obj3.AppearanceSettings;
				List<string> list4 = new List<string>();
				foreach (Effect property4 in obj3.Properties)
				{
					list4.Add(property4.ID);
				}
				CreateShroom_Client(connection, ((BaseItemDefinition)productDefinition).Name, ((BaseItemDefinition)productDefinition).ID, EDrugType.Shrooms, list4, appearanceSettings3);
			}
		}
		productDataSentTo.Add(connection);
		Console.Log("Created products sent to connection " + connection.ClientId);
		if (onProductDataSentToConnection != null)
		{
			onProductDataSentToConnection(connection);
		}
		ReplicationQueue.Enqueue(((object)this).GetType().Name, connection, ReplicateAll, byteSizeEstimate_Recipes + byteSizeEstimate_Discovered + num + num2 + byteSizeEstimate_Prices + byteSizeEstimate_Receipts);
		void DiscoveredProductReplicationDone()
		{
			for (int j = 0; j < ListedProducts.Count; j++)
			{
				SetProductListed(connection, ((BaseItemDefinition)ListedProducts[j]).ID, listed: true);
			}
			for (int k = 0; k < FavouritedProducts.Count; k++)
			{
				SetProductFavourited(connection, ((BaseItemDefinition)FavouritedProducts[k]).ID, fav: true);
			}
			Singleton<StaggeredCallbackUtility>.Instance.InvokeStaggered(ProductPrices.Count, ReplicationQueue.GetReplicationDuration(byteSizeEstimate_Prices), delegate(int i2)
			{
				ReplicateProductPrices(i2);
			}, ReplicatePricesDone);
		}
		void RecipeReplicationDone()
		{
			Singleton<StaggeredCallbackUtility>.Instance.InvokeStaggered(DiscoveredProducts.Count, ReplicationQueue.GetReplicationDuration(byteSizeEstimate_Discovered), delegate(int i2)
			{
				ReplicateDiscoveredProduct(i2);
			}, DiscoveredProductReplicationDone);
		}
		void ReplicateAll(NetworkConnection conn)
		{
			Singleton<StaggeredCallbackUtility>.Instance.InvokeStaggered(mixRecipes.Count, ReplicationQueue.GetReplicationDuration(byteSizeEstimate_Recipes), ReplicateRecipe, RecipeReplicationDone);
		}
		void ReplicateContractReceipt(int index)
		{
			RecordContractReceipt(connection, ContractReceipts[index]);
		}
		void ReplicateDiscoveredProduct(int index)
		{
			SetProductDiscovered(connection, ((BaseItemDefinition)DiscoveredProducts[index]).ID, autoList: false);
		}
		void ReplicatePricesDone()
		{
			Singleton<StaggeredCallbackUtility>.Instance.InvokeStaggered(ContractReceipts.Count, ReplicationQueue.GetReplicationDuration(byteSizeEstimate_Receipts), delegate(int i2)
			{
				ReplicateContractReceipt(i2);
			});
		}
		void ReplicateProductPrices(int index)
		{
			KeyValuePair<ProductDefinition, float> keyValuePair = ProductPrices.ElementAt(index);
			SetPrice(connection, ((BaseItemDefinition)keyValuePair.Key).ID, keyValuePair.Value);
		}
		void ReplicateRecipe(int index)
		{
			CreateMixRecipe(connection, ((BaseItemDefinition)mixRecipes[index].Ingredients[1].Items[0]).ID, ((BaseItemDefinition)mixRecipes[index].Ingredients[0].Items[0]).ID, ((BaseItemDefinition)mixRecipes[index].Product.Item).ID);
		}
	}

	private void OnMinPass()
	{
		if (!NetworkSingleton<VariableDatabase>.InstanceExists || GameManager.IS_TUTORIAL || NetworkSingleton<VariableDatabase>.Instance.GetValue<bool>("SecondUniqueProductDiscovered"))
		{
			return;
		}
		float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Inventory_OGKush");
		if (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("Inventory_Weed_Count") > value)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("SecondUniqueProductDiscovered", true.ToString());
			if (onSecondUniqueProductCreated != null)
			{
				onSecondUniqueProductCreated.Invoke();
			}
		}
	}

	private void OnNewDay()
	{
		if (InstanceFinder.IsServer && CurrentMixOperation != null && !IsMixComplete)
		{
			SetMixOperation(CurrentMixOperation, complete: true);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetProductListed(string productID, bool listed)
	{
		RpcWriter___Server_SetProductListed_310431262(productID, listed);
		RpcLogic___SetProductListed_310431262(productID, listed);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetProductListed(NetworkConnection conn, string productID, bool listed)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetProductListed_619441887(conn, productID, listed);
			RpcLogic___SetProductListed_619441887(conn, productID, listed);
		}
		else
		{
			RpcWriter___Target_SetProductListed_619441887(conn, productID, listed);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SetProductFavourited(string productID, bool listed)
	{
		RpcWriter___Server_SetProductFavourited_310431262(productID, listed);
		RpcLogic___SetProductFavourited_310431262(productID, listed);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetProductFavourited(NetworkConnection conn, string productID, bool fav)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetProductFavourited_619441887(conn, productID, fav);
			RpcLogic___SetProductFavourited_619441887(conn, productID, fav);
		}
		else
		{
			RpcWriter___Target_SetProductFavourited_619441887(conn, productID, fav);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void DiscoverProduct(string productID)
	{
		RpcWriter___Server_DiscoverProduct_3615296227(productID);
		RpcLogic___DiscoverProduct_3615296227(productID);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetProductDiscovered(NetworkConnection conn, string productID, bool autoList)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetProductDiscovered_619441887(conn, productID, autoList);
			RpcLogic___SetProductDiscovered_619441887(conn, productID, autoList);
		}
		else
		{
			RpcWriter___Target_SetProductDiscovered_619441887(conn, productID, autoList);
		}
	}

	public void SetIsAcceptingOrder(bool accepting)
	{
		IsAcceptingOrders = accepting;
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void CreateWeed_Server(string name, string id, EDrugType type, List<string> properties, WeedAppearanceSettings appearance)
	{
		RpcWriter___Server_CreateWeed_Server_2331775230(name, id, type, properties, appearance);
		RpcLogic___CreateWeed_Server_2331775230(name, id, type, properties, appearance);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void CreateWeed(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, WeedAppearanceSettings appearance)
	{
		if (conn == null)
		{
			RpcWriter___Observers_CreateWeed_1777266891(conn, name, id, type, properties, appearance);
			RpcLogic___CreateWeed_1777266891(conn, name, id, type, properties, appearance);
		}
		else
		{
			RpcWriter___Target_CreateWeed_1777266891(conn, name, id, type, properties, appearance);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void CreateCocaine_Server(string name, string id, EDrugType type, List<string> properties, CocaineAppearanceSettings appearance)
	{
		RpcWriter___Server_CreateCocaine_Server_891166717(name, id, type, properties, appearance);
		RpcLogic___CreateCocaine_Server_891166717(name, id, type, properties, appearance);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void CreateCocaine(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, CocaineAppearanceSettings appearance)
	{
		if (conn == null)
		{
			RpcWriter___Observers_CreateCocaine_1327282946(conn, name, id, type, properties, appearance);
			RpcLogic___CreateCocaine_1327282946(conn, name, id, type, properties, appearance);
		}
		else
		{
			RpcWriter___Target_CreateCocaine_1327282946(conn, name, id, type, properties, appearance);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void CreateMeth_Server(string name, string id, EDrugType type, List<string> properties, MethAppearanceSettings appearance)
	{
		RpcWriter___Server_CreateMeth_Server_4251728555(name, id, type, properties, appearance);
		RpcLogic___CreateMeth_Server_4251728555(name, id, type, properties, appearance);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void CreateMeth(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, MethAppearanceSettings appearance)
	{
		if (conn == null)
		{
			RpcWriter___Observers_CreateMeth_1869045686(conn, name, id, type, properties, appearance);
			RpcLogic___CreateMeth_1869045686(conn, name, id, type, properties, appearance);
		}
		else
		{
			RpcWriter___Target_CreateMeth_1869045686(conn, name, id, type, properties, appearance);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void CreateShroom_Server(string name, string id, EDrugType type, List<string> properties, ShroomAppearanceSettings appearance)
	{
		RpcWriter___Server_CreateShroom_Server_2261384965(name, id, type, properties, appearance);
		RpcLogic___CreateShroom_Server_2261384965(name, id, type, properties, appearance);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	private void CreateShroom_Client(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, ShroomAppearanceSettings appearance)
	{
		if (conn == null)
		{
			RpcWriter___Observers_CreateShroom_Client_812995776(conn, name, id, type, properties, appearance);
			RpcLogic___CreateShroom_Client_812995776(conn, name, id, type, properties, appearance);
		}
		else
		{
			RpcWriter___Target_CreateShroom_Client_812995776(conn, name, id, type, properties, appearance);
		}
	}

	private void RefreshHighestValueProduct()
	{
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		for (int i = 0; i < DiscoveredProducts.Count; i++)
		{
			if ((Object)(object)highestValueProduct == (Object)null || DiscoveredProducts[i].MarketValue > highestValueProduct.MarketValue)
			{
				highestValueProduct = DiscoveredProducts[i];
			}
		}
		float marketValue = highestValueProduct.MarketValue;
		if (marketValue >= 100f)
		{
			AchievementManager.UnlockAchievement(AchievementManager.EAchievement.MASTER_CHEF);
		}
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("HighestValueProduct", marketValue.ToString());
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendMixRecipe(string product, string mixer, string output)
	{
		RpcWriter___Server_SendMixRecipe_852232071(product, mixer, output);
		RpcLogic___SendMixRecipe_852232071(product, mixer, output);
	}

	[TargetRpc]
	[ObserversRpc(RunLocally = true)]
	public void CreateMixRecipe(NetworkConnection conn, string product, string mixer, string output)
	{
		if (conn == null)
		{
			RpcWriter___Observers_CreateMixRecipe_1410895574(conn, product, mixer, output);
			RpcLogic___CreateMixRecipe_1410895574(conn, product, mixer, output);
		}
		else
		{
			RpcWriter___Target_CreateMixRecipe_1410895574(conn, product, mixer, output);
		}
	}

	public StationRecipe GetRecipe(string product, string mixer)
	{
		return mixRecipes.Find((StationRecipe r) => ((BaseItemDefinition)r.Product.Item).ID == product && ((BaseItemDefinition)r.Ingredients[0].Items[0]).ID == mixer);
	}

	public StationRecipe GetRecipe(List<Effect> productProperties, Effect mixerProperty)
	{
		foreach (StationRecipe mixRecipe in mixRecipes)
		{
			if ((Object)(object)mixRecipe == (Object)null || mixRecipe.Ingredients.Count < 2)
			{
				continue;
			}
			ItemDefinition item = mixRecipe.Ingredients[0].Item;
			ItemDefinition item2 = mixRecipe.Ingredients[1].Item;
			if ((Object)(object)item == (Object)null || (Object)(object)item2 == (Object)null)
			{
				continue;
			}
			List<Effect> list = (item as PropertyItemDefinition)?.Properties;
			List<Effect> list2 = (item2 as PropertyItemDefinition)?.Properties;
			if (item2 is ProductDefinition)
			{
				list = (item2 as PropertyItemDefinition)?.Properties;
				list2 = (item as PropertyItemDefinition)?.Properties;
			}
			if (list.Count != productProperties.Count || list2.Count != 1)
			{
				continue;
			}
			bool flag = true;
			for (int i = 0; i < productProperties.Count; i++)
			{
				if (!list.Contains(productProperties[i]))
				{
					flag = false;
					break;
				}
			}
			if (flag && !((Object)(object)list2[0] != (Object)(object)mixerProperty))
			{
				return mixRecipe;
			}
		}
		return null;
	}

	public ProductDefinition GetKnownProduct(EDrugType type, List<Effect> properties)
	{
		foreach (ProductDefinition allProduct in AllProducts)
		{
			if (allProduct.DrugTypes[0].DrugType != type || allProduct.Properties.Count != properties.Count)
			{
				continue;
			}
			for (int i = 0; i < properties.Count && allProduct.Properties.Contains(properties[i]); i++)
			{
				if (i == properties.Count - 1)
				{
					return allProduct;
				}
			}
		}
		return null;
	}

	public float GetPrice(ProductDefinition product)
	{
		if ((Object)(object)product == (Object)null)
		{
			Console.LogError("Product is null");
			return 1f;
		}
		if (ProductPrices.ContainsKey(product))
		{
			return Mathf.Clamp(ProductPrices[product], 1f, 999f);
		}
		Console.LogError("Price not found for product: " + ((BaseItemDefinition)product).ID + ". Returning market value");
		return Mathf.Clamp(product.MarketValue, 1f, 999f);
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendPrice(string productID, float value)
	{
		RpcWriter___Server_SendPrice_606697822(productID, value);
		RpcLogic___SendPrice_606697822(productID, value);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	public void SetPrice(NetworkConnection conn, string productID, float value)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetPrice_4077118173(conn, productID, value);
			RpcLogic___SetPrice_4077118173(conn, productID, value);
		}
		else
		{
			RpcWriter___Target_SetPrice_4077118173(conn, productID, value);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendMixOperation(NewMixOperation operation, bool complete)
	{
		RpcWriter___Server_SendMixOperation_3670976965(operation, complete);
		RpcLogic___SendMixOperation_3670976965(operation, complete);
	}

	[ObserversRpc(RunLocally = true)]
	private void SetMixOperation(NewMixOperation operation, bool complete)
	{
		RpcWriter___Observers_SetMixOperation_3670976965(operation, complete);
		RpcLogic___SetMixOperation_3670976965(operation, complete);
	}

	public string FinishAndNameMix(string productID, string ingredientID, string mixName)
	{
		if (!IsMixNameValid(mixName))
		{
			Console.LogError("Invalid mix name. Using random name");
			mixName = Singleton<NewMixScreen>.Instance.GenerateUniqueName();
		}
		string id = mixName.ToLower().Replace(" ", string.Empty);
		id = MakeIDFileSafe(id);
		id = id.Replace(" ", string.Empty);
		id = id.Replace("(", string.Empty);
		id = id.Replace(")", string.Empty);
		id = id.Replace("'", string.Empty);
		id = id.Replace("\"", string.Empty);
		id = id.Replace(":", string.Empty);
		id = id.Replace(";", string.Empty);
		id = id.Replace(",", string.Empty);
		id = id.Replace(".", string.Empty);
		id = id.Replace("!", string.Empty);
		id = id.Replace("?", string.Empty);
		NetworkSingleton<LevelManager>.Instance.AddXP(80);
		FinishAndNameMix(productID, ingredientID, mixName, id);
		if (!InstanceFinder.IsServer)
		{
			SendFinishAndNameMix(productID, ingredientID, mixName, id);
		}
		return id;
	}

	public static string MakeIDFileSafe(string id)
	{
		id = id.ToLower();
		id = id.Replace(" ", string.Empty);
		id = id.Replace("(", string.Empty);
		id = id.Replace(")", string.Empty);
		id = id.Replace("'", string.Empty);
		id = id.Replace("\"", string.Empty);
		id = id.Replace(":", string.Empty);
		id = id.Replace(";", string.Empty);
		id = id.Replace(",", string.Empty);
		id = id.Replace(".", string.Empty);
		id = id.Replace("!", string.Empty);
		id = id.Replace("?", string.Empty);
		return id;
	}

	public static bool IsMixNameValid(string mixName)
	{
		if (string.IsNullOrEmpty(mixName))
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(mixName))
		{
			return false;
		}
		return true;
	}

	[ObserversRpc(RunLocally = true)]
	private void FinishAndNameMix(string productID, string ingredientID, string mixName, string mixID)
	{
		RpcWriter___Observers_FinishAndNameMix_4237212381(productID, ingredientID, mixName, mixID);
		RpcLogic___FinishAndNameMix_4237212381(productID, ingredientID, mixName, mixID);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SendFinishAndNameMix(string productID, string ingredientID, string mixName, string mixID)
	{
		RpcWriter___Server_SendFinishAndNameMix_4237212381(productID, ingredientID, mixName, mixID);
	}

	public static float CalculateProductValue(ProductDefinition product, float baseValue)
	{
		return CalculateProductValue(baseValue, product.Properties);
	}

	public static float CalculateProductValue(float baseValue, List<Effect> properties)
	{
		float num = baseValue;
		float num2 = 1f;
		for (int i = 0; i < properties.Count; i++)
		{
			if (!((Object)(object)properties[i] == (Object)null))
			{
				num += (float)properties[i].ValueChange;
				num += baseValue * properties[i].AddBaseValueMultiple;
				num2 *= properties[i].ValueMultiplier;
			}
		}
		num *= num2;
		return Mathf.RoundToInt(num);
	}

	public static void CheckDiscovery(ItemInstance item)
	{
		if (item != null && NetworkSingleton<ProductManager>.InstanceExists && item != null)
		{
			if (item is CocaineInstance && !CocaineDiscovered)
			{
				NetworkSingleton<ProductManager>.Instance.SetCocaineDiscovered();
			}
			else if (item is MethInstance && !MethDiscovered)
			{
				NetworkSingleton<ProductManager>.Instance.SetMethDiscovered();
			}
			else if (item is ShroomInstance && !ShroomsDiscovered)
			{
				NetworkSingleton<ProductManager>.Instance.SetShroomsDiscovered();
			}
		}
	}

	public virtual string GetSaveString()
	{
		string[] array = new string[DiscoveredProducts.Count];
		for (int i = 0; i < DiscoveredProducts.Count; i++)
		{
			if (!((Object)(object)DiscoveredProducts[i] == (Object)null))
			{
				array[i] = ((BaseItemDefinition)DiscoveredProducts[i]).ID;
			}
		}
		string[] array2 = new string[ListedProducts.Count];
		for (int j = 0; j < ListedProducts.Count; j++)
		{
			if (!((Object)(object)ListedProducts[j] == (Object)null))
			{
				array2[j] = ((BaseItemDefinition)ListedProducts[j]).ID;
			}
		}
		string[] array3 = new string[FavouritedProducts.Count];
		for (int k = 0; k < FavouritedProducts.Count; k++)
		{
			if (!((Object)(object)FavouritedProducts[k] == (Object)null))
			{
				array3[k] = ((BaseItemDefinition)FavouritedProducts[k]).ID;
			}
		}
		MixRecipeData[] array4 = new MixRecipeData[mixRecipes.Count];
		for (int l = 0; l < mixRecipes.Count; l++)
		{
			if ((Object)(object)mixRecipes[l] == (Object)null)
			{
				continue;
			}
			if (mixRecipes[l].Ingredients.Count < 2)
			{
				Console.LogWarning("Mix recipe has less than 2 ingredients");
			}
			else if (mixRecipes[l].Product != null)
			{
				try
				{
					array4[l] = new MixRecipeData(((BaseItemDefinition)mixRecipes[l].Ingredients[1].Items[0]).ID, ((BaseItemDefinition)mixRecipes[l].Ingredients[0].Items[0]).ID, ((BaseItemDefinition)mixRecipes[l].Product.Item).ID);
				}
				catch (Exception ex)
				{
					Console.LogError("Failed to save mix recipe: " + ex);
				}
			}
		}
		StringIntPair[] array5 = new StringIntPair[ProductPrices.Count];
		for (int m = 0; m < AllProducts.Count; m++)
		{
			if (!((Object)(object)AllProducts[m] == (Object)null))
			{
				float num = 0f;
				array5[m] = new StringIntPair(i: Mathf.RoundToInt((!ProductPrices.ContainsKey(AllProducts[m])) ? AllProducts[m].MarketValue : ProductPrices[AllProducts[m]]), str: ((BaseItemDefinition)AllProducts[m]).ID);
			}
		}
		List<WeedProductData> list = new List<WeedProductData>();
		List<MethProductData> list2 = new List<MethProductData>();
		List<CocaineProductData> list3 = new List<CocaineProductData>();
		List<ShroomProductData> list4 = new List<ShroomProductData>();
		for (int n = 0; n < createdProducts.Count; n++)
		{
			if (!((Object)(object)createdProducts[n] == (Object)null))
			{
				switch (createdProducts[n].DrugType)
				{
				case EDrugType.Marijuana:
					list.Add((createdProducts[n] as WeedDefinition).GetSaveData() as WeedProductData);
					break;
				case EDrugType.Methamphetamine:
					list2.Add((createdProducts[n] as MethDefinition).GetSaveData() as MethProductData);
					break;
				case EDrugType.Cocaine:
					list3.Add((createdProducts[n] as CocaineDefinition).GetSaveData() as CocaineProductData);
					break;
				case EDrugType.Shrooms:
					list4.Add((createdProducts[n] as ShroomDefinition).GetSaveData() as ShroomProductData);
					break;
				default:
					Console.LogError("Product type not supported: " + createdProducts[n].DrugType);
					break;
				}
			}
		}
		return new ProductManagerData(array, array2, CurrentMixOperation, IsMixComplete, array4, array5, array3, list.ToArray(), list2.ToArray(), list3.ToArray(), list4.ToArray(), ContractReceipts.ToArray()).GetJson();
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Expected O, but got Unknown
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Expected O, but got Unknown
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Expected O, but got Unknown
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Expected O, but got Unknown
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Expected O, but got Unknown
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Expected O, but got Unknown
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Expected O, but got Unknown
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected O, but got Unknown
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Expected O, but got Unknown
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Expected O, but got Unknown
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Expected O, but got Unknown
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Expected O, but got Unknown
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Expected O, but got Unknown
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Expected O, but got Unknown
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Expected O, but got Unknown
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Expected O, but got Unknown
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Expected O, but got Unknown
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Expected O, but got Unknown
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002EProduct_002EProductManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EProduct_002EProductManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_SetMethDiscovered_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(1u, new ServerRpcDelegate(RpcReader___Server_SetCocaineDiscovered_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(2u, new ServerRpcDelegate(RpcReader___Server_SetShroomsDiscovered_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(3u, new ClientRpcDelegate(RpcReader___Observers_RecordContractReceipt_691682765));
			((NetworkBehaviour)this).RegisterTargetRpc(4u, new ClientRpcDelegate(RpcReader___Target_RecordContractReceipt_691682765));
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SetProductListed_310431262));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_SetProductListed_619441887));
			((NetworkBehaviour)this).RegisterTargetRpc(7u, new ClientRpcDelegate(RpcReader___Target_SetProductListed_619441887));
			((NetworkBehaviour)this).RegisterServerRpc(8u, new ServerRpcDelegate(RpcReader___Server_SetProductFavourited_310431262));
			((NetworkBehaviour)this).RegisterObserversRpc(9u, new ClientRpcDelegate(RpcReader___Observers_SetProductFavourited_619441887));
			((NetworkBehaviour)this).RegisterTargetRpc(10u, new ClientRpcDelegate(RpcReader___Target_SetProductFavourited_619441887));
			((NetworkBehaviour)this).RegisterServerRpc(11u, new ServerRpcDelegate(RpcReader___Server_DiscoverProduct_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(12u, new ClientRpcDelegate(RpcReader___Observers_SetProductDiscovered_619441887));
			((NetworkBehaviour)this).RegisterTargetRpc(13u, new ClientRpcDelegate(RpcReader___Target_SetProductDiscovered_619441887));
			((NetworkBehaviour)this).RegisterServerRpc(14u, new ServerRpcDelegate(RpcReader___Server_CreateWeed_Server_2331775230));
			((NetworkBehaviour)this).RegisterTargetRpc(15u, new ClientRpcDelegate(RpcReader___Target_CreateWeed_1777266891));
			((NetworkBehaviour)this).RegisterObserversRpc(16u, new ClientRpcDelegate(RpcReader___Observers_CreateWeed_1777266891));
			((NetworkBehaviour)this).RegisterServerRpc(17u, new ServerRpcDelegate(RpcReader___Server_CreateCocaine_Server_891166717));
			((NetworkBehaviour)this).RegisterTargetRpc(18u, new ClientRpcDelegate(RpcReader___Target_CreateCocaine_1327282946));
			((NetworkBehaviour)this).RegisterObserversRpc(19u, new ClientRpcDelegate(RpcReader___Observers_CreateCocaine_1327282946));
			((NetworkBehaviour)this).RegisterServerRpc(20u, new ServerRpcDelegate(RpcReader___Server_CreateMeth_Server_4251728555));
			((NetworkBehaviour)this).RegisterTargetRpc(21u, new ClientRpcDelegate(RpcReader___Target_CreateMeth_1869045686));
			((NetworkBehaviour)this).RegisterObserversRpc(22u, new ClientRpcDelegate(RpcReader___Observers_CreateMeth_1869045686));
			((NetworkBehaviour)this).RegisterServerRpc(23u, new ServerRpcDelegate(RpcReader___Server_CreateShroom_Server_2261384965));
			((NetworkBehaviour)this).RegisterTargetRpc(24u, new ClientRpcDelegate(RpcReader___Target_CreateShroom_Client_812995776));
			((NetworkBehaviour)this).RegisterObserversRpc(25u, new ClientRpcDelegate(RpcReader___Observers_CreateShroom_Client_812995776));
			((NetworkBehaviour)this).RegisterServerRpc(26u, new ServerRpcDelegate(RpcReader___Server_SendMixRecipe_852232071));
			((NetworkBehaviour)this).RegisterTargetRpc(27u, new ClientRpcDelegate(RpcReader___Target_CreateMixRecipe_1410895574));
			((NetworkBehaviour)this).RegisterObserversRpc(28u, new ClientRpcDelegate(RpcReader___Observers_CreateMixRecipe_1410895574));
			((NetworkBehaviour)this).RegisterServerRpc(29u, new ServerRpcDelegate(RpcReader___Server_SendPrice_606697822));
			((NetworkBehaviour)this).RegisterObserversRpc(30u, new ClientRpcDelegate(RpcReader___Observers_SetPrice_4077118173));
			((NetworkBehaviour)this).RegisterTargetRpc(31u, new ClientRpcDelegate(RpcReader___Target_SetPrice_4077118173));
			((NetworkBehaviour)this).RegisterServerRpc(32u, new ServerRpcDelegate(RpcReader___Server_SendMixOperation_3670976965));
			((NetworkBehaviour)this).RegisterObserversRpc(33u, new ClientRpcDelegate(RpcReader___Observers_SetMixOperation_3670976965));
			((NetworkBehaviour)this).RegisterObserversRpc(34u, new ClientRpcDelegate(RpcReader___Observers_FinishAndNameMix_4237212381));
			((NetworkBehaviour)this).RegisterServerRpc(35u, new ServerRpcDelegate(RpcReader___Server_SendFinishAndNameMix_4237212381));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EProduct_002EProductManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EProduct_002EProductManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SetMethDiscovered_2166136261()
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

	public void RpcLogic___SetMethDiscovered_2166136261()
	{
		Console.Log("Meth discovered");
		SetProductDiscovered(null, "meth", autoList: false);
	}

	private void RpcReader___Server_SetMethDiscovered_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetMethDiscovered_2166136261();
		}
	}

	private void RpcWriter___Server_SetCocaineDiscovered_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(1u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetCocaineDiscovered_2166136261()
	{
		Console.Log("Cocaine discovered");
		SetProductDiscovered(null, "cocaine", autoList: false);
	}

	private void RpcReader___Server_SetCocaineDiscovered_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetCocaineDiscovered_2166136261();
		}
	}

	private void RpcWriter___Server_SetShroomsDiscovered_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(2u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetShroomsDiscovered_2166136261()
	{
		Console.Log("Shrooms discovered");
		SetProductDiscovered(null, "shroom", autoList: false);
	}

	private void RpcReader___Server_SetShroomsDiscovered_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetShroomsDiscovered_2166136261();
		}
	}

	private void RpcWriter___Observers_RecordContractReceipt_691682765(NetworkConnection conn, ContractReceipt receipt)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EEconomy_002EContractReceiptFishNet_002ESerializing_002EGenerated((Writer)(object)writer, receipt);
			((NetworkBehaviour)this).SendObserversRpc(3u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___RecordContractReceipt_691682765(NetworkConnection conn, ContractReceipt receipt)
	{
		if (!ContractReceipts.Exists((ContractReceipt r) => r.ReceiptId == receipt.ReceiptId))
		{
			ContractReceipts.Add(receipt);
			if (ContractReceipts.Count > 500)
			{
				ContractReceipts.RemoveAt(0);
			}
			if (onContractReceiptRecorded != null)
			{
				onContractReceiptRecorded(receipt);
			}
		}
	}

	private void RpcReader___Observers_RecordContractReceipt_691682765(PooledReader PooledReader0, Channel channel)
	{
		ContractReceipt receipt = GeneratedReaders___Internal.Read___ScheduleOne_002EEconomy_002EContractReceiptFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___RecordContractReceipt_691682765(null, receipt);
		}
	}

	private void RpcWriter___Target_RecordContractReceipt_691682765(NetworkConnection conn, ContractReceipt receipt)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EEconomy_002EContractReceiptFishNet_002ESerializing_002EGenerated((Writer)(object)writer, receipt);
			((NetworkBehaviour)this).SendTargetRpc(4u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_RecordContractReceipt_691682765(PooledReader PooledReader0, Channel channel)
	{
		ContractReceipt receipt = GeneratedReaders___Internal.Read___ScheduleOne_002EEconomy_002EContractReceiptFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___RecordContractReceipt_691682765(((NetworkBehaviour)this).LocalConnection, receipt);
		}
	}

	private void RpcWriter___Server_SetProductListed_310431262(string productID, bool listed)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteBoolean(listed);
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetProductListed_310431262(string productID, bool listed)
	{
		SetProductListed(null, productID, listed);
	}

	private void RpcReader___Server_SetProductListed_310431262(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		bool listed = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetProductListed_310431262(productID, listed);
		}
	}

	private void RpcWriter___Observers_SetProductListed_619441887(NetworkConnection conn, string productID, bool listed)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteBoolean(listed);
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetProductListed_619441887(NetworkConnection conn, string productID, bool listed)
	{
		ProductDefinition productDefinition = AllProducts.Find((ProductDefinition p) => ((BaseItemDefinition)p).ID == productID);
		if ((Object)(object)productDefinition == (Object)null)
		{
			Console.LogWarning("SetProductListed: product is not found (" + productID + ")");
			return;
		}
		if (!DiscoveredProducts.Contains(productDefinition))
		{
			Console.LogWarning("SetProductListed: product is not yet discovered");
		}
		if (listed)
		{
			if (!ListedProducts.Contains(productDefinition))
			{
				ListedProducts.Add(productDefinition);
			}
		}
		else if (ListedProducts.Contains(productDefinition))
		{
			ListedProducts.Remove(productDefinition);
		}
		if (NetworkSingleton<VariableDatabase>.InstanceExists && InstanceFinder.IsServer)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("ListedProductsCount", ListedProducts.Count.ToString());
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("OGKushListed", ((Object)(object)ListedProducts.Find((ProductDefinition x) => ((BaseItemDefinition)x).ID == "ogkush") != (Object)null).ToString());
		}
		HasChanged = true;
		TimeSinceProductListingChanged = 0f;
		if (listed)
		{
			if (onProductListed != null)
			{
				onProductListed(productDefinition);
			}
		}
		else if (onProductDelisted != null)
		{
			onProductDelisted(productDefinition);
		}
	}

	private void RpcReader___Observers_SetProductListed_619441887(PooledReader PooledReader0, Channel channel)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		bool listed = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetProductListed_619441887(null, productID, listed);
		}
	}

	private void RpcWriter___Target_SetProductListed_619441887(NetworkConnection conn, string productID, bool listed)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteBoolean(listed);
			((NetworkBehaviour)this).SendTargetRpc(7u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetProductListed_619441887(PooledReader PooledReader0, Channel channel)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		bool listed = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetProductListed_619441887(((NetworkBehaviour)this).LocalConnection, productID, listed);
		}
	}

	private void RpcWriter___Server_SetProductFavourited_310431262(string productID, bool listed)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteBoolean(listed);
			((NetworkBehaviour)this).SendServerRpc(8u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SetProductFavourited_310431262(string productID, bool listed)
	{
		SetProductFavourited(null, productID, listed);
	}

	private void RpcReader___Server_SetProductFavourited_310431262(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		bool listed = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SetProductFavourited_310431262(productID, listed);
		}
	}

	private void RpcWriter___Observers_SetProductFavourited_619441887(NetworkConnection conn, string productID, bool fav)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteBoolean(fav);
			((NetworkBehaviour)this).SendObserversRpc(9u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetProductFavourited_619441887(NetworkConnection conn, string productID, bool fav)
	{
		ProductDefinition productDefinition = AllProducts.Find((ProductDefinition p) => ((BaseItemDefinition)p).ID == productID);
		if ((Object)(object)productDefinition == (Object)null)
		{
			Console.LogWarning("SetProductFavourited: product is not found (" + productID + ")");
			return;
		}
		if (!DiscoveredProducts.Contains(productDefinition))
		{
			Console.LogWarning("SetProductFavourited: product is not yet discovered");
		}
		if (fav)
		{
			if (!FavouritedProducts.Contains(productDefinition))
			{
				FavouritedProducts.Add(productDefinition);
			}
		}
		else if (FavouritedProducts.Contains(productDefinition))
		{
			FavouritedProducts.Remove(productDefinition);
		}
		HasChanged = true;
		if (fav)
		{
			if (onProductFavourited != null)
			{
				onProductFavourited(productDefinition);
			}
		}
		else if (onProductUnfavourited != null)
		{
			onProductUnfavourited(productDefinition);
		}
	}

	private void RpcReader___Observers_SetProductFavourited_619441887(PooledReader PooledReader0, Channel channel)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		bool fav = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetProductFavourited_619441887(null, productID, fav);
		}
	}

	private void RpcWriter___Target_SetProductFavourited_619441887(NetworkConnection conn, string productID, bool fav)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteBoolean(fav);
			((NetworkBehaviour)this).SendTargetRpc(10u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetProductFavourited_619441887(PooledReader PooledReader0, Channel channel)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		bool fav = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetProductFavourited_619441887(((NetworkBehaviour)this).LocalConnection, productID, fav);
		}
	}

	private void RpcWriter___Server_DiscoverProduct_3615296227(string productID)
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
			((Writer)writer).WriteString(productID);
			((NetworkBehaviour)this).SendServerRpc(11u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___DiscoverProduct_3615296227(string productID)
	{
		SetProductDiscovered(null, productID, autoList: true);
	}

	private void RpcReader___Server_DiscoverProduct_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___DiscoverProduct_3615296227(productID);
		}
	}

	private void RpcWriter___Observers_SetProductDiscovered_619441887(NetworkConnection conn, string productID, bool autoList)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteBoolean(autoList);
			((NetworkBehaviour)this).SendObserversRpc(12u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetProductDiscovered_619441887(NetworkConnection conn, string productID, bool autoList)
	{
		ProductDefinition productDefinition = AllProducts.Find((ProductDefinition p) => ((BaseItemDefinition)p).ID == productID);
		if ((Object)(object)productDefinition == (Object)null)
		{
			Console.LogWarning("SetProductDiscovered: product is not found");
			return;
		}
		if (!DiscoveredProducts.Contains(productDefinition))
		{
			Debug.Log((object)("Product discovered: " + ((BaseItemDefinition)productDefinition).Name));
			DiscoveredProducts.Add(productDefinition);
			if (autoList)
			{
				SetProductListed(productID, listed: true);
			}
			if (onProductDiscovered != null)
			{
				onProductDiscovered(productDefinition);
			}
		}
		HasChanged = true;
	}

	private void RpcReader___Observers_SetProductDiscovered_619441887(PooledReader PooledReader0, Channel channel)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		bool autoList = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetProductDiscovered_619441887(null, productID, autoList);
		}
	}

	private void RpcWriter___Target_SetProductDiscovered_619441887(NetworkConnection conn, string productID, bool autoList)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteBoolean(autoList);
			((NetworkBehaviour)this).SendTargetRpc(13u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetProductDiscovered_619441887(PooledReader PooledReader0, Channel channel)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		bool autoList = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetProductDiscovered_619441887(((NetworkBehaviour)this).LocalConnection, productID, autoList);
		}
	}

	private void RpcWriter___Server_CreateWeed_Server_2331775230(string name, string id, EDrugType type, List<string> properties, WeedAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendServerRpc(14u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___CreateWeed_Server_2331775230(string name, string id, EDrugType type, List<string> properties, WeedAppearanceSettings appearance)
	{
		CreateWeed(null, name, id, type, properties, appearance);
	}

	private void RpcReader___Server_CreateWeed_Server_2331775230(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		WeedAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___CreateWeed_Server_2331775230(name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Target_CreateWeed_1777266891(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, WeedAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendTargetRpc(15u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___CreateWeed_1777266891(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, WeedAppearanceSettings appearance)
	{
		if (!Registry.ItemExists(id))
		{
			WeedDefinition weedDefinition = Object.Instantiate<WeedDefinition>(DefaultWeed);
			((Object)weedDefinition).name = name;
			((BaseItemDefinition)weedDefinition).Name = name;
			((BaseItemDefinition)weedDefinition).Description = string.Empty;
			((BaseItemDefinition)weedDefinition).ID = id;
			weedDefinition.Initialize(Singleton<PropertyUtility>.Instance.GetProperties(properties), new List<EDrugType> { type }, appearance);
			AllProducts.Add(weedDefinition);
			ProductPrices.Add(weedDefinition, weedDefinition.MarketValue);
			ProductNames.Add(name);
			createdProducts.Add(weedDefinition);
			Singleton<Registry>.Instance.AddToRegistry(weedDefinition);
			((BaseItemDefinition)weedDefinition).Icon = Singleton<ProductIconManager>.Instance.GenerateIcons(id);
			if ((Object)(object)((BaseItemDefinition)weedDefinition).Icon == (Object)null)
			{
				Console.LogError("Failed to generate icons for " + name);
			}
			SetProductDiscovered(null, id, autoList: false);
			RefreshHighestValueProduct();
			if (onNewProductCreated != null)
			{
				onNewProductCreated(weedDefinition);
			}
		}
	}

	private void RpcReader___Target_CreateWeed_1777266891(PooledReader PooledReader0, Channel channel)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		WeedAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateWeed_1777266891(((NetworkBehaviour)this).LocalConnection, name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Observers_CreateWeed_1777266891(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, WeedAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendObserversRpc(16u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_CreateWeed_1777266891(PooledReader PooledReader0, Channel channel)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		WeedAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EWeedAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___CreateWeed_1777266891(null, name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Server_CreateCocaine_Server_891166717(string name, string id, EDrugType type, List<string> properties, CocaineAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendServerRpc(17u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___CreateCocaine_Server_891166717(string name, string id, EDrugType type, List<string> properties, CocaineAppearanceSettings appearance)
	{
		CreateCocaine(null, name, id, type, properties, appearance);
	}

	private void RpcReader___Server_CreateCocaine_Server_891166717(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		CocaineAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___CreateCocaine_Server_891166717(name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Target_CreateCocaine_1327282946(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, CocaineAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendTargetRpc(18u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___CreateCocaine_1327282946(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, CocaineAppearanceSettings appearance)
	{
		if ((Object)(object)Registry.GetItem(id) != (Object)null)
		{
			Console.LogError("Product with ID " + id + " already exists");
			return;
		}
		CocaineDefinition cocaineDefinition = Object.Instantiate<CocaineDefinition>(DefaultCocaine);
		((Object)cocaineDefinition).name = name;
		((BaseItemDefinition)cocaineDefinition).Name = name;
		((BaseItemDefinition)cocaineDefinition).Description = string.Empty;
		((BaseItemDefinition)cocaineDefinition).ID = id;
		cocaineDefinition.Initialize(Singleton<PropertyUtility>.Instance.GetProperties(properties), new List<EDrugType> { type }, appearance);
		AllProducts.Add(cocaineDefinition);
		ProductPrices.Add(cocaineDefinition, cocaineDefinition.MarketValue);
		ProductNames.Add(name);
		createdProducts.Add(cocaineDefinition);
		Singleton<Registry>.Instance.AddToRegistry(cocaineDefinition);
		((BaseItemDefinition)cocaineDefinition).Icon = Singleton<ProductIconManager>.Instance.GenerateIcons(id);
		if ((Object)(object)((BaseItemDefinition)cocaineDefinition).Icon == (Object)null)
		{
			Console.LogError("Failed to generate icons for " + name);
		}
		SetProductDiscovered(null, id, autoList: false);
		RefreshHighestValueProduct();
		if (onNewProductCreated != null)
		{
			onNewProductCreated(cocaineDefinition);
		}
	}

	private void RpcReader___Target_CreateCocaine_1327282946(PooledReader PooledReader0, Channel channel)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		CocaineAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateCocaine_1327282946(((NetworkBehaviour)this).LocalConnection, name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Observers_CreateCocaine_1327282946(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, CocaineAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendObserversRpc(19u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_CreateCocaine_1327282946(PooledReader PooledReader0, Channel channel)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		CocaineAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002ECocaineAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___CreateCocaine_1327282946(null, name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Server_CreateMeth_Server_4251728555(string name, string id, EDrugType type, List<string> properties, MethAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendServerRpc(20u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___CreateMeth_Server_4251728555(string name, string id, EDrugType type, List<string> properties, MethAppearanceSettings appearance)
	{
		CreateMeth(null, name, id, type, properties, appearance);
	}

	private void RpcReader___Server_CreateMeth_Server_4251728555(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		MethAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___CreateMeth_Server_4251728555(name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Target_CreateMeth_1869045686(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, MethAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendTargetRpc(21u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___CreateMeth_1869045686(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, MethAppearanceSettings appearance)
	{
		if ((Object)(object)Registry.GetItem(id) != (Object)null)
		{
			Console.LogError("Product with ID " + id + " already exists");
			return;
		}
		MethDefinition methDefinition = Object.Instantiate<MethDefinition>(DefaultMeth);
		((Object)methDefinition).name = name;
		((BaseItemDefinition)methDefinition).Name = name;
		((BaseItemDefinition)methDefinition).Description = string.Empty;
		((BaseItemDefinition)methDefinition).ID = id;
		methDefinition.Initialize(Singleton<PropertyUtility>.Instance.GetProperties(properties), new List<EDrugType> { type }, appearance);
		AllProducts.Add(methDefinition);
		ProductPrices.Add(methDefinition, methDefinition.MarketValue);
		ProductNames.Add(name);
		createdProducts.Add(methDefinition);
		Singleton<Registry>.Instance.AddToRegistry(methDefinition);
		((BaseItemDefinition)methDefinition).Icon = Singleton<ProductIconManager>.Instance.GenerateIcons(id);
		if ((Object)(object)((BaseItemDefinition)methDefinition).Icon == (Object)null)
		{
			Console.LogError("Failed to generate icons for " + name);
		}
		SetProductDiscovered(null, id, autoList: false);
		RefreshHighestValueProduct();
		if (onNewProductCreated != null)
		{
			onNewProductCreated(methDefinition);
		}
	}

	private void RpcReader___Target_CreateMeth_1869045686(PooledReader PooledReader0, Channel channel)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		MethAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateMeth_1869045686(((NetworkBehaviour)this).LocalConnection, name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Observers_CreateMeth_1869045686(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, MethAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendObserversRpc(22u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_CreateMeth_1869045686(PooledReader PooledReader0, Channel channel)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		MethAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EMethAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___CreateMeth_1869045686(null, name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Server_CreateShroom_Server_2261384965(string name, string id, EDrugType type, List<string> properties, ShroomAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EShroomAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendServerRpc(23u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___CreateShroom_Server_2261384965(string name, string id, EDrugType type, List<string> properties, ShroomAppearanceSettings appearance)
	{
		CreateShroom_Client(null, name, id, type, properties, appearance);
	}

	private void RpcReader___Server_CreateShroom_Server_2261384965(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		ShroomAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EShroomAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___CreateShroom_Server_2261384965(name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Target_CreateShroom_Client_812995776(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, ShroomAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EShroomAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendTargetRpc(24u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcLogic___CreateShroom_Client_812995776(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, ShroomAppearanceSettings appearance)
	{
		if (Registry.ItemExists(id))
		{
			Console.LogError("Product with ID " + id + " already exists");
			return;
		}
		ShroomDefinition shroomDefinition = Object.Instantiate<ShroomDefinition>(DefaultShroom);
		((Object)shroomDefinition).name = name;
		((BaseItemDefinition)shroomDefinition).Name = name;
		((BaseItemDefinition)shroomDefinition).Description = string.Empty;
		((BaseItemDefinition)shroomDefinition).ID = id;
		shroomDefinition.Initialize(Singleton<PropertyUtility>.Instance.GetProperties(properties), new List<EDrugType> { type }, appearance);
		AllProducts.Add(shroomDefinition);
		ProductPrices.Add(shroomDefinition, shroomDefinition.MarketValue);
		ProductNames.Add(name);
		createdProducts.Add(shroomDefinition);
		Singleton<Registry>.Instance.AddToRegistry(shroomDefinition);
		((BaseItemDefinition)shroomDefinition).Icon = Singleton<ProductIconManager>.Instance.GenerateIcons(id);
		if ((Object)(object)((BaseItemDefinition)shroomDefinition).Icon == (Object)null)
		{
			Console.LogError("Failed to generate icons for " + name);
		}
		SetProductDiscovered(null, id, autoList: false);
		RefreshHighestValueProduct();
		if (onNewProductCreated != null)
		{
			onNewProductCreated(shroomDefinition);
		}
	}

	private void RpcReader___Target_CreateShroom_Client_812995776(PooledReader PooledReader0, Channel channel)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		ShroomAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EShroomAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateShroom_Client_812995776(((NetworkBehaviour)this).LocalConnection, name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Observers_CreateShroom_Client_812995776(NetworkConnection conn, string name, string id, EDrugType type, List<string> properties, ShroomAppearanceSettings appearance)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(name);
			((Writer)writer).WriteString(id);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerated((Writer)(object)writer, type);
			GeneratedWriters___Internal.Write___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerated((Writer)(object)writer, properties);
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002EShroomAppearanceSettingsFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((NetworkBehaviour)this).SendObserversRpc(25u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_CreateShroom_Client_812995776(PooledReader PooledReader0, Channel channel)
	{
		string name = ((Reader)PooledReader0).ReadString();
		string id = ((Reader)PooledReader0).ReadString();
		EDrugType type = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EEDrugTypeFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		List<string> properties = GeneratedReaders___Internal.Read___System_002ECollections_002EGeneric_002EList_00601_003CSystem_002EString_003EFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		ShroomAppearanceSettings appearance = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002EShroomAppearanceSettingsFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___CreateShroom_Client_812995776(null, name, id, type, properties, appearance);
		}
	}

	private void RpcWriter___Server_SendMixRecipe_852232071(string product, string mixer, string output)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(product);
			((Writer)writer).WriteString(mixer);
			((Writer)writer).WriteString(output);
			((NetworkBehaviour)this).SendServerRpc(26u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendMixRecipe_852232071(string product, string mixer, string output)
	{
		CreateMixRecipe(null, product, mixer, output);
	}

	private void RpcReader___Server_SendMixRecipe_852232071(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string product = ((Reader)PooledReader0).ReadString();
		string mixer = ((Reader)PooledReader0).ReadString();
		string output = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendMixRecipe_852232071(product, mixer, output);
		}
	}

	private void RpcWriter___Target_CreateMixRecipe_1410895574(NetworkConnection conn, string product, string mixer, string output)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(product);
			((Writer)writer).WriteString(mixer);
			((Writer)writer).WriteString(output);
			((NetworkBehaviour)this).SendTargetRpc(27u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	public void RpcLogic___CreateMixRecipe_1410895574(NetworkConnection conn, string product, string mixer, string output)
	{
		if (string.IsNullOrEmpty(product) || string.IsNullOrEmpty(mixer) || string.IsNullOrEmpty(output))
		{
			Console.LogError("Invalid mix recipe: Product:" + product + " Mixer:" + mixer + " Output:" + output);
			return;
		}
		StationRecipe stationRecipe = null;
		for (int i = 0; i < mixRecipes.Count; i++)
		{
			if (!((Object)(object)mixRecipes[i] == (Object)null) && mixRecipes[i].Product != null && mixRecipes[i].Ingredients.Count >= 2)
			{
				string iD = ((BaseItemDefinition)mixRecipes[i].Ingredients[0].Items[0]).ID;
				string iD2 = ((BaseItemDefinition)mixRecipes[i].Ingredients[1].Items[0]).ID;
				string iD3 = ((BaseItemDefinition)mixRecipes[i].Product.Item).ID;
				if (iD == product && iD2 == mixer && iD3 == output)
				{
					stationRecipe = mixRecipes[i];
					break;
				}
				if (iD2 == product && iD == mixer && iD3 == output)
				{
					stationRecipe = mixRecipes[i];
					break;
				}
			}
		}
		if ((Object)(object)stationRecipe != (Object)null)
		{
			return;
		}
		StationRecipe stationRecipe2 = ScriptableObject.CreateInstance<StationRecipe>();
		ItemDefinition item = Registry.GetItem(product);
		ItemDefinition item2 = Registry.GetItem(mixer);
		if ((Object)(object)item == (Object)null)
		{
			Console.LogError("Product not found: " + product);
			return;
		}
		if ((Object)(object)item2 == (Object)null)
		{
			Console.LogError("Mixer not found: " + mixer);
			return;
		}
		stationRecipe2.Ingredients = new List<StationRecipe.IngredientQuantity>();
		stationRecipe2.Ingredients.Add(new StationRecipe.IngredientQuantity
		{
			Items = new List<ItemDefinition> { item },
			Quantity = 1
		});
		stationRecipe2.Ingredients.Add(new StationRecipe.IngredientQuantity
		{
			Items = new List<ItemDefinition> { item2 },
			Quantity = 1
		});
		ItemDefinition item3 = Registry.GetItem(output);
		if ((Object)(object)item3 == (Object)null)
		{
			Console.LogError("Output item not found: " + output);
			return;
		}
		stationRecipe2.Product = new StationRecipe.ItemQuantity
		{
			Item = item3,
			Quantity = 1
		};
		stationRecipe2.RecipeTitle = ((BaseItemDefinition)stationRecipe2.Product.Item).Name;
		stationRecipe2.Unlocked = true;
		mixRecipes.Add(stationRecipe2);
		if (onMixRecipeAdded != null)
		{
			onMixRecipeAdded(stationRecipe2);
		}
		ProductDefinition productDefinition = stationRecipe2.Product.Item as ProductDefinition;
		if ((Object)(object)productDefinition != (Object)null)
		{
			productDefinition.AddRecipe(stationRecipe2);
		}
		else
		{
			Console.LogError("Product is not a product definition: " + product);
		}
		HasChanged = true;
	}

	private void RpcReader___Target_CreateMixRecipe_1410895574(PooledReader PooledReader0, Channel channel)
	{
		string product = ((Reader)PooledReader0).ReadString();
		string mixer = ((Reader)PooledReader0).ReadString();
		string output = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___CreateMixRecipe_1410895574(((NetworkBehaviour)this).LocalConnection, product, mixer, output);
		}
	}

	private void RpcWriter___Observers_CreateMixRecipe_1410895574(NetworkConnection conn, string product, string mixer, string output)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(product);
			((Writer)writer).WriteString(mixer);
			((Writer)writer).WriteString(output);
			((NetworkBehaviour)this).SendObserversRpc(28u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcReader___Observers_CreateMixRecipe_1410895574(PooledReader PooledReader0, Channel channel)
	{
		string product = ((Reader)PooledReader0).ReadString();
		string mixer = ((Reader)PooledReader0).ReadString();
		string output = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___CreateMixRecipe_1410895574(null, product, mixer, output);
		}
	}

	private void RpcWriter___Server_SendPrice_606697822(string productID, float value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteSingle(value, (AutoPackType)0);
			((NetworkBehaviour)this).SendServerRpc(29u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendPrice_606697822(string productID, float value)
	{
		SetPrice(null, productID, value);
	}

	private void RpcReader___Server_SendPrice_606697822(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendPrice_606697822(productID, value);
		}
	}

	private void RpcWriter___Observers_SetPrice_4077118173(NetworkConnection conn, string productID, float value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteSingle(value, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(30u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	public void RpcLogic___SetPrice_4077118173(NetworkConnection conn, string productID, float value)
	{
		ProductDefinition item = Registry.GetItem<ProductDefinition>(productID);
		if ((Object)(object)item == (Object)null)
		{
			Console.LogError("Product not found: " + productID);
			return;
		}
		value = Mathf.RoundToInt(Mathf.Clamp(value, 1f, 999f));
		if (!ProductPrices.ContainsKey(item))
		{
			ProductPrices.Add(item, value);
		}
		else
		{
			ProductPrices[item] = value;
		}
	}

	private void RpcReader___Observers_SetPrice_4077118173(PooledReader PooledReader0, Channel channel)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetPrice_4077118173(null, productID, value);
		}
	}

	private void RpcWriter___Target_SetPrice_4077118173(NetworkConnection conn, string productID, float value)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteSingle(value, (AutoPackType)0);
			((NetworkBehaviour)this).SendTargetRpc(31u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetPrice_4077118173(PooledReader PooledReader0, Channel channel)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		float value = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetPrice_4077118173(((NetworkBehaviour)this).LocalConnection, productID, value);
		}
	}

	private void RpcWriter___Server_SendMixOperation_3670976965(NewMixOperation operation, bool complete)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerated((Writer)(object)writer, operation);
			((Writer)writer).WriteBoolean(complete);
			((NetworkBehaviour)this).SendServerRpc(32u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendMixOperation_3670976965(NewMixOperation operation, bool complete)
	{
		SetMixOperation(operation, complete);
	}

	private void RpcReader___Server_SendMixOperation_3670976965(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		NewMixOperation operation = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool complete = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendMixOperation_3670976965(operation, complete);
		}
	}

	private void RpcWriter___Observers_SetMixOperation_3670976965(NewMixOperation operation, bool complete)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerated((Writer)(object)writer, operation);
			((Writer)writer).WriteBoolean(complete);
			((NetworkBehaviour)this).SendObserversRpc(33u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetMixOperation_3670976965(NewMixOperation operation, bool complete)
	{
		CurrentMixOperation = operation;
		IsMixComplete = complete;
		if (CurrentMixOperation != null && IsMixComplete && onMixCompleted != null)
		{
			onMixCompleted(CurrentMixOperation);
		}
	}

	private void RpcReader___Observers_SetMixOperation_3670976965(PooledReader PooledReader0, Channel channel)
	{
		NewMixOperation operation = GeneratedReaders___Internal.Read___ScheduleOne_002EProduct_002ENewMixOperationFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		bool complete = ((Reader)PooledReader0).ReadBoolean();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetMixOperation_3670976965(operation, complete);
		}
	}

	private void RpcWriter___Observers_FinishAndNameMix_4237212381(string productID, string ingredientID, string mixName, string mixID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteString(ingredientID);
			((Writer)writer).WriteString(mixName);
			((Writer)writer).WriteString(mixID);
			((NetworkBehaviour)this).SendObserversRpc(34u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___FinishAndNameMix_4237212381(string productID, string ingredientID, string mixName, string mixID)
	{
		if ((Object)(object)AllProducts.Find((ProductDefinition p) => ((BaseItemDefinition)p).ID == mixID) != (Object)null)
		{
			return;
		}
		ProductDefinition productDefinition = Registry.GetItem(productID) as ProductDefinition;
		PropertyItemDefinition propertyItemDefinition = Registry.GetItem(ingredientID) as PropertyItemDefinition;
		if ((Object)(object)productDefinition == (Object)null || (Object)(object)propertyItemDefinition == (Object)null)
		{
			Debug.LogError((object)"Product or mixer not found");
			return;
		}
		List<Effect> list = EffectMixCalculator.MixProperties(productDefinition.Properties, propertyItemDefinition.Properties[0], productDefinition.DrugType);
		List<string> list2 = new List<string>();
		foreach (Effect item in list)
		{
			list2.Add(item.ID);
		}
		switch (productDefinition.DrugType)
		{
		case EDrugType.Marijuana:
			CreateWeed(null, mixName, mixID, EDrugType.Marijuana, list2, WeedDefinition.GetAppearanceSettings(list));
			break;
		case EDrugType.Methamphetamine:
			CreateMeth(null, mixName, mixID, EDrugType.Methamphetamine, list2, MethDefinition.GetAppearanceSettings(list));
			break;
		case EDrugType.Cocaine:
			CreateCocaine(null, mixName, mixID, EDrugType.Cocaine, list2, CocaineDefinition.GetAppearanceSettings(list));
			break;
		case EDrugType.Shrooms:
			CreateShroom_Client(null, mixName, mixID, EDrugType.Shrooms, list2, ShroomDefinition.GetAppearanceSettings(list));
			break;
		default:
			Console.LogError("Drug type not supported");
			break;
		}
	}

	private void RpcReader___Observers_FinishAndNameMix_4237212381(PooledReader PooledReader0, Channel channel)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		string ingredientID = ((Reader)PooledReader0).ReadString();
		string mixName = ((Reader)PooledReader0).ReadString();
		string mixID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___FinishAndNameMix_4237212381(productID, ingredientID, mixName, mixID);
		}
	}

	private void RpcWriter___Server_SendFinishAndNameMix_4237212381(string productID, string ingredientID, string mixName, string mixID)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteString(productID);
			((Writer)writer).WriteString(ingredientID);
			((Writer)writer).WriteString(mixName);
			((Writer)writer).WriteString(mixID);
			((NetworkBehaviour)this).SendServerRpc(35u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendFinishAndNameMix_4237212381(string productID, string ingredientID, string mixName, string mixID)
	{
		FinishAndNameMix(productID, ingredientID, mixName, mixID);
		CreateMixRecipe(null, productID, ingredientID, mixID);
	}

	private void RpcReader___Server_SendFinishAndNameMix_4237212381(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string productID = ((Reader)PooledReader0).ReadString();
		string ingredientID = ((Reader)PooledReader0).ReadString();
		string mixName = ((Reader)PooledReader0).ReadString();
		string mixID = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___SendFinishAndNameMix_4237212381(productID, ingredientID, mixName, mixID);
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002EProduct_002EProductManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		InitializeSaveable();
	}
}
