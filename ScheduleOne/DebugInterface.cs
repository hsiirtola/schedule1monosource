using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.UI.Shop;
using TMPro;
using UnityEngine;

namespace ScheduleOne;

public class DebugInterface : MonoBehaviour
{
	public UIScreen MainScreen;

	public UIScreen SecondScreen;

	public UIPanel GridPanel;

	public Transform GridPanelContainer;

	public UISelectable ButtonPrefab;

	public UIPanel horizontalPanel;

	public TextMeshProUGUI DebugText;

	public UIHorizontalSelector testHorizontalSelector;

	public UIPopupSelector testPopupSelector;

	public Sprite testIcon;

	private void Start()
	{
		Singleton<UIScreenManager>.Instance.AddScreen(MainScreen);
		GameInput.OnInputDeviceChanged = (Action<GameInput.InputDeviceType>)Delegate.Combine(GameInput.OnInputDeviceChanged, new Action<GameInput.InputDeviceType>(HandleInputDeviceChanged));
		testHorizontalSelector.SetOptions(new List<UIOption.OptionInfo>
		{
			new UIOption.OptionInfo
			{
				OptionName = "Easy",
				OptionIndex = 0
			},
			new UIOption.OptionInfo
			{
				OptionName = "Medium",
				OptionIndex = 1
			},
			new UIOption.OptionInfo
			{
				OptionName = "Hard",
				OptionIndex = 2
			},
			new UIOption.OptionInfo
			{
				OptionName = "Extreme",
				OptionIndex = 3
			}
		}, 1);
		testPopupSelector.SetOptions(new UIPopupScreen_ContextMenu.ContextMenuOption[4]
		{
			new UIPopupScreen_ContextMenu.ContextMenuOption(0, "Option 1", delegate
			{
				Debug.Log((object)"Option 1 selected");
			}),
			new UIPopupScreen_ContextMenu.ContextMenuOption(1, "Option 2", delegate
			{
				Debug.Log((object)"Option 2 selected");
			}),
			new UIPopupScreen_ContextMenu.ContextMenuOption(2, "Option 3", delegate
			{
				Debug.Log((object)"Option 3 selected");
			}),
			new UIPopupScreen_ContextMenu.ContextMenuOption(3, "Option 4", delegate
			{
				Debug.Log((object)"Option 4 selected");
			})
		});
		SetupGridPanel();
		MainScreen.SetCurrentSelectedPanel(GridPanel);
		MainScreen.AddPanel(horizontalPanel);
	}

	private void Update()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (GameInput.GetButtonDown(GameInput.ButtonCode.Escape))
		{
			UIPopupScreen_ContextMenu.ContextMenuOption[] array = new UIPopupScreen_ContextMenu.ContextMenuOption[3]
			{
				new UIPopupScreen_ContextMenu.ContextMenuOption(0, "Open Confirm Menu", delegate
				{
					OpenConfirmationMenu();
				}),
				new UIPopupScreen_ContextMenu.ContextMenuOption(1, "Open Modify Menu", delegate
				{
					OpenModifyAmountMenu();
				}),
				new UIPopupScreen_ContextMenu.ContextMenuOption(2, "Option 3 (Close)", delegate
				{
					Debug.Log((object)"Option 3 selected");
				})
			};
			Vector2 val = (Vector2)(Object.op_Implicit((Object)(object)UIScreenManager.LastSelectedObject) ? Vector2.op_Implicit(UIScreenManager.LastSelectedObject.transform.position) : new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2)));
			Singleton<UIScreenManager>.Instance.OpenPopupScreen("ContextMenu", array, val, UIPopupScreen_ContextMenu.AnchorType.TopLeft);
		}
		else if (GameInput.GetButtonDown(GameInput.ButtonCode.TogglePhone))
		{
			if (SecondScreen.IsSelected)
			{
				Singleton<UIScreenManager>.Instance.RemoveScreen(SecondScreen);
				((Component)SecondScreen).gameObject.SetActive(false);
			}
			else if (!Singleton<UIScreenManager>.Instance.IsAnyPopupScreenActive())
			{
				Singleton<UIScreenManager>.Instance.AddScreen(SecondScreen);
				((Component)SecondScreen).gameObject.SetActive(true);
				SecondScreen.SetCurrentSelectedPanel();
			}
		}
	}

	private void OpenConfirmationMenu()
	{
		Debug.Log((object)(Time.frameCount + " : Open Confirmation Menu"));
		Singleton<UIScreenManager>.Instance.OpenPopupScreen("ConfirmationMenu", "Confirm Action", "Are you sure you want to proceed?", (Action)delegate
		{
			Debug.Log((object)(Time.frameCount + " : Confirmed!"));
		}, (Action)delegate
		{
			Debug.Log((object)"Cancelled!");
		});
	}

	private void OpenModifyAmountMenu()
	{
		ShopListing shopListing = new ShopListing();
		shopListing.Item = new StorableItemDefinition();
		((BaseItemDefinition)shopListing.Item).Icon = testIcon;
		((BaseItemDefinition)shopListing.Item).Name = "Test Item";
		shopListing.Item.BasePurchasePrice = 10f;
		Singleton<UIScreenManager>.Instance.OpenPopupScreen("ModifyAmountMenu", "Add to Cart", "Modify Amount", "", 1f, (Action<float>)delegate(float amount)
		{
			Debug.Log((object)("Amount selected: " + amount));
		}, (Action)delegate
		{
			Debug.Log((object)"Cancelled!");
		}, UIPopupScreen_ModifyAmountMenu.ModifyAmountMenuMode.Store, 1f, 1f, 5f, 20f, 3, shopListing);
	}

	private void SetupGridPanel()
	{
		if (!((Object)(object)GridPanel == (Object)null) && !((Object)(object)GridPanelContainer == (Object)null) && !((Object)(object)ButtonPrefab == (Object)null))
		{
			int num = 21;
			for (int i = 0; i < num; i++)
			{
				UISelectable uISelectable = Object.Instantiate<UISelectable>(ButtonPrefab, GridPanelContainer);
				((Object)uISelectable).name = "Button " + (i + 1);
				((Component)uISelectable).gameObject.SetActive(true);
				GridPanel.AddSelectable(uISelectable);
			}
			GridPanel.SelectSelectable(1);
		}
	}

	public void ApplyFilter(int[] filters)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		int count = 0;
		foreach (Transform item in GridPanelContainer)
		{
			((Component)item).gameObject.SetActive(Array.Exists(filters, (int element) => element == count));
			count++;
		}
		if (GameInput.GetCurrentInputDeviceIsGamepad())
		{
			GridPanel.SelectSelectable(returnFirstFound: false);
			GridPanel.ScrollToCurrentSelectedSelectable();
		}
	}

	private void HandleInputDeviceChanged(GameInput.InputDeviceType type)
	{
		if (type == GameInput.InputDeviceType.Gamepad)
		{
			((TMP_Text)DebugText).text = "Using Gamepad";
		}
		else
		{
			((TMP_Text)DebugText).text = "Using Keyboard/Mouse";
		}
	}
}
