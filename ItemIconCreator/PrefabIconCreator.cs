using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItemIconCreator;

[ExecuteInEditMode]
public class PrefabIconCreator : IconCreator
{
	[Header("Items")]
	public GameObject[] itemsToShot;

	public Transform itemPosition;

	private GameObject instantiatedItem;

	public override void BuildIcons()
	{
		((MonoBehaviour)this).StartCoroutine(BuildAllIcons());
	}

	public override bool CheckConditions()
	{
		if (!base.CheckConditions())
		{
			return false;
		}
		if (itemsToShot.Length == 0)
		{
			Debug.LogError((object)"There's no prefab to shoot");
			return false;
		}
		if ((Object)(object)itemPosition == (Object)null)
		{
			Debug.LogError((object)"Item position is null");
			return false;
		}
		return true;
	}

	protected override void Update()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		if (preview && !isCreatingIcons)
		{
			if ((Object)(object)instantiatedItem != (Object)null)
			{
				if (dynamicFov)
				{
					UpdateFOV(instantiatedItem);
				}
				if (lookAtObjectCenter)
				{
					LookAtTargetCenter(instantiatedItem);
				}
				instantiatedItem.transform.position = ((Component)itemPosition).transform.position;
				instantiatedItem.transform.rotation = ((Component)itemPosition).transform.rotation;
			}
			else if ((Object)(object)instantiatedItem == (Object)null && itemsToShot.Length != 0)
			{
				ClearShit();
				if (itemPosition.childCount > 0 && (Object)(object)((Component)itemPosition.GetChild(0)).GetComponent<MeshRenderer>() != (Object)null)
				{
					instantiatedItem = ((Component)itemPosition.GetChild(0)).gameObject;
				}
				else
				{
					instantiatedItem = Object.Instantiate<GameObject>(itemsToShot[0], ((Component)itemPosition).transform.position, ((Component)itemPosition).transform.rotation, itemPosition);
				}
			}
		}
		base.Update();
	}

	private void ClearShit()
	{
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < itemPosition.childCount; i++)
		{
			list.Add(itemPosition.GetChild(i));
		}
		for (int j = 0; j < list.Count; j++)
		{
			Object.DestroyImmediate((Object)(object)((Component)list[j]).gameObject);
		}
	}

	public IEnumerator BuildAllIcons()
	{
		Initialize();
		for (int i = 0; i < itemsToShot.Length; i++)
		{
			finalPath = "C:/Users/Tyler/Desktop/";
			if ((Object)(object)instantiatedItem != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)instantiatedItem);
			}
			if ((Object)(object)whiteCam != (Object)null)
			{
				((Behaviour)whiteCam).enabled = false;
			}
			if ((Object)(object)blackCam != (Object)null)
			{
				((Behaviour)blackCam).enabled = false;
			}
			ClearShit();
			instantiatedItem = Object.Instantiate<GameObject>(itemsToShot[i], ((Component)itemPosition).transform.position, ((Component)itemPosition).transform.rotation);
			if ((Object)(object)IconCreatorCanvas.instance != (Object)null)
			{
				IconCreatorCanvas.instance.SetInfo(itemsToShot.Length, i, ((Object)itemsToShot[i]).name, isRecording: true, nextIconKey);
			}
			currentObject = instantiatedItem.transform;
			if (dynamicFov)
			{
				UpdateFOV(instantiatedItem);
			}
			if (lookAtObjectCenter)
			{
				LookAtTargetCenter(instantiatedItem);
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
			}
			yield return CaptureFrame(((Object)itemsToShot[i]).name, i);
		}
		if ((Object)(object)IconCreatorCanvas.instance != (Object)null)
		{
			IconCreatorCanvas.instance.SetInfo(0, 0, "", isRecording: false, nextIconKey);
		}
		DeleteCameras();
	}
}
