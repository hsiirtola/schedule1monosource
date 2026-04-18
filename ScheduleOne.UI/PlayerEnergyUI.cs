using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class PlayerEnergyUI : Singleton<PlayerEnergyUI>
{
	public Slider Slider;

	public RectTransform SliderRect;

	public Image FillImage;

	public TextMeshProUGUI Label;

	[Header("Settings")]
	public Color SliderColor_Green;

	public Color SliderColor_Red;

	private float displayedValue = 1f;

	protected override void Awake()
	{
		base.Awake();
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, (Action)delegate
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Expected O, but got Unknown
			UpdateDisplayedEnergy();
			Player.Local.Energy.onEnergyChanged.AddListener(new UnityAction(UpdateDisplayedEnergy));
		});
	}

	private void UpdateDisplayedEnergy()
	{
		SetDisplayedEnergy(Player.Local.Energy.CurrentEnergy);
	}

	public void SetDisplayedEnergy(float energy)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		displayedValue = energy;
		Slider.value = energy / 100f;
		((Graphic)FillImage).color = ((energy <= 20f) ? SliderColor_Red : SliderColor_Green);
	}

	protected virtual void Update()
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (displayedValue < 20f)
		{
			float num = Mathf.Clamp((20f - displayedValue) / 20f, 0.25f, 1f);
			float num2 = num * 3f;
			SliderRect.anchoredPosition = new Vector2(Random.Range(0f - num2, num2), Random.Range(0f - num2, num2));
			Color white = Color.white;
			Color val = Color.Lerp(Color.white, Color.red, num);
			white.a = ((Graphic)Label).color.a;
			val.a = ((Graphic)Label).color.a;
			((Graphic)Label).color = Color.Lerp(white, val, (Mathf.Sin(Time.timeSinceLevelLoad * num * 10f) + 1f) / 2f);
		}
		else
		{
			SliderRect.anchoredPosition = Vector2.zero;
			((Graphic)Label).color = new Color(1f, 1f, 1f, ((Graphic)Label).color.a);
		}
	}
}
