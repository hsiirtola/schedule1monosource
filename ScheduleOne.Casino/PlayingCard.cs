using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Audio;
using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.Casino;

public class PlayingCard : MonoBehaviour
{
	[Serializable]
	public class CardSprite
	{
		public ECardSuit Suit;

		public ECardValue Value;

		public Sprite Sprite;
	}

	public struct CardData(ECardSuit suit, ECardValue value)
	{
		public ECardSuit Suit = suit;

		public ECardValue Value = value;
	}

	public enum ECardSuit
	{
		Spades,
		Hearts,
		Diamonds,
		Clubs
	}

	public enum ECardValue
	{
		Blank,
		Ace,
		Two,
		Three,
		Four,
		Five,
		Six,
		Seven,
		Eight,
		Nine,
		Ten,
		Jack,
		Queen,
		King
	}

	public string CardID = "card_1";

	[Header("References")]
	public SpriteRenderer CardSpriteRenderer;

	public CardSprite[] CardSprites;

	public Animation FlipAnimation;

	public AnimationClip FlipFaceUpClip;

	public AnimationClip FlipFaceDownClip;

	[Header("Sound")]
	public AudioSourceController FlipSound;

	public AudioSourceController LandSound;

	private Coroutine moveRoutine;

	private Tuple<Vector3, Quaternion> lastGlideTarget;

	public bool IsFaceUp { get; private set; }

	public ECardSuit Suit { get; private set; }

	public ECardValue Value { get; private set; }

	public CardController CardController { get; private set; }

	private void OnValidate()
	{
		((Object)((Component)this).gameObject).name = "PlayingCard (" + CardID + ")";
	}

	public void SetCardController(CardController cardController)
	{
		CardController = cardController;
	}

	public void SetCard(ECardSuit suit, ECardValue value, bool network = true)
	{
		if (network && (Object)(object)CardController != (Object)null)
		{
			CardController.SendCardValue(CardID, suit, value);
			return;
		}
		Suit = suit;
		Value = value;
		CardSprite cardSprite = GetCardSprite(suit, value);
		if (cardSprite != null)
		{
			CardSpriteRenderer.sprite = cardSprite.Sprite;
		}
	}

	public void ClearCard()
	{
		SetCard(ECardSuit.Spades, ECardValue.Blank);
	}

	public void SetFaceUp(bool faceUp, bool network = true)
	{
		if (network && (Object)(object)CardController != (Object)null)
		{
			CardController.SendCardFaceUp(CardID, faceUp);
		}
		if (IsFaceUp != faceUp)
		{
			IsFaceUp = faceUp;
			if (IsFaceUp)
			{
				FlipAnimation.Play(((Object)FlipFaceUpClip).name);
			}
			else
			{
				FlipAnimation.Play(((Object)FlipFaceDownClip).name);
			}
			FlipSound.Play();
		}
	}

	public void GlideTo(Vector3 position, Quaternion rotation, float duration = 0.5f, bool network = true)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (network && (Object)(object)CardController != (Object)null)
		{
			CardController.SendCardGlide(CardID, position, rotation, duration);
			return;
		}
		if (lastGlideTarget != null)
		{
			Vector3 item = lastGlideTarget.Item1;
			if (((Vector3)(ref item)).Equals(position))
			{
				Quaternion item2 = lastGlideTarget.Item2;
				if (((Quaternion)(ref item2)).Equals(rotation))
				{
					return;
				}
			}
		}
		lastGlideTarget = new Tuple<Vector3, Quaternion>(position, rotation);
		float verticalOffset = 0.02f;
		if (moveRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(moveRoutine);
		}
		moveRoutine = ((MonoBehaviour)this).StartCoroutine(MoveRoutine());
		IEnumerator MoveRoutine()
		{
			Vector3 startPosition = ((Component)this).transform.position;
			Quaternion startRotation = ((Component)this).transform.rotation;
			LandSound.Play();
			float time = 0f;
			while (time < duration)
			{
				time += Time.deltaTime;
				float num = Mathf.SmoothStep(0f, 1f, time / duration);
				Vector3 position2 = Vector3.Lerp(startPosition, position, num);
				position2.y += Mathf.Sin(num * (float)System.Math.PI) * verticalOffset;
				((Component)this).transform.position = position2;
				((Component)this).transform.rotation = Quaternion.Lerp(startRotation, rotation, num);
				yield return null;
			}
			((Component)this).transform.position = position;
			((Component)this).transform.rotation = rotation;
		}
	}

	private CardSprite GetCardSprite(ECardSuit suit, ECardValue val)
	{
		return CardSprites.FirstOrDefault((CardSprite x) => x.Suit == suit && x.Value == val);
	}

	[Button]
	public void VerifyCardSprites()
	{
		List<CardSprite> list = new List<CardSprite>(CardSprites);
		foreach (ECardSuit value in Enum.GetValues(typeof(ECardSuit)))
		{
			foreach (ECardValue value2 in Enum.GetValues(typeof(ECardValue)))
			{
				CardSprite cardSprite = GetCardSprite(value, value2);
				if (cardSprite == null)
				{
					Debug.LogError((object)$"Card sprite for {value} {value2} is missing.");
				}
				else if (list.Contains(cardSprite))
				{
					Debug.LogError((object)$"Card sprite for {value} {value2} is duplicated.");
				}
				else
				{
					list.Add(cardSprite);
				}
			}
		}
	}
}
