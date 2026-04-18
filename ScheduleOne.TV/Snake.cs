using System.Collections.Generic;
using ScheduleOne.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.TV;

public class Snake : TVApp
{
	public enum EGameState
	{
		Ready,
		Playing
	}

	public const int SIZE_X = 20;

	public const int SIZE_Y = 12;

	[Header("Settings")]
	public SnakeTile TilePrefab;

	public float TimePerTile = 0.4f;

	[Header("References")]
	public RectTransform PlaySpace;

	public SnakeTile[] Tiles;

	public TextMeshProUGUI ScoreText;

	private Vector2 lastFoodPosition = Vector2.zero;

	private float _timeSinceLastMove;

	private float _timeOnGameOver;

	public UnityEvent onStart;

	public UnityEvent onEat;

	public UnityEvent onGameOver;

	public UnityEvent onWin;

	public Vector2 HeadPosition { get; private set; } = new Vector2(10f, 6f);

	public List<Vector2> Tail { get; private set; } = new List<Vector2>();

	public Vector2 LastTailPosition { get; private set; } = Vector2.zero;

	public Vector2 Direction { get; private set; } = Vector2.right;

	public Vector2 QueuedDirection { get; private set; } = Vector2.right;

	public Vector2 NextDirection { get; private set; } = Vector2.zero;

	public EGameState GameState { get; private set; }

	protected override void Awake()
	{
		base.Awake();
	}

	private void Update()
	{
		if (!base.IsPaused && base.IsOpen)
		{
			UpdateInput();
			UpdateMovement();
			_timeOnGameOver += Time.deltaTime;
			((TMP_Text)ScoreText).text = Tail.Count.ToString();
		}
	}

	private void UpdateInput()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		if (_timeOnGameOver < 0.3f)
		{
			return;
		}
		if (GameInput.GetButtonDown(GameInput.ButtonCode.Forward) || Input.GetKeyDown((KeyCode)273))
		{
			if (Direction != Vector2.down)
			{
				QueuedDirection = Vector2.up;
			}
			NextDirection = Vector2.up;
			if (GameState == EGameState.Ready)
			{
				StartGame(Vector2.up);
			}
		}
		else if (GameInput.GetButtonDown(GameInput.ButtonCode.Backward) || Input.GetKeyDown((KeyCode)274))
		{
			if (Direction != Vector2.up)
			{
				QueuedDirection = Vector2.down;
			}
			NextDirection = Vector2.down;
			if (GameState == EGameState.Ready)
			{
				StartGame(Vector2.down);
			}
		}
		else if (GameInput.GetButtonDown(GameInput.ButtonCode.Left) || Input.GetKeyDown((KeyCode)276))
		{
			if (Direction != Vector2.right)
			{
				QueuedDirection = Vector2.left;
			}
			NextDirection = Vector2.left;
			if (GameState == EGameState.Ready)
			{
				StartGame(Vector2.left);
			}
		}
		else if (GameInput.GetButtonDown(GameInput.ButtonCode.Right) || Input.GetKeyDown((KeyCode)275))
		{
			if (Direction != Vector2.left)
			{
				QueuedDirection = Vector2.right;
			}
			NextDirection = Vector2.right;
			if (GameState == EGameState.Ready)
			{
				StartGame(Vector2.right);
			}
		}
	}

	private void UpdateMovement()
	{
		if (GameState == EGameState.Playing)
		{
			_timeSinceLastMove += Time.deltaTime;
			if (_timeSinceLastMove >= TimePerTile)
			{
				_timeSinceLastMove -= TimePerTile;
				MoveSnake();
			}
		}
	}

	private void MoveSnake()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		Direction = QueuedDirection;
		Vector2 val = HeadPosition + Direction;
		SnakeTile tile = GetTile(val);
		if ((Object)(object)tile == (Object)null)
		{
			GameOver();
			return;
		}
		if (tile.Type == SnakeTile.TileType.Snake && Tail.Count > 0 && tile.Position != Tail[Tail.Count - 1])
		{
			GameOver();
			return;
		}
		bool flag = false;
		if (tile.Type == SnakeTile.TileType.Food)
		{
			Eat();
			flag = true;
			if (GameState != EGameState.Playing)
			{
				return;
			}
		}
		GetTile(val).SetType(SnakeTile.TileType.Snake);
		Vector2 val2 = HeadPosition;
		HeadPosition = val;
		for (int i = 0; i < Tail.Count; i++)
		{
			if (i == Tail.Count - 1)
			{
				LastTailPosition = Tail[i];
			}
			Vector2 val3 = Tail[i];
			Tail[i] = val2;
			GetTile(Tail[i]).SetType(SnakeTile.TileType.Snake, 1 + i);
			val2 = val3;
		}
		GetTile(val2).SetType(SnakeTile.TileType.Empty);
		LastTailPosition = val2;
		if (NextDirection != Vector2.zero && NextDirection != -Direction)
		{
			QueuedDirection = NextDirection;
		}
		if (flag)
		{
			SpawnFood();
		}
	}

	private SnakeTile GetTile(Vector2 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (position.x < 0f || position.x >= 20f || position.y < 0f || position.y >= 12f)
		{
			return null;
		}
		return Tiles[(int)position.y * 20 + (int)position.x];
	}

	private void StartGame(Vector2 initialDir)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		SnakeTile tile = GetTile(lastFoodPosition);
		if ((Object)(object)tile != (Object)null)
		{
			tile.SetType(SnakeTile.TileType.Empty);
		}
		SpawnFood();
		GetTile(HeadPosition)?.SetType(SnakeTile.TileType.Empty);
		HeadPosition = new Vector2(10f, 6f);
		for (int i = 0; i < Tail.Count; i++)
		{
			GetTile(Tail[i]).SetType(SnakeTile.TileType.Empty);
		}
		Tail.Clear();
		QueuedDirection = initialDir;
		NextDirection = Vector2.zero;
		_timeSinceLastMove = 0f;
		MoveSnake();
		GameState = EGameState.Playing;
		if (onStart != null)
		{
			onStart.Invoke();
		}
	}

	private void Eat()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Tail.Add(LastTailPosition);
		if (onEat != null)
		{
			onEat.Invoke();
		}
	}

	private void SpawnFood()
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		List<SnakeTile> list = new List<SnakeTile>();
		SnakeTile[] tiles = Tiles;
		foreach (SnakeTile snakeTile in tiles)
		{
			if (snakeTile.Type == SnakeTile.TileType.Empty)
			{
				list.Add(snakeTile);
			}
		}
		if (list.Count == 0)
		{
			Win();
			return;
		}
		SnakeTile snakeTile2 = list[Random.Range(0, list.Count)];
		snakeTile2.SetType(SnakeTile.TileType.Food);
		lastFoodPosition = snakeTile2.Position;
	}

	private void GameOver()
	{
		GameState = EGameState.Ready;
		_timeOnGameOver = 0f;
		if (onGameOver != null)
		{
			onGameOver.Invoke();
		}
	}

	private void Win()
	{
		GameState = EGameState.Ready;
		_timeOnGameOver = 0f;
		if (onWin != null)
		{
			onWin.Invoke();
		}
	}

	protected override void TryPause()
	{
		if (GameState == EGameState.Ready)
		{
			Close();
		}
		else
		{
			base.TryPause();
		}
	}

	[Button]
	public void CreateTiles()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		SnakeTile[] tiles = Tiles;
		for (int i = 0; i < tiles.Length; i++)
		{
			Object.DestroyImmediate((Object)(object)((Component)tiles[i]).gameObject);
		}
		Tiles = new SnakeTile[240];
		Rect rect = PlaySpace.rect;
		float tileSize = ((Rect)(ref rect)).width / 20f;
		for (int j = 0; j < 12; j++)
		{
			for (int k = 0; k < 20; k++)
			{
				SnakeTile snakeTile = Object.Instantiate<SnakeTile>(TilePrefab, (Transform)(object)PlaySpace);
				snakeTile.SetType(SnakeTile.TileType.Empty);
				snakeTile.SetPosition(new Vector2((float)k, (float)j), tileSize);
				Tiles[j * 20 + k] = snakeTile;
			}
		}
	}
}
