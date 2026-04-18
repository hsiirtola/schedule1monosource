using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.Messaging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Messages;

public class ConfirmationPopup : MonoBehaviour
{
	public enum EResponse
	{
		Confirm,
		Cancel
	}

	[Header("References")]
	public GameObject Container;

	public Text TitleLabel;

	public Text MessageLabel;

	public Button ConfirmButton;

	public Button CancelButton;

	private MSGConversation conversation;

	private Action<EResponse> responseCallback;

	public bool IsOpen { get; private set; }

	private void Start()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		GameInput.RegisterExitListener(Exit, 4);
		Close(EResponse.Cancel);
		((UnityEvent)ConfirmButton.onClick).AddListener(new UnityAction(Confirm));
		((UnityEvent)CancelButton.onClick).AddListener(new UnityAction(Cancel));
	}

	public void Exit(ExitAction action)
	{
		if (!action.Used && IsOpen && PlayerSingleton<Phone>.Instance.IsOpen)
		{
			action.Used = true;
			Cancel();
		}
	}

	public void Open(string title, string message, MSGConversation conv, Action<EResponse> callback)
	{
		IsOpen = true;
		conversation = conv;
		MSGConversation mSGConversation = conversation;
		mSGConversation.onMessageRendered = (Action)Delegate.Combine(mSGConversation.onMessageRendered, new Action(Cancel));
		responseCallback = callback;
		TitleLabel.text = title;
		MessageLabel.text = message;
		Container.gameObject.SetActive(true);
	}

	public void Close(EResponse outcome)
	{
		IsOpen = false;
		if (conversation != null)
		{
			MSGConversation mSGConversation = conversation;
			mSGConversation.onMessageRendered = (Action)Delegate.Remove(mSGConversation.onMessageRendered, new Action(Cancel));
		}
		if (responseCallback != null)
		{
			responseCallback(outcome);
			responseCallback = null;
		}
		Container.gameObject.SetActive(false);
	}

	private void Confirm()
	{
		Close(EResponse.Confirm);
	}

	private void Cancel()
	{
		Close(EResponse.Cancel);
	}
}
