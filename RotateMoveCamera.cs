using UnityEngine;

public class RotateMoveCamera : MonoBehaviour
{
	public GameObject Camera;

	public float minX = -360f;

	public float maxX = 360f;

	public float minY = -45f;

	public float maxY = 45f;

	public float sensX = 100f;

	public float sensY = 100f;

	private float rotationY;

	private float rotationX;

	private float MouseX;

	private float MouseY;

	private void Update()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		float axis = Input.GetAxis("Mouse X");
		float axis2 = Input.GetAxis("Mouse Y");
		if (axis != MouseX || axis2 != MouseY)
		{
			rotationX += axis * sensX * Time.deltaTime;
			rotationY += axis2 * sensY * Time.deltaTime;
			rotationY = Mathf.Clamp(rotationY, minY, maxY);
			MouseX = axis;
			MouseY = axis2;
			Camera.transform.localEulerAngles = new Vector3(0f - rotationY, rotationX, 0f);
		}
		if (Input.GetKey((KeyCode)119))
		{
			((Component)this).transform.Translate(new Vector3(0f, 0f, 0.1f));
		}
		else if (Input.GetKey((KeyCode)115))
		{
			((Component)this).transform.Translate(new Vector3(0f, 0f, -0.1f));
		}
		if (Input.GetKey((KeyCode)100))
		{
			((Component)this).transform.Translate(new Vector3(0.1f, 0f, 0f));
		}
		else if (Input.GetKey((KeyCode)97))
		{
			((Component)this).transform.Translate(new Vector3(-0.1f, 0f, 0f));
		}
	}
}
