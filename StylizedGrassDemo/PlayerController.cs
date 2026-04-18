using UnityEngine;

namespace StylizedGrassDemo;

public class PlayerController : MonoBehaviour
{
	public Camera cam;

	private float speed = 15f;

	private float jumpForce = 350f;

	private Rigidbody rb;

	private bool isGrounded;

	public ParticleSystem landBendEffect;

	private RaycastHit raycastHit;

	private void Start()
	{
		rb = ((Component)this).GetComponent<Rigidbody>();
		if (!Object.op_Implicit((Object)(object)cam))
		{
			cam = Camera.main;
		}
		isGrounded = true;
	}

	private void FixedUpdate()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(((Component)cam).transform.forward.x, 0f, ((Component)cam).transform.forward.z);
		val *= Input.GetAxis("Vertical");
		val = ((Vector3)(ref val)).normalized;
		rb.AddForce(val * speed);
		if (Input.GetKeyDown((KeyCode)32) && isGrounded)
		{
			rb.AddForce(Vector3.up * jumpForce * rb.mass);
			isGrounded = false;
		}
	}

	private void Update()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (isGrounded)
		{
			return;
		}
		Physics.Raycast(((Component)this).transform.position, -Vector3.up, ref raycastHit, 0.5f);
		if (Object.op_Implicit((Object)(object)((RaycastHit)(ref raycastHit)).collider) && ((object)((RaycastHit)(ref raycastHit)).collider).GetType() == typeof(TerrainCollider))
		{
			isGrounded = true;
			if (Object.op_Implicit((Object)(object)landBendEffect))
			{
				landBendEffect.Emit(1);
			}
		}
	}
}
