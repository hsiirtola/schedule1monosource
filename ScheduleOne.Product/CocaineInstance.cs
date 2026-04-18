using System;
using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Tools;
using UnityEngine;

namespace ScheduleOne.Product;

[Serializable]
public class CocaineInstance : ProductItemInstance
{
	private const float WorkSpeedMultiplier = 1.2f;

	public CocaineInstance(ItemDefinition definition, int quantity, EQuality quality, PackagingDefinition packaging = null)
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
		return new CocaineInstance(base.Definition, quantity, Quality, base.AppliedPackaging);
	}

	public override ItemData GetItemData()
	{
		return new CocaineData(((BaseItemDefinition)base.Definition).ID, ((BaseItemInstance)this).Quantity, Quality.ToString(), PackagingID);
	}

	public override void ApplyEffectsToNPC(NPC npc)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		npc.Avatar.EmotionManager.AddEmotionOverride("Cocaine", ((BaseItemInstance)this).Name);
		npc.Avatar.Eyes.SetEyeballTint(Color32.op_Implicit(new Color32((byte)200, (byte)240, byte.MaxValue, byte.MaxValue)));
		npc.Avatar.Eyes.SetPupilDilation(1f, writeDefault: false);
		npc.Avatar.Eyes.ForceBlink();
		npc.Movement.MoveSpeedMultiplier = 1.25f;
		npc.Avatar.LookController.LookLerpSpeed = 10f;
		if (npc is Employee)
		{
			(npc as Employee).WorkSpeedController.Add(new FloatStack.StackEntry(((BaseItemInstance)this).Name, 1.2f, FloatStack.EStackMode.Multiplicative, 0));
		}
		base.ApplyEffectsToNPC(npc);
	}

	public override void ClearEffectsFromNPC(NPC npc)
	{
		npc.Avatar.EmotionManager.RemoveEmotionOverride(((BaseItemInstance)this).Name);
		npc.Avatar.Eyes.ResetEyeballTint();
		npc.Avatar.Eyes.ResetEyeLids();
		npc.Avatar.Eyes.ResetPupilDilation();
		npc.Avatar.Eyes.ForceBlink();
		npc.Movement.MoveSpeedMultiplier = 1f;
		npc.Avatar.LookController.LookLerpSpeed = 3f;
		if (npc is Employee)
		{
			(npc as Employee).WorkSpeedController.Remove(((BaseItemInstance)this).Name);
		}
		base.ClearEffectsFromNPC(npc);
	}

	public override void ApplyEffectsToPlayer(Player player)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		player.Avatar.EmotionManager.AddEmotionOverride("Cocaine", ((BaseItemInstance)this).Name);
		player.Avatar.Eyes.SetEyeballTint(Color32.op_Implicit(new Color32((byte)200, (byte)240, byte.MaxValue, byte.MaxValue)));
		player.Avatar.Eyes.SetPupilDilation(1f, writeDefault: false);
		player.Avatar.Eyes.ForceBlink();
		player.Avatar.LookController.LookLerpSpeed = 10f;
		if (((NetworkBehaviour)player).IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.CocaineVisuals = true;
			PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.AddOverride(10f, 6, "Cocaine");
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
		Player.Avatar.LookController.LookLerpSpeed = 3f;
		if (((NetworkBehaviour)Player).IsOwner)
		{
			PlayerSingleton<PlayerCamera>.Instance.CocaineVisuals = false;
			PlayerSingleton<PlayerCamera>.Instance.FoVChangeSmoother.RemoveOverride("Cocaine");
			Singleton<MusicManager>.Instance.SetMusicDistorted(distorted: false);
			Singleton<AudioManager>.Instance.SetDistorted(distorted: false);
		}
		base.ClearEffectsFromPlayer(Player);
	}
}
