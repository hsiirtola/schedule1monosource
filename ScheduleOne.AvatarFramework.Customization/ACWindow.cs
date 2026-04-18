using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.AvatarFramework.Customization;

public class ACWindow : MonoBehaviour
{
	[Header("Settings")]
	public string WindowTitle;

	public ACWindow Predecessor;

	[Header("References")]
	public TextMeshProUGUI TitleText;

	public Button BackButton;

	private void Start()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		((TMP_Text)TitleText).text = WindowTitle;
		if ((Object)(object)Predecessor != (Object)null)
		{
			((UnityEvent)BackButton.onClick).AddListener(new UnityAction(Close));
			((Component)BackButton).gameObject.SetActive(true);
		}
		else
		{
			((Component)BackButton).gameObject.SetActive(false);
		}
		if ((Object)(object)Predecessor != (Object)null)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void Open()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Close()
	{
		((Component)this).gameObject.SetActive(false);
		if ((Object)(object)Predecessor != (Object)null)
		{
			Predecessor.Open();
		}
	}
}
