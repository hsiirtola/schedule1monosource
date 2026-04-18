using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Growing;

public class GrowContainerSurfaceCover : MonoBehaviour
{
	public const int TextureSize = 128;

	public const int PourRadius = 32;

	public const int UpdatesPerSecond = 24;

	public const float CoveredPixelThreshold = 0.8f;

	public const float Delay = 0.35f;

	[Header("Settings")]
	public float SuccessfulCoverageThreshold = 0.825f;

	[Header("References")]
	public GrowContainer GrowContainer;

	public MeshRenderer MeshRenderer;

	public Texture2D PourMask;

	[Header("Pour Over time Settings")]
	[SerializeField]
	private float _applyPoutOverTimeDuration = 1f;

	[SerializeField]
	private AnimationCurve _applyPoutOverTimeCurve;

	public UnityEvent onSufficientCoverage;

	private bool queued;

	private Vector3 queuedWorldPos = Vector3.zero;

	private Texture2D mainTex;

	private Texture2D tempTex;

	private Vector3 relative;

	private Vector2 vector2;

	private Vector2 normalizedOffset;

	private Vector2 originPixel;

	private float _pourApplicationStrength = 1f;

	public float CurrentCoverage { get; private set; }

	public float PourApplicationStrength
	{
		get
		{
			return _pourApplicationStrength;
		}
		set
		{
			_pourApplicationStrength = value;
		}
	}

	public bool UseApplyOverTime { get; set; }

	private float _sideLength => GrowContainer.GetGrowSurfaceSideLength();

	private void Awake()
	{
	}

	private void OnEnable()
	{
		((MonoBehaviour)this).StartCoroutine(CheckQueue());
	}

	public void ConfigureAppearance(Color col, float transparency)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)MeshRenderer).material.SetColor("_MainColor", col);
		((Renderer)MeshRenderer).material.SetFloat("_Transparency", transparency);
	}

	public void Reset()
	{
		Blank();
		CurrentCoverage = 0f;
		UseApplyOverTime = false;
		_pourApplicationStrength = 1f;
		queued = false;
	}

	public void QueuePour(Vector3 worldSpacePosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		queued = true;
		queuedWorldPos = worldSpacePosition;
	}

	public float GetNormalizedProgress()
	{
		return CurrentCoverage / SuccessfulCoverageThreshold;
	}

	private IEnumerator CheckQueue()
	{
		while ((Object)(object)((Component)this).gameObject != (Object)null)
		{
			if (queued)
			{
				queued = false;
				if (!UseApplyOverTime)
				{
					DelayedApplyPour(queuedWorldPos);
				}
				else
				{
					ApplyPour(queuedWorldPos, applyOverTime: true);
				}
			}
			yield return (object)new WaitForSeconds(1f / 24f);
		}
	}

	private void Blank()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(128, 128);
		tempTex = new Texture2D(128, 128);
		Color[] array = (Color[])(object)new Color[16384];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Color.black;
		}
		tempTex.SetPixels(array);
		tempTex.Apply();
		val.SetPixels(array);
		val.Apply();
		((Renderer)MeshRenderer).material.mainTexture = (Texture)(object)val;
		mainTex = val;
		((Renderer)MeshRenderer).material.SetTexture("_TempMask", (Texture)(object)tempTex);
	}

	private void DelayedApplyPour(Vector3 worldSpace)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviour)this).StartCoroutine(Routine());
		IEnumerator Routine()
		{
			yield return (object)new WaitForSeconds(0.35f);
			ApplyPour(worldSpace);
		}
	}

	private void ApplyPour(Vector3 worldSpace, bool applyOverTime = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		relative = ((Component)this).transform.InverseTransformPoint(worldSpace);
		vector2 = new Vector2(relative.x, relative.z);
		normalizedOffset = new Vector2(vector2.x / (_sideLength * 0.5f), vector2.y / (_sideLength * 0.5f));
		originPixel = new Vector2(64f * (1f + normalizedOffset.x), 64f * (1f + normalizedOffset.y));
		if (!applyOverTime)
		{
			for (int i = 0; i < 64; i++)
			{
				for (int j = 0; j < 64; j++)
				{
					int num = (int)originPixel.x - 32 + i;
					int num2 = (int)originPixel.y - 32 + j;
					if (num >= 0 && num < 128 && num2 >= 0 && num2 < 128)
					{
						Color pixel = mainTex.GetPixel(num, num2);
						pixel.r += GetPourMaskValue(i, j);
						pixel.g = pixel.r;
						pixel.b = pixel.r;
						pixel.a = 1f;
						mainTex.SetPixel(num, num2, pixel);
					}
				}
			}
			mainTex.Apply();
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(ApplyPourOverTime());
		}
		float currentCoverage = CurrentCoverage;
		float num3 = (CurrentCoverage = GetCoverage());
		if (num3 >= SuccessfulCoverageThreshold && currentCoverage < SuccessfulCoverageThreshold && onSufficientCoverage != null)
		{
			onSufficientCoverage.Invoke();
		}
	}

	private IEnumerator ApplyPourOverTime()
	{
		Vector2 val = -originPixel / 128f;
		((Renderer)MeshRenderer).material.SetVector("_ExpandingMaskUVOffset", Vector4.op_Implicit(val));
		Color[] pixels = mainTex.GetPixels();
		for (int i = 0; i < 64; i++)
		{
			for (int j = 0; j < 64; j++)
			{
				int num = (int)originPixel.x - 32 + i;
				int num2 = (int)originPixel.y - 32 + j;
				if (num >= 0 && num < 128 && num2 >= 0 && num2 < 128)
				{
					Color val2 = pixels[num2 * 128 + num];
					val2.r += GetPourMaskValue(i, j);
					val2.g = val2.r;
					val2.b = val2.r;
					val2.a = 1f;
					pixels[num2 * 128 + num] = val2;
				}
			}
		}
		tempTex.SetPixels(pixels);
		tempTex.Apply();
		float elapasedTime = 0f;
		while (elapasedTime < _applyPoutOverTimeDuration)
		{
			elapasedTime += Time.deltaTime;
			float num3 = elapasedTime / _applyPoutOverTimeDuration;
			((Renderer)MeshRenderer).material.SetFloat("_ExpandingMaskPercentage", _applyPoutOverTimeCurve.Evaluate(num3));
			yield return null;
		}
		((Renderer)MeshRenderer).material.SetFloat("_ExpandingMaskPercentage", 0f);
		mainTex.SetPixels(pixels);
		mainTex.Apply();
	}

	private float GetPourMaskValue(int x, int y)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Color pixel = PourMask.GetPixel(x, y);
		float num = ((Color)(ref pixel)).grayscale * PourApplicationStrength;
		Mathf.Clamp01(num);
		return num;
	}

	private float GetCoverage()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		int num = 16384;
		int num2 = 0;
		for (int i = 0; i < 128; i++)
		{
			for (int j = 0; j < 128; j++)
			{
				if (mainTex.GetPixel(i, j).r > 0.8f)
				{
					num2++;
				}
			}
		}
		return Mathf.Clamp01((float)num2 / (float)num);
	}
}
