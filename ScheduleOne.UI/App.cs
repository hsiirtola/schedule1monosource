using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Phone;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public abstract class App<T> : PlayerSingleton<T> where T : PlayerSingleton<T>
{
	public enum EOrientation
	{
		Horizontal,
		Vertical
	}

	public static List<App<T>> Apps = new List<App<T>>();

	[Header("Settings")]
	public string AppName;

	public string IconLabel;

	public Sprite AppIcon;

	public EOrientation Orientation;

	public bool AvailableInTutorial = true;

	[Header("References")]
	[SerializeField]
	protected RectTransform appContainer;

	protected RectTransform notificationContainer;

	protected Text notificationText;

	protected Button appIconButton;

	public bool isOpen { get; protected set; }

	public static App<T> GetApp(int index)
	{
		if (index < 0 || index >= Apps.Count)
		{
			return null;
		}
		return Apps[index];
	}

	public override void OnStartClient(bool IsOwner)
	{
		base.OnStartClient(IsOwner);
		if (IsOwner)
		{
			if (!AvailableInTutorial && NetworkSingleton<GameManager>.Instance.IsTutorial)
			{
				((Component)appContainer).gameObject.SetActive(false);
				return;
			}
			GenerateHomeScreenIcon();
			Apps.Add(this);
		}
	}

	protected override void Start()
	{
		base.Start();
		GameInput.RegisterExitListener(Exit, 1);
		ScheduleOne.UI.Phone.Phone phone = PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance;
		phone.closeApps = (Action)Delegate.Combine(phone.closeApps, new Action(Close));
		ScheduleOne.UI.Phone.Phone phone2 = PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance;
		phone2.onPhoneOpened = (Action)Delegate.Combine(phone2.onPhoneOpened, new Action(OnPhoneOpened));
		SetOpen(open: false);
	}

	private void Close()
	{
		if (isOpen)
		{
			SetOpen(open: false);
		}
	}

	protected virtual void Update()
	{
		if (isOpen && PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.IsOpen && IsHoveringButton() && GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
		{
			SetOpen(open: false);
		}
	}

	private bool IsHoveringButton()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(Singleton<GameplayMenu>.Instance.OverlayCamera.ScreenPointToRay(GameInput.MousePosition), ref val, 2f, 1 << LayerMask.NameToLayer("Overlay")) && ((Object)((Component)((RaycastHit)(ref val)).collider).gameObject).name == "Button")
		{
			return true;
		}
		return false;
	}

	private void GenerateHomeScreenIcon()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		appIconButton = PlayerSingleton<HomeScreen>.Instance.GenerateAppIcon(this);
		((UnityEvent)appIconButton.onClick).AddListener(new UnityAction(ShortcutClicked));
		notificationContainer = ((Component)((Component)appIconButton).transform.Find("Notifications")).GetComponent<RectTransform>();
		notificationText = ((Component)((Transform)notificationContainer).Find("Text")).GetComponent<Text>();
		((Component)notificationContainer).gameObject.SetActive(false);
	}

	public void SetNotificationCount(int amount)
	{
		notificationText.text = amount.ToString();
		((Component)notificationContainer).gameObject.SetActive(amount > 0);
	}

	protected virtual void OnPhoneOpened()
	{
		if (isOpen)
		{
			if (Orientation == EOrientation.Horizontal)
			{
				PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetLookOffsetMultiplier(0.6f);
			}
			else
			{
				PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetLookOffsetMultiplier(1f);
			}
		}
	}

	private void ShortcutClicked()
	{
		SetOpen(!isOpen);
	}

	public virtual void Exit(ExitAction exit)
	{
		if (!exit.Used && isOpen && PlayerSingleton<ScheduleOne.UI.Phone.Phone>.InstanceExists && PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.IsOpen)
		{
			exit.Used = true;
			SetOpen(open: false);
		}
	}

	public virtual void SetOpen(bool open)
	{
		if (open && (Object)(object)ScheduleOne.UI.Phone.Phone.ActiveApp != (Object)null)
		{
			Console.LogWarning(((Object)ScheduleOne.UI.Phone.Phone.ActiveApp).name + " is already open");
		}
		isOpen = open;
		PlayerSingleton<AppsCanvas>.Instance.SetIsOpen(open);
		PlayerSingleton<HomeScreen>.Instance.SetIsOpen(!open);
		if (isOpen)
		{
			if (Orientation == EOrientation.Horizontal)
			{
				PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetIsHorizontal(h: true);
				PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetLookOffsetMultiplier(0.6f);
			}
			else
			{
				PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetLookOffsetMultiplier(1f);
			}
			ScheduleOne.UI.Phone.Phone.ActiveApp = ((Component)this).gameObject;
			PlayerSingleton<HomeScreen>.Instance.LastSelectedSelectable = ((Component)appIconButton).GetComponent<UISelectable>();
			Singleton<UIScreenManager>.Instance.AddScreen(((Component)this).gameObject.GetComponent<UIScreen>());
		}
		else
		{
			if ((Object)(object)ScheduleOne.UI.Phone.Phone.ActiveApp == (Object)(object)((Component)this).gameObject)
			{
				ScheduleOne.UI.Phone.Phone.ActiveApp = null;
			}
			Singleton<UIScreenManager>.Instance.RemoveScreen(((Component)this).gameObject.GetComponent<UIScreen>());
			PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetIsHorizontal(h: false);
			PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance.SetLookOffsetMultiplier(1f);
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
		}
		((Component)appContainer).gameObject.SetActive(open);
	}
}
