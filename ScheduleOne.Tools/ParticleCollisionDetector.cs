using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools;

public class ParticleCollisionDetector : MonoBehaviour
{
	public UnityEvent<GameObject> onCollision = new UnityEvent<GameObject>();

	private ParticleSystem ps;

	private void Awake()
	{
		ps = ((Component)this).GetComponent<ParticleSystem>();
	}

	public void OnParticleCollision(GameObject other)
	{
		if (onCollision != null)
		{
			onCollision.Invoke(other);
		}
	}

	private void OnParticleTrigger()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		TriggerModule trigger = ps.trigger;
		Component collider = ((TriggerModule)(ref trigger)).GetCollider(0);
		if ((Object)(object)collider != (Object)null && onCollision != null)
		{
			onCollision.Invoke(collider.gameObject);
		}
	}
}
