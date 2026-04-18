using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.PlayerTasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts.Soil;

public class PourableSoil : Pourable
{
	public const float TEAR_ANGLE = 10f;

	public const float HIGHLIGHT_CYCLE_TIME = 5f;

	public bool IsOpen;

	public SoilDefinition SoilDefinition;

	[Header("References")]
	public Transform SoilBag;

	public Transform[] Bones;

	public List<Collider> TopColliders;

	public MeshRenderer[] Highlights;

	public Transform TopParent;

	public AudioSourceController SnipSound;

	public SkinnedMeshRenderer TopMesh;

	public UnityEvent onOpened;

	private Vector3 highlightScale = Vector3.zero;

	private float timeSinceStart;

	public int currentCut { get; protected set; }

	protected override void Awake()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		highlightScale = ((Component)Highlights[0]).transform.localScale;
		UpdateHighlights();
		ClickableEnabled = false;
	}

	protected override void Update()
	{
		base.Update();
		timeSinceStart += Time.deltaTime;
		UpdateHighlights();
	}

	private void UpdateHighlights()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Highlights[0] == (Object)null)
		{
			return;
		}
		for (int i = 0; i < Highlights.Length; i++)
		{
			if (IsOpen || i < currentCut)
			{
				((Component)Highlights[i]).gameObject.SetActive(false);
				continue;
			}
			float num = (float)i / (float)Highlights.Length;
			float num2 = Mathf.Sin(Mathf.Clamp(timeSinceStart * 5f - num, 0f, float.MaxValue)) + 1f;
			((Component)Highlights[i]).transform.localScale = new Vector3(highlightScale.x * num2, highlightScale.y, highlightScale.z * num2);
		}
	}

	protected override void PourAmount(float amount)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		base.PourAmount(amount);
		SoilBag.localScale = new Vector3(1f, Mathf.Lerp(0.45f, 1f, base.CurrentQuantity / StartQuantity), 1f);
		if (IsPourPointOverPot())
		{
			TargetGrowContainer.ChangeSoilAmount(amount);
		}
		if (TargetGrowContainer.NormalizedSoilAmount >= 1f)
		{
			Singleton<TaskManager>.Instance.currentTask.Success();
		}
	}

	protected override bool CanPour()
	{
		if (!IsOpen)
		{
			return false;
		}
		return base.CanPour();
	}

	public void Cut()
	{
		TopColliders[currentCut].enabled = false;
		LerpCut(currentCut);
		if (currentCut == Bones.Length - 1)
		{
			FinishCut();
		}
		SnipSound.PitchMultiplier = 0.9f + (float)currentCut * 0.05f;
		SnipSound.PlayOneShot();
		currentCut++;
	}

	private void FinishCut()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		IsOpen = true;
		Rigidbody obj = ((Component)TopParent).gameObject.AddComponent<Rigidbody>();
		((Component)TopParent).transform.SetParent((Transform)null);
		obj.interpolation = (RigidbodyInterpolation)1;
		obj.AddRelativeForce(Vector3.forward * 1.5f, (ForceMode)2);
		obj.AddRelativeForce(Vector3.up * 0.3f, (ForceMode)2);
		obj.AddTorque(Vector3.up * 1.5f, (ForceMode)2);
		ClickableEnabled = true;
		if (onOpened != null)
		{
			onOpened.Invoke();
		}
		Object.Destroy((Object)(object)((Component)TopParent).gameObject, 3f);
		Object.Destroy((Object)(object)((Component)TopMesh).gameObject, 3f);
	}

	private void LerpCut(int cutIndex)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Transform bone = Bones[cutIndex];
		Quaternion startRot = bone.localRotation;
		Quaternion endRot = bone.localRotation * Quaternion.Euler(0f, 0f, 10f);
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			float lerpTime = 0.1f;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				bone.localRotation = Quaternion.Lerp(startRot, endRot, i / lerpTime);
				yield return (object)new WaitForEndOfFrame();
			}
			bone.localRotation = endRot;
		}
	}
}
