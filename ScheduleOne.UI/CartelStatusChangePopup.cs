using System;
using System.Collections;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class CartelStatusChangePopup : MonoBehaviour
{
	public Animation Anim;

	public TextMeshProUGUI OldStatusLabel;

	public TextMeshProUGUI NewStatusLabel;

	public Color UnknownColor;

	public Color TrucedColor;

	public Color HostileColor;

	public Color DefeatedColor;

	private void Start()
	{
		ScheduleOne.Cartel.Cartel instance = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance;
		instance.OnStatusChange = (Action<ECartelStatus, ECartelStatus>)Delegate.Combine(instance.OnStatusChange, new Action<ECartelStatus, ECartelStatus>(Show));
	}

	public void Show(ECartelStatus oldStatus, ECartelStatus newStatus)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (!Singleton<LoadManager>.Instance.IsLoading)
		{
			((TMP_Text)OldStatusLabel).text = oldStatus.ToString().ToUpper();
			((Graphic)OldStatusLabel).color = GetColor(oldStatus);
			((TMP_Text)NewStatusLabel).text = newStatus.ToString().ToUpper();
			((Graphic)NewStatusLabel).color = GetColor(newStatus);
			((MonoBehaviour)this).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			yield return (object)new WaitUntil((Func<bool>)(() => !Singleton<DialogueCanvas>.Instance.isActive));
			yield return (object)new WaitForSeconds(0.5f);
			Anim.Play();
		}
	}

	private Color GetColor(ECartelStatus status)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		return (Color)(status switch
		{
			ECartelStatus.Unknown => UnknownColor, 
			ECartelStatus.Truced => TrucedColor, 
			ECartelStatus.Hostile => HostileColor, 
			ECartelStatus.Defeated => DefeatedColor, 
			_ => Color.white, 
		});
	}
}
