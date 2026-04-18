using FishNet.Connection;
using FishNet.Object;
using ScheduleOne.Property;
using ScheduleOne.UI.Management;
using UnityEngine;

namespace ScheduleOne.Management;

public interface IConfigurable
{
	EntityConfiguration Configuration { get; }

	ConfigurationReplicator ConfigReplicator { get; }

	EConfigurableType ConfigurableType { get; }

	WorldspaceUIElement WorldspaceUI { get; set; }

	NetworkObject CurrentPlayerConfigurer { get; set; }

	bool IsBeingConfiguredByOtherPlayer
	{
		get
		{
			if ((Object)(object)CurrentPlayerConfigurer != (Object)null)
			{
				return !CurrentPlayerConfigurer.IsOwner;
			}
			return false;
		}
	}

	Sprite TypeIcon { get; }

	Transform Transform { get; }

	Transform UIPoint { get; }

	bool IsDestroyed
	{
		get
		{
			if (this != null)
			{
				return (Object)(object)Transform == (Object)null;
			}
			return true;
		}
	}

	bool CanBeSelected { get; }

	ScheduleOne.Property.Property ParentProperty { get; }

	WorldspaceUIElement CreateWorldspaceUI();

	void DestroyWorldspaceUI();

	void ShowOutline(Color color);

	void HideOutline();

	void Selected()
	{
		Configuration.Selected();
	}

	void Deselected()
	{
		Configuration.Deselected();
	}

	void SetConfigurer(NetworkObject player);

	void SendConfigurationToClient(NetworkConnection conn);
}
