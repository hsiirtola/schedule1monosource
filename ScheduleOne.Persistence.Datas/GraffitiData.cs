using System.Collections.Generic;

namespace ScheduleOne.Persistence.Datas;

public class GraffitiData : SaveData
{
	public List<WorldSpraySurfaceData> SpraySurfaces = new List<WorldSpraySurfaceData>();

	public GraffitiData(List<WorldSpraySurfaceData> spraySurfaces)
	{
		SpraySurfaces = spraySurfaces;
	}
}
