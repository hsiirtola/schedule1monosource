using FishNet.Object;
using FishNet.Serializing;

namespace ScheduleOne.Equipping.Framework;

public static class INetworkedEquippableUserSerializer
{
	public static void WriteINetworkedEquippableUser(this Writer writer, INetworkedEquippableUser value)
	{
		if (value == null)
		{
			writer.WriteNetworkBehaviour((NetworkBehaviour)null);
		}
		else
		{
			writer.WriteNetworkBehaviour(value.NetworkBehaviour);
		}
	}

	public static INetworkedEquippableUser ReadINetworkedEquippableUser(this Reader reader)
	{
		if (reader == null)
		{
			return null;
		}
		if (reader.Remaining == 0)
		{
			return null;
		}
		return reader.ReadNetworkBehaviour() as INetworkedEquippableUser;
	}
}
