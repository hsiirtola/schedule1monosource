using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Phone.Map;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.Map;

public class POI : MonoBehaviour
{
	public enum TextShowMode
	{
		Off,
		Always,
		OnHover
	}

	public TextShowMode MainTextVisibility = TextShowMode.Always;

	public string DefaultMainText = "PoI Main Text";

	public bool AutoUpdatePosition = true;

	public bool Rotate;

	[SerializeField]
	protected GameObject UIPrefab;

	protected Text mainLabel;

	protected Button button;

	protected EventTrigger eventTrigger;

	private bool mainTextSet;

	public UnityEvent onUICreated;

	public bool UISetup { get; protected set; }

	public string MainText { get; protected set; } = string.Empty;

	public RectTransform UI { get; protected set; }

	public RectTransform IconContainer { get; protected set; }

	public FontSetter FontSetter { get; protected set; }

	private void OnEnable()
	{
		if ((Object)(object)UI == (Object)null)
		{
			if ((Object)(object)PlayerSingleton<MapApp>.Instance == (Object)null)
			{
				((MonoBehaviour)this).StartCoroutine(Wait());
			}
			else if ((Object)(object)UI == (Object)null)
			{
				UI = Object.Instantiate<GameObject>(UIPrefab, (Transform)(object)PlayerSingleton<MapApp>.Instance.PoIContainer).GetComponent<RectTransform>();
				FontSetter = ((Component)UI).GetComponent<FontSetter>();
				InitializeUI();
			}
		}
		IEnumerator Wait()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => (Object)(object)PlayerSingleton<MapApp>.Instance != (Object)null));
			if (((Behaviour)this).enabled && (Object)(object)UI == (Object)null)
			{
				UI = Object.Instantiate<GameObject>(UIPrefab, (Transform)(object)PlayerSingleton<MapApp>.Instance.PoIContainer).GetComponent<RectTransform>();
				FontSetter = ((Component)UI).GetComponent<FontSetter>();
				InitializeUI();
			}
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)UI != (Object)null)
		{
			PlayerSingleton<MapApp>.Instance.TeardownMapItem(((Component)UI).gameObject);
			Object.Destroy((Object)(object)((Component)UI).gameObject);
			UI = null;
		}
	}

	private void Update()
	{
		if (AutoUpdatePosition && PlayerSingleton<MapApp>.InstanceExists && PlayerSingleton<MapApp>.Instance.isOpen)
		{
			UpdatePosition();
		}
	}

	public void SetMainText(string text)
	{
		mainTextSet = true;
		MainText = text;
		if ((Object)(object)mainLabel != (Object)null)
		{
			mainLabel.text = text;
		}
	}

	public virtual void UpdatePosition()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)UI == (Object)null) && Singleton<MapPositionUtility>.InstanceExists)
		{
			UI.anchoredPosition = Singleton<MapPositionUtility>.Instance.GetMapPosition(((Component)this).transform.position);
			if (Rotate)
			{
				((Transform)IconContainer).localEulerAngles = new Vector3(0f, 0f, Vector3.SignedAngle(((Component)this).transform.forward, Vector3.forward, Vector3.up));
			}
		}
	}

	public virtual void InitializeUI()
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Expected O, but got Unknown
		PlayerSingleton<MapApp>.Instance.SetupMapItem(((Component)UI).gameObject);
		mainLabel = ((Component)((Transform)UI).Find("MainLabel")).GetComponent<Text>();
		if ((Object)(object)mainLabel == (Object)null)
		{
			Console.LogError("Failed to find main label");
		}
		if (MainTextVisibility == TextShowMode.Off || MainTextVisibility == TextShowMode.OnHover)
		{
			((Behaviour)mainLabel).enabled = false;
		}
		else
		{
			((Behaviour)mainLabel).enabled = true;
		}
		eventTrigger = ((Component)UI).GetComponent<EventTrigger>();
		Entry val = new Entry();
		val.eventID = (EventTriggerType)0;
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverStart();
		});
		eventTrigger.triggers.Add(val);
		val = new Entry();
		val.eventID = (EventTriggerType)1;
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverEnd();
		});
		eventTrigger.triggers.Add(val);
		button = ((Component)UI).GetComponent<Button>();
		((UnityEvent)button.onClick).AddListener((UnityAction)delegate
		{
			Clicked();
		});
		IconContainer = ((Component)((Transform)UI).Find("IconContainer")).GetComponent<RectTransform>();
		if ((Object)(object)IconContainer == (Object)null)
		{
			Console.LogError("Failed to find icon container");
		}
		if (!mainTextSet)
		{
			SetMainText(DefaultMainText);
		}
		else
		{
			SetMainText(MainText);
		}
		if (onUICreated != null)
		{
			onUICreated.Invoke();
		}
		UISetup = true;
		UpdatePosition();
	}

	protected virtual void HoverStart()
	{
		if (MainTextVisibility == TextShowMode.OnHover)
		{
			((Behaviour)mainLabel).enabled = true;
		}
	}

	protected virtual void HoverEnd()
	{
		if (MainTextVisibility == TextShowMode.OnHover)
		{
			((Behaviour)mainLabel).enabled = false;
		}
	}

	protected virtual void Clicked()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (GameInput.GetCurrentInputDeviceIsKeyboardMouse())
		{
			PlayerSingleton<MapApp>.Instance.FocusPosition(UI.anchoredPosition);
		}
	}
}
