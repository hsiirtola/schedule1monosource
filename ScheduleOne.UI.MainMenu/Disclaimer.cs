using System.Collections;
using UnityEngine;

namespace ScheduleOne.UI.MainMenu;

public class Disclaimer : MonoBehaviour
{
	public static bool Shown;

	public CanvasGroup Group;

	public CanvasGroup TextGroup;

	public float Duration = 3.8f;

	private void Awake()
	{
		if (Application.isEditor || Shown)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		Shown = true;
		Group.alpha = 1f;
		TextGroup.alpha = 0f;
		Fade();
	}

	private void Fade()
	{
		((MonoBehaviour)this).StartCoroutine(Fade());
		IEnumerator Fade()
		{
			while (TextGroup.alpha < 1f)
			{
				CanvasGroup textGroup = TextGroup;
				textGroup.alpha += Time.deltaTime * 2f;
				yield return null;
			}
			for (float i = 0f; i < Duration; i += Time.deltaTime)
			{
				if (Input.GetKey((KeyCode)32))
				{
					break;
				}
				yield return (object)new WaitForEndOfFrame();
			}
			while (Group.alpha > 0f)
			{
				CanvasGroup obj = Group;
				obj.alpha -= Time.deltaTime * 2f;
				yield return null;
			}
			((Component)this).gameObject.SetActive(false);
		}
	}
}
