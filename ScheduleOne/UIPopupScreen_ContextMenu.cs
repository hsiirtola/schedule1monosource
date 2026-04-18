using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne;

public class UIPopupScreen_ContextMenu : UIPopupScreen
{
	public class ContextMenuOption
	{
		public int optionID;

		public string optionName;

		public Action optionAction;

		public ContextMenuOption(int id, string name, Action action)
		{
			optionID = id;
			optionName = name;
			optionAction = action;
		}
	}

	public enum AnchorType
	{
		TopLeft,
		BottomLeft,
		Center
	}

	[SerializeField]
	[Tooltip("Prefab for the Option Selectable")]
	private UISelectable selectablePrefab;

	[SerializeField]
	[Tooltip("Transform where the Option Selectables will be parented to")]
	private Transform contentParent;

	[SerializeField]
	[Tooltip("RectTransform where the anchoring point of the context menu will be")]
	private RectTransform anchorRectTransform;

	[SerializeField]
	[Tooltip("Canvas to control the visibility")]
	private Canvas canvas;

	[SerializeField]
	[Tooltip("Screen blocker to block mouse interaction with ui elements behind the context menu and darken the background")]
	private GameObject screenBlocker;

	private AnchorType anchor;

	private List<ContextMenuOption> options = new List<ContextMenuOption>();

	private Queue<UISelectable> selectablePool = new Queue<UISelectable>();

	private Dictionary<int, UISelectable> activeSelectables = new Dictionary<int, UISelectable>();

	public AnchorType Anchor
	{
		get
		{
			return anchor;
		}
		set
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			anchor = value;
			if (anchor == AnchorType.TopLeft)
			{
				anchorRectTransform.pivot = new Vector2(0f, 1f);
			}
			else if (anchor == AnchorType.BottomLeft)
			{
				anchorRectTransform.pivot = new Vector2(0f, 0f);
			}
			else if (anchor == AnchorType.Center)
			{
				anchorRectTransform.pivot = new Vector2(0.5f, 0.5f);
			}
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		((Behaviour)canvas).enabled = false;
		((Component)selectablePrefab).gameObject.SetActive(false);
	}

	protected override void OnStarted()
	{
		base.OnStarted();
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Combine(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(HandleInputDeviceChanged));
	}

	protected override void OnDestroyed()
	{
		base.OnDestroyed();
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Remove(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(HandleInputDeviceChanged));
	}

	private void HandleInputDeviceChanged(GameInput.InputDeviceType type)
	{
		if (type == GameInput.InputDeviceType.KeyboardMouse && ((Component)this).gameObject.activeInHierarchy)
		{
			Close();
		}
	}

	public void AddOption(int id, string name, Action action)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		ContextMenuOption item = new ContextMenuOption(id, name, action);
		options.Add(item);
		UISelectable_ContextMenu uISelectable_ContextMenu = GetSelectableFromPool() as UISelectable_ContextMenu;
		((Component)uISelectable_ContextMenu).gameObject.SetActive(true);
		uISelectable_ContextMenu.Setup(name);
		uISelectable_ContextMenu.OnTrigger.AddListener((UnityAction)delegate
		{
			action?.Invoke();
			Close();
		});
		activeSelectables[id] = uISelectable_ContextMenu;
	}

	public override void Close()
	{
		Clear();
		Singleton<UIScreenManager>.Instance.RemoveScreen(this);
		((Behaviour)canvas).enabled = false;
	}

	private void Open()
	{
		Singleton<UIScreenManager>.Instance.AddScreen(this, Close);
		((Behaviour)canvas).enabled = true;
	}

	public override void Open(params object[] args)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		ContextMenuOption[] obj = args[0] as ContextMenuOption[];
		Vector2 position = (Vector2)args[1];
		AnchorType anchorType = ((args.Length > 2) ? ((AnchorType)args[2]) : AnchorType.TopLeft);
		int selectedIndex = ((args.Length > 3) ? ((int)args[3]) : 0);
		bool active = args.Length > 4 && (bool)args[4];
		ContextMenuOption[] array = obj;
		foreach (ContextMenuOption contextMenuOption in array)
		{
			AddOption(contextMenuOption.optionID, contextMenuOption.optionName, contextMenuOption.optionAction);
		}
		screenBlocker.SetActive(active);
		Anchor = anchorType;
		Open();
		SetPosition(position);
		SelectPanel(selectedIndex);
	}

	private void Clear()
	{
		options.Clear();
		foreach (KeyValuePair<int, UISelectable> activeSelectable in activeSelectables)
		{
			selectablePool.Enqueue(activeSelectable.Value);
			((Component)activeSelectable.Value).gameObject.SetActive(false);
			((UnityEventBase)activeSelectable.Value.OnTrigger).RemoveAllListeners();
		}
		activeSelectables.Clear();
	}

	private void SelectPanel(int selectedIndex)
	{
		SetCurrentSelectedPanel(base.Panels[0]);
		base.Panels[0].SelectSelectable(selectedIndex);
	}

	private UISelectable GetSelectableFromPool()
	{
		UISelectable uISelectable;
		if (selectablePool.Count > 0)
		{
			uISelectable = selectablePool.Dequeue();
		}
		else
		{
			uISelectable = Object.Instantiate<UISelectable>(selectablePrefab, contentParent);
			base.Panels[0].AddSelectable(uISelectable);
		}
		((Component)uISelectable).transform.SetAsLastSibling();
		return uISelectable;
	}

	private void SetPosition(Vector2 pos)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		Canvas.ForceUpdateCanvases();
		Rect rect = anchorRectTransform.rect;
		Vector2 size = ((Rect)(ref rect)).size;
		Vector2 pivot = anchorRectTransform.pivot;
		float num = 0f;
		float num2 = (float)Screen.width - size.x;
		float num3;
		float num4;
		if (pivot.y == 0f)
		{
			num3 = 0f;
			num4 = (float)Screen.height - size.y;
		}
		else
		{
			num3 = size.y;
			num4 = Screen.height;
			pos.y = Mathf.Max(pos.y, num3);
		}
		pos.x = Mathf.Clamp(pos.x, num, num2);
		pos.y = Mathf.Clamp(pos.y, num3, num4);
		((Transform)anchorRectTransform).position = Vector2.op_Implicit(pos);
	}
}
