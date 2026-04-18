using ScheduleOne.DevUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.MainMenu;

public class MainMenuPopup : Singleton<MainMenuPopup>
{
	public class Data
	{
		public string Title;

		public string Description;

		public bool IsBad;

		public Data(string title, string description, bool isBad)
		{
			Title = title;
			Description = description;
			IsBad = isBad;
		}
	}

	public MainMenuScreen Screen;

	public TextMeshProUGUI Title;

	public TextMeshProUGUI Description;

	public void Open(Data data)
	{
		Open(data.Title, data.Description, data.IsBad);
	}

	public void Open(string title, string description, bool isBad)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)Title).color = (isBad ? Color32.op_Implicit(new Color32(byte.MaxValue, (byte)115, (byte)115, byte.MaxValue)) : Color.white);
		((TMP_Text)Title).text = title;
		((TMP_Text)Description).text = description;
		Screen.Open(closePrevious: false);
	}
}
