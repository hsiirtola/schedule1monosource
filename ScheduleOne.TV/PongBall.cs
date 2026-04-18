using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.TV;

public class PongBall : MonoBehaviour
{
	public Pong Game;

	public RectTransform Rect;

	public Rigidbody RB;

	public float RandomForce = 0.5f;

	public UnityEvent onHit;

	private void FixedUpdate()
	{
	}

	private void OnCollisionEnter(Collision collision)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		if (((Object)((Component)collision.collider).gameObject).name == "LeftGoal")
		{
			Game.GoalHit(Pong.ESide.Left);
		}
		else if (((Object)((Component)collision.collider).gameObject).name == "RightGoal")
		{
			Game.GoalHit(Pong.ESide.Right);
		}
		if (RB.velocity.y < 0.1f && (Object)(object)((Component)collision.collider).GetComponent<PongPaddle>() != (Object)null)
		{
			Vector3 velocity = RB.velocity;
			float magnitude = ((Vector3)(ref velocity)).magnitude;
			RB.AddForce(new Vector3(0f, Random.Range(0f - RandomForce, RandomForce), 0f), (ForceMode)2);
			Rigidbody rB = RB;
			velocity = RB.velocity;
			rB.velocity = ((Vector3)(ref velocity)).normalized * magnitude;
		}
		if (onHit != null)
		{
			onHit.Invoke();
		}
	}
}
