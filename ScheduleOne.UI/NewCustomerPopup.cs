using System;
using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class NewCustomerPopup : Singleton<NewCustomerPopup>
{
	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public CanvasGroup Group;

	public Animation Anim;

	public TextMeshProUGUI Title;

	public RectTransform[] Entries;

	public AudioSourceController SoundEffect;

	private Coroutine routine;

	public bool IsPlaying { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		DisableEntries();
	}

	public void PlayPopup(Customer customer)
	{
		IsPlaying = true;
		RectTransform val = null;
		int num = 0;
		for (int i = 0; i < Entries.Length; i++)
		{
			num++;
			if (!((Component)Entries[i]).gameObject.activeSelf)
			{
				val = Entries[i];
				break;
			}
		}
		if (!((Object)(object)val == (Object)null))
		{
			((Component)((Transform)val).Find("Mask/Icon")).GetComponent<Image>().sprite = customer.NPC.MugshotSprite;
			((TMP_Text)((Component)((Transform)val).Find("Name")).GetComponent<TextMeshProUGUI>()).text = customer.NPC.FirstName + "\n" + customer.NPC.LastName;
			((Component)val).gameObject.SetActive(true);
			if (num == 1)
			{
				((TMP_Text)Title).text = "New Customer Unlocked!";
			}
			else
			{
				((TMP_Text)Title).text = "New Customers Unlocked!";
			}
			if (routine != null)
			{
				((MonoBehaviour)this).StopCoroutine(routine);
				Anim.Stop();
				routine = null;
			}
			routine = ((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => !Singleton<DealCompletionPopup>.Instance.IsPlaying));
			Group.alpha = 0.01f;
			((Behaviour)Canvas).enabled = true;
			((Component)Container).gameObject.SetActive(true);
			SoundEffect.Play();
			Anim.Play();
			yield return (object)new WaitForSeconds(0.1f);
			yield return (object)new WaitUntil((Func<bool>)(() => Group.alpha == 0f));
			((Behaviour)Canvas).enabled = false;
			((Component)Container).gameObject.SetActive(false);
			routine = null;
			IsPlaying = false;
			DisableEntries();
		}
	}

	private void DisableEntries()
	{
		for (int i = 0; i < Entries.Length; i++)
		{
			((Component)Entries[i]).gameObject.SetActive(false);
		}
	}
}
