using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ScheduleOne.UI.Input;

public class InputPrompt : MonoBehaviour
{
	public enum EInputPromptAlignment
	{
		Left,
		Middle,
		Right
	}

	public static float Spacing = 10f;

	[Header("Settings")]
	public List<InputActionReference> Actions = new List<InputActionReference>();

	public string Label;

	public EInputPromptAlignment Alignment;

	[Header("References")]
	public RectTransform Container;

	public RectTransform ImagesContainer;

	public TextMeshProUGUI LabelComponent;

	public RectTransform Shade;

	[Header("Settings")]
	public bool OverridePromptImageColor;

	public Color PromptImageColor = Color.white;

	[SerializeField]
	private List<PromptImage> promptImages = new List<PromptImage>();

	private List<InputActionReference> displayedActions = new List<InputActionReference>();

	private EInputPromptAlignment AppliedAlignment;

	private InputPromptsManager manager
	{
		get
		{
			if (!Singleton<InputPromptsManager>.InstanceExists)
			{
				return GameObject.Find("@InputPromptsManager").GetComponent<InputPromptsManager>();
			}
			return Singleton<InputPromptsManager>.Instance;
		}
	}

	private void OnEnable()
	{
		RefreshPromptImages();
		((Component)Container).gameObject.SetActive(true);
	}

	private void OnDisable()
	{
		((Component)Container).gameObject.SetActive(false);
	}

	private void RefreshPromptImages()
	{
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		AppliedAlignment = Alignment;
		displayedActions.Clear();
		displayedActions.AddRange(Actions);
		int childCount = ((Transform)ImagesContainer).childCount;
		Transform[] array = (Transform[])(object)new Transform[childCount];
		for (int i = 0; i < childCount; i++)
		{
			array[i] = ((Transform)ImagesContainer).GetChild(i);
		}
		for (int j = 0; j < childCount; j++)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)((Component)array[j]).gameObject);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)((Component)array[j]).gameObject);
			}
		}
		promptImages.Clear();
		float num = 0f;
		string text = default(string);
		string controlPath = default(string);
		for (int k = 0; k < Actions.Count; k++)
		{
			InputActionRebindingExtensions.GetBindingDisplayString(Actions[k].action, 0, ref text, ref controlPath, (DisplayStringOptions)0);
			PromptImage promptImage = manager.GetPromptImage(controlPath, ImagesContainer);
			if ((Object)(object)promptImage == (Object)null)
			{
				continue;
			}
			num += promptImage.Width;
			Image[] componentsInChildren = ((Component)((Component)promptImage).transform).GetComponentsInChildren<Image>();
			foreach (Image val in componentsInChildren)
			{
				if (OverridePromptImageColor)
				{
					((Graphic)val).color = PromptImageColor;
				}
			}
			promptImages.Add(promptImage);
		}
		num += Spacing * (float)Actions.Count;
		((TMP_Text)LabelComponent).text = Label;
		((TMP_Text)LabelComponent).ForceMeshUpdate(false, false);
		num += ((TMP_Text)LabelComponent).preferredWidth;
		float num2 = 0f;
		if (Alignment == EInputPromptAlignment.Left)
		{
			num2 = 0f - Spacing;
		}
		else if (Alignment == EInputPromptAlignment.Middle)
		{
			num2 = (0f - num) / 2f;
		}
		else if (Alignment == EInputPromptAlignment.Right)
		{
			num2 = Spacing;
		}
		float num3 = 1f;
		if (Alignment == EInputPromptAlignment.Left)
		{
			((TMP_Text)LabelComponent).alignment = (TextAlignmentOptions)8196;
			num3 = -1f;
		}
		else
		{
			((TMP_Text)LabelComponent).alignment = (TextAlignmentOptions)8193;
		}
		float num4 = 0f;
		for (int m = 0; m < promptImages.Count; m++)
		{
			((Component)promptImages[m]).GetComponent<RectTransform>().anchoredPosition = new Vector2(num2 + num4 * num3 + promptImages[m].Width * 0.5f * num3, 0f);
			num4 += promptImages[m].Width + Spacing;
		}
		((Component)LabelComponent).GetComponent<RectTransform>().anchoredPosition = new Vector2(num2 + num4 * num3 + ((Component)LabelComponent).GetComponent<RectTransform>().sizeDelta.x * 0.5f * num3, 0f);
		UpdateShade();
	}

	public void SetLabel(string label)
	{
		Label = label;
		((TMP_Text)LabelComponent).text = Label;
		UpdateShade();
	}

	private void UpdateShade()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		float num = ((TMP_Text)LabelComponent).preferredWidth + 90f;
		Shade.sizeDelta = new Vector2(num, Shade.sizeDelta.y);
		if (Alignment == EInputPromptAlignment.Left)
		{
			Shade.anchoredPosition = new Vector2((0f - num) / 2f, 0f);
		}
		else if (Alignment == EInputPromptAlignment.Middle)
		{
			Shade.anchoredPosition = new Vector2(0f, 0f);
		}
		else if (Alignment == EInputPromptAlignment.Right)
		{
			Shade.anchoredPosition = new Vector2(num / 2f, 0f);
		}
	}
}
