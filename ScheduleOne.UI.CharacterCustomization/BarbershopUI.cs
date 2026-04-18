using HSVPicker;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCustomization;

public class BarbershopUI : CharacterCustomizationUI
{
	public ColorPicker ColorPicker;

	public Button ApplyColorButton;

	private Color appliedColor = Color.black;

	public override bool IsOptionCurrentlyApplied(CharacterCustomizationOption option)
	{
		return currentSettings.HairStyle == option.Label;
	}

	public override void OptionSelected(CharacterCustomizationOption option)
	{
		base.OptionSelected(option);
		currentSettings.HairStyle = option.Label;
		CharacterCustomizationShop.AvatarRig.ApplyHairSettings(currentSettings.GetAvatarSettings());
	}

	protected override void Update()
	{
		base.Update();
		if (base.IsOpen)
		{
			_ = (Object)(object)currentSettings == (Object)null;
		}
	}

	public override void Open()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.Open();
		ColorPicker.CurrentColor = currentSettings.HairColor;
		appliedColor = currentSettings.HairColor;
		((Selectable)ApplyColorButton).interactable = false;
	}

	public void ColorFieldChanged(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		currentSettings.HairColor = color;
		CharacterCustomizationShop.AvatarRig.ApplyHairColorSettings(currentSettings.GetAvatarSettings());
		((Selectable)ApplyColorButton).interactable = true;
	}

	public void ApplyColorChange()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		appliedColor = ColorPicker.CurrentColor;
		currentSettings.HairColor = appliedColor;
		CharacterCustomizationShop.AvatarRig.ApplyHairSettings(currentSettings.GetAvatarSettings());
		((Selectable)ApplyColorButton).interactable = false;
	}

	public void RevertColorChange()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		ColorPicker.CurrentColor = currentSettings.HairColor;
		currentSettings.HairColor = appliedColor;
		CharacterCustomizationShop.AvatarRig.ApplyHairSettings(currentSettings.GetAvatarSettings());
		((Selectable)ApplyColorButton).interactable = false;
	}
}
