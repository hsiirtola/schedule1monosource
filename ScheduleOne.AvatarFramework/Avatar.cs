using System;
using System.Collections.Generic;
using ScheduleOne.AvatarFramework.Animation;
using ScheduleOne.AvatarFramework.Emotions;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.AvatarFramework.Impostors;
using ScheduleOne.Core;
using ScheduleOne.Core.Equipping.Framework;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework;

public class Avatar : MonoBehaviour, IThirdPersonReferencesProvider
{
	public const int MAX_ACCESSORIES = 9;

	public const bool CombinedLayersEnabled = true;

	public const float DEFAULT_SMOOTHNESS = 0.25f;

	private static float maleShoulderScale = 0.93f;

	private static float femaleShoulderScale = 0.875f;

	[Header("References")]
	public AvatarAnimation Animation;

	public AvatarLookController LookController;

	public SkinnedMeshRenderer[] BodyMeshes;

	public SkinnedMeshRenderer[] ShapeKeyMeshes;

	public SkinnedMeshRenderer FaceMesh;

	public EyeController Eyes;

	public EyebrowController EyeBrows;

	public Transform BodyContainer;

	public Transform Armature;

	public Transform LeftShoulder;

	public Transform RightShoulder;

	public Transform HeadBone;

	public Transform HipBone;

	public Transform LeftFootBone;

	public Transform RightFootBone;

	public Rigidbody[] RagdollRBs;

	public Collider[] RagdollColliders;

	public Rigidbody MiddleSpineRB;

	public Rigidbody[] ImpactForceRBs;

	public AvatarEmotionManager EmotionManager;

	public AvatarEffects Effects;

	public Transform MiddleSpine;

	public Transform LowerSpine;

	public Transform LowestSpine;

	public AvatarImpostor Impostor;

	public ParticleSystem BloodParticles;

	[Header("Settings")]
	public AvatarSettings InitialAvatarSettings;

	public Material DefaultAvatarMaterial;

	public bool UseCombinedLayer = true;

	public UnityEvent<bool, bool, bool> onRagdollChange;

	[Header("Data - readonly")]
	[SerializeField]
	protected float appliedGender;

	[SerializeField]
	protected float appliedWeight;

	[SerializeField]
	protected Hair appliedHair;

	[SerializeField]
	protected Color appliedHairColor;

	[SerializeField]
	protected Accessory[] appliedAccessories = new Accessory[9];

	[SerializeField]
	protected bool wearingHairBlockingAccessory;

	private float additionalWeight;

	private float additionalGender;

	[Header("Runtime loading")]
	public AvatarSettings SettingsToLoad;

	public UnityEvent onSettingsLoaded;

	private Vector3 originalHipPos = Vector3.zero;

	private bool usingCombinedLayer;

	private bool blockEyeFaceLayers;

	public Transform RightHandContainer => Animation.RightHandContainer;

	public Transform LeftHandContainer => Animation.LeftHandContainer;

	public Transform RightHandAlignmentPoint => Animation.RightHandAlignmentPoint;

	public Transform LeftHandAlignmentPoint => Animation.LeftHandAlignmentPoint;

	public bool Ragdolled { get; protected set; }

	public AvatarEquippable CurrentEquippable { get; protected set; }

	public AvatarSettings CurrentSettings { get; protected set; }

	public Transform CenterPointTransform => MiddleSpine;

	public Vector3 CenterPoint => ((Component)CenterPointTransform).transform.position;

	[Button]
	public void Load()
	{
		LoadAvatarSettings(SettingsToLoad);
	}

	[Button]
	public void LoadNaked()
	{
		LoadNakedSettings(SettingsToLoad);
	}

	protected virtual void Awake()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		SetRagdollPhysicsEnabled(ragdollEnabled: false, playStandUpAnim: false);
		originalHipPos = HipBone.localPosition;
		if ((Object)(object)InitialAvatarSettings != (Object)null)
		{
			LoadAvatarSettings(InitialAvatarSettings);
		}
	}

	protected virtual void Update()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!Ragdolled && (Object)(object)Animation != (Object)null && !Animation.StandUpAnimationPlaying)
		{
			HipBone.localPosition = originalHipPos;
		}
	}

	public void SetVisible(bool vis)
	{
		Eyes.SetEyesOpen(open: true);
		((Component)BodyContainer).gameObject.SetActive(vis);
	}

	public void GetMugshot(Action<Texture2D> callback)
	{
		Singleton<MugshotGenerator>.Instance.GenerateMugshot(CurrentSettings, fileToFile: false, callback);
	}

	public void SetEmission(Color color)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (usingCombinedLayer)
		{
			((Renderer)BodyMeshes[0]).sharedMaterial.SetColor("_EmissionColor", color);
			return;
		}
		SkinnedMeshRenderer[] bodyMeshes = BodyMeshes;
		for (int i = 0; i < bodyMeshes.Length; i++)
		{
			((Renderer)bodyMeshes[i]).material.SetColor("_EmissionColor", color);
		}
	}

	public bool IsMale()
	{
		if ((Object)(object)CurrentSettings == (Object)null)
		{
			return true;
		}
		return CurrentSettings.Gender < 0.5f;
	}

	public bool IsWhite()
	{
		if ((Object)(object)CurrentSettings == (Object)null)
		{
			return true;
		}
		return CurrentSettings.SkinColor.r + CurrentSettings.SkinColor.g + CurrentSettings.SkinColor.b > 1.5f;
	}

	public string GetFormalAddress(bool capitalized = true)
	{
		if (IsMale())
		{
			if (!capitalized)
			{
				return "sir";
			}
			return "Sir";
		}
		if (!capitalized)
		{
			return "ma'am";
		}
		return "Ma'am";
	}

	public string GetThirdPersonAddress(bool capitalized = true)
	{
		if (IsMale())
		{
			if (!capitalized)
			{
				return "he";
			}
			return "He";
		}
		if (!capitalized)
		{
			return "she";
		}
		return "She";
	}

	public string GetThirdPersonPronoun(bool capitalized = true)
	{
		if (IsMale())
		{
			if (!capitalized)
			{
				return "him";
			}
			return "Him";
		}
		if (!capitalized)
		{
			return "her";
		}
		return "Her";
	}

	public void SetAnimationBool(string name, bool value)
	{
		Animation.SetBool(name, value);
	}

	public void SetAnimationTrigger(string name)
	{
		Animation.SetTrigger(name);
	}

	private void ApplyShapeKeys(float gender, float weight, bool bodyOnly = false)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		bool enabled = true;
		if ((Object)(object)Animation.animator != (Object)null)
		{
			enabled = ((Behaviour)Animation.animator).enabled;
			((Behaviour)Animation.animator).enabled = false;
		}
		for (int i = 0; i < ShapeKeyMeshes.Length; i++)
		{
			if (ShapeKeyMeshes[i].sharedMesh.blendShapeCount >= 2)
			{
				ShapeKeyMeshes[i].SetBlendShapeWeight(0, gender);
				ShapeKeyMeshes[i].SetBlendShapeWeight(1, weight);
			}
		}
		float num = Mathf.Lerp(maleShoulderScale, femaleShoulderScale, gender / 100f);
		LeftShoulder.localScale = new Vector3(num, num, num);
		RightShoulder.localScale = new Vector3(num, num, num);
		if ((Object)(object)Animation.animator != (Object)null)
		{
			((Behaviour)Animation.animator).enabled = enabled;
		}
		if (bodyOnly)
		{
			return;
		}
		for (int j = 0; j < appliedAccessories.Length; j++)
		{
			if ((Object)(object)appliedAccessories[j] != (Object)null)
			{
				appliedAccessories[j].ApplyShapeKeys(gender, weight);
			}
		}
	}

	private void SetFeetShrunk(bool shrink, float reduction)
	{
		if (shrink)
		{
			for (int i = 0; i < BodyMeshes.Length; i++)
			{
				BodyMeshes[i].SetBlendShapeWeight(2, reduction * 100f);
			}
		}
		else
		{
			for (int j = 0; j < BodyMeshes.Length; j++)
			{
				BodyMeshes[j].SetBlendShapeWeight(2, 0f);
			}
		}
	}

	private void SetWearingHairBlockingAccessory(bool blocked)
	{
		wearingHairBlockingAccessory = blocked;
		if ((Object)(object)appliedHair != (Object)null)
		{
			appliedHair.SetBlockedByHat(blocked);
		}
	}

	public void LoadAvatarSettings(AvatarSettings settings)
	{
		if ((Object)(object)settings == (Object)null)
		{
			Console.LogWarning("LoadAvatarSettings: given settings are null");
			return;
		}
		CurrentSettings = settings;
		ApplyBodySettings(CurrentSettings);
		ApplyHairSettings(CurrentSettings);
		ApplyHairColorSettings(CurrentSettings);
		ApplyEyeLidSettings(CurrentSettings);
		ApplyEyeLidColorSettings(CurrentSettings);
		ApplyEyebrowSettings(CurrentSettings);
		ApplyEyeBallSettings(CurrentSettings);
		ApplyFaceLayerSettings(CurrentSettings);
		ApplyBodyLayerSettings(CurrentSettings);
		ApplyAccessorySettings(CurrentSettings);
		FaceLayer faceLayer = Resources.Load(CurrentSettings.FaceLayer1Path) as FaceLayer;
		Texture2D faceTex = (((Object)(object)faceLayer != (Object)null) ? faceLayer.Texture : null);
		EmotionManager.ConfigureNeutralFace(faceTex, CurrentSettings.EyebrowRestingHeight, CurrentSettings.EyebrowRestingAngle, CurrentSettings.LeftEyeRestingState, CurrentSettings.RightEyeRestingState);
		Impostor.SetAvatarSettings(CurrentSettings);
		if (onSettingsLoaded != null)
		{
			onSettingsLoaded.Invoke();
		}
	}

	public void LoadNakedSettings(AvatarSettings settings, int maxLayerOrder = 19)
	{
		if ((Object)(object)settings == (Object)null)
		{
			Console.LogWarning("LoadAvatarSettings: given settings are null");
			return;
		}
		AvatarSettings currentSettings = CurrentSettings;
		CurrentSettings = settings;
		if ((Object)(object)CurrentSettings == (Object)null)
		{
			CurrentSettings = new AvatarSettings();
		}
		CurrentSettings = Object.Instantiate<AvatarSettings>(CurrentSettings);
		if ((Object)(object)currentSettings != (Object)null)
		{
			CurrentSettings.BodyLayerSettings.AddRange(currentSettings.BodyLayerSettings);
		}
		ApplyBodySettings(CurrentSettings);
		ApplyHairSettings(CurrentSettings);
		ApplyHairColorSettings(CurrentSettings);
		ApplyEyeLidSettings(CurrentSettings);
		ApplyEyeLidColorSettings(CurrentSettings);
		ApplyEyebrowSettings(CurrentSettings);
		ApplyEyeBallSettings(CurrentSettings);
		ApplyFaceLayerSettings(CurrentSettings);
		ApplyBodyLayerSettings(CurrentSettings, maxLayerOrder);
		FaceLayer faceLayer = Resources.Load(CurrentSettings.FaceLayer1Path) as FaceLayer;
		Texture2D faceTex = (((Object)(object)faceLayer != (Object)null) ? faceLayer.Texture : null);
		EmotionManager.ConfigureNeutralFace(faceTex, CurrentSettings.EyebrowRestingHeight, CurrentSettings.EyebrowRestingAngle, CurrentSettings.LeftEyeRestingState, CurrentSettings.RightEyeRestingState);
		if (onSettingsLoaded != null)
		{
			onSettingsLoaded.Invoke();
		}
	}

	public void ApplyBodySettings(AvatarSettings settings)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		appliedGender = settings.Gender;
		appliedWeight = settings.Weight;
		CurrentSettings.SkinColor = settings.SkinColor;
		ApplyShapeKeys(settings.Gender * 100f, settings.Weight * 100f);
		((Component)this).transform.localScale = new Vector3(settings.Height, settings.Height, settings.Height);
		if (onSettingsLoaded != null)
		{
			onSettingsLoaded.Invoke();
		}
	}

	public void SetAdditionalWeight(float weight)
	{
		additionalWeight = weight;
	}

	public void SetAdditionalGender(float gender)
	{
		additionalGender = gender;
	}

	public void SetSkinColor(Color color)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (usingCombinedLayer)
		{
			if (((Renderer)BodyMeshes[0]).sharedMaterial.GetColor("_SkinColor") == color)
			{
				return;
			}
			((Renderer)BodyMeshes[0]).sharedMaterial.SetColor("_SkinColor", color);
		}
		else
		{
			if (((Renderer)BodyMeshes[0]).material.GetColor("_SkinColor") == color)
			{
				return;
			}
			SkinnedMeshRenderer[] bodyMeshes = BodyMeshes;
			for (int i = 0; i < bodyMeshes.Length; i++)
			{
				((Renderer)bodyMeshes[i]).material.SetColor("_SkinColor", color);
			}
		}
		Eyes.leftEye.SetLidColor(color);
		Eyes.rightEye.SetLidColor(color);
	}

	public void ApplyHairSettings(AvatarSettings settings)
	{
		if ((Object)(object)appliedHair != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)appliedHair).gameObject);
		}
		Object val = ((settings.HairPath != null) ? Resources.Load(settings.HairPath) : null);
		if (val != (Object)null)
		{
			Object obj = Object.Instantiate(val, HeadBone);
			GameObject val2 = (GameObject)(object)((obj is GameObject) ? obj : null);
			appliedHair = val2.GetComponent<Hair>();
		}
		ApplyHairColorSettings(settings);
		if ((Object)(object)appliedHair != (Object)null)
		{
			appliedHair.SetBlockedByHat(wearingHairBlockingAccessory);
		}
	}

	public void SetHairVisible(bool visible)
	{
		if ((Object)(object)appliedHair != (Object)null)
		{
			((Component)appliedHair).gameObject.SetActive(visible);
		}
	}

	public void ApplyHairColorSettings(AvatarSettings settings)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		appliedHairColor = settings.HairColor;
		if ((Object)(object)appliedHair != (Object)null)
		{
			appliedHair.ApplyColor(appliedHairColor);
		}
		EyeBrows.ApplySettings(settings);
		SetFaceLayer(2, settings.FaceLayer2Path, settings.HairColor);
	}

	public void OverrideHairColor(Color color)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)appliedHair != (Object)null)
		{
			appliedHair.ApplyColor(color);
		}
		EyeBrows.leftBrow.SetColor(color);
		EyeBrows.rightBrow.SetColor(color);
		if ((Object)(object)CurrentSettings != (Object)null)
		{
			SetFaceLayer(2, CurrentSettings.FaceLayer2Path, color);
		}
	}

	public void ResetHairColor()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)CurrentSettings == (Object)null))
		{
			if ((Object)(object)appliedHair != (Object)null)
			{
				appliedHair.ApplyColor(CurrentSettings.HairColor);
			}
			EyeBrows.leftBrow.SetColor(CurrentSettings.HairColor);
			EyeBrows.rightBrow.SetColor(CurrentSettings.HairColor);
			SetFaceLayer(2, CurrentSettings.FaceLayer2Path, CurrentSettings.HairColor);
		}
	}

	public void ApplyEyeBallSettings(AvatarSettings settings)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Eyes.SetEyeballTint(settings.EyeBallTint, overrideDefault: true);
		Eyes.SetPupilDilation(settings.PupilDilation);
	}

	public void ApplyEyeLidSettings(AvatarSettings settings)
	{
		Eyes.SetLeftEyeRestingLidState(settings.LeftEyeRestingState);
		Eyes.SetRightEyeRestingLidState(settings.RightEyeRestingState);
	}

	public void ApplyEyeLidColorSettings(AvatarSettings settings)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		Eyes.leftEye.SetLidColor(settings.LeftEyeLidColor);
		Eyes.rightEye.SetLidColor(settings.RightEyeLidColor);
	}

	public void ApplyEyebrowSettings(AvatarSettings settings)
	{
		EyeBrows.ApplySettings(settings);
	}

	public void SetBlockEyeFaceLayers(bool block)
	{
		blockEyeFaceLayers = block;
		if ((Object)(object)CurrentSettings != (Object)null)
		{
			ApplyFaceLayerSettings(CurrentSettings);
		}
	}

	public void ApplyFaceLayerSettings(AvatarSettings settings)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i <= 6; i++)
		{
			SetFaceLayer(i, string.Empty, Color.white);
		}
		SetFaceLayer(1, settings.FaceLayer1Path, settings.FaceLayer1Color);
		SetFaceLayer(6, settings.FaceLayer2Path, settings.HairColor);
		List<Tuple<FaceLayer, Color>> list = new List<Tuple<FaceLayer, Color>>();
		for (int j = 2; j < settings.FaceLayerSettings.Count; j++)
		{
			if (string.IsNullOrEmpty(settings.FaceLayerSettings[j].layerPath))
			{
				continue;
			}
			FaceLayer faceLayer = Resources.Load(settings.FaceLayerSettings[j].layerPath) as FaceLayer;
			if (!blockEyeFaceLayers || !faceLayer.Name.ToLower().Contains("eye"))
			{
				if ((Object)(object)faceLayer != (Object)null)
				{
					list.Add(new Tuple<FaceLayer, Color>(faceLayer, settings.FaceLayerSettings[j].layerTint));
				}
				else
				{
					Console.LogWarning("Face layer not found at path " + settings.FaceLayerSettings[j].layerPath);
				}
			}
		}
		list.Sort((Tuple<FaceLayer, Color> x, Tuple<FaceLayer, Color> y) => x.Item1.Order.CompareTo(y.Item1.Order));
		for (int num = 0; num < list.Count; num++)
		{
			SetFaceLayer(3 + num, list[num].Item1.AssetPath, list[num].Item2);
		}
	}

	private void SetFaceLayer(int index, string assetPath, Color color)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		FaceLayer faceLayer = Resources.Load(assetPath) as FaceLayer;
		Texture2D val = (((Object)(object)faceLayer != (Object)null) ? faceLayer.Texture : null);
		if ((Object)(object)val == (Object)null)
		{
			color.a = 0f;
		}
		((Renderer)FaceMesh).material.SetTexture("_Layer_" + index + "_Texture", (Texture)(object)val);
		((Renderer)FaceMesh).material.SetColor("_Layer_" + index + "_Color", color);
	}

	public void SetFaceTexture(Texture2D tex, Color color)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)FaceMesh).material.SetTexture("_Layer_" + 1 + "_Texture", (Texture)(object)tex);
		((Renderer)FaceMesh).material.SetColor("_Layer_" + 1 + "_Color", color);
	}

	public void ApplyBodyLayerSettings(AvatarSettings settings, int maxOrder = -1)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i <= 6; i++)
		{
			SetBodyLayer(i, string.Empty, Color.white);
		}
		AvatarLayer avatarLayer = null;
		if (UseCombinedLayer && settings.UseCombinedLayer && (Object)(object)settings.CombinedLayer != (Object)null)
		{
			avatarLayer = settings.CombinedLayer;
		}
		if ((Object)(object)avatarLayer != (Object)null)
		{
			usingCombinedLayer = true;
			avatarLayer.CombinedMaterial.SetFloat("_Wetness", 0f);
			SkinnedMeshRenderer[] bodyMeshes = BodyMeshes;
			for (int j = 0; j < bodyMeshes.Length; j++)
			{
				((Renderer)bodyMeshes[j]).material = avatarLayer.CombinedMaterial;
			}
			return;
		}
		usingCombinedLayer = false;
		List<Tuple<AvatarLayer, Color>> list = new List<Tuple<AvatarLayer, Color>>();
		for (int k = 0; k < settings.BodyLayerSettings.Count; k++)
		{
			if (string.IsNullOrEmpty(settings.BodyLayerSettings[k].layerPath))
			{
				continue;
			}
			AvatarLayer avatarLayer2 = Resources.Load(settings.BodyLayerSettings[k].layerPath) as AvatarLayer;
			if (maxOrder <= -1 || avatarLayer2.Order <= maxOrder)
			{
				if ((Object)(object)avatarLayer2 != (Object)null)
				{
					list.Add(new Tuple<AvatarLayer, Color>(avatarLayer2, settings.BodyLayerSettings[k].layerTint));
				}
				else
				{
					Console.LogWarning("Body layer not found at path " + settings.BodyLayerSettings[k].layerPath);
				}
			}
		}
		list.Sort((Tuple<AvatarLayer, Color> x, Tuple<AvatarLayer, Color> y) => x.Item1.Order.CompareTo(y.Item1.Order));
		for (int num = 0; num < list.Count; num++)
		{
			SetBodyLayer(num + 1, list[num].Item1.AssetPath, list[num].Item2);
		}
	}

	private void SetBodyLayer(int index, string assetPath, Color color)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		AvatarLayer avatarLayer = Resources.Load(assetPath) as AvatarLayer;
		Texture2D val = (((Object)(object)avatarLayer != (Object)null) ? avatarLayer.Texture : null);
		if ((Object)(object)val == (Object)null)
		{
			color.a = 0f;
		}
		SkinnedMeshRenderer[] bodyMeshes = BodyMeshes;
		foreach (SkinnedMeshRenderer val2 in bodyMeshes)
		{
			if ((Object)(object)((Renderer)val2).material.shader != (Object)(object)DefaultAvatarMaterial.shader)
			{
				((Renderer)val2).material = new Material(DefaultAvatarMaterial);
			}
			((Renderer)val2).material.SetTexture("_Layer_" + index + "_Texture", (Texture)(object)val);
			((Renderer)val2).material.SetColor("_Layer_" + index + "_Color", color);
			if ((Object)(object)avatarLayer != (Object)null)
			{
				((Renderer)val2).material.SetTexture("_Layer_" + index + "_Normal", (Texture)(object)avatarLayer.Normal);
			}
		}
	}

	public void ApplyAccessorySettings(AvatarSettings settings)
	{
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		if (appliedAccessories.Length != 9)
		{
			DestroyAccessories();
			appliedAccessories = new Accessory[9];
		}
		bool shrink = false;
		float num = 0f;
		bool flag = false;
		for (int i = 0; i < 9; i++)
		{
			if (settings.AccessorySettings.Count > i && settings.AccessorySettings[i].path != string.Empty)
			{
				if ((Object)(object)appliedAccessories[i] != (Object)null && appliedAccessories[i].AssetPath != settings.AccessorySettings[i].path)
				{
					Object.Destroy((Object)(object)((Component)appliedAccessories[i]).gameObject);
					appliedAccessories[i] = null;
				}
				if ((Object)(object)appliedAccessories[i] == (Object)null)
				{
					Object obj = Object.Instantiate(Resources.Load(settings.AccessorySettings[i].path), BodyContainer);
					GameObject val = (GameObject)(object)((obj is GameObject) ? obj : null);
					appliedAccessories[i] = val.GetComponent<Accessory>();
					appliedAccessories[i].BindBones(BodyMeshes[0].bones);
					appliedAccessories[i].ApplyShapeKeys(appliedGender * 100f, appliedWeight * 100f);
				}
				if (appliedAccessories[i].ReduceFootSize)
				{
					shrink = true;
					num = Mathf.Max(num, appliedAccessories[i].FootSizeReduction);
				}
				if (appliedAccessories[i].ShouldBlockHair)
				{
					flag = true;
				}
			}
			else if ((Object)(object)appliedAccessories[i] != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)appliedAccessories[i]).gameObject);
				appliedAccessories[i] = null;
			}
		}
		SetFeetShrunk(shrink, num);
		SetWearingHairBlockingAccessory(flag);
		for (int j = 0; j < appliedAccessories.Length; j++)
		{
			if ((Object)(object)appliedAccessories[j] != (Object)null)
			{
				appliedAccessories[j].ApplyColor(settings.AccessorySettings[j].color);
			}
		}
	}

	private void DestroyAccessories()
	{
		for (int i = 0; i < appliedAccessories.Length; i++)
		{
			if ((Object)(object)appliedAccessories[i] != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)appliedAccessories[i]).gameObject);
			}
		}
	}

	public virtual void SetRagdollPhysicsEnabled(bool ragdollEnabled, bool playStandUpAnim = true)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		bool ragdolled = Ragdolled;
		Ragdolled = ragdollEnabled;
		if (onRagdollChange != null)
		{
			onRagdollChange.Invoke(ragdolled, ragdollEnabled, playStandUpAnim);
		}
		Rigidbody[] ragdollRBs = RagdollRBs;
		foreach (Rigidbody val in ragdollRBs)
		{
			if (!((Object)(object)val == (Object)null))
			{
				if (!val.isKinematic)
				{
					val.velocity = Vector3.zero;
					val.angularVelocity = Vector3.zero;
					val.position = ((Component)val).transform.position;
					val.rotation = ((Component)val).transform.rotation;
				}
				val.isKinematic = !ragdollEnabled;
			}
		}
		Collider[] ragdollColliders = RagdollColliders;
		foreach (Collider val2 in ragdollColliders)
		{
			if (!((Object)(object)val2 == (Object)null))
			{
				val2.isTrigger = !ragdollEnabled;
			}
		}
	}

	public virtual AvatarEquippable SetEquippable(string assetPath)
	{
		if ((Object)(object)CurrentEquippable != (Object)null)
		{
			CurrentEquippable.Unequip();
		}
		if (assetPath != string.Empty)
		{
			Object obj = Resources.Load(assetPath);
			GameObject val = (GameObject)(object)((obj is GameObject) ? obj : null);
			if ((Object)(object)val == (Object)null)
			{
				Console.LogError("Couldn't find equippable at path " + assetPath);
				return null;
			}
			CurrentEquippable = Object.Instantiate<GameObject>(val, (Transform)null).GetComponent<AvatarEquippable>();
			CurrentEquippable.Equip(this);
			return CurrentEquippable;
		}
		return null;
	}

	public virtual void ReceiveEquippableMessage(string message, object data)
	{
		if ((Object)(object)CurrentEquippable != (Object)null)
		{
			CurrentEquippable.ReceiveMessage(message, data);
		}
		else
		{
			Console.LogWarning("Received equippable message but no equippable is equipped!");
		}
	}
}
