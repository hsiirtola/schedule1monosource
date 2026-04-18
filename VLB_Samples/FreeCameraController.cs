using UnityEngine;

namespace VLB_Samples;

public class FreeCameraController : MonoBehaviour
{
	public float cameraSensitivity = 90f;

	public float speedNormal = 10f;

	public float speedFactorSlow = 0.25f;

	public float speedFactorFast = 3f;

	public float speedClimb = 4f;

	private float rotationH;

	private float rotationV;

	private bool m_UseMouseView = true;

	private bool useMouseView
	{
		get
		{
			return m_UseMouseView;
		}
		set
		{
			m_UseMouseView = value;
			Cursor.lockState = (CursorLockMode)(value ? 1 : 0);
			Cursor.visible = !value;
		}
	}

	private void Start()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		useMouseView = true;
		Quaternion rotation = ((Component)this).transform.rotation;
		Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
		rotationH = eulerAngles.y;
		rotationV = eulerAngles.x;
		if (rotationV > 180f)
		{
			rotationV -= 360f;
		}
	}

	private void Update()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		if (useMouseView)
		{
			rotationH += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
			rotationV -= Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
		}
		rotationV = Mathf.Clamp(rotationV, -90f, 90f);
		((Component)this).transform.rotation = Quaternion.AngleAxis(rotationH, Vector3.up);
		Transform transform = ((Component)this).transform;
		transform.rotation *= Quaternion.AngleAxis(rotationV, Vector3.right);
		float num = speedNormal;
		if (Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)303))
		{
			num *= speedFactorFast;
		}
		else if (Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305))
		{
			num *= speedFactorSlow;
		}
		Transform transform2 = ((Component)this).transform;
		transform2.position += num * Input.GetAxis("Vertical") * Time.deltaTime * ((Component)this).transform.forward;
		Transform transform3 = ((Component)this).transform;
		transform3.position += num * Input.GetAxis("Horizontal") * Time.deltaTime * ((Component)this).transform.right;
		if (Input.GetKey((KeyCode)113))
		{
			Transform transform4 = ((Component)this).transform;
			transform4.position += speedClimb * Time.deltaTime * Vector3.up;
		}
		if (Input.GetKey((KeyCode)101))
		{
			Transform transform5 = ((Component)this).transform;
			transform5.position += speedClimb * Time.deltaTime * Vector3.down;
		}
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
		{
			useMouseView = !useMouseView;
		}
		if (Input.GetKeyDown((KeyCode)27))
		{
			useMouseView = false;
		}
	}
}
