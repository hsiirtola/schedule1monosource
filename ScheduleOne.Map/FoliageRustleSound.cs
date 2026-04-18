using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Map;

public class FoliageRustleSound : MonoBehaviour
{
	public const float ACTIVATION_RANGE_SQUARED = 900f;

	public const float COOLDOWN = 1f;

	public AudioSourceController Sound;

	public GameObject Container;

	private float timeOnLastHit;

	private void Awake()
	{
		((MonoBehaviour)this).InvokeRepeating("UpdateActive", Random.Range(0f, 3f), 3f);
		Container.SetActive(false);
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(Container.transform.position, 0.5f);
		Gizmos.color = Color.yellow;
	}

	public void OnTriggerEnter(Collider other)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (!(Time.timeSinceLevelLoad - timeOnLastHit > 1f) || ((Component)other).gameObject.layer != LayerMask.NameToLayer("Player"))
		{
			return;
		}
		Player componentInParent = ((Component)other).gameObject.GetComponentInParent<Player>();
		if ((Object)(object)componentInParent != (Object)null)
		{
			if (((NetworkBehaviour)componentInParent).IsOwner)
			{
				AudioSourceController sound = Sound;
				Vector3 velocity = PlayerSingleton<PlayerMovement>.Instance.Controller.velocity;
				sound.VolumeMultiplier = Mathf.Clamp01(((Vector3)(ref velocity)).magnitude / 6.1749997f);
			}
			else
			{
				Sound.VolumeMultiplier = 1f;
			}
			Sound.Play();
			timeOnLastHit = Time.timeSinceLevelLoad;
		}
	}

	private void UpdateActive()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Player.Local == (Object)null))
		{
			float num = Vector3.SqrMagnitude(Player.Local.Avatar.CenterPoint - ((Component)this).transform.position);
			Container.SetActive(num < 900f);
		}
	}
}
