using System;
using FishNet.Object;
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
public class MethInstance : ProductItemInstance
{
	public MethInstance(ItemDefinition definition, int quantity, EQuality quality, PackagingDefinition packaging = null)
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
		return new MethInstance(base.Definition, quantity, Quality, base.AppliedPackaging);
	}

	public override ItemData GetItemData()
	{
		return new MethData(((BaseItemDefinition)base.Definition).ID, ((BaseItemInstance)this).Quantity, Quality.ToString(), PackagingID);
	}

	public override void ApplyEffectsToNPC(NPC npc)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Console.Log("Applying meth effects to NPC: " + npc.fullName);
		npc.Avatar.EmotionManager.AddEmotionOverride("Meth", ((BaseItemInstance)this).Name);
		npc.Avatar.Eyes.SetEyeballTint(Color32.op_Implicit(new Color32((byte)165, (byte)112, (byte)86, byte.MaxValue)));
		npc.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0.5f,
			topLidOpen = 0.1f
		});
		npc.Avatar.Eyes.SetPupilDilation(0.1f, writeDefault: false);
		npc.Avatar.Eyes.ForceBlink();
		npc.OverrideAggression(1f);
		base.ApplyEffectsToNPC(npc);
	}

	public override void ClearEffectsFromNPC(NPC npc)
	{
		npc.Avatar.EmotionManager.RemoveEmotionOverride(((BaseItemInstance)this).Name);
		npc.Avatar.Eyes.ResetEyeballTint();
		npc.Avatar.Eyes.ResetEyeLids();
		npc.Avatar.Eyes.ResetPupilDilation();
		npc.Avatar.Eyes.ForceBlink();
		npc.ResetAggression();
		base.ClearEffectsFromNPC(npc);
	}

	public override void ApplyEffectsToPlayer(Player player)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		player.Avatar.EmotionManager.AddEmotionOverride("Meth", ((BaseItemInstance)this).Name);
		player.Avatar.Eyes.SetEyeballTint(Color32.op_Implicit(new Color32((byte)165, (byte)112, (byte)86, byte.MaxValue)));
		player.Avatar.Eyes.SetPupilDilation(0.1f, writeDefault: false);
		player.Avatar.Eyes.ForceBlink();
		if (((NetworkBehaviour)player).IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.MethVisuals = true;
			Singleton<PostProcessingManager>.Instance.ColorFilterController.AddOverride((base.Definition as MethDefinition).TintColor, 1, "Meth");
			Singleton<MusicManager>.Instance.SetMusicDistorted(distorted: true);
			Singleton<AudioManager>.Instance.SetDistorted(distorted: true);
		}
		base.ApplyEffectsToPlayer(player);
	}

	public override void ClearEffectsFromPlayer(Player Player)
	{
		Player.Avatar.EmotionManager.RemoveEmotionOverride(((BaseItemInstance)this).Name);
		Player.Avatar.Eyes.ResetEyeballTint();
		Player.Avatar.Eyes.ResetEyeLids();
		Player.Avatar.Eyes.ResetPupilDilation();
		Player.Avatar.Eyes.ForceBlink();
		if (((NetworkBehaviour)Player).IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.MethVisuals = false;
			Singleton<PostProcessingManager>.Instance.ColorFilterController.RemoveOverride("Meth");
			Singleton<MusicManager>.Instance.SetMusicDistorted(distorted: false);
			Singleton<AudioManager>.Instance.SetDistorted(distorted: false);
		}
		base.ClearEffectsFromPlayer(Player);
	}
}
