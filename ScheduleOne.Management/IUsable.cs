using FishNet.Object;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Management;

public interface IUsable
{
	bool IsInUse
	{
		get
		{
			if (!((Object)(object)NPCUserObject != (Object)null))
			{
				return (Object)(object)PlayerUserObject != (Object)null;
			}
			return true;
		}
	}

	bool IsUsedByLocalPlayer
	{
		get
		{
			if ((Object)(object)PlayerUserObject != (Object)null)
			{
				return PlayerUserObject.Owner.IsLocalClient;
			}
			return false;
		}
	}

	NetworkObject NPCUserObject { get; set; }

	NetworkObject PlayerUserObject { get; set; }

	string UserName
	{
		get
		{
			if ((Object)(object)NPCUserObject != (Object)null)
			{
				return ((Component)NPCUserObject).GetComponent<NPC>().fullName;
			}
			if ((Object)(object)PlayerUserObject != (Object)null)
			{
				return ((Component)PlayerUserObject).GetComponent<Player>().PlayerName;
			}
			return string.Empty;
		}
	}

	bool IsInUseByNPC(NPC npc)
	{
		return (Object)(object)NPCUserObject == (Object)(object)((NetworkBehaviour)npc).NetworkObject;
	}

	void SetPlayerUser(NetworkObject playerObject);

	void SetNPCUser(NetworkObject playerObject);
}
