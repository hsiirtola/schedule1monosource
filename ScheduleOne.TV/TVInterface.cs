using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.Compass;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.TV;

public class TVInterface : MonoBehaviour
{
	public const float OPEN_TIME = 0.15f;

	public const float FOV = 60f;

	public List<Player> Players = new List<Player>();

	[Header("References")]
	public Canvas Canvas;

	public Transform CameraPosition;

	public TVHomeScreen HomeScreen;

	public TextMeshPro TimeLabel;

	public TextMeshPro Daylabel;

	public UnityEvent<Player> onPlayerAdded = new UnityEvent<Player>();

	public UnityEvent<Player> onPlayerRemoved = new UnityEvent<Player>();

	public bool IsOpen { get; private set; }

	public void Awake()
	{
		((Behaviour)Canvas).enabled = false;
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
		GameInput.RegisterExitListener(Exit, 2);
	}

	private void OnDestroy()
	{
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		}
	}

	private void MinPass()
	{
		((TMP_Text)TimeLabel).text = TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime);
		((TMP_Text)Daylabel).text = NetworkSingleton<TimeManager>.Instance.CurrentDay.ToString();
	}

	public void Open()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOpen)
		{
			IsOpen = true;
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0.15f);
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.15f);
			PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
			PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
			PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
			Singleton<CompassManager>.Instance.SetVisible(visible: false);
			AddPlayer(Player.Local);
			((Behaviour)Canvas).enabled = true;
			((Component)TimeLabel).gameObject.SetActive(false);
			HomeScreen.Open();
		}
	}

	public void Close()
	{
		if (IsOpen)
		{
			IsOpen = false;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.15f);
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.15f);
			PlayerSingleton<PlayerCamera>.Instance.LockMouse();
			PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
			RemovePlayer(Player.Local);
			((Behaviour)Canvas).enabled = false;
			((Component)TimeLabel).gameObject.SetActive(true);
			Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
			Singleton<CompassManager>.Instance.SetVisible(visible: true);
		}
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen)
		{
			action.Used = true;
			Close();
		}
	}

	public bool CanOpen()
	{
		return !IsOpen;
	}

	public void AddPlayer(Player player)
	{
		if (!Players.Contains(player))
		{
			Players.Add(player);
			if (onPlayerAdded != null)
			{
				onPlayerAdded.Invoke(player);
			}
		}
	}

	public void RemovePlayer(Player player)
	{
		if (Players.Contains(player))
		{
			Players.Remove(player);
			if (onPlayerRemoved != null)
			{
				onPlayerRemoved.Invoke(player);
			}
		}
	}
}
