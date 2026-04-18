using ScheduleOne.Core.Items.Framework;
using ScheduleOne.ObjectScripts;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Management;

public class StorageUIElement : WorldspaceUIElement
{
	public Image Icon;

	public PlaceableStorageEntity AssignedEntity { get; protected set; }

	public void Initialize(PlaceableStorageEntity entity)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		AssignedEntity = entity;
		AssignedEntity.Configuration.onChanged.AddListener(new UnityAction(RefreshUI));
		Icon.sprite = ((BaseItemInstance)entity.ItemInstance).Icon;
		RefreshUI();
		((Component)this).gameObject.SetActive(false);
	}

	protected virtual void RefreshUI()
	{
		((TMP_Text)TitleLabel).text = AssignedEntity.GetManagementName();
	}
}
