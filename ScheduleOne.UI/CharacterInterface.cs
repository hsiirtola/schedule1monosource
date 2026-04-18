using System.Collections.Generic;
using System.Linq;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class CharacterInterface : MonoBehaviour
{
	public ClothingSlotUI[] ClothingSlots;

	public RectTransform Container;

	public Slider RotationSlider;

	private Dictionary<ClothingSlotUI, Transform> SlotAlignmentPoints = new Dictionary<ClothingSlotUI, Transform>();

	public bool IsOpen { get; private set; }

	private void Awake()
	{
		Close();
	}

	private void LateUpdate()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpen)
		{
			ClothingSlotUI[] clothingSlots = ClothingSlots;
			foreach (ClothingSlotUI clothingSlotUI in clothingSlots)
			{
				RectTransform component = ((Component)clothingSlotUI).GetComponent<RectTransform>();
				Transform val = SlotAlignmentPoints[clothingSlotUI];
				((Transform)component).position = Vector2.op_Implicit(RectTransformUtility.WorldToScreenPoint(Singleton<GameplayMenu>.Instance.OverlayCamera, val.position));
			}
		}
	}

	public void Open()
	{
		if (SlotAlignmentPoints.Count == 0)
		{
			ClothingSlotUI[] clothingSlots = ClothingSlots;
			foreach (ClothingSlotUI slotUI in clothingSlots)
			{
				slotUI.AssignSlot(Player.Local.Clothing.ClothingSlots[slotUI.SlotType]);
				CharacterDisplay.SlotAlignmentPoint slotAlignmentPoint = Singleton<CharacterDisplay>.Instance.AlignmentPoints.FirstOrDefault((CharacterDisplay.SlotAlignmentPoint x) => x.SlotType == slotUI.SlotType);
				if (slotAlignmentPoint != null)
				{
					SlotAlignmentPoints.Add(slotUI, slotAlignmentPoint.Point);
				}
				else
				{
					Console.LogError($"No alignment point found for slot type {slotUI.SlotType}");
				}
			}
		}
		IsOpen = true;
		((Component)Container).gameObject.SetActive(true);
		LateUpdate();
	}

	public void Close()
	{
		IsOpen = false;
		((Component)Container).gameObject.SetActive(false);
	}
}
