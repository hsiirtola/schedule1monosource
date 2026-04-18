using System.Collections.Generic;

namespace ScheduleOne.Persistence.Datas;

public class SewerData : SaveData
{
	public bool IsSewerUnlocked;

	public bool IsRandomWorldKeyCollected;

	public int RandomSewerKeyLocationIndex = -1;

	public bool HasSewerKingBeenDefeated;

	public int HoursSinceLastSewerGoblinAppearance = 9999;

	public int RandomKeyPossessorIndex = -1;

	public bool RandomKeyPossessorSet;

	public List<int> ActiveMushroomLocationIndices = new List<int>();

	public SewerData(bool isSewerUnlocked, bool isRandomWorldKeyCollected, int randomSewerKeyLocationIndex, bool hasSewerKingBeenDefeated, int hoursSinceLastSewerGoblinAppearance, int randomKeyPossessorIndex, List<int> activeMushroomLocationIndices)
	{
		IsSewerUnlocked = isSewerUnlocked;
		IsRandomWorldKeyCollected = isRandomWorldKeyCollected;
		RandomSewerKeyLocationIndex = randomSewerKeyLocationIndex;
		HasSewerKingBeenDefeated = hasSewerKingBeenDefeated;
		HoursSinceLastSewerGoblinAppearance = hoursSinceLastSewerGoblinAppearance;
		RandomKeyPossessorIndex = randomKeyPossessorIndex;
		RandomKeyPossessorSet = true;
		ActiveMushroomLocationIndices = activeMushroomLocationIndices;
	}
}
