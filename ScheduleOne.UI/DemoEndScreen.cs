using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using Steamworks;
using UnityEngine;

namespace ScheduleOne.UI;

public class DemoEndScreen : MonoBehaviour
{
	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public bool IsOpen { get; private set; }

	public void Awake()
	{
		GameInput.RegisterExitListener(Exit, 4);
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		GameInput.DeregisterExitListener(Exit);
	}

	[Button]
	public void Open()
	{
	}

	private void Update()
	{
		if (IsOpen)
		{
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		}
	}

	public void Close()
	{
		IsOpen = false;
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
	}

	private void Exit(ExitAction action)
	{
		if (IsOpen && !action.Used && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close();
		}
	}

	public void LinkClicked()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if (SteamManager.Initialized)
		{
			SteamFriends.ActivateGameOverlayToStore(new AppId_t(3164500u), (EOverlayToStoreFlag)0);
		}
	}
}
