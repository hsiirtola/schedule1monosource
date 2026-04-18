using UnityEngine;

namespace ScheduleOne.UI;

public class DestroyUIAtBounds : MonoBehaviour
{
	public RectTransform Rect;

	public Vector2 MinBounds = new Vector2(-1000f, -1000f);

	public Vector2 MaxBounds = new Vector2(1000f, 1000f);

	public void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (Rect.anchoredPosition.x < MinBounds.x || Rect.anchoredPosition.x > MaxBounds.x || Rect.anchoredPosition.y < MinBounds.y || Rect.anchoredPosition.y > MaxBounds.y)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}
}
