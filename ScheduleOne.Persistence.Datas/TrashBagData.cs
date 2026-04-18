using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

public class TrashBagData : TrashItemData
{
	public TrashBagData(string trashID, string guid, Vector3 position, Quaternion rotation, TrashContentData contents)
		: base(trashID, guid, position, rotation)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		DataType = "TrashBagData";
		Contents = contents;
	}
}
