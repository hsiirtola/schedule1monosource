using ScheduleOne.Core.Items.Framework;
using UnityEngine;

namespace ScheduleOne.Storage;

public class StoredItem_GenericBox : StoredItem
{
	private const float ReferenceIconWidth = 1024f;

	[Header("References")]
	[SerializeField]
	protected SpriteRenderer icon1;

	[SerializeField]
	protected SpriteRenderer icon2;

	[Header("Settings")]
	public float IconScale = 0.5f;

	public override void InitializeStoredItem(StorableItemInstance _item, StorageGrid grid, Vector2 _originCoordinate, float _rotation)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		base.InitializeStoredItem(_item, grid, _originCoordinate, _rotation);
		Rect rect = ((BaseItemInstance)_item).Icon.rect;
		float num = 0.025f / (((Rect)(ref rect)).width / 1024f) * IconScale;
		if ((Object)(object)icon1 != (Object)null)
		{
			icon1.sprite = ((BaseItemInstance)_item).Icon;
			((Component)icon1).transform.localScale = new Vector3(num, num, 1f);
		}
		if ((Object)(object)icon2 != (Object)null)
		{
			icon2.sprite = ((BaseItemInstance)_item).Icon;
			((Component)icon2).transform.localScale = new Vector3(num, num, 1f);
		}
	}
}
