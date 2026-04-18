using System;
using System.Collections.Generic;
using ScheduleOne.Police;
using UnityEngine;

namespace ScheduleOne.Law;

public class SentryLocation : MonoBehaviour
{
	[Serializable]
	public class SentryRoute
	{
		public Transform[] RoutePoints;

		public int MinutesPerPoint = 15;
	}

	[Header("References")]
	public List<SentryRoute> Routes = new List<SentryRoute>();

	public List<PoliceOfficer> AssignedOfficers { get; private set; } = new List<PoliceOfficer>();
}
