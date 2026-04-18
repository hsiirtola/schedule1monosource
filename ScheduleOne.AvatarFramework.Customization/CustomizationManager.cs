using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.AvatarFramework.Customization;

public class CustomizationManager : Singleton<CustomizationManager>
{
	public delegate void AvatarSettingsChanged(AvatarSettings settings);

	public const string AppearancesFolderPath = "Assets/Data/Appearances";

	public Avatar TemplateAvatar;

	public TMP_InputField SaveInputField;

	public TMP_InputField LoadInputField;

	public Toggle GenerateCombinedLayerToggle;

	public AvatarSettingsChanged OnAvatarSettingsChanged;

	public AvatarSettings DefaultSettings;

	private bool isEditingOriginal;

	private string loadedSettingsAssetPath = string.Empty;

	private AvatarSettings ActiveSettings;

	protected override void Start()
	{
		base.Start();
		LoadSettings(Object.Instantiate<AvatarSettings>(DefaultSettings));
	}

	public void CreateSettings(string assetName, string assetPath)
	{
	}

	public void CreateSettings()
	{
		if (SaveInputField.text == "")
		{
			Console.LogWarning("No name entered for settings file.");
			return;
		}
		string assetPath = "Assets/Data/Appearances/" + ((!string.IsNullOrEmpty(loadedSettingsAssetPath)) ? loadedSettingsAssetPath : SaveInputField.text) + ".asset";
		CreateSettings(SaveInputField.text, assetPath);
	}

	public void LoadSettings(AvatarSettings loadedSettings)
	{
		if ((Object)(object)loadedSettings == (Object)null)
		{
			Console.LogWarning("Settings are null!");
			return;
		}
		ActiveSettings = loadedSettings;
		Debug.Log((object)("Settings loaded: " + ((Object)ActiveSettings).name));
		TemplateAvatar.LoadAvatarSettings(ActiveSettings);
		if (OnAvatarSettingsChanged != null)
		{
			OnAvatarSettingsChanged(ActiveSettings);
		}
	}

	public void LoadSettings(string path, bool editOriginal = false)
	{
		isEditingOriginal = editOriginal;
		loadedSettingsAssetPath = path;
		AvatarSettings loadedSettings = null;
		LoadSettings(loadedSettings);
	}

	private void ApplyDefaultSettings(AvatarSettings settings)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		settings.SkinColor = Color32.op_Implicit(new Color32((byte)150, (byte)120, (byte)95, byte.MaxValue));
		settings.Height = 0.98f;
		settings.Gender = 0f;
		settings.Weight = 0.4f;
		settings.EyebrowScale = 1f;
		settings.EyebrowThickness = 1f;
		settings.EyebrowRestingHeight = 0f;
		settings.EyebrowRestingAngle = 0f;
		settings.LeftEyeLidColor = Color32.op_Implicit(new Color32((byte)150, (byte)120, (byte)95, byte.MaxValue));
		settings.RightEyeLidColor = Color32.op_Implicit(new Color32((byte)150, (byte)120, (byte)95, byte.MaxValue));
		settings.LeftEyeRestingState = new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0.5f,
			topLidOpen = 0.5f
		};
		settings.RightEyeRestingState = new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0.5f,
			topLidOpen = 0.5f
		};
		settings.EyeballMaterialIdentifier = "Default";
		settings.EyeBallTint = Color.white;
		settings.PupilDilation = 1f;
		settings.HairPath = string.Empty;
		settings.HairColor = Color.black;
	}

	public void LoadSettings()
	{
		isEditingOriginal = true;
		Debug.Log((object)("Loading!: " + LoadInputField.text));
		LoadSettings(LoadInputField.text, LoadInputField.text != "Default");
	}

	public void GenderChanged(float genderScale)
	{
		ActiveSettings.Gender = genderScale;
		TemplateAvatar.ApplyBodySettings(ActiveSettings);
	}

	public void WeightChanged(float weightScale)
	{
		ActiveSettings.Weight = weightScale;
		TemplateAvatar.ApplyBodySettings(ActiveSettings);
	}

	public void HeightChanged(float height)
	{
		ActiveSettings.Height = height;
		TemplateAvatar.ApplyBodySettings(ActiveSettings);
	}

	public void SkinColorChanged(Color col)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		ActiveSettings.SkinColor = col;
		TemplateAvatar.ApplyBodySettings(ActiveSettings);
		if (!Input.GetKey((KeyCode)306))
		{
			ActiveSettings.LeftEyeLidColor = col;
			ActiveSettings.RightEyeLidColor = col;
		}
		TemplateAvatar.ApplyEyeLidColorSettings(ActiveSettings);
	}

	public void HairChanged(Accessory newHair)
	{
		ActiveSettings.HairPath = (((Object)(object)newHair != (Object)null) ? newHair.AssetPath : string.Empty);
		TemplateAvatar.ApplyHairSettings(ActiveSettings);
	}

	public void HairColorChanged(Color newCol)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		ActiveSettings.HairColor = newCol;
		TemplateAvatar.ApplyHairColorSettings(ActiveSettings);
	}

	public void EyeBallTintChanged(Color col)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		ActiveSettings.EyeBallTint = col;
		TemplateAvatar.ApplyEyeBallSettings(ActiveSettings);
	}

	public void UpperEyeLidRestingPositionChanged(float newVal)
	{
		ActiveSettings.LeftEyeRestingState.topLidOpen = newVal;
		ActiveSettings.RightEyeRestingState.topLidOpen = newVal;
		TemplateAvatar.ApplyEyeLidSettings(ActiveSettings);
	}

	public void LowerEyeLidRestingPositionChanged(float newVal)
	{
		ActiveSettings.LeftEyeRestingState.bottomLidOpen = newVal;
		ActiveSettings.RightEyeRestingState.bottomLidOpen = newVal;
		TemplateAvatar.ApplyEyeLidSettings(ActiveSettings);
	}

	public void EyebrowScaleChanged(float newVal)
	{
		ActiveSettings.EyebrowScale = newVal;
		TemplateAvatar.ApplyEyebrowSettings(ActiveSettings);
	}

	public void EyebrowThicknessChanged(float newVal)
	{
		ActiveSettings.EyebrowThickness = newVal;
		TemplateAvatar.ApplyEyebrowSettings(ActiveSettings);
	}

	public void EyebrowRestingHeightChanged(float newVal)
	{
		ActiveSettings.EyebrowRestingHeight = newVal;
		TemplateAvatar.ApplyEyebrowSettings(ActiveSettings);
	}

	public void EyebrowRestingAngleChanged(float newVal)
	{
		ActiveSettings.EyebrowRestingAngle = newVal;
		TemplateAvatar.ApplyEyebrowSettings(ActiveSettings);
	}

	public void PupilDilationChanged(float dilation)
	{
		ActiveSettings.PupilDilation = dilation;
		TemplateAvatar.ApplyEyeBallSettings(ActiveSettings);
	}

	public void FaceLayerChanged(FaceLayer layer, int index)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		string layerPath = (((Object)(object)layer != (Object)null) ? layer.AssetPath : string.Empty);
		Color layerTint = ActiveSettings.FaceLayerSettings[index].layerTint;
		ActiveSettings.FaceLayerSettings[index] = new AvatarSettings.LayerSetting
		{
			layerPath = layerPath,
			layerTint = layerTint
		};
		TemplateAvatar.ApplyFaceLayerSettings(ActiveSettings);
	}

	public void FaceLayerColorChanged(Color col, int index)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		string layerPath = ActiveSettings.FaceLayerSettings[index].layerPath;
		ActiveSettings.FaceLayerSettings[index] = new AvatarSettings.LayerSetting
		{
			layerPath = layerPath,
			layerTint = col
		};
		TemplateAvatar.ApplyFaceLayerSettings(ActiveSettings);
	}

	public void BodyLayerChanged(AvatarLayer layer, int index)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		string layerPath = (((Object)(object)layer != (Object)null) ? layer.AssetPath : string.Empty);
		Color layerTint = ActiveSettings.BodyLayerSettings[index].layerTint;
		ActiveSettings.BodyLayerSettings[index] = new AvatarSettings.LayerSetting
		{
			layerPath = layerPath,
			layerTint = layerTint
		};
		TemplateAvatar.ApplyBodyLayerSettings(ActiveSettings);
	}

	public void BodyLayerColorChanged(Color col, int index)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		string layerPath = ActiveSettings.BodyLayerSettings[index].layerPath;
		ActiveSettings.BodyLayerSettings[index] = new AvatarSettings.LayerSetting
		{
			layerPath = layerPath,
			layerTint = col
		};
		TemplateAvatar.ApplyBodyLayerSettings(ActiveSettings);
	}

	public void AccessoryChanged(Accessory acc, int index)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)("Accessory changed: " + acc?.AssetPath));
		string path = (((Object)(object)acc != (Object)null) ? acc.AssetPath : string.Empty);
		while (ActiveSettings.AccessorySettings.Count <= index)
		{
			ActiveSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting());
		}
		Color color = ActiveSettings.AccessorySettings[index].color;
		ActiveSettings.AccessorySettings[index] = new AvatarSettings.AccessorySetting
		{
			path = path,
			color = color
		};
		TemplateAvatar.ApplyAccessorySettings(ActiveSettings);
	}

	public void AccessoryColorChanged(Color col, int index)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		string path = ActiveSettings.AccessorySettings[index].path;
		ActiveSettings.AccessorySettings[index] = new AvatarSettings.AccessorySetting
		{
			path = path,
			color = col
		};
		TemplateAvatar.ApplyAccessorySettings(ActiveSettings);
	}
}
