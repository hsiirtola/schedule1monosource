using System;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Casino;

public class CasinoGameInteraction : MonoBehaviour
{
	public string GameName;

	[Header("References")]
	public CasinoGamePlayers Players;

	public InteractableObject IntObj;

	public Action<Player> onLocalPlayerRequestJoin;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
	}

	private void Hovered()
	{
		if (Players.CurrentPlayerCount < Players.PlayerLimit)
		{
			IntObj.SetMessage("Play " + GameName);
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
		}
		else
		{
			IntObj.SetMessage("Table is full");
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Invalid);
		}
	}

	private void Interacted()
	{
		if (Players.CurrentPlayerCount < Players.PlayerLimit && onLocalPlayerRequestJoin != null)
		{
			onLocalPlayerRequestJoin(Player.Local);
		}
	}
}
