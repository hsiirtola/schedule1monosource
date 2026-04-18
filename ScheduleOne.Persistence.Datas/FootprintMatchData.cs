using System;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class FootprintMatchData
{
	public string TileOwnerGUID;

	public int TileIndex;

	public Vector2 FootprintCoordinate;

	public FootprintMatchData(string tileOwnerGUID, int tileIndex, Vector2 footprintCoordinate)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		TileOwnerGUID = tileOwnerGUID;
		TileIndex = tileIndex;
		FootprintCoordinate = footprintCoordinate;
	}
}
