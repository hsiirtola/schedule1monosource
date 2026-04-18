using System.Collections;
using ScheduleOne.PlayerTasks;
using UnityEngine;

namespace ScheduleOne.Growing;

public class SoilChunk : Clickable
{
	public Transform EndTransform;

	public float LerpTime = 0.4f;

	private Vector3 localPos_Start;

	private Vector3 localEulerAngles_Start;

	private Vector3 localScale_Start;

	private Coroutine lerpRoutine;

	public float CurrentLerp { get; protected set; }

	protected virtual void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		localPos_Start = ((Component)this).transform.localPosition;
		localEulerAngles_Start = ((Component)this).transform.localEulerAngles;
		localScale_Start = ((Component)this).transform.localScale;
	}

	public void SetLerpedTransform(float _lerp)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		CurrentLerp = Mathf.Clamp(_lerp, 0f, 1f);
		((Component)this).transform.localPosition = Vector3.Lerp(localPos_Start, EndTransform.localPosition, CurrentLerp);
		((Component)this).transform.localRotation = Quaternion.Lerp(Quaternion.Euler(localEulerAngles_Start), Quaternion.Euler(EndTransform.localEulerAngles), CurrentLerp);
		((Component)this).transform.localScale = Vector3.Lerp(localScale_Start, EndTransform.localScale, CurrentLerp);
	}

	public override void StartClick(RaycastHit hit)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.StartClick(hit);
		ClickableEnabled = false;
		StopLerp();
		lerpRoutine = ((MonoBehaviour)this).StartCoroutine(Lerp());
		IEnumerator Lerp()
		{
			for (float i = 0f; i < LerpTime; i += Time.deltaTime)
			{
				SetLerpedTransform(Mathf.Lerp(0f, 1f, i / LerpTime));
				yield return (object)new WaitForEndOfFrame();
			}
			SetLerpedTransform(1f);
			lerpRoutine = null;
		}
	}

	public void StopLerp()
	{
		if (lerpRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(lerpRoutine);
		}
	}
}
