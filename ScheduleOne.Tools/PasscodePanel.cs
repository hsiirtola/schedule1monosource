using System.Collections;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.Tools;

public class PasscodePanel : NetworkBehaviour
{
	public const int PasscodeLength = 4;

	[Header("Settings")]
	public string CorrectPasscode = "1111";

	[Header("References")]
	public InteractableObject[] Buttons;

	public TextMeshPro CodeLabel;

	public UnityEvent onButtonPressed;

	public UnityEvent onCorrect;

	public UnityEvent onIncorrect;

	private string enteredPasscode = "";

	private bool NetworkInitialize___EarlyScheduleOne_002ETools_002EPasscodePanelAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ETools_002EPasscodePanelAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ETools_002EPasscodePanel_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		((NetworkBehaviour)this).OnSpawnServer(connection);
		if (!connection.IsHost && enteredPasscode != "")
		{
			SetEnteredPasscode(connection, enteredPasscode);
		}
	}

	private void OnButtonPressed(int number)
	{
		SetIsUsable(isUsable: false);
		OnButtonPressed_Server(number);
	}

	[ServerRpc(RequireOwnership = false)]
	private void OnButtonPressed_Server(int number)
	{
		RpcWriter___Server_OnButtonPressed_Server_3316948804(number);
	}

	[ObserversRpc]
	private void RegisterButtonPress(int number)
	{
		RpcWriter___Observers_RegisterButtonPress_3316948804(number);
	}

	public void SetIsUsable(bool isUsable)
	{
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].SetInteractableState((!isUsable) ? InteractableObject.EInteractableState.Disabled : InteractableObject.EInteractableState.Default);
		}
	}

	[ObserversRpc]
	[TargetRpc]
	private void SetEnteredPasscode(NetworkConnection conn, string passcode)
	{
		if (conn == null)
		{
			RpcWriter___Observers_SetEnteredPasscode_2971853958(conn, passcode);
		}
		else
		{
			RpcWriter___Target_SetEnteredPasscode_2971853958(conn, passcode);
		}
	}

	public override void NetworkInitialize___Early()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ETools_002EPasscodePanelAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ETools_002EPasscodePanelAssembly_002DCSharp_002Edll_Excuted = true;
			((NetworkBehaviour)this).RegisterServerRpc(0u, new ServerRpcDelegate(RpcReader___Server_OnButtonPressed_Server_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(1u, new ClientRpcDelegate(RpcReader___Observers_RegisterButtonPress_3316948804));
			((NetworkBehaviour)this).RegisterObserversRpc(2u, new ClientRpcDelegate(RpcReader___Observers_SetEnteredPasscode_2971853958));
			((NetworkBehaviour)this).RegisterTargetRpc(3u, new ClientRpcDelegate(RpcReader___Target_SetEnteredPasscode_2971853958));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ETools_002EPasscodePanelAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ETools_002EPasscodePanelAssembly_002DCSharp_002Edll_Excuted = true;
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Server_OnButtonPressed_Server_3316948804(int number)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(number, (AutoPackType)1);
			((NetworkBehaviour)this).SendServerRpc(0u, writer, val, (DataOrderType)0);
			writer.Store();
		}
	}

	private void RpcLogic___OnButtonPressed_Server_3316948804(int number)
	{
		RegisterButtonPress(number);
	}

	private void RpcReader___Server_OnButtonPressed_Server_3316948804(PooledReader PooledReader0, Channel channel, NetworkConnection conn)
	{
		int number = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsServerInitialized)
		{
			RpcLogic___OnButtonPressed_Server_3316948804(number);
		}
	}

	private void RpcWriter___Observers_RegisterButtonPress_3316948804(int number)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
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
			((Writer)writer).WriteInt32(number, (AutoPackType)1);
			((NetworkBehaviour)this).SendObserversRpc(1u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___RegisterButtonPress_3316948804(int number)
	{
		if (onButtonPressed != null)
		{
			onButtonPressed.Invoke();
		}
		enteredPasscode += number;
		((TMP_Text)CodeLabel).text = enteredPasscode;
		if (enteredPasscode.Length >= 4)
		{
			((MonoBehaviour)this).StartCoroutine(Evaluate());
		}
		else
		{
			SetIsUsable(isUsable: true);
		}
		IEnumerator Evaluate()
		{
			yield return (object)new WaitForSeconds(0.25f);
			if (enteredPasscode == CorrectPasscode)
			{
				((Graphic)CodeLabel).color = Color32.op_Implicit(new Color32((byte)100, byte.MaxValue, (byte)0, byte.MaxValue));
				onCorrect.Invoke();
			}
			else
			{
				enteredPasscode = "";
				((TMP_Text)CodeLabel).text = enteredPasscode;
				onIncorrect.Invoke();
				SetIsUsable(isUsable: true);
			}
		}
	}

	private void RpcReader___Observers_RegisterButtonPress_3316948804(PooledReader PooledReader0, Channel channel)
	{
		int number = ((Reader)PooledReader0).ReadInt32((AutoPackType)1);
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___RegisterButtonPress_3316948804(number);
		}
	}

	private void RpcWriter___Observers_SetEnteredPasscode_2971853958(NetworkConnection conn, string passcode)
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
			((Writer)writer).WriteString(passcode);
			((NetworkBehaviour)this).SendObserversRpc(2u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___SetEnteredPasscode_2971853958(NetworkConnection conn, string passcode)
	{
		enteredPasscode = passcode;
		((TMP_Text)CodeLabel).text = enteredPasscode;
	}

	private void RpcReader___Observers_SetEnteredPasscode_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string passcode = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetEnteredPasscode_2971853958(null, passcode);
		}
	}

	private void RpcWriter___Target_SetEnteredPasscode_2971853958(NetworkConnection conn, string passcode)
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
			((Writer)writer).WriteString(passcode);
			((NetworkBehaviour)this).SendTargetRpc(3u, writer, val, (DataOrderType)0, conn, false, true);
			writer.Store();
		}
	}

	private void RpcReader___Target_SetEnteredPasscode_2971853958(PooledReader PooledReader0, Channel channel)
	{
		string passcode = ((Reader)PooledReader0).ReadString();
		if (((NetworkBehaviour)this).IsClientInitialized)
		{
			RpcLogic___SetEnteredPasscode_2971853958(((NetworkBehaviour)this).LocalConnection, passcode);
		}
	}

	private void Awake_UserLogic_ScheduleOne_002ETools_002EPasscodePanel_Assembly_002DCSharp_002Edll()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		if (Buttons.Length != 9)
		{
			Console.LogError("PasscodePanel on " + ((Object)this).name + " does not have exactly 9 buttons assigned.");
		}
		for (int i = 0; i < Buttons.Length; i++)
		{
			int index = i;
			Buttons[i].SetMessage((i + 1).ToString());
			Buttons[i].onInteractStart.AddListener((UnityAction)delegate
			{
				OnButtonPressed(index + 1);
			});
		}
		((TMP_Text)CodeLabel).text = "";
	}
}
