using UnityEngine;

namespace ScheduleOne.Tools;

public class LogToConsole : MonoBehaviour
{
	public void Log(string message)
	{
		Console.Log(message);
	}

	public void LogWarning(string message)
	{
		Console.LogWarning(message);
	}

	public void LogError(string message)
	{
		Console.LogError(message);
	}
}
