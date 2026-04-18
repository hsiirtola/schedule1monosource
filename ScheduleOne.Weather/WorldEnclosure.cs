using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Weather;

public class WorldEnclosure : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private List<BasicEnclosure> _enclosures = new List<BasicEnclosure>();

	private List<BasicEnclosure> _blendZones = new List<BasicEnclosure>();

	private List<BasicEnclosure> _Enclosures = new List<BasicEnclosure>();

	public List<BasicEnclosure> Enclosures => _enclosures;

	private void Start()
	{
		foreach (BasicEnclosure enclosure in _enclosures)
		{
			if (enclosure.IsBlendZone)
			{
				_blendZones.Add(enclosure);
			}
			else
			{
				_Enclosures.Add(enclosure);
			}
		}
		NetworkSingleton<EnvironmentManager>.Instance.RegisterEnclosure(this);
	}

	public bool WithinEnclosure(Vector3 targetPosition, out float blend)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		blend = 0f;
		foreach (BasicEnclosure blendZone in _blendZones)
		{
			if (blendZone.WithinEnclosure(targetPosition))
			{
				blend = blendZone.GetEnclosureBlend(targetPosition);
				return true;
			}
		}
		foreach (BasicEnclosure enclosure in _Enclosures)
		{
			if (enclosure.WithinEnclosure(targetPosition))
			{
				blend = 1f;
				return true;
			}
		}
		return false;
	}
}
