using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.TV;

public class Pong : TVApp
{
	public enum EGameMode
	{
		SinglePlayer,
		MultiPlayer
	}

	public enum ESide
	{
		Left,
		Right
	}

	public enum EState
	{
		Ready,
		Playing,
		GameOver
	}

	public RectTransform Rect;

	public PongPaddle LeftPaddle;

	public PongPaddle RightPaddle;

	public PongBall Ball;

	public TextMeshProUGUI LeftScoreLabel;

	public TextMeshProUGUI RightScoreLabel;

	public TextMeshProUGUI WinnerLabel;

	[Header("Settings")]
	public float InitialVelocity = 0.8f;

	public float VelocityGainPerSecond = 0.05f;

	public float MaxVelocity = 2f;

	public int GoalsToWin = 10;

	[Header("AI")]
	public float ReactionTime = 0.1f;

	public float TargetRandomization = 10f;

	public float SpeedMultiplier = 0.5f;

	public UnityEvent onServe;

	public UnityEvent onLeftScore;

	public UnityEvent onRightScore;

	public UnityEvent onGameOver;

	public UnityEvent onLocalPlayerWin;

	public UnityEvent onReset;

	private ESide nextBallSide;

	private Vector3 ballVelocity = Vector3.zero;

	private float reactionTimer;

	public EGameMode GameMode { get; set; }

	public EState State { get; set; }

	public int LeftScore { get; set; }

	public int RightScore { get; set; }

	private void Update()
	{
		if (base.IsOpen && !base.IsPaused)
		{
			UpdateInputs();
			if (GameMode == EGameMode.SinglePlayer)
			{
				UpdateAI();
			}
		}
	}

	private void FixedUpdate()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsOpen || base.IsPaused)
		{
			Ball.RB.isKinematic = true;
			return;
		}
		ballVelocity = Ball.RB.velocity;
		Rigidbody rB = Ball.RB;
		Vector3 velocity = rB.velocity;
		Vector3 velocity2 = Ball.RB.velocity;
		rB.velocity = velocity + ((Vector3)(ref velocity2)).normalized * VelocityGainPerSecond * Time.deltaTime;
		velocity2 = Ball.RB.velocity;
		if (((Vector3)(ref velocity2)).magnitude > MaxVelocity)
		{
			Rigidbody rB2 = Ball.RB;
			velocity2 = Ball.RB.velocity;
			rB2.velocity = ((Vector3)(ref velocity2)).normalized * MaxVelocity;
		}
	}

	protected override void TryPause()
	{
		Ball.RB.isKinematic = true;
		if (State == EState.Ready || State == EState.GameOver)
		{
			Close();
		}
		else
		{
			base.TryPause();
		}
	}

	public void UpdateInputs()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (State == EState.Playing)
		{
			Vector2 val = default(Vector2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, Vector2.op_Implicit(Input.mousePosition), PlayerSingleton<PlayerCamera>.Instance.Camera, ref val);
			if (GameMode == EGameMode.SinglePlayer)
			{
				SetPaddleTargetY(ESide.Left, val.y);
			}
		}
		else if (State == EState.Ready)
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.Jump))
			{
				ServeBall();
			}
		}
		else if (State == EState.GameOver && GameInput.GetButtonDown(GameInput.ButtonCode.Jump))
		{
			ResetGame();
		}
	}

	private void UpdateAI()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (State == EState.Playing)
		{
			reactionTimer += Time.deltaTime;
			if (reactionTimer >= ReactionTime)
			{
				float num = (Mathf.Clamp01(Ball.Rect.anchoredPosition.x / 300f) + 1f) / 2f;
				reactionTimer = 0f;
				float num2 = TargetRandomization * Mathf.Lerp(3f, 1f, num);
				float targetY = Ball.Rect.anchoredPosition.y + Random.Range(0f - num2, num2);
				RightPaddle.SetTargetY(targetY);
				RightPaddle.SpeedMultiplier = Mathf.Lerp(0.1f, 1f, num) * SpeedMultiplier;
			}
		}
	}

	public void GoalHit(ESide side)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (State != EState.Playing)
		{
			return;
		}
		if (side == ESide.Left)
		{
			RightScore++;
			if (onRightScore != null)
			{
				onRightScore.Invoke();
			}
		}
		else
		{
			LeftScore++;
			if (onLeftScore != null)
			{
				onLeftScore.Invoke();
			}
		}
		((TMP_Text)LeftScoreLabel).text = LeftScore.ToString();
		((TMP_Text)RightScoreLabel).text = RightScore.ToString();
		Ball.RB.velocity = Vector3.zero;
		Ball.RB.isKinematic = true;
		State = EState.Ready;
		if (LeftScore >= GoalsToWin)
		{
			Win(ESide.Left);
		}
		else if (RightScore >= GoalsToWin)
		{
			Win(ESide.Right);
		}
	}

	private void Win(ESide winner)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (winner == ESide.Left)
		{
			((TMP_Text)WinnerLabel).text = "Player 1 Wins!";
			((Graphic)WinnerLabel).color = ((Graphic)((Component)LeftPaddle).GetComponent<Image>()).color;
			if (onLocalPlayerWin != null)
			{
				onLocalPlayerWin.Invoke();
			}
		}
		else
		{
			((TMP_Text)WinnerLabel).text = "Player 2 Wins!";
			((Graphic)WinnerLabel).color = ((Graphic)((Component)RightPaddle).GetComponent<Image>()).color;
		}
		State = EState.GameOver;
		if (onGameOver != null)
		{
			onGameOver.Invoke();
		}
	}

	private void ResetBall()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Ball.RB.isKinematic = true;
		Ball.Rect.anchoredPosition = Vector2.zero;
		((Component)Ball).transform.localPosition = Vector3.zero;
		((Component)Ball).transform.localRotation = Quaternion.identity;
		Ball.RB.velocity = Vector3.zero;
		Ball.RB.isKinematic = false;
	}

	private void ServeBall()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		ResetBall();
		Ball.RB.isKinematic = false;
		Vector2 val;
		if (nextBallSide == ESide.Left)
		{
			val = new Vector2(-1f, Random.Range(-0.5f, 0.5f));
			Vector2 normalized = ((Vector2)(ref val)).normalized;
			Ball.RB.AddRelativeForce(Vector2.op_Implicit(normalized * InitialVelocity), (ForceMode)2);
		}
		else
		{
			val = new Vector2(1f, Random.Range(-0.5f, 0.5f));
			Vector2 normalized2 = ((Vector2)(ref val)).normalized;
			Ball.RB.AddRelativeForce(Vector2.op_Implicit(normalized2 * InitialVelocity), (ForceMode)2);
		}
		State = EState.Playing;
		nextBallSide = ((nextBallSide == ESide.Left) ? ESide.Right : ESide.Left);
		if (onServe != null)
		{
			onServe.Invoke();
		}
	}

	private void ResetGame()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		State = EState.Ready;
		LeftScore = 0;
		RightScore = 0;
		((TMP_Text)LeftScoreLabel).text = LeftScore.ToString();
		((TMP_Text)RightScoreLabel).text = RightScore.ToString();
		ResetBall();
		nextBallSide = ESide.Left;
		ballVelocity = Vector3.zero;
		if (onReset != null)
		{
			onReset.Invoke();
		}
	}

	public void SetPaddleTargetY(ESide player, float y)
	{
		if (player == ESide.Left)
		{
			LeftPaddle.SetTargetY(y);
		}
		else
		{
			RightPaddle.SetTargetY(y);
		}
	}

	public override void Resume()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		base.Resume();
		Ball.RB.isKinematic = false;
		Ball.RB.velocity = ballVelocity;
	}
}
