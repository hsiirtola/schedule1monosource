using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone;

public class HomeScreen : PlayerSingleton<HomeScreen>
{
	[Header("References")]
	[SerializeField]
	protected Canvas canvas;

	[SerializeField]
	protected Text timeText;

	[SerializeField]
	protected RectTransform appIconContainer;

	[Header("Prefabs")]
	[SerializeField]
	protected GameObject appIconPrefab;

	[Header("Custom UI")]
	[SerializeField]
	protected UIScreen uiScreen;

	[SerializeField]
	protected UIPanel uiPanel;

	protected List<Button> appIcons = new List<Button>();

	private Coroutine delayedSetOpenRoutine;

	private UISelectable lastSelectedSelectable;

	public bool isOpen { get; protected set; } = true;

	public UISelectable LastSelectedSelectable
	{
		get
		{
			return lastSelectedSelectable;
		}
		set
		{
			lastSelectedSelectable = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		SetIsOpen(o: true);
	}

	public override void OnStartClient(bool IsOwner)
	{
		base.OnStartClient(IsOwner);
		if (IsOwner)
		{
			NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass += new Action(OnUncappedMinPass);
			Phone phone = PlayerSingleton<Phone>.Instance;
			phone.onPhoneOpened = (Action)Delegate.Combine(phone.onPhoneOpened, new Action(PhoneOpened));
			Phone phone2 = PlayerSingleton<Phone>.Instance;
			phone2.onPhoneClosed = (Action)Delegate.Combine(phone2.onPhoneClosed, new Action(PhoneClosed));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (NetworkSingleton<TimeManager>.InstanceExists)
		{
			NetworkSingleton<TimeManager>.Instance.onUncappedMinutePass -= new Action(OnUncappedMinPass);
		}
	}

	protected void PhoneOpened()
	{
		if (isOpen)
		{
			SetCanvasActive(a: true);
		}
	}

	protected void PhoneClosed()
	{
		delayedSetOpenRoutine = ((MonoBehaviour)this).StartCoroutine(DelayedSetCanvasActive(active: false, 0.25f));
	}

	private IEnumerator DelayedSetCanvasActive(bool active, float delay)
	{
		yield return (object)new WaitForSeconds(delay);
		delayedSetOpenRoutine = null;
		SetCanvasActive(active);
	}

	public void SetIsOpen(bool o)
	{
		isOpen = o;
		SetCanvasActive(isOpen && PlayerSingleton<Phone>.Instance.IsOpen);
	}

	public void SetCanvasActive(bool a)
	{
		if (delayedSetOpenRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(delayedSetOpenRoutine);
		}
		((Behaviour)canvas).enabled = a;
		if (a)
		{
			Singleton<UIScreenManager>.Instance.AddScreen(uiScreen);
			UIScreenManager.LastSelectedObject = null;
			((MonoBehaviour)this).StartCoroutine(SelectUIPanel());
		}
		else
		{
			Singleton<UIScreenManager>.Instance.RemoveScreen(uiScreen);
		}
	}

	private IEnumerator SelectUIPanel()
	{
		yield return null;
		uiScreen.SetCurrentSelectedPanel(uiPanel);
		if ((Object)(object)lastSelectedSelectable != (Object)null)
		{
			uiPanel.SelectSelectable(lastSelectedSelectable);
		}
		else
		{
			uiPanel.SelectSelectable(returnFirstFound: true);
		}
	}

	protected virtual void Update()
	{
		if (PlayerSingleton<Phone>.Instance.IsOpen && isOpen)
		{
			int num = -1;
			if (Input.GetKeyDown((KeyCode)49))
			{
				num = 0;
			}
			else if (Input.GetKeyDown((KeyCode)50))
			{
				num = 1;
			}
			else if (Input.GetKeyDown((KeyCode)51))
			{
				num = 2;
			}
			else if (Input.GetKeyDown((KeyCode)52))
			{
				num = 3;
			}
			else if (Input.GetKeyDown((KeyCode)53))
			{
				num = 4;
			}
			else if (Input.GetKeyDown((KeyCode)54))
			{
				num = 5;
			}
			else if (Input.GetKeyDown((KeyCode)55))
			{
				num = 6;
			}
			else if (Input.GetKeyDown((KeyCode)56))
			{
				num = 7;
			}
			else if (Input.GetKeyDown((KeyCode)57))
			{
				num = 8;
			}
			if (num != -1 && appIcons.Count > num)
			{
				((UnityEvent)appIcons[num].onClick).Invoke();
			}
		}
	}

	protected virtual void OnUncappedMinPass()
	{
		if (NetworkSingleton<GameManager>.Instance.IsTutorial)
		{
			int num = TimeManager.Get24HourTimeFromMinSum(Mathf.RoundToInt(Mathf.Round((float)NetworkSingleton<TimeManager>.Instance.DailyMinSum / 60f) * 60f));
			timeText.text = TimeManager.Get12HourTime(num) + " " + NetworkSingleton<TimeManager>.Instance.CurrentDay;
		}
		else
		{
			timeText.text = TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime) + " " + NetworkSingleton<TimeManager>.Instance.CurrentDay;
		}
	}

	public Button GenerateAppIcon<T>(App<T> prog) where T : PlayerSingleton<T>
	{
		RectTransform component = Object.Instantiate<GameObject>(appIconPrefab, (Transform)(object)appIconContainer).GetComponent<RectTransform>();
		((Component)((Transform)component).Find("Mask/Image")).GetComponent<Image>().sprite = prog.AppIcon;
		((Component)((Transform)component).Find("Label")).GetComponent<Text>().text = prog.IconLabel;
		appIcons.Add(((Component)component).GetComponent<Button>());
		uiPanel.AddSelectable(((Component)component).GetComponent<UISelectable>());
		return ((Component)component).GetComponent<Button>();
	}
}
