using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Trash;

public class TrashBag : TrashItem
{
	public TrashContent Content { get; private set; } = new TrashContent();

	public void LoadContent(TrashContentData data)
	{
		Content.LoadFromData(data);
	}

	public override TrashItemData GetData()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		return new TrashBagData(ID, base.GUID.ToString(), ((Component)this).transform.position, ((Component)this).transform.rotation, Content.GetData());
	}
}
