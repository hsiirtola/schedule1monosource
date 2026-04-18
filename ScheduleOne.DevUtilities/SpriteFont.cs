using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

[CreateAssetMenu(fileName = "SpriteFont", menuName = "ScriptableObjects/Fonts/SpriteFont")]
public class SpriteFont : ScriptableObject
{
	[Serializable]
	public class SpriteFontItem
	{
		public string Name;

		public Sprite Sprite;
	}

	[SerializeField]
	private List<SpriteFontItem> SpriteFontItems = new List<SpriteFontItem>();

	public Sprite GetSprite(string name)
	{
		return SpriteFontItems.Find((SpriteFontItem x) => x.Name == name)?.Sprite;
	}
}
