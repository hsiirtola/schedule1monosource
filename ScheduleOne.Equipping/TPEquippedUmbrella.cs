using System;
using ScheduleOne.Core.Equipping.Framework;
using UnityEngine;

namespace ScheduleOne.Equipping;

public class TPEquippedUmbrella : TPEquippedItem
{
	public MeshRenderer[] CanopyMeshes;

	public SkinnedMeshRenderer[] CanopySkinnedMeshes;

	private Random _random;

	public override void Equip(IEquippedItemHandler handler)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		((TPEquippedItem)this).Equip(handler);
		if (!(handler.EquippableData is EquippableUmbrellaData equippableUmbrellaData))
		{
			return;
		}
		int hashCode = ((object)handler.User).GetHashCode();
		_random = new Random(hashCode);
		float num = (float)_random.NextDouble();
		Color val = equippableUmbrellaData.CanopyColor.Evaluate(num);
		MeshRenderer[] canopyMeshes = CanopyMeshes;
		for (int i = 0; i < canopyMeshes.Length; i++)
		{
			Material[] materials = ((Renderer)canopyMeshes[i]).materials;
			foreach (Material obj in materials)
			{
				obj.SetColor("_CanopyColor", val);
				obj.SetTexture("_CanopyDecal", (Texture)(object)equippableUmbrellaData.CanopyDecal);
				obj.SetColor("_CanopyDecalColor", equippableUmbrellaData.CanopyDecalColor);
			}
		}
		SkinnedMeshRenderer[] canopySkinnedMeshes = CanopySkinnedMeshes;
		for (int i = 0; i < canopySkinnedMeshes.Length; i++)
		{
			Material[] materials = ((Renderer)canopySkinnedMeshes[i]).materials;
			foreach (Material obj2 in materials)
			{
				obj2.SetColor("_CanopyColor", val);
				obj2.SetTexture("_CanopyDecal", (Texture)(object)equippableUmbrellaData.CanopyDecal);
				obj2.SetColor("_CanopyDecalColor", equippableUmbrellaData.CanopyDecalColor);
			}
		}
	}
}
