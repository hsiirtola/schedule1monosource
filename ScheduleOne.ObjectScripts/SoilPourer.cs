using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tiles;
using UnityEngine;

namespace ScheduleOne.ObjectScripts;

public class SoilPourer : GridItem
{
	public float AnimationDuration = 8f;

	[Header("References")]
	public InteractableObject HandleIntObj;

	public InteractableObject FillIntObj;

	public MeshRenderer DirtPlane;

	public Transform Dirt_Min;

	public Transform Dirt_Max;

	public ParticleSystem PourParticles;

	public Animation PourAnimation;

	public AudioSourceController FillSound;

	public AudioSourceController ActivateSound;

	public AudioSourceController DirtPourSound;

	private bool isDispensing;

	private bool NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted;

	public string SoilID { get; protected set; } = string.Empty;

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);
		if (SoilID != string.Empty)
		{
			SoilDefinition item = Registry.GetItem<SoilDefinition>(SoilID);
			((Renderer)DirtPlane).material = item.DrySoilMat;
			SetSoilLevel(1f);
		}
	}

	public void HandleHovered()
	{
		if (!string.IsNullOrEmpty(SoilID) && !isDispensing)
		{
			HandleIntObj.SetMessage("Dispense soil");
			HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			HandleIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	public void HandleInteracted()
	{
		if (!string.IsNullOrEmpty(SoilID) && !isDispensing)
		{
			SendPourSoil();
			isDispensing = true;
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	private void SendPourSoil()
	{
		RpcWriter___Server_SendPourSoil_2166136261();
		RpcLogic___SendPourSoil_2166136261();
	}

	[ObserversRpc(RunLocally = true)]
	private void PourSoil()
	{
		RpcWriter___Observers_PourSoil_2166136261();
		RpcLogic___PourSoil_2166136261();
	}

	public void FillHovered()
	{
		bool flag = false;
		if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance.Definition is SoilDefinition)
		{
			flag = true;
		}
		if (SoilID == string.Empty && flag)
		{
			FillIntObj.SetMessage("Insert soil");
			FillIntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			FillIntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	public void FillInteracted()
	{
		bool flag = false;
		if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance.Definition is SoilDefinition)
		{
			flag = true;
		}
		if (SoilID == string.Empty && flag)
		{
			FillSound.Play();
			SendSoil(((BaseItemDefinition)PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance.Definition).ID);
			PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ChangeQuantity(-1);
		}
	}

	[ServerRpc(RequireOwnership = false, RunLocally = true)]
	public void SendSoil(string ID)
	{
		RpcWriter___Server_SendSoil_3615296227(ID);
		RpcLogic___SendSoil_3615296227(ID);
	}

	[ObserversRpc(RunLocally = true)]
	[TargetRpc]
	protected void SetSoil(NetworkConnection conn, string ID)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetSoil_2971853958(conn, ID);
			RpcLogic___SetSoil_2971853958(conn, ID);
		}
		else
		{
			RpcWriter___Target_SetSoil_2971853958(conn, ID);
		}
	}

	public void SetSoilLevel(float level)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Component)DirtPlane).transform.localPosition = Vector3.Lerp(Dirt_Min.localPosition, Dirt_Max.localPosition, level);
		((Component)DirtPlane).gameObject.SetActive(level > 0f);
	}

	protected virtual List<Pot> GetPots()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		List<Pot> list = new List<Pot>();
		Coordinate coord = new Coordinate(_originCoordinate) + Coordinate.RotateCoordinates(new Coordinate(0, 1), _rotation);
		Coordinate coord2 = new Coordinate(_originCoordinate) + Coordinate.RotateCoordinates(new Coordinate(1, 1), _rotation);
		Tile tile = base.OwnerGrid.GetTile(coord);
		Tile tile2 = base.OwnerGrid.GetTile(coord2);
		if ((Object)(object)tile != (Object)null && (Object)(object)tile2 != (Object)null)
		{
			Pot pot = null;
			foreach (GridItem buildableOccupant in tile.BuildableOccupants)
			{
				if (buildableOccupant is Pot)
				{
					pot = buildableOccupant as Pot;
					break;
				}
			}
			if ((Object)(object)pot != (Object)null && tile2.BuildableOccupants.Contains(pot))
			{
				list.Add(pot);
			}
		}
		return list;
	}

	public override BuildableItemData GetBaseData()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return new SoilPourerData(base.GUID, base.ItemInstance, 0, base.OwnerGrid, _originCoordinate, _rotation, SoilID);
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
		if (!NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterServerRpc(5u, new ServerRpcDelegate(RpcReader___Server_SendPourSoil_2166136261));
			((NetworkBehaviour)this).RegisterObserversRpc(6u, new ClientRpcDelegate(RpcReader___Observers_PourSoil_2166136261));
			((NetworkBehaviour)this).RegisterServerRpc(7u, new ServerRpcDelegate(RpcReader___Server_SendSoil_3615296227));
			((NetworkBehaviour)this).RegisterObserversRpc(8u, new ClientRpcDelegate(RpcReader___Observers_SetSoil_2971853958));
			((NetworkBehaviour)this).RegisterTargetRpc(9u, new ClientRpcDelegate(RpcReader___Target_SetSoil_2971853958));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EObjectScripts_002ESoilPourerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_SendPourSoil_2166136261()
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
			((NetworkBehaviour)this).SendServerRpc(5u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___SendPourSoil_2166136261()
	{
		PourSoil();
	}

	private void RpcReader___Server_SendPourSoil_2166136261(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendPourSoil_2166136261();
		}
	}

	private void RpcWriter___Observers_PourSoil_2166136261()
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
			((NetworkBehaviour)this).SendObserversRpc(6u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___PourSoil_2166136261()
	{
		if (!isDispensing)
		{
			isDispensing = true;
			((MonoBehaviour)this).StartCoroutine(PourRoutine());
		}
		IEnumerator PourRoutine()
		{
			SoilDefinition item = Registry.GetItem<SoilDefinition>(SoilID);
			if ((Object)(object)item == (Object)null)
			{
				Console.LogError("Soil definition not found for ID: " + SoilID);
				isDispensing = false;
			}
			else
			{
				ActivateSound.Play();
				PourParticles.startColor = item.ParticleColor;
				PourParticles.Play();
				PourAnimation.Play();
				DirtPourSound.Play();
				Pot targetPot = GetPots().FirstOrDefault();
				if ((Object)(object)targetPot != (Object)null && (Object)(object)targetPot.CurrentSoil != (Object)null && ((BaseItemDefinition)targetPot.CurrentSoil).ID != ((BaseItemDefinition)item).ID)
				{
					targetPot = null;
				}
				if ((Object)(object)targetPot != (Object)null)
				{
					targetPot.SetSoil(item);
					targetPot.SetSoilState(Pot.ESoilState.Flat);
					targetPot.SetRemainingSoilUses(item.Uses);
				}
				for (float i = 0f; i < AnimationDuration; i += Time.deltaTime)
				{
					float num = i / AnimationDuration;
					SetSoilLevel(1f - num);
					if ((Object)(object)targetPot != (Object)null)
					{
						targetPot.ChangeSoilAmount(targetPot.SoilCapacity * (Time.deltaTime / AnimationDuration) * 1.1f);
					}
					yield return (object)new WaitForEndOfFrame();
				}
				if ((Object)(object)targetPot != (Object)null && InstanceFinder.IsServer)
				{
					targetPot.SyncSoilData();
				}
				SetSoil(null, string.Empty);
				PourParticles.Stop();
				isDispensing = false;
				yield return (object)new WaitForSeconds(1f);
				DirtPourSound.Stop();
			}
		}
	}

	private void RpcReader___Observers_PourSoil_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___PourSoil_2166136261();
		}
	}

	private void RpcWriter___Server_SendSoil_3615296227(string ID)
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
			((Writer)writer).WriteString(ID);
			((NetworkBehaviour)this).SendServerRpc(7u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	public void RpcLogic___SendSoil_3615296227(string ID)
	{
		SetSoil(null, ID);
	}

	private void RpcReader___Server_SendSoil_3615296227(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		string iD = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsServerInitialized && !conn.IsLocalClient)
		{
			RpcLogic___SendSoil_3615296227(iD);
		}
	}

	private void RpcWriter___Observers_SetSoil_2971853958(NetworkConnection conn, string ID)
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
			((Writer)writer).WriteString(ID);
			((NetworkBehaviour)this).SendObserversRpc(8u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	protected void RpcLogic___SetSoil_2971853958(NetworkConnection conn, string ID)
	{
		SoilID = ID;
		if (ID != string.Empty)
		{
			SoilDefinition item = Registry.GetItem<SoilDefinition>(SoilID);
			((Renderer)DirtPlane).material = item.DrySoilMat;
			SetSoilLevel(1f);
		}
	}

	private void RpcReader___Observers_SetSoil_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string iD = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___SetSoil_2971853958(null, iD);
		}
	}

	private void RpcWriter___Target_SetSoil_2971853958(NetworkConnection conn, string ID)
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
			((Writer)writer).WriteString(ID);
			((NetworkBehaviour)this).SendTargetRpc(9u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetSoil_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string iD = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetSoil_2971853958(((NetworkBehaviour)this).LocalConnection, iD);
		}
	}

	public override void Awake()
	{
		NetworkInitialize___Early();
		base.Awake();
		NetworkInitialize__Late();
	}
}
