using UnityEngine;

namespace ScheduleOne.Tools;

public struct ApproximateVector3
{
	public short X;

	public short Y;

	public short Z;

	public ApproximateVector3(float x, float y, float z)
	{
		X = (short)(x * 10f);
		Y = (short)(y * 10f);
		Z = (short)(z * 10f);
	}

	public ApproximateVector3(Vector3 vector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		X = (short)(vector.x * 10f);
		Y = (short)(vector.y * 10f);
		Z = (short)(vector.z * 10f);
	}

	public Vector3 ToVector3()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return new Vector3((float)X / 10f, (float)Y / 10f, (float)Z / 10f);
	}
}
