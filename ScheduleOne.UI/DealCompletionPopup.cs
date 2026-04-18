using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.Money;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.Quests;
using ScheduleOne.UI.Relations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class DealCompletionPopup : Singleton<DealCompletionPopup>
{
	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public CanvasGroup Group;

	public Animation Anim;

	public TextMeshProUGUI Title;

	public TextMeshProUGUI PaymentLabel;

	public TextMeshProUGUI SatisfactionValueLabel;

	public RelationCircle RelationCircle;

	public TextMeshProUGUI RelationshipLabel;

	public Gradient SatisfactionGradient;

	public AudioSourceController SoundEffect;

	public TextMeshProUGUI[] BonusLabels;

	[Header("Animations")]
	[SerializeField]
	private Animation _animation;

	private Coroutine routine;

	private AnimationState _animationState;

	public bool IsPlaying { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		_animationState = _animation["DealCompletedPopup"];
	}

	public void PlayPopup(Customer customer, float satisfaction, float originalRelationshipDelta, float basePayment, List<Contract.BonusPayment> bonuses)
	{
		Debug.Log((object)$"Playing deal completion popup for {customer.NPC.fullName} with satisfaction {satisfaction:P0} and base payment {basePayment}");
		if (routine != null)
		{
			((MonoBehaviour)this).StopCoroutine(routine);
		}
		routine = ((MonoBehaviour)this).StartCoroutine(PlayPopupRoutine(customer, satisfaction, originalRelationshipDelta, basePayment, bonuses));
	}

	private IEnumerator PlayPopupRoutine(Customer customer, float satisfaction, float originalRelationshipDelta, float basePayment, List<Contract.BonusPayment> bonuses)
	{
		IsPlaying = true;
		_animationState.time = 0f;
		_animation.Play("DealCompletedPopup");
		Group.alpha = 0f;
		((Behaviour)Canvas).enabled = true;
		((Component)Container).gameObject.SetActive(true);
		((TMP_Text)Title).text = "Deal completed for " + customer.NPC.fullName;
		((TMP_Text)PaymentLabel).text = "+$0";
		((TMP_Text)SatisfactionValueLabel).text = "0%";
		((Graphic)SatisfactionValueLabel).color = SatisfactionGradient.Evaluate(0f);
		for (int i = 0; i < BonusLabels.Length; i++)
		{
			if (bonuses.Count > i)
			{
				((TMP_Text)BonusLabels[i]).text = "<color=#54E717>+" + MoneyManager.FormatAmount(bonuses[i].Amount) + "</color> " + bonuses[i].Title;
				((Component)BonusLabels[i]).gameObject.SetActive(true);
			}
			else
			{
				((Component)BonusLabels[i]).gameObject.SetActive(false);
			}
		}
		yield return (object)new WaitForSeconds(0.2f);
		Anim.Play();
		SoundEffect.Play();
		RelationCircle.AssignNPC(customer.NPC);
		RelationCircle.SetUnlocked(NPCRelationData.EUnlockType.Recommendation, notify: false);
		RelationCircle.SetNotchPosition(originalRelationshipDelta);
		SetRelationshipLabel(originalRelationshipDelta);
		yield return (object)new WaitForSeconds(0.2f);
		float paymentLerpTime = 1.5f;
		for (float i2 = 0f; i2 < paymentLerpTime; i2 += Time.deltaTime)
		{
			((TMP_Text)PaymentLabel).text = "+" + MoneyManager.FormatAmount(basePayment * (i2 / paymentLerpTime));
			yield return (object)new WaitForEndOfFrame();
		}
		((TMP_Text)PaymentLabel).text = "+" + MoneyManager.FormatAmount(basePayment);
		yield return (object)new WaitForSeconds(1.5f);
		float satisfactionLerpTime = 1f;
		for (float i2 = 0f; i2 < satisfactionLerpTime; i2 += Time.deltaTime)
		{
			((Graphic)SatisfactionValueLabel).color = SatisfactionGradient.Evaluate(i2 / satisfactionLerpTime * satisfaction);
			float num = Mathf.Lerp(0f, satisfaction, i2 / satisfactionLerpTime);
			((TMP_Text)SatisfactionValueLabel).text = num.ToString("P0");
			yield return (object)new WaitForEndOfFrame();
		}
		((Graphic)SatisfactionValueLabel).color = SatisfactionGradient.Evaluate(satisfaction);
		((TMP_Text)SatisfactionValueLabel).text = satisfaction.ToString("P0");
		yield return (object)new WaitForSeconds(0.25f);
		float endDelta = customer.NPC.RelationData.RelationDelta;
		float lerpTime = Mathf.Abs(customer.NPC.RelationData.RelationDelta - originalRelationshipDelta);
		for (float i2 = 0f; i2 < lerpTime; i2 += Time.deltaTime)
		{
			float num2 = Mathf.Lerp(originalRelationshipDelta, endDelta, i2 / lerpTime);
			RelationCircle.SetNotchPosition(num2);
			SetRelationshipLabel(num2);
			yield return (object)new WaitForEndOfFrame();
		}
		RelationCircle.SetNotchPosition(endDelta);
		SetRelationshipLabel(endDelta);
		yield return (object)new WaitUntil((Func<bool>)(() => Group.alpha == 0f));
		((Behaviour)Canvas).enabled = false;
		((Component)Container).gameObject.SetActive(false);
		routine = null;
		IsPlaying = false;
	}

	private void SetRelationshipLabel(float delta)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		ERelationshipCategory category = RelationshipCategory.GetCategory(delta);
		((TMP_Text)RelationshipLabel).text = category.ToString();
		((Graphic)RelationshipLabel).color = Color32.op_Implicit(RelationshipCategory.GetColor(category));
	}
}
