using System;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Cartel;
using ScheduleOne.DevUtilities;
using ScheduleOne.Map;
using ScheduleOne.NPCs;
using ScheduleOne.UI.Relations;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.ContactsApp;

public class ContactsApp : App<ContactsApp>
{
	[Serializable]
	public class RegionUI
	{
		public EMapRegion Region;

		public Button Button;

		public RectTransform Container;

		public RectTransform ConnectionsContainer;

		public List<NPC> npcs { get; set; } = new List<NPC>();
	}

	public EMapRegion SelectedRegion;

	private Dictionary<EMapRegion, RegionUI> RegionDict = new Dictionary<EMapRegion, RegionUI>();

	[Header("References")]
	public PinchableScrollRect ScrollRect;

	public RectTransform CirclesContainer;

	public RectTransform DemoCirclesContainer;

	public RectTransform TutorialCirclesContainer;

	public RectTransform ConnectionsContainer;

	public RectTransform ContentRect;

	public RectTransform SelectionIndicator;

	public ContactsDetailPanel DetailPanel;

	public RegionUI[] RegionUIs;

	public RectTransform RegionSelectionContainer;

	public RectTransform RegionSelectionIndicator;

	public RectTransform InfluenceContainer;

	public Slider InfluenceSlider;

	public Text InfluenceCountLabel;

	public RectTransform UnlockRegionSliderNotch;

	public Text InfluenceText;

	public RectTransform LowerContainer;

	public RectTransform HorizontalScrollbarRectTransform;

	public RectTransform RegionLockedContainer;

	public RectTransform RegionLocked_Rank;

	public RectTransform RegionLocked_CartelInfluence;

	public Text RegionLocked_CartelInfluence_Text;

	public RectTransform RegionLocked_Unavailable;

	[Header("Prefabs")]
	public GameObject ConnectionPrefab;

	[Header("Custom UI")]
	[SerializeField]
	protected UIScreen uiScreen;

	[SerializeField]
	protected UIMapPanel uiPanel;

	private List<RelationCircle> RelationCircles = new List<RelationCircle>();

	private Coroutine contentMoveRoutine;

	private List<Tuple<NPC, NPC>> connections = new List<Tuple<NPC, NPC>>();

	protected override void Start()
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Expected O, but got Unknown
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Expected O, but got Unknown
		base.Start();
		if (NetworkSingleton<GameManager>.Instance.IsTutorial)
		{
			((Component)CirclesContainer).gameObject.SetActive(false);
			((Component)DemoCirclesContainer).gameObject.SetActive(false);
			((Component)TutorialCirclesContainer).gameObject.SetActive(true);
			CirclesContainer = TutorialCirclesContainer;
			((Component)RegionSelectionContainer).gameObject.SetActive(false);
		}
		else
		{
			((Component)DemoCirclesContainer).gameObject.SetActive(false);
			((Component)TutorialCirclesContainer).gameObject.SetActive(false);
			((Component)CirclesContainer).gameObject.SetActive(true);
			((Component)RegionSelectionContainer).gameObject.SetActive(true);
			RegionUI[] regionUIs = RegionUIs;
			foreach (RegionUI regionUI in regionUIs)
			{
				RegionUI cacheReg = regionUI;
				((UnityEvent)regionUI.Button.onClick).AddListener((UnityAction)delegate
				{
					SetSelectedRegion(cacheReg.Region, selectNPC: true);
				});
				RegionDict.Add(regionUI.Region, regionUI);
			}
			SetSelectedRegion(SelectedRegion, selectNPC: true);
		}
		RelationCircles = ((Component)CirclesContainer).GetComponentsInChildren<RelationCircle>(true).ToList();
		foreach (RelationCircle rel in RelationCircles)
		{
			rel.LoadNPCData();
			if ((Object)(object)rel.AssignedNPC == (Object)null)
			{
				Console.LogWarning("Failed to find NPC for relation circle with ID '" + rel.AssignedNPC_ID + "'");
				continue;
			}
			rel.uiMapItem.SetMapPanel(uiPanel);
			uiPanel.RegisterMapItem(rel.uiMapItem);
			RegionUIs.First((RegionUI x) => x.Region == rel.AssignedNPC.Region).npcs.Add(rel.AssignedNPC);
			foreach (NPC other in rel.AssignedNPC.RelationData.Connections)
			{
				if ((Object)(object)other == (Object)null)
				{
					continue;
				}
				if (other.Region != rel.AssignedNPC.Region)
				{
					Console.LogWarning("Connection between " + rel.AssignedNPC.fullName + " and " + other.fullName + " is invalid because they are in different regions");
				}
				else
				{
					if (connections.Exists((Tuple<NPC, NPC> x) => (Object)(object)x.Item1 == (Object)(object)rel.AssignedNPC && (Object)(object)x.Item2 == (Object)(object)other) || connections.Exists((Tuple<NPC, NPC> x) => (Object)(object)x.Item1 == (Object)(object)other && (Object)(object)x.Item2 == (Object)(object)rel.AssignedNPC))
					{
						continue;
					}
					connections.Add(new Tuple<NPC, NPC>(rel.AssignedNPC, other));
					RelationCircle otherCirc = GetRelationCircle(other.ID);
					if ((Object)(object)otherCirc == (Object)null)
					{
						Console.LogWarning("Failed to find relation circle for NPC with ID '" + other.ID + "'");
						continue;
					}
					RectTransform connectionsContainer = ConnectionsContainer;
					if (!GameManager.IS_TUTORIAL)
					{
						connectionsContainer = RegionDict[rel.AssignedNPC.Region].ConnectionsContainer;
					}
					RectTransform component = Object.Instantiate<GameObject>(ConnectionPrefab, (Transform)(object)connectionsContainer).GetComponent<RectTransform>();
					component.anchoredPosition = (otherCirc.Rect.anchoredPosition + rel.Rect.anchoredPosition) / 2f;
					Vector3 val = Vector2.op_Implicit(otherCirc.Rect.anchoredPosition - rel.Rect.anchoredPosition);
					float num = (0f - Mathf.Atan2(val.x, val.y)) * 57.29578f;
					((Transform)component).localRotation = Quaternion.Euler(0f, 0f, num);
					component.sizeDelta = new Vector2(component.sizeDelta.x, Vector3.Distance(Vector2.op_Implicit(otherCirc.Rect.anchoredPosition), Vector2.op_Implicit(rel.Rect.anchoredPosition)));
					RelationCircle cacheRel = rel;
					((Object)component).name = rel.AssignedNPC_ID + " -> " + other.ID;
					((UnityEvent)((Component)((Transform)component).Find("StartButton")).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
					{
						ZoomToRect(otherCirc.Rect);
					});
					((UnityEvent)((Component)((Transform)component).Find("EndButton")).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
					{
						ZoomToRect(cacheRel.Rect);
					});
				}
			}
		}
		foreach (RelationCircle relationCircle in RelationCircles)
		{
			RelationCircle circ = relationCircle;
			relationCircle.onClicked = (Action)Delegate.Combine(relationCircle.onClicked, (Action)delegate
			{
				CircleClicked(circ);
			});
		}
		if (RelationCircles.Count > 0)
		{
			uiPanel.ResetSnappedItem();
			Select(RelationCircles[0]);
		}
	}

	protected override void Update()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (base.isOpen)
		{
			if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) && contentMoveRoutine != null)
			{
				StopContentMove();
			}
			if (!GameManager.IS_TUTORIAL && ((Component)RegionSelectionIndicator).gameObject.activeSelf)
			{
				float x = ((Component)RegionDict[SelectedRegion].Button).GetComponent<RectTransform>().anchoredPosition.x;
				float x2 = RegionSelectionIndicator.anchoredPosition.x;
				RegionSelectionIndicator.anchoredPosition = new Vector2(Mathf.MoveTowards(x2, x, 1500f * Time.deltaTime), RegionSelectionIndicator.anchoredPosition.y);
			}
		}
	}

	private RelationCircle GetRelationCircle(string npcID)
	{
		return RelationCircles.Find((RelationCircle x) => x.AssignedNPC_ID.ToLower() == npcID.ToLower());
	}

	private void CircleClicked(RelationCircle circ)
	{
		Select(circ);
	}

	private void Select(RelationCircle circ)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		DetailPanel.Open(circ.AssignedNPC);
		if (GameInput.GetCurrentInputDeviceIsKeyboardMouse())
		{
			ZoomToRect(circ.Rect);
		}
		((Transform)SelectionIndicator).position = ((Transform)circ.Rect).position;
	}

	public void SetSelectedRegion(EMapRegion region, bool selectNPC)
	{
		if (NetworkSingleton<GameManager>.Instance.IsTutorial)
		{
			((Component)RegionLockedContainer).gameObject.SetActive(false);
			SetCartelInfluenceDisplayVisible(vis: false);
			return;
		}
		SelectedRegion = region;
		MapRegionData regionData = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(region);
		uiPanel.LockMapInput = !regionData.IsUnlocked;
		RegionUI[] regionUIs = RegionUIs;
		foreach (RegionUI regionUI in regionUIs)
		{
			MapRegionData regionData2 = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(regionUI.Region);
			((Component)regionUI.Container).gameObject.SetActive(regionUI.Region == region);
			((Component)regionUI.ConnectionsContainer).gameObject.SetActive(regionUI.Region == region);
			((Selectable)regionUI.Button).interactable = regionUI.Region != region;
			((Component)((Component)regionUI.Button).transform.Find("Locked")).gameObject.SetActive(!regionData2.IsUnlocked);
		}
		if (regionData.StartingNPCs.Length != 0 && selectNPC)
		{
			RelationCircle relationCircle = GetRelationCircle(regionData.StartingNPCs[0].ID);
			if ((Object)(object)relationCircle != (Object)null)
			{
				uiPanel.ResetSnappedItem();
				Select(relationCircle);
			}
		}
		((ScrollRect)ScrollRect).vertical = regionData.IsUnlocked;
		((ScrollRect)ScrollRect).horizontal = regionData.IsUnlocked;
		((Behaviour)ScrollRect).enabled = regionData.IsUnlocked;
		if (NetworkSingleton<ScheduleOne.Cartel.Cartel>.InstanceExists)
		{
			SetCartelInfluenceDisplayVisible(region != EMapRegion.Northtown && regionData.IsUnlocked && NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status == ECartelStatus.Hostile && !GameManager.IS_TUTORIAL);
			((Component)UnlockRegionSliderNotch).gameObject.SetActive(false);
			InfluenceSlider.value = NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Influence.GetInfluence(region);
			InfluenceCountLabel.text = Mathf.RoundToInt(NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Influence.GetInfluence(region) * 1000f) + " / 1000";
			InfluenceText.text = "Cartel influence in " + region;
			if (region != EMapRegion.Uptown)
			{
				EMapRegion region2 = region + 1;
				MapRegionData regionData3 = Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(region2);
				((Component)UnlockRegionSliderNotch).gameObject.SetActive(!regionData3.IsUnlocked);
			}
		}
		else
		{
			SetCartelInfluenceDisplayVisible(vis: false);
		}
		if (regionData.IsUnlocked)
		{
			((Component)RegionLockedContainer).gameObject.SetActive(false);
			return;
		}
		((Component)RegionLockedContainer).gameObject.SetActive(true);
		((Component)RegionLocked_CartelInfluence).gameObject.SetActive(false);
		((Component)RegionLocked_Rank).gameObject.SetActive(false);
		((Component)RegionLocked_Unavailable).gameObject.SetActive(false);
		if (region == EMapRegion.Westville)
		{
			((Component)RegionLocked_Rank).gameObject.SetActive(true);
			return;
		}
		if (NetworkSingleton<ScheduleOne.Cartel.Cartel>.Instance.Status == ECartelStatus.Truced)
		{
			((Component)RegionLocked_Unavailable).gameObject.SetActive(true);
			return;
		}
		EMapRegion eMapRegion = region - 1;
		((Component)RegionLocked_CartelInfluence).gameObject.SetActive(true);
		RegionLocked_CartelInfluence_Text.text = "Reduce cartel influence in " + eMapRegion.ToString() + " to\n\n\nto unlock this region";
		void SetCartelInfluenceDisplayVisible(bool vis)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			((Component)LowerContainer).gameObject.SetActive(vis);
			HorizontalScrollbarRectTransform.anchoredPosition = new Vector2(0f, vis ? 77f : 7f);
		}
	}

	private void ZoomToRect(RectTransform rect)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		ContentRect.pivot = new Vector2(0f, 1f);
		float startScale = ((Transform)ContentRect).localScale.x;
		float endScale = 1f;
		Vector2 endPos = new Vector2((0f - ContentRect.sizeDelta.x) / 2f, ContentRect.sizeDelta.y / 2f);
		endPos.x -= rect.anchoredPosition.x;
		endPos.y -= rect.anchoredPosition.y;
		StopContentMove();
		((Transform)ContentRect).localScale = new Vector3(endScale, endScale, endScale);
		ContentRect.anchoredPosition = endPos;
	}

	private void StopContentMove()
	{
		if (contentMoveRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(contentMoveRoutine);
		}
	}

	public override void SetOpen(bool open)
	{
		base.SetOpen(open);
		if (NetworkSingleton<VariableDatabase>.InstanceExists)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("Contacts_App_Open", open.ToString(), network: false);
		}
		if (open)
		{
			DetailPanel.Open(DetailPanel.SelectedNPC);
			SetSelectedRegion(SelectedRegion, selectNPC: false);
			uiScreen.SetCurrentSelectedPanel(uiPanel);
		}
	}
}
