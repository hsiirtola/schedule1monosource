using System;
using UnityEngine;

namespace ScheduleOne.Persistence.Datas;

[Serializable]
public class PlayerData : SaveData
{
	public string PlayerCode;

	public Vector3 Position = Vector3.zero;

	public float Rotation;

	public bool IntroCompleted;

	public PlayerData(string playerCode, Vector3 playerPos, float playerRot, bool introCompleted)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		PlayerCode = playerCode;
		Position = playerPos;
		Rotation = playerRot;
		IntroCompleted = introCompleted;
	}

	public PlayerData()
	{
	}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
	//IL_0006: Unknown result type (might be due to invalid IL or missing references)

}
