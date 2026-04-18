using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Temperature;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class TemperatureDisplay : MonoBehaviour
{
	public const float MaxCameraDistance = 8f;

	public const float MinCameraDistance = 0.5f;

	public const float FadeInDistance = 2f;

	public const float FadeOutDistance = 0.25f;

	public bool UseColor;

	[SerializeField]
	private Gradient _temperatureColorGradient;

	[SerializeField]
	private TextMeshPro _label;

	private Func<float> _getCelsiusTemperature;

	private Func<bool> _getIsVisible;

	private void Awake()
	{
		((Behaviour)_label).enabled = false;
	}

	private void LateUpdate()
	{
		UpdateCanvas();
	}

	private void UpdateCanvas()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.Local == (Object)null)
		{
			return;
		}
		if (_getCelsiusTemperature == null)
		{
			((Behaviour)_label).enabled = false;
			return;
		}
		if (_getIsVisible != null && !_getIsVisible())
		{
			((Behaviour)_label).enabled = false;
			return;
		}
		float num = Vector3.Distance(_label.transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
		if (num > 8f)
		{
			((Behaviour)_label).enabled = false;
			return;
		}
		_label.transform.rotation = Quaternion.LookRotation(_label.transform.position - ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.up);
		float num2 = 1f - Mathf.Clamp01(Mathf.InverseLerp(6f, 8f, num));
		float num3 = Mathf.Clamp01(Mathf.InverseLerp(0.5f, 0.75f, num));
		float celsius = _getCelsiusTemperature();
		Color color = Color.white;
		if (UseColor)
		{
			color = _temperatureColorGradient.Evaluate(TemperatureUtility.NormalizeTemperature(celsius));
		}
		color.a = Mathf.Min(num2, num3);
		((Graphic)_label).color = color;
		((TMP_Text)_label).text = TemperatureUtility.FormatTemperatureWithAppropriateUnit(celsius);
		((Behaviour)_label).enabled = true;
	}

	public void SetTemperatureGetter(Func<float> getCelsiusTemperature)
	{
		_getCelsiusTemperature = getCelsiusTemperature;
	}

	public void SetVisibilityGetter(Func<bool> getIsVisible)
	{
		_getIsVisible = getIsVisible;
	}

	public void SetEnabled(bool enabled)
	{
		((Component)this).gameObject.SetActive(enabled);
	}
}
