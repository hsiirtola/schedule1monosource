using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.Persistence;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI;

public class SaveIndicator : MonoBehaviour
{
	public Canvas Canvas;

	public RectTransform Icon;

	public Animation Anim;

	public void Awake()
	{
		((Behaviour)Canvas).enabled = false;
	}

	public void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Singleton<SaveManager>.Instance.onSaveStart.AddListener(new UnityAction(Display));
	}

	public void OnDestroy()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		if (Singleton<SaveManager>.InstanceExists)
		{
			Singleton<SaveManager>.Instance.onSaveStart.RemoveListener(new UnityAction(Display));
		}
	}

	public void Display()
	{
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			((Behaviour)Canvas).enabled = true;
			((Component)Icon).gameObject.SetActive(true);
			while (Singleton<SaveManager>.Instance.IsSaving)
			{
				((Transform)Icon).Rotate(Vector3.forward, 360f * Time.unscaledDeltaTime);
				yield return (object)new WaitForEndOfFrame();
			}
			((Component)Icon).gameObject.SetActive(false);
			Anim.Play();
			yield return (object)new WaitForSecondsRealtime(5f);
			((Behaviour)Canvas).enabled = false;
		}
	}
}
