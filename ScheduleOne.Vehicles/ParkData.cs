using System;

namespace ScheduleOne.Vehicles;

[Serializable]
public class ParkData
{
	public Guid lotGUID;

	public int spotIndex;

	public EParkingAlignment alignment;

	public ParkData(Guid lotGUID, int spotIndex, EParkingAlignment alignment)
	{
		this.lotGUID = lotGUID;
		this.spotIndex = spotIndex;
		this.alignment = alignment;
	}

	public ParkData()
	{
	}
}
