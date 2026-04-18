using Pathfinding;
using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.Tools;

public class CleanNodeLinks : MonoBehaviour
{
	[Button]
	public void Clean()
	{
		NodeLink[] componentsInChildren = ((Component)this).GetComponentsInChildren<NodeLink>();
		foreach (NodeLink val in componentsInChildren)
		{
			if ((Object)(object)val.End == (Object)null)
			{
				Console.Log("Destroying link: " + ((Object)val).name);
				Object.DestroyImmediate((Object)(object)val);
			}
		}
	}
}
