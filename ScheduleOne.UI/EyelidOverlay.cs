using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.UI;

public class EyelidOverlay : Singleton<EyelidOverlay>
{
	public const float MaxTiredOpenAmount = 0.625f;

	public bool AutoUpdate = true;

	[Header("Settings")]
	public float Open = 400f;

	public float Closed = 30f;

	[Header("References")]
	public RectTransform Upper;

	public RectTransform Lower;

	public Canvas Canvas;

	[Range(0f, 1f)]
	public float CurrentOpen = 1f;

	public FloatSmoother OpenMultiplier;

	protected override void Awake()
	{
		base.Awake();
		OpenMultiplier.Initialize();
		SetOpen(1f);
	}

	private void Update()
	{
		if ((Object)(object)Player.Local == (Object)null)
		{
			return;
		}
		if (AutoUpdate)
		{
			if (Player.Local.Energy.CurrentEnergy < 20f)
			{
				CurrentOpen = Mathf.Lerp(0.625f, 1f, Player.Local.Energy.CurrentEnergy / 20f);
			}
			else
			{
				CurrentOpen = 1f;
			}
		}
		SetOpen(CurrentOpen * OpenMultiplier.CurrentValue);
	}

	public void SetOpen(float openness)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		CurrentOpen = openness;
		Upper.anchoredPosition = new Vector2(0f, Mathf.Lerp(Closed, Open, openness));
		Lower.anchoredPosition = new Vector2(0f, 0f - Mathf.Lerp(Closed, Open, openness));
		((Behaviour)Canvas).enabled = openness < 1f;
	}
}
