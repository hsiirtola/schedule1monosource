using System;
using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.Money;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI;

public class HospitalBillScreen : Singleton<HospitalBillScreen>
{
	public const float BILL_COST = 250f;

	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public CanvasGroup CanvasGroup;

	public TextMeshProUGUI PatientNameLabel;

	public TextMeshProUGUI BillNumberLabel;

	public TextMeshProUGUI PaidAmountLabel;

	private bool arrested;

	public bool isOpen { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		isOpen = false;
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		CanvasGroup.alpha = 0f;
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		GameInput.RegisterExitListener(Exit, 20);
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && isOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close();
		}
	}

	private void PlayerSpawned()
	{
		((TMP_Text)PatientNameLabel).text = Player.Local.PlayerName;
	}

	public void Open()
	{
		isOpen = true;
		arrested = Player.Local.IsArrested;
		((Behaviour)Canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
		CanvasGroup.alpha = 1f;
		CanvasGroup.interactable = true;
		((TMP_Text)BillNumberLabel).text = Random.Range(10000000, 100000000).ToString();
		float amount = Mathf.Min(250f, NetworkSingleton<MoneyManager>.Instance.cashBalance);
		((TMP_Text)PaidAmountLabel).text = MoneyManager.FormatAmount(amount, showDecimals: true);
		Singleton<PostProcessingManager>.Instance.SetBlur(1f);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		Player.Deactivate(freeMouse: false);
	}

	public void Close()
	{
		if (CanvasGroup.interactable && isOpen)
		{
			CanvasGroup.interactable = false;
			float num = Mathf.Min(250f, NetworkSingleton<MoneyManager>.Instance.cashBalance);
			NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(0f - num);
			if (arrested)
			{
				CanvasGroup.alpha = 0f;
				((Behaviour)Canvas).enabled = false;
				((Component)Container).gameObject.SetActive(false);
				PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
				isOpen = false;
				Singleton<ArrestNoticeScreen>.Instance.Open();
			}
			else
			{
				((MonoBehaviour)this).StartCoroutine(CloseRoutine());
			}
		}
		IEnumerator CloseRoutine()
		{
			float lerpTime = 0.3f;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				CanvasGroup.alpha = Mathf.Lerp(1f, 0f, i / lerpTime);
				Singleton<PostProcessingManager>.Instance.SetBlur(CanvasGroup.alpha);
				yield return (object)new WaitForEndOfFrame();
			}
			CanvasGroup.alpha = 0f;
			((Behaviour)Canvas).enabled = false;
			((Component)Container).gameObject.SetActive(false);
			Singleton<PostProcessingManager>.Instance.SetBlur(0f);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			Player.Activate();
			isOpen = false;
		}
	}
}
