using ScheduleOne.ItemFramework;
using UnityEngine;

namespace ScheduleOne.Growing;

public class Additive : MonoBehaviour
{
	public string AdditiveName = "Name";

	public AdditiveDefinition Definition;

	[Header("Plant effector settings")]
	public float QualityChange;

	public float YieldChange;

	public float GrowSpeedMultiplier = 1f;

	public float InstantGrowth;
}
