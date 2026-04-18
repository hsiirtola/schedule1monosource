using System;
using System.Collections.Generic;
using ScheduleOne.Management;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class RouteListFieldUI : MonoBehaviour
{
	[Header("References")]
	public string FieldText = "Routes";

	public TextMeshProUGUI FieldLabel;

	public RouteEntryUI[] RouteEntries;

	public RectTransform MultiEditBlocker;

	public Button AddButton;

	public List<RouteListField> Fields { get; protected set; } = new List<RouteListField>();

	private void Start()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		((TMP_Text)FieldLabel).text = FieldText;
		for (int i = 0; i < RouteEntries.Length; i++)
		{
			RouteEntryUI entry = RouteEntries[i];
			RouteEntries[i].onDeleteClicked.AddListener((UnityAction)delegate
			{
				EntryDeleteClicked(entry);
			});
		}
		((UnityEvent)AddButton.onClick).AddListener(new UnityAction(AddClicked));
	}

	public void Bind(List<RouteListField> field)
	{
		Fields = new List<RouteListField>();
		Fields.AddRange(field);
		Refresh(Fields[0].Routes);
		Fields[0].onListChanged.AddListener((UnityAction<List<AdvancedTransitRoute>>)Refresh);
		((Component)MultiEditBlocker).gameObject.SetActive(Fields.Count > 1);
	}

	private void Refresh(List<AdvancedTransitRoute> newVal)
	{
		int num = 0;
		for (int i = 0; i < RouteEntries.Length; i++)
		{
			if (newVal.Count > i)
			{
				num++;
				RouteEntries[i].AssignRoute(newVal[i]);
				((Component)RouteEntries[i]).gameObject.SetActive(true);
			}
			else
			{
				RouteEntries[i].ClearRoute();
				((Component)RouteEntries[i]).gameObject.SetActive(false);
			}
		}
		for (int j = 0; j < newVal.Count; j++)
		{
			AdvancedTransitRoute advancedTransitRoute = newVal[j];
			advancedTransitRoute.onSourceChange = (Action<ITransitEntity>)Delegate.Remove(advancedTransitRoute.onSourceChange, new Action<ITransitEntity>(RouteChanged));
			AdvancedTransitRoute advancedTransitRoute2 = newVal[j];
			advancedTransitRoute2.onDestinationChange = (Action<ITransitEntity>)Delegate.Remove(advancedTransitRoute2.onDestinationChange, new Action<ITransitEntity>(RouteChanged));
			AdvancedTransitRoute advancedTransitRoute3 = newVal[j];
			advancedTransitRoute3.onSourceChange = (Action<ITransitEntity>)Delegate.Combine(advancedTransitRoute3.onSourceChange, new Action<ITransitEntity>(RouteChanged));
			AdvancedTransitRoute advancedTransitRoute4 = newVal[j];
			advancedTransitRoute4.onDestinationChange = (Action<ITransitEntity>)Delegate.Combine(advancedTransitRoute4.onDestinationChange, new Action<ITransitEntity>(RouteChanged));
		}
		((Component)AddButton).gameObject.SetActive(num < Fields[0].MaxRoutes);
	}

	private void EntryDeleteClicked(RouteEntryUI entry)
	{
		Fields[0].RemoveItem(entry.AssignedRoute);
		entry.ClearRoute();
	}

	private void AddClicked()
	{
		Fields[0].AddItem(new AdvancedTransitRoute(null, null));
	}

	private void RouteChanged(ITransitEntity newEntity)
	{
		Fields[0].Replicate();
	}
}
