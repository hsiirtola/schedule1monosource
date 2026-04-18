using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Graffiti;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class GraffitiMenu : Singleton<GraffitiMenu>
{
	[Header("References")]
	public Canvas Canvas;

	public RectTransform ColorButtonContainer;

	public Button ClearButton;

	public Button DoneButton;

	public Transform ConfirmPanel;

	public Button ConfirmButton;

	public Button CancelButton;

	public Button UndoButton;

	public RectTransform RemainigPaintContainer;

	public Slider RemainingPaintSlider;

	public Image[] RemainingPaintImages;

	public TextMeshProUGUI RemainingPaintLabel;

	public Button[] WeightButtons;

	[Header("Prefabs")]
	public GameObject ColorButtonPrefab;

	public Action<ESprayColor> onColorSelected;

	public Action<byte> onWeightSelected;

	public Action onClearClicked;

	public Action onDone;

	public Action onUndoClicked;

	private List<Button> colorButtons = new List<Button>();

	private SpraySurface activeSurface;

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)ClearButton.onClick).AddListener(new UnityAction(ClearClicked));
		((UnityEvent)ConfirmButton.onClick).AddListener(new UnityAction(Done));
		((UnityEvent)CancelButton.onClick).AddListener(new UnityAction(CancelClicked));
		((UnityEvent)UndoButton.onClick).AddListener(new UnityAction(UndoClicked));
		((UnityEvent)DoneButton.onClick).AddListener(new UnityAction(Done));
		for (int i = 0; i < Enum.GetValues(typeof(ESprayColor)).Length; i++)
		{
			ESprayColor color = (ESprayColor)i;
			if (color != ESprayColor.None)
			{
				GameObject val = Object.Instantiate<GameObject>(ColorButtonPrefab, (Transform)(object)ColorButtonContainer);
				((Graphic)val.GetComponent<Image>()).color = color.GetColor();
				((UnityEvent)val.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
				{
					SelectColor(color);
				});
				colorButtons.Add(val.GetComponent<Button>());
			}
		}
		for (int num = 0; num < WeightButtons.Length; num++)
		{
			int index = num;
			((UnityEvent)WeightButtons[num].onClick).AddListener((UnityAction)delegate
			{
				WeightButtonClicked(index);
			});
		}
		((Transform)RemainigPaintContainer).SetAsLastSibling();
	}

	public void Open()
	{
		SelectColor(ESprayColor.Black);
		WeightButtonClicked(2);
		((Component)ConfirmPanel).gameObject.SetActive(false);
		((Behaviour)Canvas).enabled = true;
		UpdateUndoInteraction();
	}

	public void Close()
	{
		((Behaviour)Canvas).enabled = false;
	}

	private void Update()
	{
		if (((Behaviour)Canvas).enabled)
		{
			((Selectable)DoneButton).interactable = (Object)(object)activeSurface != (Object)null && activeSurface.DrawingStrokeCount > 0;
		}
	}

	public void ShowConfirmPanel()
	{
		((Component)ConfirmPanel).gameObject.SetActive(true);
	}

	private void SelectColor(ESprayColor color)
	{
		if (onColorSelected != null)
		{
			onColorSelected(color);
		}
		for (int i = 0; i < colorButtons.Count; i++)
		{
			if (i + 1 == (int)color)
			{
				((Selectable)colorButtons[i]).interactable = false;
				((Component)((Component)colorButtons[i]).transform.Find("Selected")).gameObject.SetActive(true);
			}
			else
			{
				((Selectable)colorButtons[i]).interactable = true;
				((Component)((Component)colorButtons[i]).transform.Find("Selected")).gameObject.SetActive(false);
			}
		}
	}

	private void WeightButtonClicked(int buttonIndex)
	{
		if (onWeightSelected != null)
		{
			onWeightSelected(SprayStroke.StrokeSizePresets[buttonIndex]);
		}
		for (int i = 0; i < WeightButtons.Length; i++)
		{
			((Selectable)WeightButtons[i]).interactable = i != buttonIndex;
		}
	}

	public void UpdateRemainingPaintIndicator(float remainingPaint)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		RemainingPaintSlider.value = remainingPaint;
		((TMP_Text)RemainingPaintLabel).text = Mathf.RoundToInt(remainingPaint * 100f) + "%";
		Color val = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)80, (byte)80, byte.MaxValue));
		((Graphic)RemainingPaintLabel).color = ((remainingPaint > 0.001f) ? Color.white : val);
		Image[] remainingPaintImages = RemainingPaintImages;
		for (int i = 0; i < remainingPaintImages.Length; i++)
		{
			((Graphic)remainingPaintImages[i]).color = ((remainingPaint > 0.001f) ? Color.white : val);
		}
	}

	private void ClearClicked()
	{
		if (onClearClicked != null)
		{
			onClearClicked();
		}
	}

	private void UndoClicked()
	{
		if (onUndoClicked != null)
		{
			onUndoClicked();
		}
	}

	private void Done()
	{
		if (onDone != null)
		{
			onDone();
		}
	}

	private void CancelClicked()
	{
		((Component)ConfirmPanel).gameObject.SetActive(false);
	}

	public void SetActiveSurface(SpraySurface surface)
	{
		activeSurface = surface;
		SpraySurface spraySurface = activeSurface;
		spraySurface.onDrawingChanged = (Action)Delegate.Combine(spraySurface.onDrawingChanged, new Action(UpdateUndoInteraction));
	}

	public void ClearActiveSurface()
	{
		SpraySurface spraySurface = activeSurface;
		spraySurface.onDrawingChanged = (Action)Delegate.Remove(spraySurface.onDrawingChanged, new Action(UpdateUndoInteraction));
		activeSurface = null;
	}

	private void UpdateUndoInteraction()
	{
		((Selectable)UndoButton).interactable = (Object)(object)activeSurface != (Object)null && activeSurface.CanUndo();
	}
}
