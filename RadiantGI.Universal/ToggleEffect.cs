using UnityEngine;
using UnityEngine.Rendering;

namespace RadiantGI.Universal;

public class ToggleEffect : MonoBehaviour
{
	public VolumeProfile profile;

	private RadiantGlobalIllumination radiant;

	private void Start()
	{
		profile.TryGet<RadiantGlobalIllumination>(ref radiant);
	}

	private void Update()
	{
		if (Input.GetKeyDown((KeyCode)32))
		{
			((VolumeComponent)radiant).active = !((VolumeComponent)radiant).active;
		}
	}
}
