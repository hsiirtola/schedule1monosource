using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class ReticleLineUI : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private Image _line;

	[SerializeField]
	private Image _border;

	public void SetPosition(Vector2 position)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)_border).rectTransform.anchoredPosition = position;
	}

	public void SetSize(float sizeX, float sizeY, float thickness)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)_border).rectTransform.sizeDelta = new Vector2(sizeX, sizeY);
		((Graphic)_line).rectTransform.offsetMin = new Vector2(thickness, thickness);
		((Graphic)_line).rectTransform.offsetMax = new Vector2(0f - thickness, 0f - thickness);
	}

	public void SetColor(Color lineColor, Color borderColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)_line).color = lineColor;
		((Graphic)_border).color = borderColor;
	}
}
