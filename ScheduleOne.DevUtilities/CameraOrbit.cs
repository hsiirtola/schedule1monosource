using System.Collections.Generic;
using ScheduleOne.AvatarFramework.Animation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.DevUtilities;

public class CameraOrbit : MonoBehaviour
{
	[Header("Required")]
	public Transform target;

	public Transform cam;

	public GraphicRaycaster raycaster;

	public AvatarLookController LookAt;

	[Header("Config")]
	public float targetdistance = 5f;

	public float xSpeed = 120f;

	public float ySpeed = 120f;

	public float sideOffset = 1f;

	public float yMinLimit = -20f;

	public float yMaxLimit = 80f;

	public float distanceMin = 0.5f;

	public float distanceMax = 15f;

	public float ScrollSensativity = 4f;

	private Rigidbody rb;

	private float x;

	private float y;

	private float targetx;

	private float targety;

	private float distance = 5f;

	private bool hoveringUI;

	private void Start()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Vector3 eulerAngles = ((Component)this).transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
		rb = ((Component)this).GetComponent<Rigidbody>();
		if ((Object)(object)rb != (Object)null)
		{
			rb.freezeRotation = true;
		}
	}

	private void Update()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		PointerEventData val = new PointerEventData(EventSystem.current);
		val.position = Vector2.op_Implicit(Input.mousePosition);
		List<RaycastResult> list = new List<RaycastResult>();
		((BaseRaycaster)raycaster).Raycast(val, list);
		hoveringUI = list.Count > 0;
		LookAt.OverrideLookTarget(((Component)cam).transform.position, 100);
	}

	private void LateUpdate()
	{
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)target))
		{
			if (Input.GetMouseButton(0) && !hoveringUI)
			{
				targetx += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f * (5f / (distance + 2f));
				targety -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
			}
			targety = ClampAngle(targety, yMinLimit, yMaxLimit);
			x = Mathf.LerpAngle(x, targetx, 0.1f);
			y = Mathf.LerpAngle(y, targety, 1f);
			Quaternion val = Quaternion.Euler(y, x, 0f);
			if (!hoveringUI)
			{
				targetdistance = Mathf.Clamp(targetdistance - Input.GetAxis("Mouse ScrollWheel") * ScrollSensativity, distanceMin, distanceMax);
			}
			distance = Mathf.Lerp(distance, targetdistance, 0.1f);
			RaycastHit val2 = default(RaycastHit);
			if (Physics.Linecast(target.position, ((Component)this).transform.position, ref val2))
			{
				targetdistance -= ((RaycastHit)(ref val2)).distance;
			}
			Vector3 val3 = default(Vector3);
			((Vector3)(ref val3))._002Ector(0f, 0f, 0f - distance);
			Vector3 position = val * val3 + target.position;
			((Component)this).transform.rotation = val;
			((Component)this).transform.position = position;
		}
		cam.position = ((Component)this).transform.position;
		cam.rotation = ((Component)this).transform.rotation;
		cam.position -= ((Component)this).transform.right * sideOffset * Vector3.Distance(cam.position, target.position);
		if (Input.GetKey((KeyCode)270))
		{
			Camera component = ((Component)this).GetComponent<Camera>();
			component.fieldOfView += 0.3f;
		}
		if (Input.GetKey((KeyCode)269))
		{
			Camera component2 = ((Component)this).GetComponent<Camera>();
			component2.fieldOfView -= 0.3f;
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
