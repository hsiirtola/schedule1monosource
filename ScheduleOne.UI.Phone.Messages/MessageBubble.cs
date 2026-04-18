using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages;

public class MessageBubble : MonoBehaviour
{
	public enum Alignment
	{
		Center,
		Left,
		Right
	}

	[Header("Settings")]
	public string text = string.Empty;

	public Alignment alignment = Alignment.Left;

	public bool showTriangle;

	public float bubble_MinWidth = 75f;

	public float bubble_MaxWidth = 500f;

	public bool alignTextCenter;

	public bool autosetPosition = true;

	private string displayedText = string.Empty;

	private bool triangleShown;

	[Header("References")]
	public RectTransform container;

	[SerializeField]
	protected Image bubble;

	[SerializeField]
	protected Text content;

	[SerializeField]
	protected Image triangle_Left;

	[SerializeField]
	protected Image triangle_Right;

	public Button button;

	public float height;

	public float spacingAbove;

	public static Color32 backgroundColor_Left = new Color32((byte)225, (byte)225, (byte)225, byte.MaxValue);

	public static Color32 textColor_Left = new Color32((byte)50, (byte)50, (byte)50, byte.MaxValue);

	public static Color32 backgroundColor_Right = new Color32((byte)75, (byte)175, (byte)225, byte.MaxValue);

	public static Color32 textColor_Right = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static float baseBubbleSpacing = 5f;

	public void SetupBubble(string _text, Alignment _alignment, bool alignCenter = false)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		alignment = _alignment;
		text = _text;
		alignTextCenter = alignCenter;
		ColorBlock colors = ((Selectable)button).colors;
		if (alignment == Alignment.Left)
		{
			container.anchorMin = new Vector2(0f, 1f);
			container.anchorMax = new Vector2(0f, 1f);
			((ColorBlock)(ref colors)).normalColor = Color32.op_Implicit(backgroundColor_Left);
			((ColorBlock)(ref colors)).disabledColor = Color32.op_Implicit(backgroundColor_Left);
			((Graphic)content).color = Color32.op_Implicit(textColor_Left);
		}
		else if (alignment == Alignment.Right)
		{
			container.anchorMin = new Vector2(1f, 1f);
			container.anchorMax = new Vector2(1f, 1f);
			((ColorBlock)(ref colors)).normalColor = Color32.op_Implicit(backgroundColor_Right);
			((ColorBlock)(ref colors)).disabledColor = Color32.op_Implicit(backgroundColor_Right);
			((Graphic)content).color = Color32.op_Implicit(textColor_Right);
		}
		else
		{
			container.anchorMin = new Vector2(0.5f, 1f);
			container.anchorMax = new Vector2(0.5f, 1f);
			((ColorBlock)(ref colors)).normalColor = Color32.op_Implicit(backgroundColor_Right);
			((ColorBlock)(ref colors)).disabledColor = Color32.op_Implicit(backgroundColor_Right);
			((Graphic)content).color = Color32.op_Implicit(textColor_Right);
		}
		((Selectable)button).colors = colors;
		RefreshDisplayedText();
		RefreshTriangle();
	}

	protected virtual void Update()
	{
		if (text != displayedText)
		{
			RefreshDisplayedText();
		}
		if (showTriangle != triangleShown)
		{
			RefreshTriangle();
		}
	}

	public virtual void RefreshDisplayedText()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		displayedText = text;
		content.text = displayedText;
		if (alignTextCenter)
		{
			content.alignment = (TextAnchor)1;
		}
		else
		{
			content.alignment = (TextAnchor)0;
		}
		RectTransform component = ((Component)this).GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(Mathf.Clamp(content.preferredWidth + 50f, bubble_MinWidth, bubble_MaxWidth), 75f);
		height = Mathf.Clamp(content.preferredHeight + 25f, 75f, float.MaxValue);
		component.sizeDelta = new Vector2(component.sizeDelta.x, height);
		float num = 1f;
		if (alignment == Alignment.Right)
		{
			num = -1f;
		}
		else if (alignment == Alignment.Center)
		{
			num = 0f;
		}
		if (autosetPosition)
		{
			component.anchoredPosition = new Vector2((component.sizeDelta.x / 2f + 25f) * num, (0f - height) / 2f);
		}
	}

	protected virtual void RefreshTriangle()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		triangleShown = showTriangle;
		((Component)triangle_Left).gameObject.SetActive(false);
		((Component)triangle_Right).gameObject.SetActive(false);
		if (showTriangle)
		{
			Image obj = triangle_Left;
			ColorBlock colors = ((Selectable)button).colors;
			((Graphic)obj).color = ((ColorBlock)(ref colors)).normalColor;
			Image obj2 = triangle_Right;
			colors = ((Selectable)button).colors;
			((Graphic)obj2).color = ((ColorBlock)(ref colors)).normalColor;
			if (alignment == Alignment.Left)
			{
				((Component)triangle_Left).gameObject.SetActive(true);
			}
			else
			{
				((Component)triangle_Right).gameObject.SetActive(true);
			}
		}
	}
}
