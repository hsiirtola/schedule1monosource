using FishNet;
using UnityEngine;

namespace ScheduleOne.NPCs.Other;

public abstract class NPCDiscreteAction : MonoBehaviour
{
	public bool IsActive { get; protected set; }

	protected virtual void BeginOnServer()
	{
	}

	protected virtual void BeginOnClient()
	{
	}

	protected virtual void EndOnServer()
	{
	}

	protected virtual void EndOnClient()
	{
	}

	public void Begin()
	{
		IsActive = true;
		if (InstanceFinder.IsServer)
		{
			BeginOnServer();
		}
		if (InstanceFinder.IsClient)
		{
			BeginOnClient();
		}
	}

	public void End()
	{
		IsActive = false;
		if (InstanceFinder.IsServer)
		{
			EndOnServer();
		}
		if (InstanceFinder.IsClient)
		{
			EndOnClient();
		}
	}
}
