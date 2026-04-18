using System;
using UnityEngine;

namespace ScheduleOne.Cartel;

[Serializable]
public class CartelGoonAppearance
{
	public bool IsMale;

	public int BaseAppearanceIndex;

	public Color SkinColor;

	public Color HairColor;

	public int ClothingIndex;

	public int VoiceIndex;

	public CartelGoonAppearance(bool isMale, int baseAppearanceIndex, Color skinColor, Color hairColor, int clothingIndex, int voiceIndex)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		IsMale = isMale;
		BaseAppearanceIndex = baseAppearanceIndex;
		SkinColor = skinColor;
		HairColor = hairColor;
		ClothingIndex = clothingIndex;
		VoiceIndex = voiceIndex;
	}

	public CartelGoonAppearance()
	{
	}
}
