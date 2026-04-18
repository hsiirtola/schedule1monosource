using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.TV;

public class SnakeTile : MonoBehaviour
{
	public enum TileType
	{
		Empty,
		Snake,
		Food
	}

	public Vector2 Position = Vector2.zero;

	public Color SnakeColor;

	public Color FoodColor;

	public RectTransform RectTransform;

	public Image Image;

	public TileType Type { get; private set; }

	public void SetType(TileType type, int index = 0)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Type = type;
		switch (Type)
		{
		case TileType.Empty:
			((Component)this).gameObject.SetActive(false);
			break;
		case TileType.Snake:
			((Graphic)Image).color = SnakeColor;
			if (index > 0)
			{
				float num = 1f - 0.8f * Mathf.Sqrt((float)index / 240f);
				((Graphic)Image).color = new Color(SnakeColor.r, SnakeColor.g, SnakeColor.b, num);
			}
			((Component)this).gameObject.SetActive(true);
			break;
		case TileType.Food:
			((Graphic)Image).color = FoodColor;
			((Component)this).gameObject.SetActive(true);
			break;
		}
	}

	public void SetPosition(Vector2 position, float tileSize)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Position = position;
		RectTransform.anchoredPosition = new Vector2((0.5f + position.x) * tileSize, (0.5f + position.y) * tileSize);
		((Object)((Component)this).gameObject).name = $"Tile {position.x}, {position.y}";
		RectTransform.sizeDelta = new Vector2(tileSize, tileSize);
	}
}
