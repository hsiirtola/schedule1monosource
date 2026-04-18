using System;
using System.Collections.Generic;
using ScheduleOne.Graffiti;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class SpraySurfaceData : SaveData
{
	public List<SprayStroke> Strokes = new List<SprayStroke>();

	public bool ContainsCartelGraffiti;

	public SpraySurfaceData(List<SprayStroke> strokes, bool containsCartelGraffiti)
	{
		Strokes = strokes;
		ContainsCartelGraffiti = containsCartelGraffiti;
	}
}
