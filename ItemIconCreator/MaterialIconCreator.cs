using System;
using System.Collections;
using UnityEngine;

namespace ItemIconCreator;

[ExecuteInEditMode]
public class MaterialIconCreator : IconCreator
{
	public Renderer targetRenderer;

	public Material[] materials;

	public override void BuildIcons()
	{
		((MonoBehaviour)this).StartCoroutine(BuildIconsRotine());
	}

	public override bool CheckConditions()
	{
		if (!base.CheckConditions())
		{
			return false;
		}
		if (materials.Length == 0)
		{
			Debug.LogError((object)"There's no materials");
			return false;
		}
		if ((Object)(object)targetRenderer == (Object)null)
		{
			Debug.LogError((object)"There's no target renderer");
			return false;
		}
		return true;
	}

	private IEnumerator BuildIconsRotine()
	{
		Initialize();
		if (dynamicFov)
		{
			UpdateFOV(((Component)targetRenderer).gameObject);
		}
		if (lookAtObjectCenter)
		{
			LookAtTargetCenter(((Component)targetRenderer).gameObject);
		}
		currentObject = ((Component)targetRenderer).transform;
		yield return CaptureFrame(((Object)targetRenderer).name, 0);
		for (int i = 0; i < materials.Length; i++)
		{
			targetRenderer.material = materials[i];
			targetRenderer.materials[0] = materials[i];
			if ((Object)(object)IconCreatorCanvas.instance != (Object)null)
			{
				IconCreatorCanvas.instance.SetInfo(materials.Length, i, ((Object)materials[i]).name, isRecording: true, nextIconKey);
			}
			if ((Object)(object)whiteCam != (Object)null)
			{
				((Behaviour)whiteCam).enabled = false;
			}
			if ((Object)(object)whiteCam != (Object)null)
			{
				((Behaviour)blackCam).enabled = false;
			}
			if (mode == Mode.Manual)
			{
				CanMove = true;
				yield return (object)new WaitUntil((Func<bool>)(() => Input.GetKeyDown(nextIconKey)));
				CanMove = false;
			}
			if ((Object)(object)IconCreatorCanvas.instance != (Object)null)
			{
				IconCreatorCanvas.instance.SetTakingPicture();
				yield return null;
				yield return null;
				yield return null;
			}
			yield return CaptureFrame(((Object)materials[i]).name, i);
		}
		if ((Object)(object)IconCreatorCanvas.instance != (Object)null)
		{
			IconCreatorCanvas.instance.SetInfo(0, 0, "", isRecording: false, nextIconKey);
		}
		RevealInFinder();
		DeleteCameras();
	}

	private void Reset()
	{
		targetRenderer = null;
		materials = (Material[])(object)new Material[0];
	}

	protected override void Update()
	{
		if (preview && !isCreatingIcons)
		{
			if ((Object)(object)targetRenderer != (Object)null)
			{
				if (dynamicFov)
				{
					UpdateFOV(((Component)targetRenderer).gameObject);
				}
				if (lookAtObjectCenter)
				{
					LookAtTargetCenter(((Component)targetRenderer).gameObject);
				}
			}
		}
		else
		{
			base.Update();
		}
	}
}
