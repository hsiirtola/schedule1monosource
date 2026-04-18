using UnityEngine;

namespace ScheduleOne.UI;

public class UIMover : MonoBehaviour
{
	public RectTransform Rect;

	public Vector2 MinSpeed = Vector2.one;

	public Vector2 MaxSpeed = Vector2.one;

	public float SpeedMultiplier = 1f;

	private Vector2 speed = Vector2.zero;

	private void Start()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		speed = new Vector2(Random.Range(MinSpeed.x, MaxSpeed.x), Random.Range(MinSpeed.y, MaxSpeed.y));
	}

	public void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = speed * SpeedMultiplier * Time.deltaTime;
		RectTransform rect = Rect;
		rect.anchoredPosition += val;
	}
}
