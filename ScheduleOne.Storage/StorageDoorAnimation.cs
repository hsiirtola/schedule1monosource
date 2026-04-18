using System;
using ScheduleOne.Audio;
using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.Storage;

public class StorageDoorAnimation : MonoBehaviour
{
	private bool overriddeIsOpen;

	private bool overrideState;

	[SerializeField]
	private bool _disableItemContainerWhenClosed = true;

	[Header("Animations")]
	public Animation[] Anims;

	public AnimationClip OpenAnim;

	public AnimationClip CloseAnim;

	public AudioSourceController OpenSound;

	public AudioSourceController CloseSound;

	private StorageEntity storageEntity;

	private Transform itemContainer;

	public bool IsOpen { get; protected set; }

	private void Start()
	{
		storageEntity = ((Component)this).GetComponentInParent<StorageEntity>();
		if ((Object)(object)storageEntity == (Object)null)
		{
			((Behaviour)this).enabled = false;
			return;
		}
		StorageEntity obj = storageEntity;
		obj.onOpened = (Action)Delegate.Combine(obj.onOpened, new Action(Open));
		StorageEntity obj2 = storageEntity;
		obj2.onClosed = (Action)Delegate.Combine(obj2.onClosed, new Action(Close));
		StorageEntityVisualizer component = ((Component)storageEntity).GetComponent<StorageEntityVisualizer>();
		if ((Object)(object)component != (Object)null)
		{
			itemContainer = component.ItemContainer;
		}
		RefreshItemsVisible();
	}

	[Button]
	public void Open()
	{
		SetIsOpen(open: true);
	}

	[Button]
	public void Close()
	{
		SetIsOpen(open: false);
	}

	public void SetIsOpen(bool open)
	{
		if (overriddeIsOpen)
		{
			open = overrideState;
		}
		if (IsOpen == open)
		{
			return;
		}
		IsOpen = open;
		if (IsOpen)
		{
			RefreshItemsVisible();
		}
		if (Anims != null)
		{
			for (int i = 0; i < Anims.Length; i++)
			{
				Anims[i].Play(IsOpen ? ((Object)OpenAnim).name : ((Object)CloseAnim).name);
			}
		}
		if (IsOpen)
		{
			if ((Object)(object)OpenSound != (Object)null)
			{
				OpenSound.Play();
			}
		}
		else if ((Object)(object)CloseSound != (Object)null)
		{
			CloseSound.Play();
		}
		if (!open)
		{
			((MonoBehaviour)this).Invoke("RefreshItemsVisible", CloseAnim.length);
		}
	}

	protected virtual void RefreshItemsVisible()
	{
		if ((Object)(object)itemContainer != (Object)null)
		{
			((Component)itemContainer).gameObject.SetActive(IsOpen || !_disableItemContainerWhenClosed);
		}
	}

	public void OverrideState(bool open)
	{
		overriddeIsOpen = true;
		overrideState = open;
		SetIsOpen(open);
	}

	public void ResetOverride()
	{
		overriddeIsOpen = false;
	}
}
