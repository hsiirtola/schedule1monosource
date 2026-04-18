using System;
using FishNet.Object;

namespace ScheduleOne.Vision;

[Serializable]
public class VisionEventReceipt
{
	public NetworkObject Target;

	public EVisualState State;

	public VisionEventReceipt(NetworkObject target, EVisualState state)
	{
		Target = target;
		State = state;
	}

	public VisionEventReceipt()
	{
	}
}
