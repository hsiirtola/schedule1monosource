using System;
using System.Collections;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class CartelInfluenceChangePopup : MonoBehaviour
{
	public const float SLIDER_ANIMATION_DURATION = 1.5f;

	public Animation Anim;

	public Slider Slider;

	public TextMeshProUGUI TitleLabel;

	public TextMeshProUGUI InfluenceCountLabel;

	private void Start()
	{
		CartelInfluence influence = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Influence;
		influence.OnInfluenceChanged = (Action<EMapRegion, float, float>)Delegate.Combine(influence.OnInfluenceChanged, new Action<EMapRegion, float, float>(Show));
	}

	public void Show(EMapRegion region, float oldInfluence, float newInfluence)
	{
		if (!Singleton<LoadManager>.Instance.IsLoading && NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status == ECartelStatus.Hostile && !(newInfluence >= oldInfluence))
		{
			((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => !Singleton<DialogueCanvas>.Instance.isActive));
			yield return (object)new WaitUntil((Func<bool>)(() => !Singleton<DealCompletionPopup>.Instance.IsPlaying));
			yield return (object)new WaitUntil((Func<bool>)(() => !Singleton<NewCustomerPopup>.Instance.IsPlaying));
			yield return (object)new WaitForSeconds(0.5f);
			SetDisplayedInfluence(oldInfluence);
			((TMP_Text)TitleLabel).text = "Benzies' Influence in " + region;
			Anim.Play();
			yield return (object)new WaitForSeconds(0.8f);
			for (float i = 0f; i < 1.5f; i += Time.deltaTime)
			{
				float displayedInfluence = Mathf.Lerp(oldInfluence, newInfluence, i / 1.5f);
				SetDisplayedInfluence(displayedInfluence);
				yield return (object)new WaitForEndOfFrame();
			}
		}
	}

	private void SetDisplayedInfluence(float influence)
	{
		((TMP_Text)InfluenceCountLabel).text = Mathf.RoundToInt(influence * 1000f) + " / 1000";
		Slider.value = influence;
	}
}
