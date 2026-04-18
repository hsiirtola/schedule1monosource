using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.Management;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.Management;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Tools;

public class ManagementClipboard : Singleton<ManagementClipboard>
{
	public bool IsEquipped;

	public const float OpenTime = 0.06f;

	[Header("References")]
	public Transform ClipboardTransform;

	public Camera OverlayCamera;

	public Light OverlayLight;

	public SelectionInfoUI SelectionInfo;

	[Header("Settings")]
	public float ClosedOffset = -0.2f;

	public UnityEvent onClipboardEquipped;

	public UnityEvent onClipboardUnequipped;

	public UnityEvent onOpened;

	public UnityEvent onClosed;

	private Coroutine lerpRoutine;

	private List<IConfigurable> CurrentConfigurables = new List<IConfigurable>();

	public bool IsOpen { get; protected set; }

	public bool StatePreserved { get; protected set; }

	protected override void Awake()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		((Component)ClipboardTransform).gameObject.SetActive(false);
		ClipboardTransform.localPosition = new Vector3(ClipboardTransform.localPosition.x, ClosedOffset, ClipboardTransform.localPosition.z);
		GameInput.RegisterExitListener(Exit, 10);
	}

	private void Update()
	{
		for (int i = 0; i < CurrentConfigurables.Count; i++)
		{
			if (CurrentConfigurables[i].IsBeingConfiguredByOtherPlayer)
			{
				Close();
			}
		}
	}

	private void Exit(ExitAction exitAction)
	{
		if (IsOpen && !exitAction.Used)
		{
			Close();
			exitAction.Used = true;
		}
	}

	public void Open(List<IConfigurable> selection, ManagementClipboard_Equippable equippable)
	{
		IsOpen = true;
		((Behaviour)OverlayCamera).enabled = true;
		((Behaviour)OverlayLight).enabled = true;
		((Component)ClipboardTransform).gameObject.SetActive(true);
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0.06f);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: true, 0.06f);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		SelectionInfo.Set(selection);
		LerpToVerticalPosition(open: true, null);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		Singleton<ManagementInterface>.Instance.Open(selection, equippable);
		CurrentConfigurables.AddRange(selection);
		for (int i = 0; i < CurrentConfigurables.Count; i++)
		{
			CurrentConfigurables[i].SetConfigurer(((NetworkBehaviour)Player.Local).NetworkObject);
		}
		if (onOpened != null)
		{
			onOpened.Invoke();
		}
	}

	public void Close(bool preserveState = false)
	{
		IsOpen = false;
		StatePreserved = preserveState;
		((Behaviour)OverlayLight).enabled = false;
		PlayerSingleton<PlayerCamera>.Instance.SetDoFActive(active: false, 0.06f);
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		Singleton<ManagementInterface>.Instance.Close(preserveState);
		if (onClosed != null)
		{
			onClosed.Invoke();
		}
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		for (int i = 0; i < CurrentConfigurables.Count; i++)
		{
			if ((Object)(object)CurrentConfigurables[i].CurrentPlayerConfigurer == (Object)(object)((NetworkBehaviour)Player.Local).NetworkObject)
			{
				CurrentConfigurables[i].SetConfigurer(null);
			}
		}
		CurrentConfigurables.Clear();
		LerpToVerticalPosition(open: false, delegate
		{
			Done();
		});
		void Done()
		{
			if (!Singleton<GameplayMenu>.Instance.IsOpen)
			{
				((Component)ClipboardTransform).gameObject.SetActive(false);
				((Behaviour)OverlayCamera).enabled = false;
			}
		}
	}

	private void LerpToVerticalPosition(bool open, Action callback)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 endPos = new Vector3(ClipboardTransform.localPosition.x, open ? 0f : ClosedOffset, ClipboardTransform.localPosition.z);
		Vector3 startPos = ClipboardTransform.localPosition;
		if (lerpRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(lerpRoutine);
		}
		lerpRoutine = ((MonoBehaviour)this).StartCoroutine(Lerp());
		IEnumerator Lerp()
		{
			for (float i = 0f; i < 0.06f; i += Time.deltaTime)
			{
				((Component)ClipboardTransform).transform.localPosition = Vector3.Lerp(startPos, endPos, i / 0.06f);
				yield return (object)new WaitForEndOfFrame();
			}
			ClipboardTransform.localPosition = endPos;
			if (callback != null)
			{
				callback();
			}
			lerpRoutine = null;
		}
	}
}
