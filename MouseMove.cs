using UnityEngine;

public class MouseMove : MonoBehaviour
{
	[SerializeField]
	private float _sensitivity = 0.5f;

	private Vector3 _originalPos;

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		_originalPos = ((Component)this).transform.position;
	}

	private void Update()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 mousePosition = Input.mousePosition;
		mousePosition.x /= Screen.width;
		mousePosition.y /= Screen.height;
		mousePosition.x -= 0.5f;
		mousePosition.y -= 0.5f;
		mousePosition *= 2f * _sensitivity;
		((Component)this).transform.position = _originalPos + mousePosition;
	}
}
