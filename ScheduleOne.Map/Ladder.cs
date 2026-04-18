using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Doors;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.Map;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Ladder : MonoBehaviour
{
	public const float NPCClimbOffset = 0.42f;

	public const float LadderMountDismountTimeMultiplier = 0.4f;

	public const float LadderClimbTimeMultiplier = 0.75f;

	public const float NPCClimbSoundInterval = 0.3f;

	public const float PlayerClimbSoundLengthInterval = 0.8f;

	[Header("References")]
	public OffMeshLink OffMeshLink;

	public AudioSourceController ClimbSound;

	public SewerDoorController LinkedManholeCover;

	private BoxCollider boxCollider;

	private float timeOnLastClimbSound;

	public Transform LadderTransform => ((Component)boxCollider).transform;

	public Vector2 LadderSize => new Vector2(boxCollider.size.x * ((Component)this).transform.localScale.x, boxCollider.size.y * ((Component)this).transform.localScale.y);

	public Vector3 BottomCenter => LadderTransform.position + LadderTransform.right * LadderSize.x * 0.5f;

	public Vector3 TopCenter => LadderTransform.position + LadderTransform.right * LadderSize.x * 0.5f + LadderTransform.up * LadderSize.y;

	private void Awake()
	{
		boxCollider = ((Component)this).GetComponent<BoxCollider>();
		((Collider)boxCollider).isTrigger = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (((Component)other).gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			Player componentInParent = ((Component)other).GetComponentInParent<Player>();
			if ((Object)(object)componentInParent == (Object)(object)Player.Local && (Object)(object)other == (Object)(object)componentInParent.CapCol)
			{
				PlayerSingleton<PlayerMovement>.Instance.MountLadder(this);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (((Component)other).gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			Player componentInParent = ((Component)other).GetComponentInParent<Player>();
			if ((Object)(object)componentInParent == (Object)(object)Player.Local && (Object)(object)other == (Object)(object)componentInParent.CapCol)
			{
				PlayerSingleton<PlayerMovement>.Instance.DismountLadder();
			}
		}
	}

	private void OnDrawGizmos()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)boxCollider == (Object)null)
		{
			boxCollider = ((Component)this).GetComponent<BoxCollider>();
		}
		Gizmos.color = Color.green;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(boxCollider.size.x * ((Component)this).transform.localScale.x, boxCollider.size.y * ((Component)this).transform.localScale.y, boxCollider.size.z * ((Component)this).transform.localScale.z);
		Gizmos.matrix = Matrix4x4.TRS(((Component)this).transform.position, ((Component)this).transform.rotation, Vector3.one);
		Gizmos.DrawWireCube(boxCollider.center, val);
	}

	public Vector2 ProjectOnLadderSurface(Vector3 position)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)boxCollider).transform.InverseTransformPoint(position);
		return new Vector2(Mathf.Clamp(val.x, 0f, LadderSize.x), Mathf.Clamp(val.y, 0f, LadderSize.y));
	}

	public Vector2 NormalizeProjectedPosition(Vector2 projectedPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.InverseLerp(0f, LadderSize.x, projectedPosition.x), Mathf.InverseLerp(0f, LadderSize.y, projectedPosition.y));
	}

	public void PlayClimbSound(Vector3 position)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (!(Time.timeSinceLevelLoad - timeOnLastClimbSound < 0.1f))
		{
			timeOnLastClimbSound = Time.timeSinceLevelLoad;
			if ((Object)(object)ClimbSound != (Object)null)
			{
				((Component)ClimbSound).transform.position = position;
				ClimbSound.Play();
			}
		}
	}
}
