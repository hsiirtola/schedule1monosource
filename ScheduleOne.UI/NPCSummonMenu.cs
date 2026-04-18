using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class NPCSummonMenu : Singleton<NPCSummonMenu>
{
	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public RectTransform EntryContainer;

	public RectTransform[] Entries;

	private Action<NPC> callback;

	public bool IsOpen { get; private set; }

	protected override void Start()
	{
		base.Start();
		GameInput.RegisterExitListener(Exit, 5);
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
	}

	private void Exit(ExitAction exit)
	{
		if (IsOpen && !exit.Used && exit.exitType == ExitType.Escape)
		{
			exit.Used = true;
			Close();
		}
	}

	public void Open(List<NPC> npcs, Action<NPC> _callback)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		IsOpen = true;
		((Behaviour)Canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
		callback = _callback;
		for (int i = 0; i < Entries.Length; i++)
		{
			if (npcs.Count > i)
			{
				((Component)((Transform)Entries[i]).Find("Icon")).GetComponent<Image>().sprite = npcs[i].MugshotSprite;
				((TMP_Text)((Component)((Transform)Entries[i]).Find("Name")).GetComponent<TextMeshProUGUI>()).text = npcs[i].fullName;
				((Component)Entries[i]).gameObject.SetActive(true);
				NPC npc = npcs[i];
				((UnityEventBase)((Component)Entries[i]).GetComponent<Button>().onClick).RemoveAllListeners();
				((UnityEvent)((Component)Entries[i]).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
				{
					NPCSelected(npc);
				});
			}
			else
			{
				((Component)Entries[i]).gameObject.SetActive(false);
			}
		}
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
	}

	public void Close()
	{
		IsOpen = false;
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		callback = null;
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
	}

	public void NPCSelected(NPC npc)
	{
		callback(npc);
		Close();
	}
}
