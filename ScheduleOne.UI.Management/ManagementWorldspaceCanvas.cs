using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Property;
using ScheduleOne.UI.Input;
using UnityEngine;

namespace ScheduleOne.UI.Management;

public class ManagementWorldspaceCanvas : Singleton<ManagementWorldspaceCanvas>
{
	public const float VISIBILITY_RANGE = 6f;

	public const float PROPERTY_CANVAS_RANGE = 50f;

	[Header("References")]
	public Canvas Canvas;

	public AnimationCurve ScaleCurve;

	public TransitLineVisuals TransitRouteVisualsPrefab;

	public InputPrompt CrosshairPrompt;

	[Header("Settings")]
	public LayerMask ObjectSelectionLayerMask;

	public Color HoveredOutlineColor = Color.white;

	public Color SelectedOutlineColor = Color.white;

	private List<IConfigurable> ShownConfigurables = new List<IConfigurable>();

	public IConfigurable HoveredConfigurable;

	private IConfigurable OutlinedConfigurable;

	public List<IConfigurable> SelectedConfigurables = new List<IConfigurable>();

	public bool IsOpen { get; protected set; }

	public ScheduleOne.Property.Property CurrentProperty => Singleton<PropertyManager>.Instance.GetNearestProperty(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);

	public void Open()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		IsOpen = true;
		((Behaviour)Canvas).enabled = true;
		for (int i = 0; i < SelectedConfigurables.Count; i++)
		{
			SelectedConfigurables[i].Selected();
			SelectedConfigurables[i].ShowOutline(SelectedOutlineColor);
		}
		UpdateInputPrompt();
	}

	public void Close(bool preserveSelection = false)
	{
		IsOpen = false;
		((Behaviour)Canvas).enabled = false;
		if (OutlinedConfigurable != null && !SelectedConfigurables.Contains(OutlinedConfigurable))
		{
			OutlinedConfigurable.Deselected();
			OutlinedConfigurable.HideOutline();
			OutlinedConfigurable = null;
		}
		if (HoveredConfigurable != null)
		{
			HoveredConfigurable.HideOutline();
			HoveredConfigurable = null;
		}
		if (!preserveSelection)
		{
			ClearSelection();
		}
	}

	private void Update()
	{
		if ((Object)(object)Player.Local == (Object)null)
		{
			return;
		}
		UpdateUIs();
		if (IsOpen)
		{
			IConfigurable hoveredConfigurable = GetHoveredConfigurable();
			if (hoveredConfigurable != null && !hoveredConfigurable.IsBeingConfiguredByOtherPlayer)
			{
				HoveredConfigurable = hoveredConfigurable;
			}
			else
			{
				HoveredConfigurable = null;
			}
			UpdateSelection();
		}
		else if (HoveredConfigurable != null)
		{
			HoveredConfigurable.Deselected();
			HoveredConfigurable.HideOutline();
			HoveredConfigurable = null;
		}
		UpdateInputPrompt();
		if ((Object)(object)Player.Local.CurrentProperty == (Object)null)
		{
			ClearSelection();
		}
	}

	private void UpdateInputPrompt()
	{
		List<IConfigurable> list = new List<IConfigurable>();
		if (HoveredConfigurable != null && !SelectedConfigurables.Contains(HoveredConfigurable))
		{
			list.Add(HoveredConfigurable);
		}
		list.AddRange(SelectedConfigurables);
		if (list.Count == 0)
		{
			HideCrosshairPrompt();
			return;
		}
		bool flag = true;
		if (list.Count > 1)
		{
			for (int i = 0; i < list.Count - 1; i++)
			{
				if (list[i].ConfigurableType != list[i + 1].ConfigurableType)
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			HideCrosshairPrompt();
			return;
		}
		string typeName = ConfigurableType.GetTypeName(list[0].ConfigurableType);
		if (list.Count > 1)
		{
			ShowCrosshairPrompt("Manage " + list.Count + "x " + typeName);
		}
		else
		{
			ShowCrosshairPrompt("Manage " + typeName);
		}
	}

	private void UpdateUIs()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		foreach (ScheduleOne.Property.Property ownedProperty in ScheduleOne.Property.Property.OwnedProperties)
		{
			float num = Vector3.Distance(((Component)ownedProperty).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
			((Component)ownedProperty.WorldspaceUIContainer).gameObject.SetActive(IsOpen && num < 50f);
		}
		List<IConfigurable> configurablesToShow = GetConfigurablesToShow();
		RemoveNullConfigurables();
		for (int i = 0; i < ShownConfigurables.Count; i++)
		{
			if (!configurablesToShow.Contains(ShownConfigurables[i]) && ShownConfigurables[i].WorldspaceUI.IsEnabled)
			{
				IConfigurable config = ShownConfigurables[i];
				ShownConfigurables[i].WorldspaceUI.Hide(delegate
				{
					ShownConfigurables.Remove(config);
				});
			}
		}
		for (int num2 = 0; num2 < configurablesToShow.Count; num2++)
		{
			if (!ShownConfigurables.Contains(configurablesToShow[num2]))
			{
				configurablesToShow[num2].WorldspaceUI.Show();
				if (!ShownConfigurables.Contains(configurablesToShow[num2]))
				{
					ShownConfigurables.Add(configurablesToShow[num2]);
				}
			}
		}
	}

	private void LateUpdate()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return;
		}
		RemoveNullConfigurables();
		ShownConfigurables.Sort((IConfigurable a, IConfigurable b) => Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, a.UIPoint.position).CompareTo(Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, b.UIPoint.position)));
		for (int num = 0; num < ShownConfigurables.Count; num++)
		{
			if (ShownConfigurables[num] != null && !ShownConfigurables[num].IsDestroyed)
			{
				ShownConfigurables[num].WorldspaceUI.SetInternalScale(ScaleCurve.Evaluate(Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ShownConfigurables[num].UIPoint.position) / 6f));
				ShownConfigurables[num].WorldspaceUI.UpdatePosition(ShownConfigurables[num].UIPoint.position);
				((Component)ShownConfigurables[num].WorldspaceUI).transform.SetAsFirstSibling();
			}
		}
	}

	private void UpdateSelection()
	{
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		if (GameInput.GetButtonDown(GameInput.ButtonCode.SecondaryClick))
		{
			ClearSelection();
		}
		if (HoveredConfigurable == null)
		{
			if (OutlinedConfigurable != null && !SelectedConfigurables.Contains(OutlinedConfigurable))
			{
				OutlinedConfigurable.Deselected();
				OutlinedConfigurable.HideOutline();
				OutlinedConfigurable = null;
			}
			return;
		}
		if (HoveredConfigurable != null && HoveredConfigurable.IsBeingConfiguredByOtherPlayer)
		{
			HoveredConfigurable.Deselected();
			HoveredConfigurable.HideOutline();
			HoveredConfigurable = null;
			return;
		}
		for (int i = 0; i < SelectedConfigurables.Count; i++)
		{
			if (SelectedConfigurables[i].IsBeingConfiguredByOtherPlayer)
			{
				RemoveFromSelection(SelectedConfigurables[i]);
				i--;
			}
		}
		if (!SelectedConfigurables.Contains(HoveredConfigurable) && OutlinedConfigurable != HoveredConfigurable)
		{
			if (OutlinedConfigurable != null && !SelectedConfigurables.Contains(OutlinedConfigurable))
			{
				OutlinedConfigurable.Deselected();
				OutlinedConfigurable.HideOutline();
				OutlinedConfigurable = null;
			}
			HoveredConfigurable.Selected();
			HoveredConfigurable.ShowOutline(HoveredOutlineColor);
			OutlinedConfigurable = HoveredConfigurable;
		}
		if (HoveredConfigurable != null && HoveredConfigurable.CanBeSelected && GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
		{
			if (SelectedConfigurables.Contains(HoveredConfigurable))
			{
				RemoveFromSelection(HoveredConfigurable);
			}
			else
			{
				AddToSelection(HoveredConfigurable);
			}
		}
	}

	private void AddToSelection(IConfigurable config)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!SelectedConfigurables.Contains(config))
		{
			config.ShowOutline(SelectedOutlineColor);
			config.Selected();
			SelectedConfigurables.Add(config);
		}
	}

	private void RemoveFromSelection(IConfigurable config)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (HoveredConfigurable != config)
		{
			config.Deselected();
			config.HideOutline();
		}
		else
		{
			config.ShowOutline(HoveredOutlineColor);
		}
		if (SelectedConfigurables.Contains(config))
		{
			SelectedConfigurables.Remove(config);
		}
	}

	private void ClearSelection()
	{
		IConfigurable[] array = SelectedConfigurables.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			RemoveFromSelection(array[i]);
		}
	}

	private void RemoveNullConfigurables()
	{
		for (int i = 0; i < ShownConfigurables.Count; i++)
		{
			if (ShownConfigurables[i].IsDestroyed)
			{
				ShownConfigurables.RemoveAt(i);
				i--;
			}
		}
	}

	private IConfigurable GetHoveredConfigurable()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSingleton<PlayerCamera>.Instance.LookRaycast(6f, out var hit, ObjectSelectionLayerMask))
		{
			IConfigurable componentInParent = ((Component)((RaycastHit)(ref hit)).collider).GetComponentInParent<IConfigurable>();
			if (componentInParent != null)
			{
				return componentInParent;
			}
		}
		return null;
	}

	private List<IConfigurable> GetConfigurablesToShow()
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return new List<IConfigurable>();
		}
		List<IConfigurable> list = new List<IConfigurable>();
		if ((Object)(object)CurrentProperty != (Object)null && CurrentProperty.IsOwned)
		{
			for (int i = 0; i < CurrentProperty.Configurables.Count; i++)
			{
				if (CurrentProperty.Configurables[i] != null && !CurrentProperty.Configurables[i].IsDestroyed && Vector3.Distance(CurrentProperty.Configurables[i].Transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position) <= 6f)
				{
					list.Add(CurrentProperty.Configurables[i]);
				}
			}
		}
		for (int j = 0; j < SelectedConfigurables.Count; j++)
		{
			if (!list.Contains(SelectedConfigurables[j]))
			{
				list.Add(SelectedConfigurables[j]);
			}
		}
		if (!list.Contains(HoveredConfigurable) && HoveredConfigurable != null)
		{
			list.Add(HoveredConfigurable);
		}
		return list;
	}

	public void ShowCrosshairPrompt(string message)
	{
		CrosshairPrompt.SetLabel(message);
		((Component)CrosshairPrompt).gameObject.SetActive(true);
		((Component)CrosshairPrompt).transform.SetAsLastSibling();
	}

	public void HideCrosshairPrompt()
	{
		((Component)CrosshairPrompt).gameObject.SetActive(false);
	}
}
