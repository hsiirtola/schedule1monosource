using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.AvatarFramework;

public class Eye : MonoBehaviour
{
	[Serializable]
	public struct EyeLidConfiguration
	{
		[Range(0f, 1f)]
		public float topLidOpen;

		[Range(0f, 1f)]
		public float bottomLidOpen;

		public override string ToString()
		{
			return "Top: " + topLidOpen + ", Bottom: " + bottomLidOpen;
		}

		public static EyeLidConfiguration Lerp(EyeLidConfiguration start, EyeLidConfiguration end, float lerp)
		{
			return new EyeLidConfiguration
			{
				topLidOpen = Mathf.Lerp(start.topLidOpen, end.topLidOpen, lerp),
				bottomLidOpen = Mathf.Lerp(start.bottomLidOpen, end.bottomLidOpen, lerp)
			};
		}
	}

	public const float PupilLookSpeed = 10f;

	private static Vector3 defaultScale = new Vector3(0.03f, 0.03f, 0.015f);

	private static Vector3 maxRotation = new Vector3(40f, 30f, 0f);

	private static Vector3 minRotation = new Vector3(-40f, -20f, 0f);

	[Header("References")]
	public Transform Container;

	public Transform TopLidContainer;

	public Transform BottomLidContainer;

	public Transform PupilContainer;

	public MeshRenderer TopLidRend;

	public MeshRenderer BottomLidRend;

	public MeshRenderer EyeBallRend;

	public Transform EyeLookOrigin;

	public OptimizedLight EyeLight;

	public SkinnedMeshRenderer PupilRend;

	private Coroutine blinkRoutine;

	private Coroutine stateRoutine;

	private Avatar avatar;

	private Color defaultEyeColor = Color.white;

	public Vector2 AngleOffset = Vector2.zero;

	public EyeLidConfiguration CurrentConfiguration { get; protected set; }

	public bool IsBlinking => blinkRoutine != null;

	private void Awake()
	{
		avatar = ((Component)this).GetComponentInParent<Avatar>();
		EyeLight.Enabled = false;
	}

	public void SetSize(float size)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Container.localScale = defaultScale * size;
	}

	public void SetLidColor(Color color)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)TopLidRend).material.color = color;
		((Renderer)BottomLidRend).material.color = color;
	}

	public void SetEyeballMaterial(Material mat)
	{
		((Renderer)EyeBallRend).material = mat;
	}

	public void SetEyeballColor(Color col, float emission = 0.115f, bool writeDefault = true)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)EyeBallRend).material.color = col;
		((Renderer)EyeBallRend).material.SetColor("_EmissionColor", col * emission);
		if (writeDefault)
		{
			defaultEyeColor = col;
		}
	}

	public void ResetEyeballColor()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)EyeBallRend).material.color = defaultEyeColor;
		((Renderer)EyeBallRend).material.SetColor("_EmissionColor", defaultEyeColor * 0.115f);
	}

	public void ConfigureEyeLight(Color color, float intensity)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)EyeLight == (Object)null) && !((Object)(object)EyeLight._Light == (Object)null))
		{
			EyeLight._Light.color = color;
			EyeLight._Light.intensity = intensity;
			EyeLight.Enabled = intensity > 0f;
		}
	}

	public void SetDilation(float dil)
	{
		PupilRend.SetBlendShapeWeight(0, dil * 100f);
	}

	public void SetEyeLidState(EyeLidConfiguration config, float time)
	{
		EyeLidConfiguration startConfig = CurrentConfiguration;
		StopExistingRoutines();
		if (Singleton<CoroutineService>.InstanceExists)
		{
			stateRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			for (float i = 0f; i < time; i += Time.deltaTime)
			{
				EyeLidConfiguration config2 = new EyeLidConfiguration
				{
					topLidOpen = Mathf.Lerp(startConfig.topLidOpen, config.topLidOpen, i / time),
					bottomLidOpen = Mathf.Lerp(startConfig.bottomLidOpen, config.bottomLidOpen, i / time)
				};
				SetEyeLidState(config2);
				yield return (object)new WaitForEndOfFrame();
			}
			SetEyeLidState(config);
			stateRoutine = null;
		}
	}

	private void StopExistingRoutines()
	{
		if (blinkRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(blinkRoutine);
			blinkRoutine = null;
		}
		if (stateRoutine != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(stateRoutine);
			stateRoutine = null;
		}
	}

	public void SetEyeLidState(EyeLidConfiguration config, bool debug = false)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)TopLidContainer == (Object)null) && !((Object)(object)BottomLidContainer == (Object)null))
		{
			if (debug)
			{
				EyeLidConfiguration eyeLidConfiguration = config;
				Console.Log("Setting eye lid state: " + eyeLidConfiguration.ToString());
			}
			TopLidContainer.localRotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f), config.topLidOpen);
			BottomLidContainer.localRotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, 0f), Quaternion.Euler(90f, 0f, 0f), config.bottomLidOpen);
			CurrentConfiguration = config;
		}
	}

	public unsafe void LookAt(Vector3 position, bool instant = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = position - EyeLookOrigin.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		normalized = EyeLookOrigin.InverseTransformDirection(normalized);
		normalized.z = Mathf.Clamp(normalized.z, 0.1f, float.MaxValue);
		normalized = EyeLookOrigin.TransformDirection(normalized);
		Vector3 val2 = EyeLookOrigin.InverseTransformDirection(normalized);
		val2.x = 0f;
		val2 = EyeLookOrigin.TransformDirection(val2);
		float num = Vector3.SignedAngle(EyeLookOrigin.forward, val2, EyeLookOrigin.right);
		Vector3 val3 = EyeLookOrigin.InverseTransformDirection(normalized);
		val3.y = 0f;
		val3 = EyeLookOrigin.TransformDirection(val3);
		float num2 = Vector3.SignedAngle(EyeLookOrigin.forward, val3, EyeLookOrigin.up);
		Vector3 val4 = default(Vector3);
		((Vector3)(ref val4))._002Ector(Mathf.Clamp(num + AngleOffset.x, minRotation.y, maxRotation.y), Mathf.Clamp(num2 + AngleOffset.y, minRotation.x, maxRotation.x), 0f);
		if (instant)
		{
			val = val4;
			Debug.Log((object)("instant: " + ((object)(*(Vector3*)(&val))/*cast due to .constrained prefix*/).ToString()));
			PupilContainer.localRotation = Quaternion.Euler(val4);
		}
		else
		{
			PupilContainer.localRotation = Quaternion.Lerp(PupilContainer.localRotation, Quaternion.Euler(val4), Time.deltaTime * 10f);
		}
	}

	public void Blink(float blinkDuration, EyeLidConfiguration endState, bool debug = false)
	{
		StopExistingRoutines();
		if (!((Object)(object)avatar == (Object)null) && !((Object)(object)avatar.EmotionManager == (Object)null) && !avatar.EmotionManager.IsSwitchingEmotion)
		{
			blinkRoutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Routine());
		}
		IEnumerator Routine()
		{
			EyeLidConfiguration start = CurrentConfiguration;
			EyeLidConfiguration end = new EyeLidConfiguration
			{
				bottomLidOpen = 0f,
				topLidOpen = 0f
			};
			float holdTime = blinkDuration * 0.1f;
			float duration = (blinkDuration - holdTime) / 2f;
			for (float i = 0f; i < duration; i += Time.deltaTime)
			{
				EyeLidConfiguration config = new EyeLidConfiguration
				{
					bottomLidOpen = Mathf.Lerp(start.bottomLidOpen, end.bottomLidOpen, i / duration),
					topLidOpen = Mathf.Lerp(start.topLidOpen, end.topLidOpen, i / duration)
				};
				SetEyeLidState(config, debug);
				yield return (object)new WaitForEndOfFrame();
			}
			SetEyeLidState(end, debug);
			yield return (object)new WaitForSeconds(holdTime);
			start = CurrentConfiguration;
			end = new EyeLidConfiguration
			{
				bottomLidOpen = endState.bottomLidOpen,
				topLidOpen = endState.topLidOpen
			};
			for (float i = 0f; i < duration; i += Time.deltaTime)
			{
				EyeLidConfiguration config2 = new EyeLidConfiguration
				{
					bottomLidOpen = Mathf.Lerp(start.bottomLidOpen, end.bottomLidOpen, i / duration),
					topLidOpen = Mathf.Lerp(start.topLidOpen, end.topLidOpen, i / duration)
				};
				SetEyeLidState(config2, debug);
				yield return (object)new WaitForEndOfFrame();
			}
			blinkRoutine = null;
		}
	}
}
