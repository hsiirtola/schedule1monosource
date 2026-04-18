using System;
using ScheduleOne.Graffiti;
using ScheduleOne.Persistence.Datas;
using UnityEngine;

namespace ScheduleOne.Persistence.Loaders;

public class GraffitiLoader : Loader
{
	public override void Load(string mainPath)
	{
		if (!TryLoadFile(mainPath, out var contents))
		{
			return;
		}
		GraffitiData graffitiData = null;
		try
		{
			graffitiData = JsonUtility.FromJson<GraffitiData>(contents);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Error loading data: " + ex.Message));
		}
		if (graffitiData == null)
		{
			return;
		}
		foreach (WorldSpraySurfaceData spraySurface in graffitiData.SpraySurfaces)
		{
			try
			{
				LoadSpraySurface(spraySurface);
			}
			catch (Exception ex2)
			{
				Debug.LogError((object)("Failed to load spray surface: " + ex2.Message));
			}
		}
	}

	private void LoadSpraySurface(WorldSpraySurfaceData surfaceData)
	{
		if (surfaceData != null)
		{
			EnsureStrokesHaveValidSize(surfaceData);
			WorldSpraySurface worldSpraySurface = GUIDManager.GetObject<WorldSpraySurface>(new Guid(surfaceData.GUID));
			if ((Object)(object)worldSpraySurface != (Object)null)
			{
				worldSpraySurface.Set(null, surfaceData.Strokes.ToArray(), surfaceData.HasDrawingBeenFinalized, surfaceData.ContainsCartelGraffiti);
			}
		}
	}

	private void EnsureStrokesHaveValidSize(SpraySurfaceData surfaceData)
	{
		if (surfaceData == null)
		{
			return;
		}
		for (int i = 0; i < surfaceData.Strokes.Count; i++)
		{
			if (surfaceData.Strokes[i].StrokeSize <= 0)
			{
				surfaceData.Strokes[i].StrokeSize = 16;
			}
		}
	}
}
