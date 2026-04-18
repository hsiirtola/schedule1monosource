using System;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScaler : MonoBehaviour
{
	public static float CanvasScaleFactor = 1f;

	public static Action OnCanvasScaleFactorChanged;

	public float ScaleMultiplier = 1f;

	private Vector2 referenceResolution = new Vector2(1920f, 1080f);

	private CanvasScaler canvasScaler;

	public static float NormalizedCanvasScaleFactor => Mathf.InverseLerp(0.7f, 1.4f, CanvasScaleFactor);

	public void Awake()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		canvasScaler = ((Component)this).GetComponent<CanvasScaler>();
		referenceResolution = canvasScaler.referenceResolution;
		OnCanvasScaleFactorChanged = (Action)Delegate.Combine(OnCanvasScaleFactorChanged, new Action(RefreshScale));
		RefreshScale();
	}

	private void OnDestroy()
	{
		OnCanvasScaleFactorChanged = (Action)Delegate.Remove(OnCanvasScaleFactorChanged, new Action(RefreshScale));
	}

	private void RefreshScale()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		canvasScaler.referenceResolution = referenceResolution / CanvasScaleFactor / ScaleMultiplier;
	}

	public static void SetScaleFactor(float scaleFactor)
	{
		scaleFactor = Mathf.Clamp(scaleFactor, 0.7f, 1.4f);
		CanvasScaleFactor = scaleFactor;
		OnCanvasScaleFactorChanged?.Invoke();
	}
}
