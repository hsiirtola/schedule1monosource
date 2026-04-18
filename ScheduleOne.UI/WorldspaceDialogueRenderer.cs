using System.Collections;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI;

public class WorldspaceDialogueRenderer : MonoBehaviour
{
	private const float FadeDist = 2f;

	[Header("Settings")]
	public float MaxRange = 10f;

	public float BaseScale = 0.01f;

	public AnimationCurve Scale;

	public Vector2 Padding;

	public Vector3 WorldSpaceOffset = Vector3.zero;

	[Header("References")]
	public Canvas Canvas;

	public CanvasGroup CanvasGroup;

	public RectTransform Background;

	public TextMeshProUGUI Text;

	public Animation Anim;

	private Vector3 localOffset = Vector3.zero;

	private float CurrentOpacity;

	private Coroutine hideCoroutine;

	public string ShownText { get; protected set; } = string.Empty;

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		localOffset = ((Component)this).transform.localPosition;
		SetOpacity(0f);
	}

	private void FixedUpdate()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (ShownText == string.Empty)
		{
			if (CurrentOpacity != 0f)
			{
				SetOpacity(0f);
			}
			return;
		}
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			if (CurrentOpacity != 0f)
			{
				SetOpacity(0f);
			}
			return;
		}
		if (Singleton<DialogueCanvas>.Instance.isActive)
		{
			if (CurrentOpacity != 0f)
			{
				SetOpacity(0f);
			}
			return;
		}
		if (Vector3.Distance(((Component)this).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position) > MaxRange)
		{
			if (CurrentOpacity != 0f)
			{
				SetOpacity(0f);
			}
			return;
		}
		float num = Vector3.Distance(((Component)this).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
		if (num < MaxRange - 2f)
		{
			SetOpacity(1f);
		}
		else
		{
			SetOpacity(1f - (num - (MaxRange - 2f)) / 2f);
		}
		((TMP_Text)Text).text = ShownText;
	}

	private void LateUpdate()
	{
		if (CurrentOpacity > 0f)
		{
			UpdatePosition();
		}
	}

	private void UpdatePosition()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			float num = BaseScale * Scale.Evaluate(Vector3.Distance(((Component)this).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position) / MaxRange);
			((Component)Canvas).transform.localScale = new Vector3(num, num, num);
			Background.sizeDelta = new Vector2(((TMP_Text)Text).renderedWidth + Padding.x, ((TMP_Text)Text).renderedHeight + Padding.y);
			((Component)Canvas).transform.LookAt(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
			((Component)this).transform.localPosition = localOffset;
			((Component)this).transform.position = ((Component)this).transform.position + WorldSpaceOffset;
		}
	}

	public void ShowText(string text, float duration = 0f)
	{
		if (hideCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(hideCoroutine);
			hideCoroutine = null;
		}
		text = text.Replace("<color=red>", "<color=#FF6666>");
		text = text.Replace("<color=green>", "<color=#93FF58>");
		text = text.Replace("<color=blue>", "<color=#76C9FF>");
		ShownText = text;
		if (ShownText != string.Empty)
		{
			((TMP_Text)Text).text = ShownText;
			((TMP_Text)Text).ForceMeshUpdate(false, false);
			UpdatePosition();
		}
		if (!((Behaviour)Canvas).enabled && (Object)(object)Anim != (Object)null)
		{
			Anim.Play();
		}
		if (duration > 0f)
		{
			hideCoroutine = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(Wait(duration));
		}
		IEnumerator Wait(float dur)
		{
			yield return (object)new WaitForSeconds(dur);
			ShownText = string.Empty;
			hideCoroutine = null;
		}
	}

	public void HideText()
	{
		if (hideCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(hideCoroutine);
			hideCoroutine = null;
		}
		ShownText = string.Empty;
	}

	private void SetOpacity(float op)
	{
		CurrentOpacity = op;
		CanvasGroup.alpha = op;
		((Behaviour)Canvas).enabled = op > 0f;
	}
}
