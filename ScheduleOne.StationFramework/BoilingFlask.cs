using ScheduleOne.Audio;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.StationFramework;

public class BoilingFlask : Fillable
{
	public const float TEMPERATURE_MAX = 500f;

	public float TEMPERATURE_MAX_VELOCITY = 200f;

	public float TEMPERATURE_ACCELERATION = 50f;

	public const float OVERHEAT_TIME = 1.25f;

	public bool LockTemperature;

	public AnimationCurve BoilSoundPitchCurve;

	public float LabelJitterScale = 1f;

	[Header("References")]
	public BunsenBurner Burner;

	public Canvas TemperatureCanvas;

	public TextMeshProUGUI TemperatureLabel;

	public Slider TemperatureSlider;

	public RectTransform TemperatureRangeIndicator;

	public ParticleSystem SmokeParticles;

	public AudioSourceController BoilSound;

	public MeshRenderer OverheatMesh;

	public float CurrentTemperature { get; private set; }

	public float CurrentTemperatureVelocity { get; private set; }

	public bool IsTemperatureInRange
	{
		get
		{
			if ((Object)(object)Recipe != (Object)null)
			{
				if (CurrentTemperature >= Recipe.CookTemperatureLowerBound)
				{
					return CurrentTemperature <= Recipe.CookTemperatureUpperBound;
				}
				return false;
			}
			return false;
		}
	}

	public float OverheatScale { get; private set; }

	public StationRecipe Recipe { get; private set; }

	public void Update()
	{
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Burner == (Object)null)
		{
			return;
		}
		if (!LockTemperature)
		{
			float num = Burner.CurrentHeat - CurrentTemperature / 500f;
			CurrentTemperatureVelocity = Mathf.MoveTowards(CurrentTemperatureVelocity, num * TEMPERATURE_MAX_VELOCITY, TEMPERATURE_ACCELERATION * Time.deltaTime);
			CurrentTemperature = Mathf.Clamp(CurrentTemperature + CurrentTemperatureVelocity * Time.deltaTime, 0f, 500f);
		}
		if (CurrentTemperature > 0f)
		{
			BoilSound.VolumeMultiplier = Mathf.Clamp01(CurrentTemperature / 500f);
			BoilSound.PitchMultiplier = BoilSoundPitchCurve.Evaluate(Mathf.Clamp01(CurrentTemperature / 500f));
			if (!BoilSound.IsPlaying)
			{
				BoilSound.Play();
			}
		}
		else
		{
			BoilSound.Stop();
		}
		if ((Object)(object)Recipe != (Object)null && CurrentTemperature >= Recipe.CookTemperatureUpperBound)
		{
			float num2 = Mathf.Clamp((CurrentTemperature - Recipe.CookTemperatureUpperBound) / (500f - Recipe.CookTemperatureUpperBound), 0.25f, 1f);
			OverheatScale += num2 * Time.deltaTime / 1.25f;
		}
		else
		{
			OverheatScale = Mathf.MoveTowards(OverheatScale, 0f, Time.deltaTime / 1.25f);
		}
		if (OverheatScale > 0f)
		{
			((Renderer)OverheatMesh).material.color = new Color(1f, 1f, 1f, Mathf.Pow(OverheatScale, 2f));
			((Renderer)OverheatMesh).enabled = true;
		}
		else
		{
			((Renderer)OverheatMesh).enabled = false;
		}
	}

	private void FixedUpdate()
	{
		UpdateCanvas();
		UpdateSmoke();
	}

	private void UpdateCanvas()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)TemperatureCanvas).gameObject.activeSelf)
		{
			((TMP_Text)TemperatureLabel).text = Mathf.RoundToInt(CurrentTemperature) + "°C";
			if (CurrentTemperature < Recipe.CookTemperatureLowerBound)
			{
				((Graphic)TemperatureLabel).color = Color.white;
			}
			else if (CurrentTemperature > Recipe.CookTemperatureUpperBound)
			{
				((Graphic)TemperatureLabel).color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)90, (byte)90, byte.MaxValue));
			}
			else
			{
				((Graphic)TemperatureLabel).color = Color.green;
			}
			TemperatureSlider.value = CurrentTemperature / 500f;
			if (OverheatScale > 0f)
			{
				((TMP_Text)TemperatureLabel).transform.localPosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * Mathf.Clamp(OverheatScale, 0.3f, 1f) * LabelJitterScale;
			}
			else
			{
				((TMP_Text)TemperatureLabel).transform.localPosition = Vector3.zero;
			}
		}
	}

	private void UpdateSmoke()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentTemperature < 1f)
		{
			if (SmokeParticles.isPlaying)
			{
				SmokeParticles.Stop();
			}
			return;
		}
		MainModule main = SmokeParticles.main;
		((MainModule)(ref main)).simulationSpeed = Mathf.Lerp(1f, 3f, CurrentTemperature / 500f);
		((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, CurrentTemperature / 500f)));
		if (!SmokeParticles.isPlaying)
		{
			SmokeParticles.Play();
		}
	}

	public void SetCanvasVisible(bool visible)
	{
		((Component)TemperatureCanvas).gameObject.SetActive(visible);
	}

	public void SetTemperature(float temp)
	{
		CurrentTemperature = temp;
	}

	public void SetRecipe(StationRecipe recipe)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Recipe = recipe;
		if (!((Object)(object)recipe == (Object)null))
		{
			float num = Recipe.CookTemperatureLowerBound / 500f;
			float num2 = Recipe.CookTemperatureUpperBound / 500f;
			TemperatureRangeIndicator.anchorMin = new Vector2(num, TemperatureRangeIndicator.anchorMin.y);
			TemperatureRangeIndicator.anchorMax = new Vector2(num2, TemperatureRangeIndicator.anchorMax.y);
		}
	}
}
