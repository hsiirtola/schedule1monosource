using Beautify.Universal;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beautify.Demos;

public class ToggleDoF : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			bool value = ((VolumeParameter<bool>)(object)BeautifySettings.settings.depthOfField).value;
			((VolumeParameter<bool>)(object)BeautifySettings.settings.depthOfField).Override(!value);
		}
	}
}
