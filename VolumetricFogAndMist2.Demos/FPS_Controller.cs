using UnityEngine;

namespace VolumetricFogAndMist2.Demos;

public class FPS_Controller : MonoBehaviour
{
	private CharacterController characterController;

	private Transform mainCamera;

	private float inputHor;

	private float inputVert;

	private float mouseHor;

	private float mouseVert;

	private float mouseInvertX = 1f;

	private float mouseInvertY = -1f;

	private float camVertAngle;

	private bool isGrounded;

	private Vector3 jumpDirection = Vector3.zero;

	private float sprint = 1f;

	public float sprintMax = 2f;

	public float airControl = 1.5f;

	public float jumpHeight = 10f;

	public float gravity = 20f;

	public float characterHeight = 1.8f;

	public float cameraHeight = 1.7f;

	public float speed = 15f;

	public float rotationSpeed = 2f;

	public float mouseSensitivity = 1f;

	private void Start()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		characterController = ((Component)this).gameObject.AddComponent<CharacterController>();
		mainCamera = ((Component)Camera.main).transform;
		characterController.height = characterHeight;
		characterController.center = Vector3.up * characterHeight / 2f;
		mainCamera.position = ((Component)this).transform.position + Vector3.up * characterHeight;
		mainCamera.rotation = Quaternion.identity;
		mainCamera.parent = ((Component)this).transform;
		Cursor.lockState = (CursorLockMode)1;
		Cursor.visible = false;
	}

	private void Update()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 mousePosition = Input.mousePosition;
		if (mousePosition.x < 0f || mousePosition.x >= (float)Screen.width || mousePosition.y < 0f || mousePosition.y >= (float)Screen.height)
		{
			return;
		}
		isGrounded = characterController.isGrounded;
		inputHor = Input.GetAxis("Horizontal");
		inputVert = Input.GetAxis("Vertical");
		mouseHor = Input.GetAxis("Mouse X");
		mouseVert = Input.GetAxis("Mouse Y");
		((Component)this).transform.Rotate(0f, mouseHor * rotationSpeed * mouseSensitivity * mouseInvertX, 0f);
		Vector3 val = ((Component)this).transform.forward * inputVert + ((Component)this).transform.right * inputHor;
		val *= speed;
		if (isGrounded)
		{
			if (Input.GetKey((KeyCode)304))
			{
				if (sprint < sprintMax)
				{
					sprint += 10f * Time.deltaTime;
				}
			}
			else if (sprint > 1f)
			{
				sprint -= 10f * Time.deltaTime;
			}
			if (Input.GetKeyDown((KeyCode)32))
			{
				jumpDirection.y = jumpHeight;
			}
			else
			{
				jumpDirection.y = -1f;
			}
		}
		else
		{
			val *= airControl;
		}
		jumpDirection.y -= gravity * Time.deltaTime;
		characterController.Move(val * sprint * Time.deltaTime);
		characterController.Move(jumpDirection * Time.deltaTime);
		camVertAngle += mouseVert * rotationSpeed * mouseSensitivity * mouseInvertY;
		camVertAngle = Mathf.Clamp(camVertAngle, -85f, 85f);
		mainCamera.localEulerAngles = new Vector3(camVertAngle, 0f, 0f);
	}
}
