using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.AvatarFramework;

[ExecuteInEditMode]
public class EyeController : MonoBehaviour
{
	private static float eyeHeightMultiplier = 0.03f;

	public bool DEBUG;

	[Header("References")]
	[SerializeField]
	public Eye leftEye;

	[SerializeField]
	public Eye rightEye;

	[Header("Location Settings")]
	[Range(0f, 45f)]
	[SerializeField]
	protected float eyeSpacing = 20f;

	[Range(-1f, 1f)]
	[SerializeField]
	protected float eyeHeight;

	[Range(0.5f, 1.5f)]
	[SerializeField]
	protected float eyeSize = 1f;

	[Header("Eyelid Settings")]
	public Eye.EyeLidConfiguration LeftRestingEyeState;

	public Eye.EyeLidConfiguration RightRestingEyeState;

	[Header("Eyeball Settings")]
	[SerializeField]
	protected Material eyeBallMaterial;

	[Header("Pupil State")]
	[Range(0f, 1f)]
	public float PupilDilation = 0.5f;

	[Header("Blinking Settings")]
	public bool BlinkingEnabled = true;

	[SerializeField]
	[Range(0f, 10f)]
	protected float blinkInterval = 3.5f;

	[SerializeField]
	[Range(0f, 2f)]
	protected float blinkIntervalSpread = 0.5f;

	[SerializeField]
	[Range(0f, 1f)]
	protected float blinkDuration = 0.2f;

	private Avatar avatar;

	private Coroutine blinkRoutine;

	private float timeUntilNextBlink;

	private bool eyeBallTintOverridden;

	private bool eyeLidOverridden;

	private Eye.EyeLidConfiguration defaultLeftEyeRestingState;

	private Eye.EyeLidConfiguration defaultRightEyeRestingState;

	private float defaultDilation = 0.5f;

	private Color defaultEyeballColor = Color.white;

	private Color currentEyeballColor = Color.white;

	public bool EyesOpen { get; protected set; } = true;

	protected virtual void Awake()
	{
		avatar = ((Component)this).GetComponentInParent<Avatar>();
		avatar.onRagdollChange.AddListener((UnityAction<bool, bool, bool>)RagdollChange);
		SetEyesOpen(open: true);
		ApplyDilation();
	}

	protected void Update()
	{
		if (Application.isPlaying)
		{
			if (BlinkingEnabled && blinkRoutine == null)
			{
				blinkRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(BlinkRoutine());
			}
			if (BlinkingEnabled)
			{
				timeUntilNextBlink -= Time.deltaTime;
			}
		}
	}

	private void OnEnable()
	{
		ApplyRestingEyeLidState();
	}

	public void SetEyeballTint(Color col, bool overrideDefault = false)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if (overrideDefault)
		{
			defaultEyeballColor = col;
		}
		leftEye.SetEyeballColor(col);
		rightEye.SetEyeballColor(col);
		currentEyeballColor = col;
	}

	public void ResetEyeballTint()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SetEyeballTint(defaultEyeballColor);
	}

	public void OverrideEyeLids(Eye.EyeLidConfiguration eyeLidConfiguration)
	{
		if (!eyeLidOverridden)
		{
			defaultLeftEyeRestingState = LeftRestingEyeState;
			defaultRightEyeRestingState = RightRestingEyeState;
		}
		LeftRestingEyeState = eyeLidConfiguration;
		RightRestingEyeState = eyeLidConfiguration;
		eyeLidOverridden = true;
	}

	public void ResetEyeLids()
	{
		LeftRestingEyeState = defaultLeftEyeRestingState;
		RightRestingEyeState = defaultRightEyeRestingState;
		eyeLidOverridden = false;
	}

	private void RagdollChange(bool oldValue, bool newValue, bool playStandUpAnim)
	{
		if (newValue)
		{
			ForceBlink();
		}
	}

	public void SetEyesOpen(bool open)
	{
		if (DEBUG)
		{
			Debug.Log((object)("Setting eyes open: " + open));
		}
		EyesOpen = open;
		leftEye.SetEyeLidState(open ? LeftRestingEyeState : new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0f,
			topLidOpen = 0f
		}, 0.1f);
		rightEye.SetEyeLidState(open ? RightRestingEyeState : new Eye.EyeLidConfiguration
		{
			bottomLidOpen = 0f,
			topLidOpen = 0f
		}, 0.1f);
	}

	private void ApplyDilation()
	{
		leftEye.SetDilation(PupilDilation);
		rightEye.SetDilation(PupilDilation);
	}

	public void SetPupilDilation(float dilation, bool writeDefault = true)
	{
		PupilDilation = dilation;
		ApplyDilation();
		if (writeDefault)
		{
			defaultDilation = PupilDilation;
		}
	}

	public void SetEyeballMaterial(Material material)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		leftEye.SetEyeballMaterial(material);
		rightEye.SetEyeballMaterial(material);
		SetEyeballTint(defaultEyeballColor);
	}

	public void ResetEyeballMaterial()
	{
		leftEye.SetEyeballMaterial(eyeBallMaterial);
		rightEye.SetEyeballMaterial(eyeBallMaterial);
	}

	public void ResetPupilDilation()
	{
		SetPupilDilation(defaultDilation);
	}

	private void ApplyRestingEyeLidState()
	{
		leftEye.SetEyeLidState(LeftRestingEyeState);
		rightEye.SetEyeLidState(RightRestingEyeState);
	}

	public void ForceBlink()
	{
		leftEye.Blink(blinkDuration, LeftRestingEyeState);
		rightEye.Blink(blinkDuration, RightRestingEyeState);
		ResetBlinkCounter();
	}

	public void SetLeftEyeRestingLidState(Eye.EyeLidConfiguration config)
	{
		LeftRestingEyeState = config;
		if (!leftEye.IsBlinking)
		{
			leftEye.SetEyeLidState(config);
		}
	}

	public void SetRightEyeRestingLidState(Eye.EyeLidConfiguration config)
	{
		RightRestingEyeState = config;
		if (!rightEye.IsBlinking)
		{
			rightEye.SetEyeLidState(config);
		}
	}

	private IEnumerator BlinkRoutine()
	{
		while (BlinkingEnabled)
		{
			if (EyesOpen)
			{
				if (DEBUG)
				{
					Debug.Log((object)"Blinking");
				}
				leftEye.Blink(blinkDuration, LeftRestingEyeState, DEBUG);
				rightEye.Blink(blinkDuration, RightRestingEyeState, DEBUG);
			}
			ResetBlinkCounter();
			yield return (object)new WaitUntil((Func<bool>)(() => timeUntilNextBlink <= 0f));
		}
		blinkRoutine = null;
	}

	private void ResetBlinkCounter()
	{
		timeUntilNextBlink = Random.Range(Mathf.Clamp(blinkInterval - blinkIntervalSpread, blinkDuration, float.MaxValue), blinkInterval + blinkIntervalSpread);
	}

	public void LookAt(Vector3 position, bool instant = false)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		_ = DEBUG;
		leftEye.LookAt(position, instant);
		rightEye.LookAt(position, instant);
	}
}
