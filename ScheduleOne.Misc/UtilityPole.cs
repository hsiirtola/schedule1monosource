using System.Collections.Generic;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Misc;

public class UtilityPole : MonoBehaviour
{
	public const float CABLE_CULL_DISTANCE = 100f;

	public const float CABLE_CULL_DISTANCE_SQR = 10000f;

	public UtilityPole previousPole;

	public UtilityPole nextPole;

	public bool Connection1Enabled = true;

	public bool Connection2Enabled = true;

	public float LengthFactor = 1.002f;

	[Header("References")]
	public Transform cable1Connection;

	public Transform cable2Connection;

	public List<Transform> cable1Segments = new List<Transform>();

	public List<Transform> cable2Segments = new List<Transform>();

	public Transform Cable1Container;

	public Transform Cable2Container;

	[Button]
	public void Orient()
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val;
		if ((Object)(object)previousPole == (Object)null && (Object)(object)nextPole == (Object)null)
		{
			Console.LogWarning("No neighbour poles!");
		}
		else if ((Object)(object)nextPole != (Object)null && (Object)(object)previousPole != (Object)null)
		{
			val = ((Component)this).transform.position - ((Component)previousPole).transform.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			val = ((Component)nextPole).transform.position - ((Component)this).transform.position;
			Vector3 normalized2 = ((Vector3)(ref val)).normalized;
			val = normalized + normalized2;
			Vector3 normalized3 = ((Vector3)(ref val)).normalized;
			((Component)this).transform.rotation = Quaternion.LookRotation(normalized3, Vector3.up);
		}
		else if ((Object)(object)previousPole != (Object)null)
		{
			val = ((Component)this).transform.position - ((Component)previousPole).transform.position;
			Vector3 normalized4 = ((Vector3)(ref val)).normalized;
			((Component)this).transform.rotation = Quaternion.LookRotation(normalized4, Vector3.up);
		}
		else if ((Object)(object)nextPole != (Object)null)
		{
			val = ((Component)nextPole).transform.position - ((Component)this).transform.position;
			Vector3 normalized5 = ((Vector3)(ref val)).normalized;
			((Component)this).transform.rotation = Quaternion.LookRotation(normalized5, Vector3.up);
		}
	}

	[Button]
	public void DrawLines()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)previousPole == (Object)null)
		{
			if (Connection1Enabled)
			{
				foreach (Transform cable1Segment in cable1Segments)
				{
					((Component)cable1Segment).gameObject.SetActive(false);
				}
			}
			if (!Connection2Enabled)
			{
				return;
			}
			{
				foreach (Transform cable2Segment in cable2Segments)
				{
					((Component)cable2Segment).gameObject.SetActive(false);
				}
				return;
			}
		}
		if (Connection1Enabled)
		{
			PowerLineUtility.DrawPowerLine(cable1Connection.position, previousPole.cable1Connection.position, cable1Segments, LengthFactor);
			foreach (Transform cable1Segment2 in cable1Segments)
			{
				((Component)cable1Segment2).gameObject.SetActive(true);
			}
		}
		if (!Connection2Enabled)
		{
			return;
		}
		PowerLineUtility.DrawPowerLine(cable2Connection.position, previousPole.cable2Connection.position, cable2Segments, LengthFactor);
		foreach (Transform cable2Segment2 in cable2Segments)
		{
			((Component)cable2Segment2).gameObject.SetActive(true);
		}
	}
}
