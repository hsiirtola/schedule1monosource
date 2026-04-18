using UnityEngine;

namespace ScheduleOne.ObjectScripts;

public class CauldronDisplayTub : MonoBehaviour
{
	public enum EContents
	{
		None,
		CocaLeaf
	}

	public Transform CocaLeafContainer;

	public Transform Container_Min;

	public Transform Container_Max;

	public void Configure(EContents contentsType, float fillLevel)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		((Component)CocaLeafContainer).gameObject.SetActive(false);
		Transform val = null;
		if (contentsType == EContents.CocaLeaf)
		{
			val = CocaLeafContainer;
		}
		if ((Object)(object)val != (Object)null)
		{
			((Component)val).transform.localPosition = Vector3.Lerp(Container_Min.localPosition, Container_Max.localPosition, fillLevel);
			((Component)val).gameObject.SetActive(fillLevel > 0f);
		}
	}
}
