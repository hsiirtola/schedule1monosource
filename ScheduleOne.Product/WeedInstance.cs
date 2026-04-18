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
public class WeedInstance : ProductItemInstance
{
	public WeedInstance(ItemDefinition definition, int quantity, EQuality quality, PackagingDefinition packaging = null)
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
		return new WeedInstance(base.Definition, quantity, Quality, base.AppliedPackaging);
	}

	public override ItemData GetItemData()
	{
		return new WeedData(((BaseItemDefinition)base.Definition).ID, ((BaseItemInstance)this).Quantity, Quality.ToString(), PackagingID);
	}

	public override void ApplyEffectsToNPC(NPC npc)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		npc.Avatar.Eyes.SetEyeballTint(Color32.op_Implicit(new Color32(byte.MaxValue, (byte)170, (byte)170, byte.MaxValue)));
		npc.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0.3f,
			topLidOpen = 0.3f
		});
		npc.Avatar.Eyes.ForceBlink();
		base.ApplyEffectsToNPC(npc);
	}

	public override void ClearEffectsFromNPC(NPC npc)
	{
		npc.Avatar.Eyes.ResetEyeballTint();
		npc.Avatar.Eyes.ResetEyeLids();
		npc.Avatar.Eyes.ForceBlink();
		base.ClearEffectsFromNPC(npc);
	}

	public override void ApplyEffectsToPlayer(Player player)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		player.Avatar.Eyes.SetEyeballTint(Color32.op_Implicit(new Color32(byte.MaxValue, (byte)170, (byte)170, byte.MaxValue)));
		player.Avatar.Eyes.OverrideEyeLids(new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0.3f,
			topLidOpen = 0.3f
		});
		if (((NetworkBehaviour)player).IsOwner)
		{
			Singleton<PostProcessingManager>.Instance.ChromaticAberrationController.AddOverride(0.2f, 5, "weed");
			Singleton<PostProcessingManager>.Instance.SaturationController.AddOverride(70f, 5, "weed");
			Singleton<PostProcessingManager>.Instance.BloomController.AddOverride(3f, 5, "weed");
			Singleton<MusicManager>.Instance.SetMusicDistorted(distorted: true);
			Singleton<AudioManager>.Instance.SetDistorted(distorted: true);
		}
		base.ApplyEffectsToPlayer(player);
	}

	public override void ClearEffectsFromPlayer(Player Player)
	{
		Player.Avatar.Eyes.ResetEyeballTint();
		Player.Avatar.Eyes.ResetEyeLids();
		Player.Avatar.Eyes.ForceBlink();
		if (((NetworkBehaviour)Player).IsOwner)
		{
			Singleton<PostProcessingManager>.Instance.ChromaticAberrationController.RemoveOverride("weed");
			Singleton<PostProcessingManager>.Instance.SaturationController.RemoveOverride("weed");
			Singleton<PostProcessingManager>.Instance.BloomController.RemoveOverride("weed");
			Singleton<MusicManager>.Instance.SetMusicDistorted(distorted: false);
			Singleton<AudioManager>.Instance.SetDistorted(distorted: false);
		}
		base.ClearEffectsFromPlayer(Player);
	}
}
