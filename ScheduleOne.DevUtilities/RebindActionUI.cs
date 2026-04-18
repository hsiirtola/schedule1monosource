using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace ScheduleOne.DevUtilities;

public class RebindActionUI : MonoBehaviour
{
	[Serializable]
	public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
	{
	}

	[Serializable]
	public class InteractiveRebindEvent : UnityEvent<RebindActionUI, RebindingOperation>
	{
	}

	public Action onRebind;

	[Tooltip("Reference to action that is to be rebound from the UI.")]
	[SerializeField]
	private InputActionReference m_Action;

	[SerializeField]
	private string m_BindingId;

	[SerializeField]
	private DisplayStringOptions m_DisplayStringOptions;

	[Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the rebind UI not show a label for the action.")]
	[SerializeField]
	private TextMeshProUGUI m_ActionLabel;

	[Tooltip("Text label that will receive the current, formatted binding string.")]
	[SerializeField]
	private TextMeshProUGUI m_BindingText;

	[Tooltip("Optional UI that will be shown while a rebind is in progress.")]
	[SerializeField]
	private GameObject m_RebindOverlay;

	[Tooltip("Optional text label that will be updated with prompt for user input.")]
	[SerializeField]
	private TextMeshProUGUI m_RebindText;

	[Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying bindings in custom ways, e.g. using images instead of text.")]
	[SerializeField]
	private UpdateBindingUIEvent m_UpdateBindingUIEvent;

	[Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, to implement custom UI behavior while a rebind is in progress. It can also be used to further customize the rebind.")]
	[SerializeField]
	private InteractiveRebindEvent m_RebindStartEvent;

	[Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
	[SerializeField]
	private InteractiveRebindEvent m_RebindStopEvent;

	private RebindingOperation m_RebindOperation;

	private static List<RebindActionUI> s_RebindActionUIs;

	public InputActionReference actionReference
	{
		get
		{
			return m_Action;
		}
		set
		{
			m_Action = value;
			UpdateActionLabel();
			UpdateBindingDisplay();
		}
	}

	public string bindingId
	{
		get
		{
			return m_BindingId;
		}
		set
		{
			m_BindingId = value;
			UpdateBindingDisplay();
		}
	}

	public DisplayStringOptions displayStringOptions
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_DisplayStringOptions;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			m_DisplayStringOptions = value;
			UpdateBindingDisplay();
		}
	}

	public TextMeshProUGUI actionLabel
	{
		get
		{
			return m_ActionLabel;
		}
		set
		{
			m_ActionLabel = value;
			UpdateActionLabel();
		}
	}

	public TextMeshProUGUI bindingText
	{
		get
		{
			return m_BindingText;
		}
		set
		{
			m_BindingText = value;
			UpdateBindingDisplay();
		}
	}

	public TextMeshProUGUI rebindPrompt
	{
		get
		{
			return m_RebindText;
		}
		set
		{
			m_RebindText = value;
		}
	}

	public GameObject rebindOverlay
	{
		get
		{
			return m_RebindOverlay;
		}
		set
		{
			m_RebindOverlay = value;
		}
	}

	public UpdateBindingUIEvent updateBindingUIEvent
	{
		get
		{
			if (m_UpdateBindingUIEvent == null)
			{
				m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
			}
			return m_UpdateBindingUIEvent;
		}
	}

	public InteractiveRebindEvent startRebindEvent
	{
		get
		{
			if (m_RebindStartEvent == null)
			{
				m_RebindStartEvent = new InteractiveRebindEvent();
			}
			return m_RebindStartEvent;
		}
	}

	public InteractiveRebindEvent stopRebindEvent
	{
		get
		{
			if (m_RebindStopEvent == null)
			{
				m_RebindStopEvent = new InteractiveRebindEvent();
			}
			return m_RebindStopEvent;
		}
	}

	public RebindingOperation ongoingRebind => m_RebindOperation;

	public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		bindingIndex = -1;
		InputActionReference action2 = m_Action;
		action = ((action2 != null) ? action2.action : null);
		if (action == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(m_BindingId))
		{
			return false;
		}
		Guid bindingId = new Guid(m_BindingId);
		bindingIndex = action.bindings.IndexOf((Predicate<InputBinding>)((InputBinding x) => ((InputBinding)(ref x)).id == bindingId));
		if (bindingIndex == -1)
		{
			Debug.LogError((object)$"Cannot find binding with ID '{bindingId}' on '{action}'", (Object)(object)this);
			return false;
		}
		return true;
	}

	public bool IsRebinding()
	{
		if (m_RebindOperation != null && !m_RebindOperation.canceled)
		{
			return !m_RebindOperation.completed;
		}
		return false;
	}

	public void UpdateBindingDisplay()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		string text = string.Empty;
		string text2 = null;
		string text3 = null;
		InputActionReference action = m_Action;
		InputAction val = ((action != null) ? action.action : null);
		if (val != null)
		{
			int num = val.bindings.IndexOf((Predicate<InputBinding>)((InputBinding x) => ((InputBinding)(ref x)).id.ToString() == m_BindingId));
			if (num != -1)
			{
				text = InputActionRebindingExtensions.GetBindingDisplayString(val, num, ref text2, ref text3, displayStringOptions);
			}
		}
		((Component)m_BindingText).gameObject.SetActive(true);
		if ((Object)(object)m_BindingText != (Object)null)
		{
			((TMP_Text)m_BindingText).text = text;
		}
		((UnityEvent<RebindActionUI, string, string, string>)m_UpdateBindingUIEvent)?.Invoke(this, text, text2, text3);
	}

	public void ResetToDefault()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!ResolveActionAndBinding(out var action, out var bindingIndex))
		{
			return;
		}
		InputBinding val = action.bindings[bindingIndex];
		if (((InputBinding)(ref val)).isComposite)
		{
			for (int i = bindingIndex + 1; i < action.bindings.Count; i++)
			{
				val = action.bindings[i];
				if (((InputBinding)(ref val)).isPartOfComposite)
				{
					InputActionRebindingExtensions.RemoveBindingOverride(action, i);
					continue;
				}
				break;
			}
		}
		else
		{
			InputActionRebindingExtensions.RemoveBindingOverride(action, bindingIndex);
		}
		UpdateBindingDisplay();
	}

	public void StartInteractiveRebind()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		m_Action.action.Disable();
		if (!ResolveActionAndBinding(out var action, out var bindingIndex))
		{
			return;
		}
		InputBinding val = action.bindings[bindingIndex];
		if (((InputBinding)(ref val)).isComposite)
		{
			int num = bindingIndex + 1;
			if (num < action.bindings.Count)
			{
				val = action.bindings[num];
				if (((InputBinding)(ref val)).isPartOfComposite)
				{
					PerformInteractiveRebind(action, num, allCompositeParts: true);
				}
			}
		}
		else
		{
			PerformInteractiveRebind(action, bindingIndex);
		}
	}

	private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		RebindingOperation rebindOperation = m_RebindOperation;
		if (rebindOperation != null)
		{
			rebindOperation.Cancel();
		}
		m_RebindOperation = InputActionRebindingExtensions.PerformInteractiveRebinding(action, bindingIndex).OnCancel((Action<RebindingOperation>)delegate(RebindingOperation operation)
		{
			((UnityEvent<RebindActionUI, RebindingOperation>)m_RebindStopEvent)?.Invoke(this, operation);
			if ((Object)(object)m_RebindOverlay != (Object)null)
			{
				GameObject obj2 = m_RebindOverlay;
				if (obj2 != null)
				{
					obj2.SetActive(false);
				}
			}
			UpdateBindingDisplay();
			CleanUp();
		}).OnComplete((Action<RebindingOperation>)delegate(RebindingOperation operation)
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			GameObject obj2 = m_RebindOverlay;
			if (obj2 != null)
			{
				obj2.SetActive(false);
			}
			((UnityEvent<RebindActionUI, RebindingOperation>)m_RebindStopEvent)?.Invoke(this, operation);
			UpdateBindingDisplay();
			CleanUp();
			if (allCompositeParts)
			{
				int num = bindingIndex + 1;
				if (num < action.bindings.Count)
				{
					InputBinding val2 = action.bindings[num];
					if (((InputBinding)(ref val2)).isPartOfComposite)
					{
						PerformInteractiveRebind(action, num, allCompositeParts: true);
					}
				}
			}
			onRebind?.Invoke();
		})
			.WithControlsExcluding("Mouse")
			.WithControlsExcluding("Gamepad");
		InputBinding val = action.bindings[bindingIndex];
		if (((InputBinding)(ref val)).isPartOfComposite)
		{
			val = action.bindings[bindingIndex];
			_ = "Binding '" + ((InputBinding)(ref val)).name + "'. ";
		}
		if ((Object)(object)m_RebindOverlay != (Object)null)
		{
			GameObject obj = m_RebindOverlay;
			if (obj != null)
			{
				obj.SetActive(true);
			}
		}
		if ((Object)(object)m_RebindText != (Object)null)
		{
			((TMP_Text)m_RebindText).text = "Press key...";
		}
		if ((Object)(object)m_RebindOverlay == (Object)null && (Object)(object)m_RebindText == (Object)null && m_RebindStartEvent == null && (Object)(object)m_BindingText != (Object)null)
		{
			((TMP_Text)m_BindingText).text = "<Waiting...>";
		}
		((Component)m_BindingText).gameObject.SetActive(false);
		((UnityEvent<RebindActionUI, RebindingOperation>)m_RebindStartEvent)?.Invoke(this, m_RebindOperation);
		m_RebindOperation.Start();
		void CleanUp()
		{
			RebindingOperation rebindOperation2 = m_RebindOperation;
			if (rebindOperation2 != null)
			{
				rebindOperation2.Dispose();
			}
			m_RebindOperation = null;
			m_Action.action.Enable();
		}
	}

	protected void OnEnable()
	{
		if (s_RebindActionUIs == null)
		{
			s_RebindActionUIs = new List<RebindActionUI>();
		}
		s_RebindActionUIs.Add(this);
		if (s_RebindActionUIs.Count == 1)
		{
			InputSystem.onActionChange += OnActionChange;
		}
		GameObject obj = rebindOverlay;
		if (obj != null)
		{
			obj.SetActive(false);
		}
		TextMeshProUGUI obj2 = bindingText;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(true);
		}
	}

	protected void OnDisable()
	{
		RebindingOperation rebindOperation = m_RebindOperation;
		if (rebindOperation != null)
		{
			rebindOperation.Dispose();
		}
		m_RebindOperation = null;
		s_RebindActionUIs.Remove(this);
		if (s_RebindActionUIs.Count == 0)
		{
			s_RebindActionUIs = null;
			InputSystem.onActionChange -= OnActionChange;
		}
	}

	private static void OnActionChange(object obj, InputActionChange change)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)change != 8)
		{
			return;
		}
		InputAction val = (InputAction)((obj is InputAction) ? obj : null);
		InputActionMap val2 = (InputActionMap)(((val != null) ? val.actionMap : null) ?? ((obj is InputActionMap) ? obj : null));
		InputActionAsset val3 = (InputActionAsset)(((val2 != null) ? val2.asset : null) ?? ((obj is InputActionAsset) ? obj : null));
		for (int i = 0; i < s_RebindActionUIs.Count; i++)
		{
			RebindActionUI rebindActionUI = s_RebindActionUIs[i];
			if (rebindActionUI.IsRebinding())
			{
				continue;
			}
			InputActionReference obj2 = rebindActionUI.actionReference;
			InputAction val4 = ((obj2 != null) ? obj2.action : null);
			if (val4 == null)
			{
				continue;
			}
			if (val4 != val && val4.actionMap != val2)
			{
				InputActionMap actionMap = val4.actionMap;
				if (!((Object)(object)((actionMap != null) ? actionMap.asset : null) == (Object)(object)val3))
				{
					continue;
				}
			}
			rebindActionUI.UpdateBindingDisplay();
		}
	}

	private void UpdateActionLabel()
	{
		if ((Object)(object)m_ActionLabel != (Object)null)
		{
			InputActionReference action = m_Action;
			InputAction val = ((action != null) ? action.action : null);
			((TMP_Text)m_ActionLabel).text = ((val != null) ? val.name : string.Empty);
		}
	}
}
