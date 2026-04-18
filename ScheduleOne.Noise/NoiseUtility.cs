using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using UnityEngine;

namespace ScheduleOne.Noise;

public static class NoiseUtility
{
	public static void EmitNoise(Vector3 origin, ENoiseType type, float range, GameObject source = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		NoiseEvent noiseEvent = new NoiseEvent(origin, range, type, source);
		for (int i = 0; i < Listener.listeners.Count; i++)
		{
			if (((Behaviour)Listener.listeners[i]).enabled && Vector3.Magnitude(origin - Listener.listeners[i].HearingOrigin.position) <= Listener.listeners[i].Sensitivity * range && (!noiseEvent.OriginInSewer || !Singleton<SewerCameraPresense>.InstanceExists || Singleton<SewerCameraPresense>.Instance.IsPointInSewerArea(Listener.listeners[i].HearingOrigin.position)))
			{
				Listener.listeners[i].Notify(noiseEvent);
			}
		}
	}
}
