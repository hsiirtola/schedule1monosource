using ScheduleOne.Audio;
using ScheduleOne.PlayerTasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class BunsenBurner : MonoBehaviour
{
	public bool LockDial;

	[Header("Settings")]
	public Gradient FlameColor;

	public AnimationCurve LightIntensity;

	public float HandleRotationSpeed = 1f;

	public AnimationCurve FlamePitch;

	[Header("References")]
	public ParticleSystem Flame;

	public Light Light;

	public Transform Handle;

	public Clickable HandleClickable;

	public Transform Handle_Min;

	public Transform Handle_Max;

	public Transform Highlight;

	public Animation Anim;

	public AudioSourceController FlameSound;

	public bool Interactable { get; private set; }

	public bool IsDialHeld { get; private set; }

	public float CurrentDialValue { get; private set; }

	public float CurrentHeat { get; private set; }

	private void Start()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		SetInteractable(e: false);
		HandleClickable.onClickStart.AddListener((UnityAction<RaycastHit>)ClickStart);
		HandleClickable.onClickEnd.AddListener(new UnityAction(ClickEnd));
	}

	private void Update()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (!LockDial)
		{
			if (IsDialHeld)
			{
				CurrentDialValue = Mathf.Clamp01(CurrentDialValue + HandleRotationSpeed * Time.deltaTime);
			}
			else
			{
				CurrentDialValue = Mathf.Clamp01(CurrentDialValue - HandleRotationSpeed * Time.deltaTime);
			}
			Handle.localRotation = Quaternion.Lerp(Handle_Min.localRotation, Handle_Max.localRotation, CurrentDialValue);
		}
		CurrentHeat = CurrentDialValue;
		((Component)Highlight).gameObject.SetActive(Interactable && !IsDialHeld);
		if (CurrentHeat > 0f)
		{
			FlameSound.VolumeMultiplier = CurrentHeat;
			FlameSound.PitchMultiplier = FlamePitch.Evaluate(CurrentHeat);
			if (!FlameSound.IsPlaying)
			{
				FlameSound.Play();
			}
		}
		else if (FlameSound.IsPlaying)
		{
			FlameSound.Stop();
		}
		UpdateEffects();
	}

	private void UpdateEffects()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentHeat > 0f)
		{
			if (!Flame.isPlaying)
			{
				Flame.Play();
			}
			((Component)Light).gameObject.SetActive(true);
			Flame.startColor = FlameColor.Evaluate(CurrentHeat);
			Light.color = Flame.startColor;
			Light.intensity = LightIntensity.Evaluate(CurrentHeat);
		}
		else
		{
			if (Flame.isPlaying)
			{
				Flame.Stop();
			}
			((Component)Light).gameObject.SetActive(false);
		}
	}

	public void SetDialPosition(float pos)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		CurrentDialValue = Mathf.Clamp01(pos);
		Handle.localRotation = Quaternion.Lerp(Handle_Min.localRotation, Handle_Max.localRotation, CurrentDialValue);
	}

	public void SetInteractable(bool e)
	{
		Interactable = e;
		HandleClickable.ClickableEnabled = e;
		if (!Interactable)
		{
			IsDialHeld = false;
		}
		if (Interactable)
		{
			Anim.Play();
		}
		else
		{
			Anim.Stop();
		}
	}

	public void ClickStart(RaycastHit hit)
	{
		IsDialHeld = true;
	}

	public void ClickEnd()
	{
		IsDialHeld = false;
	}
}
