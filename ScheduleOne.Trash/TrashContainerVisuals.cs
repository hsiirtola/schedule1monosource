using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Trash;

[RequireComponent(typeof(TrashContainer))]
public class TrashContainerVisuals : MonoBehaviour
{
	public TrashContainer TrashContainer;

	[Header("References")]
	public Transform ContentsTransform;

	public Transform VisualsContainer;

	public Transform VisualsMinTransform;

	public Transform VisualsMaxTransform;

	public Collider Collider;

	protected void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		TrashContainer.onTrashLevelChanged.AddListener(new UnityAction(UpdateVisuals));
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)TrashContainer.TrashLevel / (float)TrashContainer.TrashCapacity;
		((Component)ContentsTransform).transform.localPosition = Vector3.Lerp(VisualsMinTransform.localPosition, VisualsMaxTransform.localPosition, num);
		((Component)ContentsTransform).transform.localScale = Vector3.Lerp(VisualsMinTransform.localScale, VisualsMaxTransform.localScale, num);
		((Component)VisualsContainer).gameObject.SetActive(TrashContainer.TrashLevel > 0);
		Collider.enabled = TrashContainer.TrashLevel >= TrashContainer.TrashCapacity;
	}
}
