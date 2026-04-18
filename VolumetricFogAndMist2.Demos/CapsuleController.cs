using UnityEngine;

namespace VolumetricFogAndMist2.Demos;

public class CapsuleController : MonoBehaviour
{
	public VolumetricFog fogVolume;

	public float moveSpeed = 10f;

	public float fogHoleRadius = 8f;

	public float clearDuration = 0.2f;

	public float distanceCheck = 1f;

	private Vector3 lastPos = new Vector3(float.MaxValue, 0f, 0f);

	private void Update()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.deltaTime * moveSpeed;
		if (Input.GetKey((KeyCode)276))
		{
			((Component)this).transform.Translate(0f - num, 0f, 0f);
		}
		else if (Input.GetKey((KeyCode)275))
		{
			((Component)this).transform.Translate(num, 0f, 0f);
		}
		if (Input.GetKey((KeyCode)273))
		{
			((Component)this).transform.Translate(0f, 0f, num);
		}
		else if (Input.GetKey((KeyCode)274))
		{
			((Component)this).transform.Translate(0f, 0f, 0f - num);
		}
		Vector3 val = ((Component)this).transform.position - lastPos;
		if (((Vector3)(ref val)).magnitude > distanceCheck)
		{
			lastPos = ((Component)this).transform.position;
			fogVolume.SetFogOfWarAlpha(((Component)this).transform.position, fogHoleRadius, 0f, clearDuration);
		}
	}
}
