using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.Interaction;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts.Soil;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks.Tasks;

public class PourSoilTask : GrowContainerPourTask
{
	private SoilDefinition _soilDefinition;

	private PourableSoil _pourableSoil;

	private Collider _hoveredTopCollider;

	private GrowContainer _growContainer;

	public PourSoilTask(GrowContainer growContainer, ItemInstance itemInstance, Pourable pourablePrefab)
		: base(growContainer, itemInstance, pourablePrefab)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		base.CurrentInstruction = "Click and drag to cut bag";
		_pourableSoil = pourable as PourableSoil;
		_pourableSoil.onOpened.AddListener(new UnityAction(base.RemoveItem));
		_growContainer = growContainer;
		_soilDefinition = itemInstance.Definition as SoilDefinition;
	}

	protected override void OnInitialPour()
	{
		base.OnInitialPour();
		_growContainer.SetSoil(_pourableSoil.SoilDefinition);
		_growContainer.SetRemainingSoilUses(_soilDefinition.Uses);
		Singleton<OnScreenMouse>.Instance.Activate();
	}

	public override void Update()
	{
		base.Update();
		if (_pourableSoil.IsOpen)
		{
			base.CurrentInstruction = "Pour into pot (" + Mathf.FloorToInt(growContainer.NormalizedSoilAmount * 100f) + "%)";
		}
		UpdateHover();
		UpdateCursor();
		if ((Object)(object)_hoveredTopCollider != (Object)null && GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && _pourableSoil.TopColliders.IndexOf(_hoveredTopCollider) == _pourableSoil.currentCut)
		{
			_pourableSoil.Cut();
		}
	}

	public override void StopTask()
	{
		growContainer.SyncSoilData();
		Singleton<OnScreenMouse>.Instance.Deactivate();
		base.StopTask();
	}

	protected override void UpdateCursor()
	{
		if (_pourableSoil.IsOpen)
		{
			base.UpdateCursor();
		}
		else if ((Object)(object)_hoveredTopCollider != (Object)null && _pourableSoil.TopColliders.IndexOf(_hoveredTopCollider) == _pourableSoil.currentCut)
		{
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Scissors);
		}
		else
		{
			Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
		}
	}

	private void UpdateHover()
	{
		_hoveredTopCollider = GetHoveredTopCollider();
	}

	private Collider GetHoveredTopCollider()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSingleton<PlayerCamera>.Instance.MouseRaycast(3f, out var hit, Singleton<InteractionManager>.Instance.Interaction_SearchMask) && _pourableSoil.TopColliders.Contains(((RaycastHit)(ref hit)).collider))
		{
			return ((RaycastHit)(ref hit)).collider;
		}
		return null;
	}
}
