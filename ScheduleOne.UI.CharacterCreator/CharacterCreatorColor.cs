using System.Collections.Generic;
using ScheduleOne.Clothing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCreator;

public class CharacterCreatorColor : CharacterCreatorField<Color>
{
	public static EClothingColor[] ClothingColorsToUse = new EClothingColor[20]
	{
		EClothingColor.White,
		EClothingColor.LightGrey,
		EClothingColor.DarkGrey,
		EClothingColor.Charcoal,
		EClothingColor.Black,
		EClothingColor.Red,
		EClothingColor.Crimson,
		EClothingColor.Orange,
		EClothingColor.Tan,
		EClothingColor.Brown,
		EClothingColor.Yellow,
		EClothingColor.Lime,
		EClothingColor.DarkGreen,
		EClothingColor.Cyan,
		EClothingColor.SkyBlue,
		EClothingColor.Blue,
		EClothingColor.Navy,
		EClothingColor.Purple,
		EClothingColor.Magenta,
		EClothingColor.BrightPink
	};

	[Header("References")]
	public RectTransform OptionContainer;

	[Header("Settings")]
	public bool UseClothingColors;

	public List<Color> Colors;

	public GameObject OptionPrefab;

	private List<Button> optionButtons = new List<Button>();

	private Button selectedButton;

	protected override void Awake()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		if (UseClothingColors)
		{
			Colors = new List<Color>();
			EClothingColor[] clothingColorsToUse = ClothingColorsToUse;
			foreach (EClothingColor color in clothingColorsToUse)
			{
				Colors.Add(color.GetActualColor());
			}
		}
		for (int j = 0; j < Colors.Count; j++)
		{
			GameObject val = Object.Instantiate<GameObject>(OptionPrefab, (Transform)(object)OptionContainer);
			((Graphic)((Component)val.transform.Find("Color")).GetComponent<Image>()).color = Colors[j];
			Color col = Colors[j];
			((UnityEvent)val.GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				OptionClicked(col);
			});
			optionButtons.Add(val.GetComponent<Button>());
		}
	}

	public override void ApplyValue()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.ApplyValue();
		Button val = null;
		for (int i = 0; i < Colors.Count; i++)
		{
			if (ClothingColorExtensions.ColorEquals(base.value, Colors[i]) && i < optionButtons.Count)
			{
				val = optionButtons[i];
				break;
			}
		}
		if ((Object)(object)selectedButton != (Object)null)
		{
			((Selectable)selectedButton).interactable = true;
		}
		selectedButton = val;
		if ((Object)(object)selectedButton != (Object)null)
		{
			((Selectable)selectedButton).interactable = false;
		}
	}

	public void OptionClicked(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.value = color;
		WriteValue();
	}
}
