using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Management;

public class WorldspaceUIElement : MonoBehaviour
{
	public const float TRANSITION_TIME = 0.1f;

	[Header("References")]
	public RectTransform RectTransform;

	public RectTransform Container;

	public TextMeshProUGUI TitleLabel;

	public AssignedWorkerDisplay AssignedWorkerDisplay;

	private Coroutine scaleRoutine;

	public bool IsEnabled { get; protected set; }

	public bool IsVisible => ((Component)this).gameObject.activeSelf;

	public virtual void Show()
	{
		if (!((Object)(object)this == (Object)null) && !((Object)(object)Container == (Object)null) && !((Object)(object)((Component)this).gameObject == (Object)null))
		{
			IsEnabled = true;
			((Component)this).gameObject.SetActive(true);
			SetScale(1f, null);
		}
	}

	public virtual void Hide(Action callback = null)
	{
		if (!((Object)(object)this == (Object)null) && !((Object)(object)Container == (Object)null))
		{
			IsEnabled = false;
			SetScale(0f, delegate
			{
				Done();
			});
		}
		void Done()
		{
			((Component)this).gameObject.SetActive(false);
			if (callback != null)
			{
				callback();
			}
		}
	}

	public virtual void Destroy()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void UpdatePosition(Vector3 worldSpacePosition)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)this == (Object)null) && !((Object)(object)Container == (Object)null))
		{
			if (((Component)PlayerSingleton<PlayerCamera>.Instance).transform.InverseTransformPoint(worldSpacePosition).z > 0f)
			{
				((Transform)RectTransform).position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(worldSpacePosition);
				((Component)Container).gameObject.SetActive(true);
			}
			else
			{
				((Component)Container).gameObject.SetActive(false);
			}
		}
	}

	public virtual void SetInternalScale(float scale)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)this == (Object)null) && !((Object)(object)Container == (Object)null))
		{
			((Transform)Container).localScale = new Vector3(scale, scale, 1f);
		}
	}

	private void SetScale(float scale, Action callback)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null || (Object)(object)Container == (Object)null)
		{
			return;
		}
		if (scaleRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(scaleRoutine);
		}
		float startScale;
		float lerpTime;
		if (!((Component)this).gameObject.activeInHierarchy)
		{
			((Transform)RectTransform).localScale = new Vector3(scale, scale, 1f);
			if (callback != null)
			{
				callback();
			}
		}
		else
		{
			startScale = ((Transform)RectTransform).localScale.x;
			lerpTime = 0.1f / Mathf.Abs(startScale - scale);
			scaleRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				if ((Object)(object)RectTransform == (Object)null)
				{
					break;
				}
				float num = Mathf.Lerp(startScale, scale, i / lerpTime);
				((Transform)RectTransform).localScale = new Vector3(num, num, 1f);
				yield return (object)new WaitForEndOfFrame();
			}
			if ((Object)(object)RectTransform != (Object)null)
			{
				((Transform)RectTransform).localScale = new Vector3(scale, scale, 1f);
			}
			if (callback != null)
			{
				callback();
			}
		}
	}

	public virtual void HoverStart()
	{
	}

	public virtual void HoverEnd()
	{
	}

	public void SetAssignedNPC(NPC npc)
	{
		if (!((Object)(object)this == (Object)null) && !((Object)(object)Container == (Object)null))
		{
			AssignedWorkerDisplay.Set(npc);
		}
	}
}
