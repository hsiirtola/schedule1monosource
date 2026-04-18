using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class MouseTooltip : Singleton<MouseTooltip>
{
	[Header("References")]
	public RectTransform IconRect;

	public Image IconImg;

	public RectTransform TooltipRect;

	public TextMeshProUGUI TooltipLabel;

	[Header("Settings")]
	public Vector3 TooltipOffset_NoIcon;

	public Vector3 TooltipOffset_WithIcon;

	public Vector3 IconOffset;

	[Header("Colors")]
	public Color Color_Invalid;

	[Header("Sprites")]
	public Sprite Sprite_Cross;

	private bool tooltipShownThisFrame;

	private bool iconShownThisFrame;

	public void ShowTooltip(string text, Color col)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)TooltipLabel).text = text;
		((Graphic)TooltipLabel).color = col;
		tooltipShownThisFrame = true;
	}

	public void ShowIcon(Sprite sprite, Color col)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		IconImg.sprite = sprite;
		((Graphic)IconImg).color = col;
		iconShownThisFrame = true;
	}

	private void LateUpdate()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		((Component)TooltipLabel).gameObject.SetActive(tooltipShownThisFrame);
		((Component)IconRect).gameObject.SetActive(iconShownThisFrame);
		((Transform)IconRect).position = GameInput.MousePosition + IconOffset;
		if (iconShownThisFrame)
		{
			((Transform)TooltipRect).position = GameInput.MousePosition + TooltipOffset_WithIcon;
		}
		else
		{
			((Transform)TooltipRect).position = GameInput.MousePosition + TooltipOffset_NoIcon;
		}
		tooltipShownThisFrame = false;
		iconShownThisFrame = false;
	}
}
