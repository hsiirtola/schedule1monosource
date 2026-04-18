using UnityEngine;
using UnityEngine.UI;

namespace VolumetricFogAndMist2.Demos;

public class DemoSceneControls : MonoBehaviour
{
	public VolumetricFogProfile[] profiles;

	public VolumetricFog fogVolume;

	public Text presetNameDisplay;

	private int index;

	private void Start()
	{
		SetProfile(index);
	}

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)102))
		{
			index++;
			if (index >= profiles.Length)
			{
				index = 0;
			}
			SetProfile(index);
		}
		if (Input.GetKeyDown((KeyCode)116))
		{
			((Component)fogVolume).gameObject.SetActive(!((Component)fogVolume).gameObject.activeSelf);
		}
	}

	private void SetProfile(int profileIndex)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (profileIndex < 2)
		{
			((Component)fogVolume).transform.position = Vector3.up * 25f;
		}
		else
		{
			((Component)fogVolume).transform.position = Vector3.zero;
		}
		fogVolume.profile = profiles[profileIndex];
		presetNameDisplay.text = "Current fog preset: " + ((Object)profiles[profileIndex]).name;
		fogVolume.UpdateMaterialPropertiesNow(false, false);
	}
}
