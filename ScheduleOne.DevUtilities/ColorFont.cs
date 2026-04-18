using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

[CreateAssetMenu(fileName = "ColorFont", menuName = "ScriptableObjects/Fonts/ColorFont", order = 1)]
public class ColorFont : ScriptableObject
{
	[Serializable]
	public class ColorFontItem
	{
		public string Name;

		public Color Colour;
	}

	[SerializeField]
	private List<ColorFontItem> ColorFontItems = new List<ColorFontItem>();

	public Color GetColour(string name)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		return ColorFontItems.Find((ColorFontItem x) => x.Name == name)?.Colour ?? Color.white;
	}
}
