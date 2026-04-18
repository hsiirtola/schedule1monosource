using Steamworks;
using UnityEngine;

namespace ScheduleOne.UI;

public class OpenSteamOverlay : MonoBehaviour
{
	public enum EType
	{
		Store,
		CustomLink
	}

	public const uint APP_ID = 3164500u;

	public EType Type;

	public string CustomLink;

	public void OpenOverlay()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (SteamManager.Initialized)
		{
			switch (Type)
			{
			case EType.Store:
				SteamFriends.ActivateGameOverlayToStore(new AppId_t(3164500u), (EOverlayToStoreFlag)0);
				break;
			case EType.CustomLink:
				SteamFriends.ActivateGameOverlayToWebPage(CustomLink, (EActivateGameOverlayToWebPageMode)0);
				break;
			}
		}
	}
}
