using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework;

public class AvatarEffects : MonoBehaviour
{
	[Header("References")]
	public Avatar Avatar;

	public ParticleSystem[] StinkParticles;

	public ParticleSystem VomitParticles;

	public ParticleSystem HeadPoofParticles;

	public ParticleSystem FartParticles;

	public ParticleSystem AntiGravParticles;

	public ParticleSystem FireParticles;

	public OptimizedLight FireLight;

	public ParticleSystem FoggyEffects;

	public Transform HeadBone;

	public Transform NeckBone;

	public AvatarEffects[] MirrorEffectsTo;

	public ParticleSystem ZapParticles;

	public CountdownExplosion CountdownExplosion;

	public GameObject[] ObjectsToCull;

	[Header("Settings")]
	public bool DisableHead;

	[Header("Sounds")]
	public AudioSourceController GurgleSound;

	public AudioSourceController VomitSound;

	public AudioSourceController PoofSound;

	public AudioSourceController FartSound;

	public AudioSourceController FireSound;

	public AudioSourceController ZapSound;

	public AudioSourceController ZapLoopSound;

	[Header("Smoothers")]
	[SerializeField]
	private FloatSmoother AdditionalWeightController;

	[SerializeField]
	private FloatSmoother AdditionalGenderController;

	[SerializeField]
	private FloatSmoother HeadSizeBoost;

	[SerializeField]
	private FloatSmoother NeckSizeBoost;

	[SerializeField]
	private ColorSmoother SkinColorSmoother;

	private bool laxativeEnabled;

	private Color currentEmission = Color.black;

	private Color targetEmission = Color.black;

	private bool isCulled;

	private void Start()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		AdditionalWeightController.Initialize();
		AdditionalWeightController.SetDefault(0f);
		AdditionalGenderController.Initialize();
		AdditionalGenderController.SetDefault(0f);
		HeadSizeBoost.Initialize();
		HeadSizeBoost.SetDefault(0f);
		NeckSizeBoost.Initialize();
		NeckSizeBoost.SetDefault(0f);
		SkinColorSmoother.Initialize();
		if ((Object)(object)Avatar.CurrentSettings != (Object)null)
		{
			SetDefaultSkinColor();
		}
		ZapLoopSound.VolumeMultiplier = 0f;
		Avatar.onSettingsLoaded.AddListener((UnityAction)delegate
		{
			SetDefaultSkinColor();
		});
	}

	public void Update()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		SetEffectsCulled(Avatar.Animation.IsAvatarCulled);
		if (Avatar.Animation.IsAvatarCulled)
		{
			return;
		}
		Avatar.SetAdditionalWeight(AdditionalWeightController.CurrentValue);
		Avatar.SetAdditionalGender(AdditionalGenderController.CurrentValue);
		Avatar.SetSkinColor(SkinColorSmoother.CurrentValue);
		currentEmission = Color.Lerp(currentEmission, targetEmission, Time.deltaTime * 0.5f);
		Avatar.SetEmission(currentEmission);
		((Component)HeadBone).transform.localScale = Vector3.one * (1f + HeadSizeBoost.CurrentValue);
		if (DisableHead)
		{
			((Component)NeckBone).transform.localScale = Vector3.zero;
		}
		else
		{
			((Component)NeckBone).transform.localScale = Vector3.one * (1f + NeckSizeBoost.CurrentValue);
		}
		if (FireParticles.isPlaying)
		{
			FireSound.VolumeMultiplier = Mathf.MoveTowards(FireSound.VolumeMultiplier, 1f, Time.deltaTime);
			if (!FireSound.IsPlaying)
			{
				FireSound.Play();
			}
		}
		else if (FireSound.VolumeMultiplier > 0f)
		{
			FireSound.VolumeMultiplier = Mathf.MoveTowards(FireSound.VolumeMultiplier, 0f, Time.deltaTime);
			if (FireSound.VolumeMultiplier <= 0f)
			{
				FireSound.Stop();
			}
		}
		if (ZapParticles.isPlaying)
		{
			ZapLoopSound.VolumeMultiplier = Mathf.MoveTowards(ZapLoopSound.VolumeMultiplier, 1f, Time.deltaTime * 2f);
			if (!ZapLoopSound.IsPlaying)
			{
				ZapLoopSound.Play();
			}
		}
		else if (ZapLoopSound.VolumeMultiplier > 0f)
		{
			ZapLoopSound.VolumeMultiplier = Mathf.MoveTowards(ZapLoopSound.VolumeMultiplier, 0f, Time.deltaTime * 2f);
			if (ZapLoopSound.VolumeMultiplier <= 0f)
			{
				ZapLoopSound.Stop();
			}
		}
	}

	private void SetEffectsCulled(bool culled)
	{
		if (isCulled != culled)
		{
			isCulled = culled;
			GameObject[] objectsToCull = ObjectsToCull;
			for (int i = 0; i < objectsToCull.Length; i++)
			{
				objectsToCull[i].SetActive(!culled);
			}
		}
	}

	public void SetStinkParticlesActive(bool active, bool mirror = true)
	{
		ParticleSystem[] stinkParticles = StinkParticles;
		foreach (ParticleSystem val in stinkParticles)
		{
			if (active)
			{
				val.Play();
			}
			else
			{
				val.Stop();
			}
		}
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetStinkParticlesActive(active, mirror: false);
			}
		}
	}

	public void TriggerSick(bool mirror = true)
	{
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].TriggerSick(mirror: false);
			}
		}
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			GurgleSound.Play();
			yield return (object)new WaitForSeconds(4.5f);
			VomitSound.Play();
			((Component)VomitParticles).gameObject.layer = LayerMask.NameToLayer("Default");
			VomitParticles.Play();
		}
	}

	public void SetAntiGrav(bool active, bool mirror = true)
	{
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetAntiGrav(active, mirror: false);
			}
		}
		if (active)
		{
			AntiGravParticles.Play();
		}
		else
		{
			AntiGravParticles.Stop();
		}
	}

	public void SetFoggy(bool active, bool mirror = true)
	{
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetFoggy(active, mirror: false);
			}
		}
		if (active)
		{
			FoggyEffects.Play();
		}
		else
		{
			FoggyEffects.Stop();
		}
	}

	public void VanishHair(bool mirror = true)
	{
		HeadPoofParticles.Play();
		PoofSound.Play();
		Avatar.SetHairVisible(visible: false);
		((Component)Avatar.EyeBrows.leftBrow).gameObject.SetActive(false);
		((Component)Avatar.EyeBrows.rightBrow).gameObject.SetActive(false);
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].VanishHair(mirror: false);
			}
		}
	}

	public void SetZapped(bool zapped, bool mirror = true)
	{
		if (zapped)
		{
			LayerUtility.SetLayerRecursively(((Component)ZapParticles).gameObject, LayerMask.NameToLayer("Default"));
			ZapParticles.Play();
			ZapSound.Play();
		}
		else
		{
			ZapParticles.Stop();
			ZapSound.Stop();
		}
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetZapped(zapped, mirror: false);
			}
		}
	}

	public void ReturnHair(bool mirror = true)
	{
		HeadPoofParticles.Play();
		PoofSound.Play();
		Avatar.SetHairVisible(visible: true);
		((Component)Avatar.EyeBrows.leftBrow).gameObject.SetActive(true);
		((Component)Avatar.EyeBrows.rightBrow).gameObject.SetActive(true);
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].ReturnHair(mirror: false);
			}
		}
	}

	public void OverrideHairColor(Color color, bool mirror = true)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		HeadPoofParticles.Play();
		PoofSound.Play();
		Avatar.OverrideHairColor(color);
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].OverrideHairColor(color, mirror: false);
			}
		}
	}

	public void ResetHairColor(bool mirror = true)
	{
		HeadPoofParticles.Play();
		PoofSound.Play();
		Avatar.ResetHairColor();
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].ResetHairColor(mirror: false);
			}
		}
	}

	public void OverrideEyeColor(Color color, float emission = 0.115f, bool mirror = true)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Avatar.Eyes.rightEye.SetEyeballColor(color, emission, writeDefault: false);
		Avatar.Eyes.leftEye.SetEyeballColor(color, emission, writeDefault: false);
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].OverrideEyeColor(color, emission, mirror: false);
			}
		}
	}

	public void ResetEyeColor(bool mirror = true)
	{
		Avatar.Eyes.rightEye.ResetEyeballColor();
		Avatar.Eyes.leftEye.ResetEyeballColor();
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].ResetEyeColor(mirror: false);
			}
		}
	}

	public void SetEyeLightEmission(float intensity, Color color, bool mirror = true)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Avatar.Eyes.rightEye.ConfigureEyeLight(color, intensity);
		Avatar.Eyes.leftEye.ConfigureEyeLight(color, intensity);
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetEyeLightEmission(intensity, color, mirror: false);
			}
		}
	}

	public void EnableLaxative(bool mirror = true)
	{
		laxativeEnabled = true;
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].EnableLaxative(mirror: false);
			}
		}
		((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			do
			{
				FartParticles.Play();
				FartSound.Play();
				yield return (object)new WaitForSeconds(Random.Range(3f, 20f));
			}
			while (laxativeEnabled);
		}
	}

	public void DisableLaxative(bool mirror = true)
	{
		laxativeEnabled = false;
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].DisableLaxative(mirror: false);
			}
		}
	}

	public void SetFireActive(bool active, bool mirror = true)
	{
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetFireActive(active, mirror: false);
			}
		}
		FireLight.Enabled = active;
		if (active)
		{
			FireParticles.Play();
		}
		else
		{
			FireParticles.Stop();
		}
	}

	public void SetBigHeadActive(bool active, bool mirror = true)
	{
		if (active)
		{
			HeadSizeBoost.AddOverride(0.4f, 7, "big head");
		}
		else
		{
			HeadSizeBoost.RemoveOverride("big head");
		}
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetBigHeadActive(active, mirror: false);
			}
		}
	}

	public void SetGiraffeActive(bool active, bool mirror = true)
	{
		if (active)
		{
			HeadSizeBoost.AddOverride(-0.5f, 8, "giraffe");
			NeckSizeBoost.AddOverride(1f, 8, "giraffe");
		}
		else
		{
			HeadSizeBoost.RemoveOverride("giraffe");
			NeckSizeBoost.RemoveOverride("giraffe");
		}
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetGiraffeActive(active, mirror: false);
			}
		}
	}

	public void SetSkinColorInverted(bool inverted, bool mirror = true)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (inverted)
		{
			if (Avatar.IsWhite())
			{
				SkinColorSmoother.AddOverride(Color32.op_Implicit(new Color32((byte)58, (byte)49, (byte)42, byte.MaxValue)), 7, "inverted");
			}
			else
			{
				SkinColorSmoother.AddOverride(Color32.op_Implicit(new Color32((byte)223, (byte)189, (byte)161, byte.MaxValue)), 7, "inverted");
			}
		}
		else
		{
			SkinColorSmoother.RemoveOverride("inverted");
		}
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetSkinColorInverted(inverted, mirror: false);
			}
		}
	}

	public unsafe void SetSicklySkinColor(bool mirror = true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		Color skinColor = Avatar.CurrentSettings.SkinColor;
		float num = 0.5f;
		float num2 = 0.3f * skinColor.r + 0.59f * skinColor.g + 0.11f * skinColor.b;
		Color white = Color.white;
		white.r = skinColor.r + (num2 - skinColor.r) * num;
		white.g = skinColor.g + (num2 - skinColor.g) * num;
		white.b = skinColor.b + (num2 - skinColor.b) * num;
		white *= 1.1f;
		Color val = white;
		Console.Log("Sickly Color: " + ((object)(*(Color*)(&val))/*cast due to .constrained prefix*/).ToString());
		SkinColorSmoother.AddOverride(white, 6, "sickly");
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetSicklySkinColor(mirror: false);
			}
		}
	}

	private void SetDefaultSkinColor(bool mirror = true)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Avatar.CurrentSettings == (Object)null)
		{
			return;
		}
		SkinColorSmoother.SetDefault(Avatar.CurrentSettings.SkinColor);
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetDefaultSkinColor(mirror: false);
			}
		}
	}

	public void SetGenderInverted(bool inverted, bool mirror = true)
	{
		if (inverted)
		{
			if (Avatar.IsMale())
			{
				AdditionalGenderController.AddOverride(1f, 7, "jennerising");
			}
			else
			{
				AdditionalGenderController.AddOverride(-1f, 7, "jennerising");
			}
		}
		else
		{
			AdditionalGenderController.RemoveOverride("jennerising");
		}
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetGenderInverted(inverted, mirror: false);
			}
		}
	}

	public void AddAdditionalWeightOverride(float value, int priority, string label, bool mirror = true)
	{
		AdditionalWeightController.AddOverride(value, priority, label);
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].AddAdditionalWeightOverride(value, priority, label, mirror: false);
			}
		}
	}

	public void RemoveAdditionalWeightOverride(string label, bool mirror = true)
	{
		AdditionalWeightController.RemoveOverride(label);
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].RemoveAdditionalWeightOverride(label, mirror: false);
			}
		}
	}

	public void SetGlowingOn(Color color, bool mirror = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		targetEmission = color;
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetGlowingOn(color, mirror: false);
			}
		}
	}

	public void SetGlowingOff(bool mirror = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		targetEmission = Color.black;
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetGlowingOff(mirror: false);
			}
		}
	}

	public void TriggerCountdownExplosion(bool mirror = true)
	{
		CountdownExplosion.Trigger();
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].TriggerCountdownExplosion(mirror: false);
			}
		}
	}

	public void StopCountdownExplosion(bool mirror = true)
	{
		CountdownExplosion.StopCountdown();
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].StopCountdownExplosion(mirror: false);
			}
		}
	}

	public void SetCyclopean(bool enabled, bool mirror = true)
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		HeadPoofParticles.Play();
		PoofSound.Play();
		if (enabled)
		{
			((Component)Avatar.Eyes.rightEye).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			((Component)Avatar.Eyes.rightEye).transform.localScale = new Vector3(1.2f, 1.2f, 1f);
			((Component)Avatar.Eyes.leftEye).gameObject.SetActive(false);
			Avatar.SetBlockEyeFaceLayers(block: true);
		}
		else
		{
			((Component)Avatar.Eyes.rightEye).transform.localRotation = Quaternion.Euler(0f, 22f, 0f);
			((Component)Avatar.Eyes.rightEye).transform.localScale = new Vector3(1f, 1f, 1f);
			((Component)Avatar.Eyes.leftEye).gameObject.SetActive(true);
			Avatar.SetBlockEyeFaceLayers(block: false);
		}
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetCyclopean(enabled, mirror: false);
			}
		}
	}

	public void SetZombified(bool zombified, bool mirror = true)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (zombified)
		{
			SkinColorSmoother.AddOverride(Color32.op_Implicit(new Color32((byte)117, (byte)122, (byte)92, byte.MaxValue)), 10, "Zombified");
			((Component)Avatar.Eyes.leftEye.PupilContainer).gameObject.SetActive(!zombified);
			((Component)Avatar.Eyes.rightEye.PupilContainer).gameObject.SetActive(!zombified);
			OverrideEyeColor(Color32.op_Implicit(new Color32((byte)159, (byte)129, (byte)129, byte.MaxValue)), 0.115f, mirror: false);
			Avatar.EmotionManager.AddEmotionOverride("Zombie", "Zombified", 0f, 10);
		}
		else
		{
			SkinColorSmoother.RemoveOverride("Zombified");
			((Component)Avatar.Eyes.leftEye.PupilContainer).gameObject.SetActive(true);
			((Component)Avatar.Eyes.rightEye.PupilContainer).gameObject.SetActive(true);
			ResetEyeColor(mirror: false);
			Avatar.EmotionManager.RemoveEmotionOverride("Zombified");
		}
		if (mirror)
		{
			AvatarEffects[] mirrorEffectsTo = MirrorEffectsTo;
			for (int i = 0; i < mirrorEffectsTo.Length; i++)
			{
				mirrorEffectsTo[i].SetZombified(zombified, mirror: false);
			}
		}
	}
}
