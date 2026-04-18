using System;
using ScheduleOne.Core;
using ScheduleOne.Economy;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Relation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI.Relations;

public class RelationCircle : MonoBehaviour
{
	public const float NotchMinRot = 90f;

	public const float NotchMaxRot = -90f;

	public static Color PortraitColor_ZeroDependence = Color32.op_Implicit(new Color32((byte)60, (byte)60, (byte)60, byte.MaxValue));

	public static Color PortraitColor_MaxDependence = Color32.op_Implicit(new Color32((byte)120, (byte)15, (byte)15, byte.MaxValue));

	public string AssignedNPC_ID = string.Empty;

	public NPC AssignedNPC;

	public Action onClicked;

	public Action onHoverStart;

	public Action onHoverEnd;

	public bool AutoSetName;

	[Header("References")]
	public RectTransform Rect;

	public Image PortraitBackground;

	public Image HeadshotImg;

	public RectTransform NotchPivot;

	public RectTransform Locked;

	public Button Button;

	public EventTrigger Trigger;

	[Header("Custom UI")]
	public UIMapItem uiMapItem;

	private void Awake()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		LoadNPCData();
		if ((Object)(object)AssignedNPC != (Object)null)
		{
			AssignNPC(AssignedNPC);
		}
		else if (AssignedNPC_ID != string.Empty)
		{
			Console.LogWarning("Failed to find NPC with ID '" + AssignedNPC_ID + "'");
		}
		((UnityEvent)Button.onClick).AddListener(new UnityAction(ButtonClicked));
		Entry val = new Entry();
		val.eventID = (EventTriggerType)0;
		((UnityEvent<BaseEventData>)(object)val.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverStart();
		});
		Trigger.triggers.Add(val);
		Entry val2 = new Entry();
		val2.eventID = (EventTriggerType)1;
		((UnityEvent<BaseEventData>)(object)val2.callback).AddListener((UnityAction<BaseEventData>)delegate
		{
			HoverEnd();
		});
		Trigger.triggers.Add(val2);
	}

	private void OnValidate()
	{
		if ((Object)(object)AssignedNPC != (Object)null)
		{
			AssignedNPC_ID = AssignedNPC.ID;
			HeadshotImg.sprite = AssignedNPC.MugshotSprite;
		}
		if (AutoSetName && (Object)(object)AssignedNPC != (Object)null)
		{
			((Object)((Component)this).gameObject).name = AssignedNPC_ID;
		}
	}

	public void AssignNPC(NPC npc)
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		if ((Object)(object)npc != (Object)null)
		{
			UnassignNPC();
		}
		AssignedNPC = npc;
		NPCRelationData relationData = AssignedNPC.RelationData;
		relationData.onRelationshipChange = (Action<float>)Delegate.Combine(relationData.onRelationshipChange, new Action<float>(RelationshipChange));
		NPCRelationData relationData2 = AssignedNPC.RelationData;
		relationData2.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData2.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(SetUnlocked));
		foreach (NPC connection in AssignedNPC.RelationData.Connections)
		{
			NPCRelationData relationData3 = connection.RelationData;
			relationData3.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Combine(relationData3.onUnlocked, (Action<NPCRelationData.EUnlockType, bool>)delegate
			{
				UpdateBlackout();
			});
		}
		if (npc.RelationData.Unlocked)
		{
			SetUnlocked(npc.RelationData.UnlockType, notify: false);
		}
		else
		{
			SetLocked();
		}
		if (npc is Dealer)
		{
			(npc as Dealer).onRecommended.AddListener(new UnityAction(UpdateBlackout));
		}
		HeadshotImg.sprite = AssignedNPC.MugshotSprite;
		RefreshNotchPosition();
		RefreshDependenceDisplay();
		UpdateBlackout();
	}

	private void UnassignNPC()
	{
		if ((Object)(object)AssignedNPC != (Object)null)
		{
			NPCRelationData relationData = AssignedNPC.RelationData;
			relationData.onRelationshipChange = (Action<float>)Delegate.Remove(relationData.onRelationshipChange, new Action<float>(RelationshipChange));
			NPCRelationData relationData2 = AssignedNPC.RelationData;
			relationData2.onUnlocked = (Action<NPCRelationData.EUnlockType, bool>)Delegate.Remove(relationData2.onUnlocked, new Action<NPCRelationData.EUnlockType, bool>(SetUnlocked));
		}
	}

	private void RelationshipChange(float change)
	{
		RefreshNotchPosition();
	}

	public void SetNotchPosition(float relationshipDelta)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		((Transform)NotchPivot).localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(90f, -90f, relationshipDelta / 5f));
	}

	private void RefreshNotchPosition()
	{
		SetNotchPosition(AssignedNPC.RelationData.RelationDelta);
	}

	private void RefreshDependenceDisplay()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Customer component = ((Component)AssignedNPC).GetComponent<Customer>();
		if ((Object)(object)component == (Object)null)
		{
			((Graphic)PortraitBackground).color = PortraitColor_ZeroDependence;
		}
		else
		{
			((Graphic)PortraitBackground).color = Color.Lerp(PortraitColor_ZeroDependence, PortraitColor_MaxDependence, component.CurrentAddiction);
		}
	}

	[Button]
	public void SetLocked()
	{
		((Component)Locked).gameObject.SetActive(true);
		((Component)NotchPivot).gameObject.SetActive(false);
	}

	[Button]
	public void SetUnlocked(NPCRelationData.EUnlockType unlockType, bool notify = true)
	{
		((Component)Locked).gameObject.SetActive(false);
		((Component)NotchPivot).gameObject.SetActive(true);
		SetBlackedOut(blackedOut: false);
	}

	[Button]
	public void LoadNPCData()
	{
		AssignedNPC = NPCManager.GetNPC(AssignedNPC_ID);
	}

	private void UpdateBlackout()
	{
		bool blackedOut = false;
		if (!AssignedNPC.RelationData.Unlocked)
		{
			if (AssignedNPC is Dealer)
			{
				blackedOut = !(AssignedNPC as Dealer).HasBeenRecommended;
			}
			else if (AssignedNPC is Supplier)
			{
				blackedOut = true;
			}
			else if ((Object)(object)((Component)AssignedNPC).GetComponent<Customer>() != (Object)null)
			{
				blackedOut = !AssignedNPC.RelationData.Unlocked && !AssignedNPC.RelationData.IsMutuallyKnown();
			}
		}
		SetBlackedOut(blackedOut);
	}

	public void SetBlackedOut(bool blackedOut)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)HeadshotImg).color = (blackedOut ? Color.black : Color.white);
	}

	private void ButtonClicked()
	{
		if (onClicked != null)
		{
			onClicked();
		}
	}

	private void HoverStart()
	{
		if (onHoverStart != null)
		{
			onHoverStart();
		}
	}

	private void HoverEnd()
	{
		if (onHoverEnd != null)
		{
			onHoverEnd();
		}
	}
}
