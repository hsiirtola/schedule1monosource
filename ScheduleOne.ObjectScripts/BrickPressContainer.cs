using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.ObjectScripts;

public class BrickPressContainer : MonoBehaviour
{
	public MultiTypeVisualsSetter Visuals;

	public Transform ContentsContainer;

	public Transform Contents_Min;

	public Transform Contents_Max;

	public void SetContents(ProductItemInstance product, float fillLevel)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		fillLevel = Mathf.Clamp01(fillLevel);
		if (product == null || fillLevel == 0f)
		{
			((Component)ContentsContainer).gameObject.SetActive(false);
			return;
		}
		Visuals.ApplyVisuals(product);
		ContentsContainer.localPosition = Vector3.Lerp(Contents_Min.localPosition, Contents_Max.localPosition, fillLevel);
		((Component)ContentsContainer).gameObject.SetActive(true);
	}
}
