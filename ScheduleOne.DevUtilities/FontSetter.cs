using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.DevUtilities;

public class FontSetter : MonoBehaviour
{
	[Serializable]
	public class ImageItem
	{
		public string Name;

		public Image Image;
	}

	[Header("Components")]
	[SerializeField]
	private List<ImageItem> _imageItems;

	[Header("Fonts")]
	[SerializeField]
	private ColorFont _colourFont;

	public void SetColour(string componentName, string ColourName)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		ImageItem imageItem = _imageItems.Find((ImageItem x) => x.Name == componentName);
		if (!((Object)(object)_colourFont == (Object)null) && imageItem != null)
		{
			Color colour = _colourFont.GetColour(ColourName);
			if ((Object)(object)imageItem.Image != (Object)null)
			{
				((Graphic)imageItem.Image).color = colour;
			}
		}
	}
}
