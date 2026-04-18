using Beautify.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Beautify.Demos;

public class Demo : MonoBehaviour
{
	public Texture lutTexture;

	private void Start()
	{
		UpdateText();
	}

	private void Update()
	{
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown((KeyCode)106))
		{
			FloatParameter bloomIntensity = BeautifySettings.settings.bloomIntensity;
			((VolumeParameter<float>)(object)bloomIntensity).value = ((VolumeParameter<float>)(object)bloomIntensity).value + 0.1f;
		}
		if (Input.GetKeyDown((KeyCode)116) || Input.GetMouseButtonDown(0))
		{
			((VolumeParameter<bool>)(object)BeautifySettings.settings.disabled).value = !((VolumeParameter<bool>)(object)BeautifySettings.settings.disabled).value;
			UpdateText();
		}
		if (Input.GetKeyDown((KeyCode)98))
		{
			BeautifySettings.Blink(0.2f, 1f);
		}
		if (Input.GetKeyDown((KeyCode)99))
		{
			((VolumeParameter<bool>)(object)BeautifySettings.settings.compareMode).value = !((VolumeParameter<bool>)(object)BeautifySettings.settings.compareMode).value;
		}
		if (Input.GetKeyDown((KeyCode)110))
		{
			((VolumeParameter<bool>)(object)BeautifySettings.settings.nightVision).Override(!((VolumeParameter<bool>)(object)BeautifySettings.settings.nightVision).value);
		}
		if (Input.GetKeyDown((KeyCode)102))
		{
			if (((VolumeParameter)BeautifySettings.settings.blurIntensity).overrideState)
			{
				((VolumeParameter)BeautifySettings.settings.blurIntensity).overrideState = false;
			}
			else
			{
				((VolumeParameter<float>)(object)BeautifySettings.settings.blurIntensity).Override(4f);
			}
		}
		if (Input.GetKeyDown((KeyCode)49))
		{
			((VolumeParameter<float>)(object)BeautifySettings.settings.brightness).Override(0.1f);
		}
		if (Input.GetKeyDown((KeyCode)50))
		{
			((VolumeParameter<float>)(object)BeautifySettings.settings.brightness).Override(0.5f);
		}
		if (Input.GetKeyDown((KeyCode)51))
		{
			((VolumeParameter)BeautifySettings.settings.brightness).overrideState = false;
		}
		if (Input.GetKeyDown((KeyCode)52))
		{
			((VolumeParameter<bool>)(object)BeautifySettings.settings.outline).Override(true);
			((VolumeParameter<Color>)(object)BeautifySettings.settings.outlineColor).Override(Color.cyan);
			((VolumeParameter<bool>)(object)BeautifySettings.settings.outlineCustomize).Override(true);
			((VolumeParameter<float>)(object)BeautifySettings.settings.outlineSpread).Override(1.5f);
		}
		if (Input.GetKeyDown((KeyCode)53))
		{
			((VolumeParameter)BeautifySettings.settings.outline).overrideState = false;
		}
		if (Input.GetKeyDown((KeyCode)54))
		{
			((VolumeParameter<bool>)(object)BeautifySettings.settings.lut).Override(true);
			((VolumeParameter<float>)(object)BeautifySettings.settings.lutIntensity).Override(1f);
			((VolumeParameter<Texture>)(object)BeautifySettings.settings.lutTexture).Override(lutTexture);
		}
		if (Input.GetKeyDown((KeyCode)55))
		{
			((VolumeParameter<bool>)(object)BeautifySettings.settings.lut).Override(false);
		}
	}

	private void UpdateText()
	{
		if (((VolumeParameter<bool>)(object)BeautifySettings.settings.disabled).value)
		{
			GameObject.Find("Beautify").GetComponent<Text>().text = "Beautify OFF";
		}
		else
		{
			GameObject.Find("Beautify").GetComponent<Text>().text = "Beautify ON";
		}
	}
}
