using UnityEngine;

namespace ScheduleOne.UI;

public class SlidingRect : MonoBehaviour
{
	public RectTransform Rect;

	public Vector2 Start;

	public Vector2 End;

	public float Duration = 1f;

	public float SpeedMultiplier = 1f;

	private float _time;

	public void Update()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		_time += Time.deltaTime * SpeedMultiplier;
		if (_time > Duration)
		{
			_time -= Duration;
		}
		float num = _time / Duration;
		Rect.anchoredPosition = Vector2.Lerp(Start, End, num);
	}
}
