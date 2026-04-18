using UnityEngine;

namespace LiquidVolumeFX;

public class LiquidLevelPouringSync : MonoBehaviour
{
	public float fillSpeed = 0.01f;

	public float sinkFactor = 0.1f;

	private LiquidVolume lv;

	private Rigidbody rb;

	private void Start()
	{
		rb = ((Component)this).GetComponent<Rigidbody>();
		lv = ((Component)((Component)this).transform.parent).GetComponent<LiquidVolume>();
		UpdateColliderPos();
	}

	private void OnParticleCollision(GameObject other)
	{
		if (lv.level < 1f)
		{
			lv.level += fillSpeed;
		}
		UpdateColliderPos();
	}

	private void UpdateColliderPos()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = default(Vector3);
		((Vector3)(ref position))._002Ector(((Component)this).transform.position.x, lv.liquidSurfaceYPosition - ((Component)this).transform.localScale.y * 0.5f - sinkFactor, ((Component)this).transform.position.z);
		rb.position = position;
		if (lv.level >= 1f)
		{
			((Component)this).transform.localRotation = Quaternion.Euler(Random.value * 30f - 15f, Random.value * 30f - 15f, Random.value * 30f - 15f);
		}
		else
		{
			((Component)this).transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
	}
}
