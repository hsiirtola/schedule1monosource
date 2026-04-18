using System;
using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product.Packaging;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
public class ShroomInstance : ProductItemInstance
{
	private static Coroutine _psychedelicEffectCoroutine;

	public override string Name
	{
		get
		{
			if (base.Name == "Shroom" && base.Amount > 1)
			{
				return "Shrooms";
			}
			return base.Name;
		}
	}

	private ShroomDefinition _shroomDefinition => base.Definition as ShroomDefinition;

	public ShroomInstance(ItemDefinition definition, int quantity, EQuality quality, PackagingDefinition packaging = null)
		: base(definition, quantity, quality, packaging)
	{
	}

	public override ItemInstance GetCopy(int overrideQuantity = -1)
	{
		int quantity = ((BaseItemInstance)this).Quantity;
		if (overrideQuantity != -1)
		{
			quantity = overrideQuantity;
		}
		return new ShroomInstance(base.Definition, quantity, Quality, base.AppliedPackaging);
	}

	public override ItemData GetItemData()
	{
		return new ShroomData(((BaseItemDefinition)base.Definition).ID, ((BaseItemInstance)this).Quantity, Quality.ToString(), PackagingID);
	}

	public override void ApplyEffectsToNPC(NPC npc)
	{
		ApplyEffectsToAvatar(npc.Avatar);
		base.ApplyEffectsToNPC(npc);
	}

	public override void ClearEffectsFromNPC(NPC npc)
	{
		ClearEffectsFromAvatar(npc.Avatar);
		base.ClearEffectsFromNPC(npc);
	}

	public override void ApplyEffectsToPlayer(Player player)
	{
		ApplyEffectsToAvatar(player.Avatar);
		if (player.IsLocalPlayer)
		{
			Singleton<PostProcessingManager>.Instance.ChromaticAberrationController.AddOverride(0.35f, 5, ((BaseItemInstance)this).Name);
			Singleton<PostProcessingManager>.Instance.SaturationController.AddOverride(80f, 5, ((BaseItemInstance)this).Name);
			Singleton<PostProcessingManager>.Instance.BloomController.AddOverride(3f, 5, ((BaseItemInstance)this).Name);
			Singleton<MusicManager>.Instance.SetMusicDistorted(distorted: true);
			Singleton<AudioManager>.Instance.SetDistorted(distorted: true);
			if (_psychedelicEffectCoroutine != null)
			{
				((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(_psychedelicEffectCoroutine);
			}
			PsychedelicFullScreenData psychedelicEffectDataPreset = Singleton<PostProcessingManager>.Instance.GetPsychedelicEffectDataPreset("Active");
			_psychedelicEffectCoroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(DoPsychedlicEffectBlend(psychedelicEffectDataPreset.ConvertToMaterialProperties(), 1f, 3f));
		}
		base.ApplyEffectsToPlayer(player);
	}

	public override void ClearEffectsFromPlayer(Player player)
	{
		ClearEffectsFromAvatar(player.Avatar);
		if (player.IsLocalPlayer)
		{
			Singleton<PostProcessingManager>.Instance.ChromaticAberrationController.RemoveOverride(((BaseItemInstance)this).Name);
			Singleton<PostProcessingManager>.Instance.SaturationController.RemoveOverride(((BaseItemInstance)this).Name);
			Singleton<PostProcessingManager>.Instance.BloomController.RemoveOverride(((BaseItemInstance)this).Name);
			Singleton<MusicManager>.Instance.SetMusicDistorted(distorted: false);
			Singleton<AudioManager>.Instance.SetDistorted(distorted: false);
			if (_psychedelicEffectCoroutine != null)
			{
				((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(_psychedelicEffectCoroutine);
			}
			PsychedelicFullScreenData psychedelicEffectDataPreset = Singleton<PostProcessingManager>.Instance.GetPsychedelicEffectDataPreset("Default");
			_psychedelicEffectCoroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(DoPsychedlicEffectBlend(psychedelicEffectDataPreset.ConvertToMaterialProperties(), 0f, 3f));
		}
		base.ClearEffectsFromPlayer(player);
	}

	private void ApplyEffectsToAvatar(Avatar avatar)
	{
		avatar.EmotionManager.AddEmotionOverride("Shroom", ((BaseItemInstance)this).Name);
		avatar.Eyes.SetEyeballMaterial(_shroomDefinition.EyeballMaterial);
		avatar.Eyes.SetPupilDilation(1f, writeDefault: false);
		avatar.Eyes.ForceBlink();
	}

	private void ClearEffectsFromAvatar(Avatar avatar)
	{
		avatar.EmotionManager.RemoveEmotionOverride(((BaseItemInstance)this).Name);
		avatar.Eyes.ResetEyeballMaterial();
		avatar.Eyes.ResetPupilDilation();
		avatar.Eyes.ForceBlink();
	}

	private IEnumerator DoPsychedlicEffectBlend(PsychedelicFullScreenFeature.MaterialProperties targetMaterialProperties, float targetValuePercentage, float duration)
	{
		float elapsed = 0f;
		PsychedelicFullScreenFeature.MaterialProperties activeProperties = Singleton<PostProcessingManager>.Instance.GetActivePsychedelicEffectProperties();
		PsychedelicFullScreenFeature.MaterialProperties sourceProperties = activeProperties.Clone();
		float startValue = Mathf.Abs(1f - targetValuePercentage);
		if (targetValuePercentage > 0f)
		{
			Singleton<EnvironmentFX>.Instance.SetEnvironmentScrollingActive(active: true);
			Singleton<PostProcessingManager>.Instance.SetPsychedelicEffectActive(isActive: true);
		}
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / duration);
			activeProperties.Blend = Mathf.Lerp(sourceProperties.Blend, targetMaterialProperties.Blend, num);
			Singleton<EnvironmentFX>.Instance.SetEnvironmentScrollingSpeedByPercentage(Mathf.Lerp(startValue, targetValuePercentage, num));
			Debug.Log((object)$"Setting psychedelic blend to {activeProperties.Blend}");
			Singleton<PostProcessingManager>.Instance.PrintValueOfPsychedelicEffectBlend();
			yield return null;
		}
		Singleton<PostProcessingManager>.Instance.SetPsychedelicEffectProperties(targetMaterialProperties);
		if (targetValuePercentage <= 0f)
		{
			Singleton<EnvironmentFX>.Instance.SetEnvironmentScrollingActive(active: false);
			Singleton<PostProcessingManager>.Instance.SetPsychedelicEffectActive(isActive: false);
		}
	}
}
