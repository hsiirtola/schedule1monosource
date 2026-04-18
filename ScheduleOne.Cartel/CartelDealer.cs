using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Serializing.Generated;
using FishNet.Transporting;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Cartel;

public class CartelDealer : Dealer
{
	public const float DEALER_DEFEATED_INFLUENCE_CHANGE = -0.1f;

	public const int PRODUCT_COUNT_MIN = 2;

	public const int PRODUCT_COUNT_MAX = 4;

	public const int PRODUCT_QUANTITY_MIN = 1;

	public const int PRODUCT_QUANTITY_MAX = 10;

	[Header("Cartel Dealer Inventory Settings")]
	public ProductDefinition[] RandomProducts;

	public EQuality ProductQuality;

	public PackagingDefinition DefaultPackaging;

	private CartelGoonAppearance appearance;

	private bool NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelDealerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ECartel_002ECartelDealerAssembly_002DCSharp_002Edll_Excuted;

	public bool IsAcceptingDeals { get; private set; }

	private GoonPool GoonPool => NetworkSingleton<Cartel>.Instance.GoonPool;

	protected override void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.Start();
		Health.onDieOrKnockedOut.AddListener(new UnityAction(DiedOrKnockedOut));
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (!connection.IsHost && appearance != null)
		{
			ConfigureGoonSettings(connection, appearance, Movement.MoveSpeedMultiplier);
		}
	}

	public void RandomizeInventory()
	{
		if (InstanceFinder.IsServer)
		{
			Inventory.Clear();
			int num = Random.Range(2, 5);
			for (int i = 0; i < num; i++)
			{
				ProductDefinition obj = RandomProducts[Random.Range(0, RandomProducts.Length)];
				int quantity = Random.Range(1, 11);
				ProductItemInstance productItemInstance = obj.GetDefaultInstance(quantity) as ProductItemInstance;
				productItemInstance.SetQuality(ProductQuality);
				productItemInstance.SetPackaging(DefaultPackaging);
				Inventory.InsertItem(productItemInstance);
			}
		}
	}

	public void RandomizeAppearance()
	{
		if (InstanceFinder.IsServer)
		{
			appearance = GoonPool.GetRandomAppearance();
			ConfigureGoonSettings(null, appearance, Random.Range(0.8f, 1.1f));
		}
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	private void ConfigureGoonSettings(NetworkConnection conn, CartelGoonAppearance appearance, float moveSpeed)
	{
		if (conn == null)
		{
			RpcWriter___Observers_ConfigureGoonSettings_3427656873(conn, appearance, moveSpeed);
			RpcLogic___ConfigureGoonSettings_3427656873(conn, appearance, moveSpeed);
		}
		else
		{
			RpcWriter___Target_ConfigureGoonSettings_3427656873(conn, appearance, moveSpeed);
		}
	}

	public void SetIsAcceptingDeals(bool accepting)
	{
		Console.Log("Cartel dealer " + base.fullName + " is now " + (accepting ? "accepting" : "not accepting") + " deals.");
		IsAcceptingDeals = accepting;
	}

	public bool CanCurrentlyAcceptDeal()
	{
		if (!base.IsConscious)
		{
			return false;
		}
		return IsAcceptingDeals;
	}

	private void DiedOrKnockedOut()
	{
		if (InstanceFinder.IsServer && NetworkSingleton<Cartel>.Instance.Status == ECartelStatus.Hostile)
		{
			NetworkSingleton<Cartel>.Instance.Influence.ChangeInfluence(Region, -0.1f);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelDealerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ECartel_002ECartelDealerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(63u, new ClientRpcDelegate(RpcReader___Observers_ConfigureGoonSettings_3427656873));
			((NetworkBehaviour)this).RegisterTargetRpc(64u, new ClientRpcDelegate(RpcReader___Target_ConfigureGoonSettings_3427656873));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ECartel_002ECartelDealerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ECartel_002ECartelDealerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_ConfigureGoonSettings_3427656873(NetworkConnection conn, CartelGoonAppearance appearance, float moveSpeed)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((Writer)writer).WriteSingle(moveSpeed, (AutoPackType)0);
			((NetworkBehaviour)this).SendObserversRpc(63u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___ConfigureGoonSettings_3427656873(NetworkConnection conn, CartelGoonAppearance appearance, float moveSpeed)
	{
		if (appearance == null)
		{
			Console.LogError("CartelDealer.ConfigureGoonSettings called with null appearance. Cannot configure goon without a valid appearance.");
			return;
		}
		Avatar.LoadAvatarSettings(appearance.IsMale ? GoonPool.MaleClothing[appearance.ClothingIndex] : GoonPool.FemaleClothing[appearance.ClothingIndex]);
		Avatar.LoadNakedSettings(appearance.IsMale ? GoonPool.MaleBaseAppearances[appearance.BaseAppearanceIndex] : GoonPool.FemaleBaseAppearances[appearance.BaseAppearanceIndex], 100);
		VoiceOverEmitter.SetDatabase(appearance.IsMale ? GoonPool.MaleVoices[appearance.VoiceIndex] : GoonPool.FemaleVoices[appearance.VoiceIndex]);
		Movement.MoveSpeedMultiplier = moveSpeed;
	}

	private void RpcReader___Observers_ConfigureGoonSettings_3427656873(PooledReader PooledReader0, Channel channel)
	{
		CartelGoonAppearance cartelGoonAppearance = GeneratedReaders___Internal.Read___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float moveSpeed = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___ConfigureGoonSettings_3427656873(null, cartelGoonAppearance, moveSpeed);
		}
	}

	private void RpcWriter___Target_ConfigureGoonSettings_3427656873(NetworkConnection conn, CartelGoonAppearance appearance, float moveSpeed)
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
			GeneratedWriters___Internal.Write___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerated((Writer)(object)writer, appearance);
			((Writer)writer).WriteSingle(moveSpeed, (AutoPackType)0);
			((NetworkBehaviour)this).SendTargetRpc(64u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_ConfigureGoonSettings_3427656873(PooledReader PooledReader0, Channel channel)
	{
		CartelGoonAppearance cartelGoonAppearance = GeneratedReaders___Internal.Read___ScheduleOne_002ECartel_002ECartelGoonAppearanceFishNet_002ESerializing_002EGenerateds((Reader)(object)PooledReader0);
		float moveSpeed = ((Reader)PooledReader0).ReadSingle((AutoPackType)0);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___ConfigureGoonSettings_3427656873(((NetworkBehaviour)this).LocalConnection, cartelGoonAppearance, moveSpeed);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
