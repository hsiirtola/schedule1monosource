using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class InteractionCanvas : Singleton<InteractionCanvas>
{
	public const float DISPLAY_SIZE_MULTIPLIER = 0.75f;

	[Header("Settings")]
	public Color DefaultMessageColor;

	public Color DefaultIconColor;

	public Color DefaultKeyColor;

	public Color InvalidMessageColor;

	public Color InvalidIconColor;

	public Sprite KeyIcon;

	public Sprite LeftMouseIcon;

	public Sprite CrossIcon;

	[Header("References")]
	public Canvas Canvas;

	public RectTransform Container;

	public Image Icon;

	public Text IconText;

	public Text MessageText;

	public RectTransform WSLabelContainer;

	public RectTransform BackgroundImage;

	[Header("Prefabs")]
	public GameObject WSLabelPrefab;

	private bool _interactionDisplayEnabledThisFrame;

	private Coroutine _displayScaleLerpRoutine;

	[HideInInspector]
	public List<WorldSpaceLabel> ActiveWSlabels = new List<WorldSpaceLabel>();

	public float displayScale { get; set; } = 1f;

	protected virtual void LateUpdate()
	{
		if (Singleton<GameInput>.InstanceExists)
		{
			if (Singleton<InteractionManager>.Instance.IsAnythingBlockingInteraction())
			{
				_interactionDisplayEnabledThisFrame = false;
			}
			((Behaviour)Canvas).enabled = _interactionDisplayEnabledThisFrame || ActiveWSlabels.Count > 0;
			((Component)Container).gameObject.SetActive(_interactionDisplayEnabledThisFrame);
			if (!_interactionDisplayEnabledThisFrame)
			{
				displayScale = 1f;
			}
			for (int i = 0; i < ActiveWSlabels.Count; i++)
			{
				ActiveWSlabels[i].RefreshDisplay();
			}
			_interactionDisplayEnabledThisFrame = false;
		}
	}

	public void EnableInteractionDisplay(Vector3 pos, Sprite icon, string spriteText, string message, Color messageColor, Color iconColor)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		_interactionDisplayEnabledThisFrame = true;
		((Transform)Container).position = PlayerSingleton<PlayerCamera>.Instance.Camera.WorldToScreenPoint(pos);
		((Component)Icon).gameObject.SetActive((Object)(object)icon != (Object)null);
		Icon.sprite = icon;
		((Graphic)Icon).color = iconColor;
		((Behaviour)IconText).enabled = spriteText != string.Empty;
		IconText.text = spriteText.ToUpper();
		MessageText.text = message;
		((Graphic)MessageText).color = messageColor;
		Container.sizeDelta = new Vector2(60f + MessageText.preferredWidth, Container.sizeDelta.y);
		BackgroundImage.sizeDelta = new Vector2(MessageText.preferredWidth + 180f, 140f);
		float num = Mathf.Clamp(1f / Vector3.Distance(pos, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position), 0f, 1f) * displayScale * 0.75f;
		((Transform)Container).localScale = new Vector3(num, num, 1f);
	}

	public void LerpDisplayScale(float endScale)
	{
		if (_displayScaleLerpRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(_displayScaleLerpRoutine);
		}
		_displayScaleLerpRoutine = ((MonoBehaviour)this).StartCoroutine(ILerpDisplayScale(displayScale, endScale));
		IEnumerator ILerpDisplayScale(float startScale, float num)
		{
			float lerpTime = Mathf.Abs(startScale - num) * 0.75f;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				displayScale = Mathf.Lerp(startScale, num, i / lerpTime);
				yield return (object)new WaitForEndOfFrame();
			}
			displayScale = num;
			_displayScaleLerpRoutine = null;
		}
	}
}
