namespace ScheduleOne.Persistence.Datas;

public class NPCHealthData : SaveData
{
	public float Health;

	public bool IsDead;

	public int DaysPassedSinceDeath;

	public int HoursSinceAttackedByPlayer = 9999;

	public NPCHealthData(float health, bool isDead, int daysPassedSinceDeath, int hoursSinceAttackedByPlayer)
	{
		DataVersion = 1;
		Health = health;
		IsDead = isDead;
		DaysPassedSinceDeath = daysPassedSinceDeath;
		HoursSinceAttackedByPlayer = hoursSinceAttackedByPlayer;
	}
}
