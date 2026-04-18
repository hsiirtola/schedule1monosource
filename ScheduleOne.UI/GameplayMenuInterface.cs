using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class GameplayMenuInterface : Singleton<GameplayMenuInterface>
{
	public Canvas Canvas;

	public Button PhoneButton;

	public Button CharacterButton;

	public RectTransform SelectionIndicator;

	public CharacterInterface CharacterInterface;

	private Coroutine selectionLerp;

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)PhoneButton.onClick).AddListener(new UnityAction(PhoneClicked));
		((UnityEvent)CharacterButton.onClick).AddListener(new UnityAction(CharacterClicked));
		Close();
	}

	protected override void Start()
	{
		base.Start();
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)Canvas).GetComponent<GraphicRaycaster>());
	}

	public void Open()
	{
		((Behaviour)Canvas).enabled = true;
	}

	public void Close()
	{
		((Behaviour)Canvas).enabled = false;
	}

	public void PhoneClicked()
	{
		Singleton<GameplayMenu>.Instance.SetScreen(GameplayMenu.EGameplayScreen.Phone);
	}

	public void CharacterClicked()
	{
		Singleton<GameplayMenu>.Instance.SetScreen(GameplayMenu.EGameplayScreen.Character);
	}

	public void SetSelected(GameplayMenu.EGameplayScreen screen)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		Vector2 pos = Vector2.zero;
		((Selectable)PhoneButton).interactable = true;
		((Selectable)CharacterButton).interactable = true;
		if (screen == GameplayMenu.EGameplayScreen.Character)
		{
			CharacterInterface.Open();
		}
		else
		{
			CharacterInterface.Close();
		}
		switch (screen)
		{
		case GameplayMenu.EGameplayScreen.Phone:
			pos = Vector2.op_Implicit(((Component)PhoneButton).transform.position);
			((Selectable)PhoneButton).interactable = false;
			break;
		case GameplayMenu.EGameplayScreen.Character:
			pos = Vector2.op_Implicit(((Component)CharacterButton).transform.position);
			((Selectable)CharacterButton).interactable = false;
			break;
		}
		if (selectionLerp != null)
		{
			((MonoBehaviour)this).StopCoroutine(selectionLerp);
		}
		selectionLerp = ((MonoBehaviour)this).StartCoroutine(Lerp());
		IEnumerator Lerp()
		{
			float startX = ((Transform)SelectionIndicator).position.x;
			for (float t = 0f; t < 0.12f; t += Time.deltaTime)
			{
				((Transform)SelectionIndicator).position = Vector2.op_Implicit(new Vector2(Mathf.Lerp(startX, pos.x, t / 0.12f), ((Transform)SelectionIndicator).position.y));
				yield return (object)new WaitForEndOfFrame();
			}
			((Transform)SelectionIndicator).position = Vector2.op_Implicit(new Vector2(pos.x, ((Transform)SelectionIndicator).position.y));
			selectionLerp = null;
		}
	}
}
