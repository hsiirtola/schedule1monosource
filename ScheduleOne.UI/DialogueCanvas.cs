using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Dialogue;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class DialogueCanvas : Singleton<DialogueCanvas>
{
	public const float TIME_PER_CHAR = 0.015f;

	public bool SkipNextRollout;

	[Header("References")]
	[SerializeField]
	protected Canvas canvas;

	public RectTransform Container;

	[SerializeField]
	protected TextMeshProUGUI dialogueText;

	[SerializeField]
	protected GameObject continuePopup;

	[SerializeField]
	protected List<DialogueChoiceEntry> dialogueChoices = new List<DialogueChoiceEntry>();

	[Header("Custom UI")]
	[SerializeField]
	protected UIScreen uiScreen;

	[SerializeField]
	protected UIPanel uiPanel;

	private DialogueHandler currentHandler;

	private DialogueNodeData currentNode;

	private bool spaceDownThisFrame;

	private bool leftClickThisFrame;

	private string overrideText = string.Empty;

	private Coroutine dialogueRollout;

	private Coroutine choiceSelectionResidualCoroutine;

	private bool hasChoiceBeenSelected;

	public bool isActive => (Object)(object)currentHandler != (Object)null;

	protected override void Awake()
	{
		base.Awake();
		((Behaviour)canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		GameInput.RegisterExitListener(Exit);
	}

	public void DisplayDialogueNode(DialogueHandler diag, DialogueNodeData node, string dialogueText, List<DialogueChoiceData> choices)
	{
		if ((Object)(object)diag != (Object)(object)currentHandler)
		{
			StartDialogue(diag);
		}
		if (dialogueRollout != null)
		{
			((MonoBehaviour)this).StopCoroutine(dialogueRollout);
		}
		currentNode = node;
		dialogueRollout = ((MonoBehaviour)this).StartCoroutine(RolloutDialogue(dialogueText, choices));
	}

	public void OverrideText(string text)
	{
		overrideText = text;
		if (dialogueRollout != null)
		{
			((MonoBehaviour)this).StopCoroutine(dialogueRollout);
		}
		((TMP_Text)dialogueText).text = overrideText;
		((Behaviour)canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
	}

	public void StopTextOverride()
	{
		overrideText = string.Empty;
	}

	private void Update()
	{
		if (isActive)
		{
			if (Input.GetKeyDown((KeyCode)32))
			{
				spaceDownThisFrame = true;
			}
			else
			{
				spaceDownThisFrame = false;
			}
			if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick))
			{
				leftClickThisFrame = true;
			}
			else
			{
				leftClickThisFrame = false;
			}
		}
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && isActive && action.exitType == ExitType.Escape && DialogueHandler.activeDialogue.AllowExit)
		{
			action.Used = true;
			currentHandler.EndDialogue();
		}
	}

	protected IEnumerator RolloutDialogue(string text, List<DialogueChoiceData> choices)
	{
		List<int> activeDialogueChoices = new List<int>();
		text = text.Replace("<color=red>", "<color=#FF6666>");
		text = text.Replace("<color=green>", "<color=#93FF58>");
		text = text.Replace("<color=blue>", "<color=#76C9FF>");
		((TMP_Text)dialogueText).maxVisibleCharacters = 0;
		((TMP_Text)dialogueText).text = text;
		((Behaviour)canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
		float rolloutTime = (float)text.Length * 0.015f;
		if (SkipNextRollout)
		{
			SkipNextRollout = false;
			rolloutTime = 0f;
		}
		for (float i = 0f; i < rolloutTime; i += Time.deltaTime)
		{
			if (spaceDownThisFrame)
			{
				break;
			}
			if (leftClickThisFrame)
			{
				break;
			}
			int maxVisibleCharacters = (int)(i / 0.015f);
			((TMP_Text)dialogueText).maxVisibleCharacters = maxVisibleCharacters;
			yield return (object)new WaitForEndOfFrame();
		}
		((TMP_Text)dialogueText).maxVisibleCharacters = text.Length;
		spaceDownThisFrame = false;
		leftClickThisFrame = false;
		hasChoiceBeenSelected = false;
		if (choiceSelectionResidualCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(choiceSelectionResidualCoroutine);
		}
		continuePopup.gameObject.SetActive(false);
		for (int j = 0; j < dialogueChoices.Count; j++)
		{
			dialogueChoices[j].gameObject.SetActive(false);
			dialogueChoices[j].canvasGroup.alpha = 1f;
			if (choices.Count > j)
			{
				((TMP_Text)dialogueChoices[j].text).text = choices[j].ChoiceText;
				((Selectable)dialogueChoices[j].button).interactable = true;
				string reason = string.Empty;
				if (IsChoiceValid(j, out reason))
				{
					dialogueChoices[j].notPossibleGameObject.SetActive(false);
					((Selectable)dialogueChoices[j].button).interactable = true;
					ColorBlock colors = ((Selectable)dialogueChoices[j].button).colors;
					((ColorBlock)(ref colors)).disabledColor = ((ColorBlock)(ref colors)).pressedColor;
					((Selectable)dialogueChoices[j].button).colors = colors;
					((Component)dialogueChoices[j].text).GetComponent<RectTransform>().offsetMax = new Vector2(0f, 0f);
				}
				else
				{
					((TMP_Text)dialogueChoices[j].notPossibleText).text = reason.ToUpper();
					dialogueChoices[j].notPossibleGameObject.SetActive(true);
					ColorBlock colors2 = ((Selectable)dialogueChoices[j].button).colors;
					((ColorBlock)(ref colors2)).disabledColor = ((ColorBlock)(ref colors2)).normalColor;
					((Selectable)dialogueChoices[j].button).colors = colors2;
					((Selectable)dialogueChoices[j].button).interactable = false;
					((TMP_Text)dialogueChoices[j].notPossibleText).ForceMeshUpdate(false, false);
					((Component)dialogueChoices[j].text).GetComponent<RectTransform>().offsetMax = new Vector2(0f - (((TMP_Text)dialogueChoices[j].notPossibleText).preferredWidth + 20f), 0f);
				}
				activeDialogueChoices.Add(j);
			}
		}
		uiPanel.ClearAllSelectables();
		Singleton<UIScreenManager>.Instance.AddScreen(uiScreen);
		if (activeDialogueChoices.Count == 0 || (activeDialogueChoices.Count == 1 && choices[0].ChoiceText == ""))
		{
			continuePopup.gameObject.SetActive(true);
			uiPanel.AddSelectable(continuePopup.GetComponent<UISelectable>());
			((MonoBehaviour)this).StartCoroutine(SelectPanel(continuePopup.GetComponent<UISelectable>()));
			yield return (object)new WaitUntil((Func<bool>)(() => spaceDownThisFrame || leftClickThisFrame || GameInput.GetButtonDown(GameInput.ButtonCode.Submit)));
			continuePopup.gameObject.SetActive(false);
			spaceDownThisFrame = false;
			leftClickThisFrame = false;
			currentHandler.ContinueSubmitted();
			yield break;
		}
		for (int num = 0; num < activeDialogueChoices.Count; num++)
		{
			dialogueChoices[activeDialogueChoices[num]].gameObject.SetActive(true);
			uiPanel.AddSelectable(((Component)dialogueChoices[activeDialogueChoices[num]].button).GetComponent<UISelectable>());
		}
		((MonoBehaviour)this).StartCoroutine(SelectPanel(((Component)dialogueChoices[0].button).GetComponent<UISelectable>()));
		while (!hasChoiceBeenSelected)
		{
			string reason2 = string.Empty;
			if (Input.GetKey((KeyCode)49) && IsChoiceValid(0, out reason2))
			{
				ChoiceSelected(0);
			}
			else if (Input.GetKey((KeyCode)50) && IsChoiceValid(1, out reason2))
			{
				ChoiceSelected(1);
			}
			else if (Input.GetKey((KeyCode)51) && IsChoiceValid(2, out reason2))
			{
				ChoiceSelected(2);
			}
			else if (Input.GetKey((KeyCode)52) && IsChoiceValid(3, out reason2))
			{
				ChoiceSelected(3);
			}
			else if (Input.GetKey((KeyCode)53) && IsChoiceValid(4, out reason2))
			{
				ChoiceSelected(4);
			}
			else if (Input.GetKey((KeyCode)54) && IsChoiceValid(5, out reason2))
			{
				ChoiceSelected(5);
			}
			else if (Input.GetKey((KeyCode)54) && IsChoiceValid(6, out reason2))
			{
				ChoiceSelected(6);
			}
			else if (Input.GetKey((KeyCode)54) && IsChoiceValid(7, out reason2))
			{
				ChoiceSelected(7);
			}
			else if (Input.GetKey((KeyCode)54) && IsChoiceValid(8, out reason2))
			{
				ChoiceSelected(8);
			}
			yield return (object)new WaitForEndOfFrame();
		}
	}

	private IEnumerator SelectPanel(UISelectable selectable)
	{
		yield return null;
		uiScreen.SetCurrentSelectedPanel(uiPanel);
		uiPanel.SelectSelectable(selectable);
	}

	private IEnumerator ChoiceSelectionResidual(DialogueChoiceEntry choice, float fadeTime)
	{
		yield return (object)new WaitForSeconds(0.25f);
		float realFadeTime = fadeTime - 0.25f;
		for (float i = 0f; i < realFadeTime; i += Time.deltaTime)
		{
			choice.canvasGroup.alpha = Mathf.Sqrt(Mathf.Lerp(1f, 0f, i / realFadeTime));
			yield return (object)new WaitForEndOfFrame();
		}
		choice.gameObject.SetActive(false);
		choiceSelectionResidualCoroutine = null;
	}

	private void StartDialogue(DialogueHandler handler)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		currentHandler = handler;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		Vector3 val = ((Component)currentHandler.LookPosition).transform.position - ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Quaternion val2 = Quaternion.LookRotation(new Vector3(normalized.x, 0f, normalized.z), Vector3.up);
		PlayerSingleton<PlayerMovement>.Instance.LerpPlayerRotation(val2, 0.3f);
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(Mathf.Sqrt(Mathf.Pow(normalized.x, 2f) + Mathf.Pow(normalized.z, 2f)), normalized.y, 0f);
		float num = (0f - Mathf.Atan2(val3.y, val3.x)) * (180f / (float)System.Math.PI);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, val2 * Quaternion.Euler(num, 0f, 0f), 0.3f, keepParented: true);
	}

	public void EndDialogue()
	{
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		continuePopup.gameObject.SetActive(false);
		for (int i = 0; i < dialogueChoices.Count; i++)
		{
			dialogueChoices[i].gameObject.SetActive(false);
		}
		if (dialogueRollout != null)
		{
			((MonoBehaviour)this).StopCoroutine(dialogueRollout);
		}
		if (choiceSelectionResidualCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(choiceSelectionResidualCoroutine);
		}
		((Behaviour)canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		currentHandler = null;
		currentNode = null;
		if (PlayerSingleton<PlayerCamera>.Instance.activeUIElementCount == 0)
		{
			((MonoBehaviour)this).StartCoroutine(UnlockPlayer());
		}
		else
		{
			PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: false, returnToOriginalRotation: false);
		}
		Singleton<UIScreenManager>.Instance.RemoveScreen(uiScreen);
	}

	private IEnumerator UnlockPlayer()
	{
		yield return null;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: true);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: true, returnToOriginalRotation: false);
	}

	public void ChoiceSelected(int choiceIndex)
	{
		string reason = string.Empty;
		if (!IsChoiceValid(choiceIndex, out reason))
		{
			return;
		}
		hasChoiceBeenSelected = true;
		for (int i = 0; i < dialogueChoices.Count; i++)
		{
			if (i == choiceIndex)
			{
				((Selectable)dialogueChoices[i].button).interactable = false;
				if (choiceSelectionResidualCoroutine != null)
				{
					((MonoBehaviour)this).StopCoroutine(choiceSelectionResidualCoroutine);
				}
				choiceSelectionResidualCoroutine = ((MonoBehaviour)this).StartCoroutine(ChoiceSelectionResidual(dialogueChoices[i], 0.75f));
			}
			else
			{
				dialogueChoices[i].gameObject.SetActive(false);
			}
		}
		continuePopup.gameObject.SetActive(false);
		currentHandler.ChoiceSelected(choiceIndex);
	}

	private bool IsChoiceValid(int choiceIndex, out string reason)
	{
		if (currentNode != null && currentHandler.CurrentChoices.Count > choiceIndex)
		{
			return currentHandler.CheckChoice(currentHandler.CurrentChoices[choiceIndex].ChoiceLabel, out reason);
		}
		reason = string.Empty;
		return false;
	}
}
