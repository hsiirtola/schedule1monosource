using ScheduleOne;
using UnityEngine;

namespace StylizedGrassDemo;

public class OrbitCamera : MonoBehaviour
{
	[Space]
	public Transform pivot;

	[Space]
	public bool enableMouse = true;

	public float idleRotationSpeed = 0.05f;

	public float lookSmoothSpeed = 5f;

	public float moveSmoothSpeed = 5f;

	public float scrollSmoothSpeed = 5f;

	private Transform cam;

	private float cameraRotSide;

	private float cameraRotUp;

	private float cameraRotSideCur;

	private float cameraRotUpCur;

	private float distance;

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		cam = ((Component)Camera.main).transform;
		cameraRotSide = ((Component)this).transform.eulerAngles.y;
		cameraRotSideCur = ((Component)this).transform.eulerAngles.y;
		cameraRotUp = ((Component)this).transform.eulerAngles.x;
		cameraRotUpCur = ((Component)this).transform.eulerAngles.x;
		distance = 0f - cam.localPosition.z;
	}

	private void LateUpdate()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		Cursor.visible = false;
		if (Object.op_Implicit((Object)(object)pivot))
		{
			if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && enableMouse)
			{
				cameraRotSide += GameInput.MouseDelta.x * 5f;
				cameraRotUp -= GameInput.MouseDelta.y * 5f;
			}
			else
			{
				cameraRotSide += idleRotationSpeed;
			}
			cameraRotSideCur = Mathf.LerpAngle(cameraRotSideCur, cameraRotSide, Time.deltaTime * lookSmoothSpeed);
			cameraRotUpCur = Mathf.Lerp(cameraRotUpCur, cameraRotUp, Time.deltaTime * lookSmoothSpeed);
			if (GameInput.GetButton(GameInput.ButtonCode.SecondaryClick) && enableMouse)
			{
				distance *= 1f - 0.1f * GameInput.MouseDelta.y;
			}
			if (enableMouse)
			{
				distance *= 1f - 1f * GameInput.MouseScrollDelta;
			}
			Vector3 position = pivot.position;
			((Component)this).transform.position = Vector3.Lerp(((Component)this).transform.position, position, Time.deltaTime * moveSmoothSpeed);
			((Component)this).transform.rotation = Quaternion.Euler(cameraRotUpCur, cameraRotSideCur, 0f);
			float num = Mathf.Lerp(0f - ((Component)cam).transform.localPosition.z, distance, Time.deltaTime * scrollSmoothSpeed);
			cam.localPosition = -Vector3.forward * num;
		}
	}
}
