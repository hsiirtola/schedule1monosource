using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone;

public class Phone : PlayerSingleton<Phone>
{
	public static GameObject ActiveApp;

	[Header("References")]
	[SerializeField]
	protected GameObject phoneModel;

	[SerializeField]
	protected Transform orientation_Vertical;

	[SerializeField]
	protected Transform orientation_Horizontal;

	[SerializeField]
	protected GraphicRaycaster raycaster;

	[SerializeField]
	protected GameObject PhoneFlashlight;

	[SerializeField]
	protected AudioSourceController FlashlightToggleSound;

	[Header("Settings")]
	public float rotationTime = 0.1f;

	public float LookOffsetMax = 0.45f;

	public float LookOffsetMin = 0.29f;

	public float OpenVerticalOffset = 0.1f;

	[Header("Fonts")]
	[SerializeField]
	private ColorFont _generalColorFont;

	[SerializeField]
	private ColorFont _productColorFont;

	public Action onPhoneOpened;

	public Action onPhoneClosed;

	public Action closeApps;

	private EventSystem eventSystem;

	private VisibilityAttribute flashlightVisibility;

	private Coroutine rotationCoroutine;

	private Coroutine lookOffsetCoroutine;

	public bool IsOpen { get; protected set; }

	public bool isHorizontal { get; protected set; }

	public bool isOpenable { get; protected set; } = true;

	public bool FlashlightOn { get; protected set; }

	public float ScaledLookOffset => Mathf.Lerp(LookOffsetMax, LookOffsetMin, CanvasScaler.NormalizedCanvasScaleFactor);

	public ColorFont GeneralColorFont => _generalColorFont;

	protected override void Awake()
	{
		base.Awake();
		eventSystem = EventSystem.current;
	}

	public override void OnStartClient(bool IsOwner)
	{
		base.OnStartClient(IsOwner);
		if (!IsOwner)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	protected override void Start()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.Start();
		if (flashlightVisibility == null)
		{
			flashlightVisibility = new VisibilityAttribute("Flashlight", 0f);
		}
		((Component)this).transform.localRotation = orientation_Vertical.localRotation;
	}

	protected virtual void Update()
	{
		if (!GameInput.IsTyping && !Singleton<PauseMenu>.Instance.IsPaused && (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount == 0 || IsOpen) && !Player.Local.IsInVehicle)
		{
			if (GameInput.GetCurrentInputDeviceIsGamepad() && PlayerSingleton<Phone>.Instance.IsOpen)
			{
				return;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.ToggleFlashlight))
			{
				ToggleFlashlight();
			}
		}
		PhoneFlashlight.SetActive(FlashlightOn && !Player.Local.IsInVehicle);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ActiveApp = null;
	}

	private void ToggleFlashlight()
	{
		FlashlightOn = !FlashlightOn;
		FlashlightToggleSound.PitchMultiplier = (FlashlightOn ? 1f : 0.9f);
		FlashlightToggleSound.Play();
		flashlightVisibility.pointsChange = (FlashlightOn ? 10f : 0f);
		flashlightVisibility.multiplier = (FlashlightOn ? 1.5f : 1f);
		Player.Local.SetFlashlightOn_Server(FlashlightOn);
	}

	public void SetOpenable(bool o)
	{
		isOpenable = o;
	}

	public void SetIsOpen(bool o)
	{
		IsOpen = o;
		if (IsOpen)
		{
			if (onPhoneOpened != null)
			{
				onPhoneOpened();
			}
			if ((Object)(object)ActiveApp == (Object)null)
			{
				SetLookOffsetMultiplier(1f);
			}
		}
		else if (onPhoneClosed != null)
		{
			onPhoneClosed();
		}
	}

	public void SetIsHorizontal(bool h)
	{
		isHorizontal = h;
		if (rotationCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(rotationCoroutine);
		}
		rotationCoroutine = ((MonoBehaviour)this).StartCoroutine(SetIsHorizontal_Process(h));
	}

	protected IEnumerator SetIsHorizontal_Process(bool h)
	{
		float adjustedRotationTime = rotationTime;
		Quaternion startRotation = ((Component)this).transform.localRotation;
		_ = Quaternion.identity;
		Quaternion endRotation;
		if (h)
		{
			endRotation = orientation_Horizontal.localRotation;
			adjustedRotationTime *= Quaternion.Angle(((Component)this).transform.localRotation, orientation_Horizontal.localRotation) / 90f;
		}
		else
		{
			endRotation = orientation_Vertical.localRotation;
			adjustedRotationTime *= Quaternion.Angle(((Component)this).transform.localRotation, orientation_Vertical.localRotation) / 90f;
		}
		for (float i = 0f; i < adjustedRotationTime; i += Time.deltaTime)
		{
			((Component)this).transform.localRotation = Quaternion.Lerp(startRotation, endRotation, i / adjustedRotationTime);
			yield return (object)new WaitForEndOfFrame();
		}
		((Component)this).transform.localRotation = endRotation;
		rotationCoroutine = null;
	}

	public void SetLookOffsetMultiplier(float multiplier)
	{
		float lookOffset_Process = ScaledLookOffset * multiplier;
		if (lookOffsetCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(lookOffsetCoroutine);
		}
		lookOffsetCoroutine = ((MonoBehaviour)this).StartCoroutine(SetLookOffset_Process(lookOffset_Process));
	}

	public void RequestCloseApp()
	{
		if ((Object)(object)ActiveApp != (Object)null && closeApps != null)
		{
			closeApps();
		}
	}

	protected IEnumerator SetLookOffset_Process(float lookOffset)
	{
		float startOffset = ((Component)this).transform.localPosition.z;
		float moveTime = 0.1f;
		for (float i = 0f; i < moveTime; i += Time.deltaTime)
		{
			((Component)this).transform.localPosition = new Vector3(((Component)this).transform.localPosition.x, ((Component)this).transform.localPosition.y, Mathf.Lerp(startOffset, lookOffset, i / moveTime));
			yield return (object)new WaitForEndOfFrame();
		}
		((Component)this).transform.localPosition = new Vector3(((Component)this).transform.localPosition.x, ((Component)this).transform.localPosition.y, lookOffset);
		rotationCoroutine = null;
	}

	public bool MouseRaycast(out RaycastResult result)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		PointerEventData val = new PointerEventData(eventSystem);
		val.position = Vector2.op_Implicit(GameInput.MousePosition);
		List<RaycastResult> list = new List<RaycastResult>();
		((BaseRaycaster)raycaster).Raycast(val, list);
		if (list.Count > 0)
		{
			result = list[0];
		}
		else
		{
			result = default(RaycastResult);
		}
		return list.Count > 0;
	}
}
