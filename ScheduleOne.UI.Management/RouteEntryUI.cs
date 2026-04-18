using System;
using System.Collections.Generic;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.EntityFramework;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class RouteEntryUI : MonoBehaviour
{
	[Header("References")]
	public Image SourceIcon;

	public TextMeshProUGUI SourceLabel;

	public Image DestinationIcon;

	public TextMeshProUGUI DestinationLabel;

	public Image FilterIcon;

	public UnityEvent onDeleteClicked = new UnityEvent();

	private bool settingSource;

	private bool settingDestination;

	public AdvancedTransitRoute AssignedRoute { get; private set; }

	public void AssignRoute(AdvancedTransitRoute route)
	{
		AssignedRoute = route;
		RefreshUI();
	}

	public void ClearRoute()
	{
		AssignedRoute = null;
	}

	public void RefreshUI()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		SourceIcon.sprite = null;
		DestinationIcon.sprite = null;
		if (AssignedRoute != null && AssignedRoute.Source != null)
		{
			if (AssignedRoute.Source is BuildableItem)
			{
				SourceIcon.sprite = ((BaseItemInstance)(AssignedRoute.Source as BuildableItem).ItemInstance).Icon;
			}
			((TMP_Text)SourceLabel).text = AssignedRoute.Source.Name;
		}
		else
		{
			((TMP_Text)SourceLabel).text = "None";
		}
		((Component)SourceIcon).gameObject.SetActive((Object)(object)SourceIcon.sprite != (Object)null);
		((TMP_Text)SourceLabel).rectTransform.offsetMin = new Vector2(((Object)(object)SourceIcon.sprite != (Object)null) ? 29f : 5f, ((TMP_Text)SourceLabel).rectTransform.offsetMin.y);
		if (AssignedRoute != null && AssignedRoute.Destination != null)
		{
			if (AssignedRoute.Destination is BuildableItem)
			{
				DestinationIcon.sprite = ((BaseItemInstance)(AssignedRoute.Destination as BuildableItem).ItemInstance).Icon;
			}
			((TMP_Text)DestinationLabel).text = AssignedRoute.Destination.Name;
		}
		else
		{
			((TMP_Text)DestinationLabel).text = "None";
		}
		((Component)DestinationIcon).gameObject.SetActive((Object)(object)DestinationIcon.sprite != (Object)null);
		((TMP_Text)DestinationLabel).rectTransform.offsetMin = new Vector2(((Object)(object)DestinationIcon.sprite != (Object)null) ? 29f : 5f, ((TMP_Text)DestinationLabel).rectTransform.offsetMin.y);
	}

	public void SourceClicked()
	{
		settingSource = true;
		settingDestination = false;
		List<ITransitEntity> selectedObjects = new List<ITransitEntity>();
		List<Transform> list = new List<Transform>();
		if (AssignedRoute.Destination != null)
		{
			list.Add(AssignedRoute.Destination.LinkOrigin);
		}
		Singleton<ManagementInterface>.Instance.TransitEntitySelector.Open("Select source", "Click an entity to set it as the route source", 1, selectedObjects, new List<Type>(), ObjectValid, ObjectsSelected, list, selectingDestination: false);
	}

	public void DestinationClicked()
	{
		settingDestination = true;
		settingSource = false;
		List<ITransitEntity> selectedObjects = new List<ITransitEntity>();
		List<Transform> list = new List<Transform>();
		if (AssignedRoute.Source != null)
		{
			list.Add(AssignedRoute.Source.LinkOrigin);
		}
		Singleton<ManagementInterface>.Instance.TransitEntitySelector.Open("Select destination", "Click an entity to set it as the route destination", 1, selectedObjects, new List<Type>(), ObjectValid, ObjectsSelected, list);
	}

	public void FilterClicked()
	{
	}

	public void DeleteClicked()
	{
		if (onDeleteClicked != null)
		{
			onDeleteClicked.Invoke();
		}
	}

	private bool ObjectValid(ITransitEntity obj, out string reason)
	{
		reason = string.Empty;
		if (AssignedRoute == null)
		{
			return false;
		}
		if (obj == null)
		{
			return false;
		}
		if (settingDestination && obj == AssignedRoute.Source)
		{
			reason = "Destination cannot be the same as the source";
			return false;
		}
		if (settingSource && obj == AssignedRoute.Destination)
		{
			reason = "Source cannot be the same as the destination";
			return false;
		}
		return true;
	}

	public void ObjectsSelected(List<ITransitEntity> objs)
	{
		if (objs.Count > 1)
		{
			objs.RemoveAt(0);
		}
		if (settingSource)
		{
			AssignedRoute.SetSource((objs.Count > 0) ? objs[0] : null);
		}
		if (settingDestination)
		{
			AssignedRoute.SetDestination((objs.Count > 0) ? objs[0] : null);
		}
	}
}
