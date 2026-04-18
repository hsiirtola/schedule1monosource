using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Customization;

[Serializable]
[CreateAssetMenu(fileName = "BasicAvatarSettings", menuName = "ScriptableObjects/BasicAvatarSettings", order = 1)]
public class BasicAvatarSettings : ScriptableObject
{
	public const float GenderScaleMultiplier = 0.7f;

	public const string MaleUnderwearPath = "Avatar/Layers/Bottom/MaleUnderwear";

	public const string FemaleUnderwearPath = "Avatar/Layers/Bottom/FemaleUnderwear";

	public int Gender;

	public float Weight;

	public Color SkinColor;

	public string HairStyle;

	public Color HairColor;

	public string Mouth;

	public string FacialHair;

	public string FacialDetails;

	public float FacialDetailsIntensity;

	public Color EyeballColor;

	public float UpperEyeLidRestingPosition;

	public float LowerEyeLidRestingPosition;

	public float PupilDilation = 1f;

	public float EyebrowScale;

	public float EyebrowThickness;

	public float EyebrowRestingHeight;

	public float EyebrowRestingAngle;

	public string Top;

	public Color TopColor;

	public string Bottom;

	public Color BottomColor;

	public string Shoes;

	public Color ShoesColor;

	public string Headwear;

	public Color HeadwearColor;

	public string Eyewear;

	public Color EyewearColor;

	public List<string> Tattoos = new List<string>();

	public T SetValue<T>(string fieldName, T value)
	{
		((object)this).GetType().GetField(fieldName).SetValue(this, value);
		return value;
	}

	public T GetValue<T>(string fieldName)
	{
		FieldInfo field = ((object)this).GetType().GetField(fieldName);
		if (field == null)
		{
			return default(T);
		}
		return (T)field.GetValue(this);
	}

	public AvatarSettings GetAvatarSettings()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		AvatarSettings avatarSettings = ScriptableObject.CreateInstance<AvatarSettings>();
		avatarSettings.Gender = (float)Gender * 0.7f;
		avatarSettings.Weight = Weight;
		avatarSettings.Height = 1f;
		avatarSettings.SkinColor = SkinColor;
		avatarSettings.HairPath = HairStyle;
		avatarSettings.HairColor = HairColor;
		avatarSettings.FaceLayerSettings.Add(new AvatarSettings.LayerSetting
		{
			layerPath = Mouth,
			layerTint = Color.black
		});
		avatarSettings.FaceLayerSettings.Add(new AvatarSettings.LayerSetting
		{
			layerPath = FacialHair,
			layerTint = Color.white
		});
		avatarSettings.FaceLayerSettings.Add(new AvatarSettings.LayerSetting
		{
			layerPath = FacialDetails,
			layerTint = new Color(0f, 0f, 0f, FacialDetailsIntensity)
		});
		avatarSettings.FaceLayerSettings.Add(new AvatarSettings.LayerSetting
		{
			layerPath = "Avatar/Layers/Face/EyeShadow",
			layerTint = new Color(0f, 0f, 0f, 0.7f)
		});
		avatarSettings.EyeBallTint = EyeballColor;
		avatarSettings.LeftEyeLidColor = SkinColor;
		avatarSettings.RightEyeLidColor = SkinColor;
		avatarSettings.EyeballMaterialIdentifier = "Default";
		avatarSettings.PupilDilation = PupilDilation;
		avatarSettings.RightEyeRestingState = (avatarSettings.LeftEyeRestingState = new Eye.EyeLidConfiguration
		{
			topLidOpen = UpperEyeLidRestingPosition,
			bottomLidOpen = LowerEyeLidRestingPosition
		});
		avatarSettings.EyebrowScale = EyebrowScale;
		avatarSettings.EyebrowThickness = EyebrowThickness;
		avatarSettings.EyebrowRestingHeight = EyebrowRestingHeight;
		avatarSettings.EyebrowRestingAngle = EyebrowRestingAngle;
		avatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
		{
			layerPath = "Avatar/Layers/Top/Nipples",
			layerTint = GetNippleColor(avatarSettings.SkinColor)
		});
		string layerPath = (((float)Gender <= 0.5f) ? "Avatar/Layers/Bottom/MaleUnderwear" : "Avatar/Layers/Bottom/FemaleUnderwear");
		avatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
		{
			layerPath = layerPath,
			layerTint = Color.white
		});
		if (!string.IsNullOrEmpty(Top))
		{
			avatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
			{
				layerPath = Top,
				layerTint = TopColor
			});
		}
		if (!string.IsNullOrEmpty(Bottom))
		{
			avatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
			{
				layerPath = Bottom,
				layerTint = BottomColor
			});
		}
		if (Tattoos != null)
		{
			for (int i = 0; i < Tattoos.Count; i++)
			{
				if (Tattoos[i].Contains("/Face/"))
				{
					avatarSettings.FaceLayerSettings.Add(new AvatarSettings.LayerSetting
					{
						layerPath = Tattoos[i],
						layerTint = Color.white
					});
				}
				else
				{
					avatarSettings.BodyLayerSettings.Add(new AvatarSettings.LayerSetting
					{
						layerPath = Tattoos[i],
						layerTint = Color.white
					});
				}
			}
		}
		if (!string.IsNullOrEmpty(Shoes))
		{
			avatarSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting
			{
				path = Shoes,
				color = ShoesColor
			});
		}
		if (!string.IsNullOrEmpty(Headwear))
		{
			avatarSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting
			{
				path = Headwear,
				color = HeadwearColor
			});
		}
		if (!string.IsNullOrEmpty(Eyewear))
		{
			avatarSettings.AccessorySettings.Add(new AvatarSettings.AccessorySetting
			{
				path = Eyewear,
				color = EyewearColor
			});
		}
		return avatarSettings;
	}

	public static Color GetNippleColor(Color skinColor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return Color.Lerp(skinColor, new Color(0.5f, 0.5f, 0.5f, 1f), 0.2f);
	}

	public virtual string GetJson(bool prettyPrint = true)
	{
		return JsonUtility.ToJson((object)this, prettyPrint);
	}
}
