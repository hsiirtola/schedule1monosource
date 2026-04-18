using System.Collections;
using System.Collections.Generic;
using ScheduleOne.Combat;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using ScheduleOne.Law;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class HUD : Singleton<HUD>
{
	[Header("References")]
	public Canvas canvas;

	public RectTransform canvasRect;

	public Image crosshair;

	[SerializeField]
	protected Image blackOverlay;

	[SerializeField]
	protected Image radialIndicator;

	[SerializeField]
	protected GraphicRaycaster raycaster;

	[SerializeField]
	protected TextMeshProUGUI topScreenText;

	[SerializeField]
	protected RectTransform topScreenText_Background;

	public Text fpsLabel;

	public RectTransform cashSlotContainer;

	public RectTransform cashSlotUI;

	public RectTransform onlineBalanceContainer;

	public RectTransform onlineBalanceSlotUI;

	public RectTransform managementSlotContainer;

	public ItemSlotUI managementSlotUI;

	public RectTransform HotbarContainer;

	public RectTransform SlotContainer;

	public ItemSlotUI discardSlot;

	public Image discardSlotFill;

	public TextMeshProUGUI selectedItemLabel;

	public RectTransform QuestEntryContainer;

	public TextMeshProUGUI QuestEntryTitle;

	public CrimeStatusUI CrimeStatusUI;

	public BalanceDisplay OnlineBalanceDisplay;

	public BalanceDisplay SafeBalanceDisplay;

	public CrosshairText CrosshairText;

	public RectTransform UnreadMessagesPrompt;

	public TextMeshProUGUI SleepPrompt;

	public TextMeshProUGUI CurfewPrompt;

	public CanvasGroup NotificationsCanvasGroup;

	public Animation CashSlotHintAnim;

	public CanvasGroup CashSlotHintAnimCanvasGroup;

	[SerializeField]
	private ReticleController _reticleController;

	[Header("Settings")]
	public Gradient RedGreenGradient;

	private int SampleSize = 60;

	private List<float> _previousFPS = new List<float>();

	private EventSystem eventSystem;

	private Coroutine blackOverlayFade;

	private bool radialIndicatorSetThisFrame;

	protected override void Awake()
	{
		base.Awake();
		eventSystem = EventSystem.current;
		((Component)managementSlotContainer).gameObject.SetActive(true);
		HideTopScreenText();
	}

	protected override void Start()
	{
		base.Start();
		Singleton<ItemUIManager>.Instance.AddRaycaster(((Component)canvas).GetComponent<GraphicRaycaster>());
	}

	public void SetCrosshairVisible(bool vis)
	{
		((Component)crosshair).gameObject.SetActive(vis);
	}

	public void SetBlackOverlayVisible(bool vis, float fadeTime)
	{
		if (blackOverlayFade != null)
		{
			((MonoBehaviour)this).StopCoroutine(blackOverlayFade);
		}
		blackOverlayFade = ((MonoBehaviour)this).StartCoroutine(FadeBlackOverlay(vis, fadeTime));
	}

	private void Update()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		RefreshFPS();
		if (!Singleton<GameInput>.InstanceExists)
		{
			return;
		}
		((Component)SleepPrompt).gameObject.SetActive(NetworkSingleton<TimeManager>.Instance.CurrentTime == 400);
		if (NetworkSingleton<CurfewManager>.InstanceExists)
		{
			if (NetworkSingleton<CurfewManager>.Instance.IsCurrentlyActive)
			{
				((TMP_Text)CurfewPrompt).text = "Police curfew in effect until 5AM";
				((Graphic)CurfewPrompt).color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)108, (byte)88, (byte)60));
				((Component)CurfewPrompt).gameObject.SetActive(true);
			}
			else if (NetworkSingleton<CurfewManager>.Instance.IsEnabled && NetworkSingleton<TimeManager>.Instance.IsCurrentTimeWithinRange(2030, 500))
			{
				((TMP_Text)CurfewPrompt).text = "Police curfew starting soon";
				((Graphic)CurfewPrompt).color = Color32.op_Implicit(new Color32(byte.MaxValue, (byte)182, (byte)88, (byte)60));
				((Component)CurfewPrompt).gameObject.SetActive(true);
			}
			else
			{
				((Component)CurfewPrompt).gameObject.SetActive(false);
			}
		}
		UpdateQuestEntryTitle();
	}

	private void UpdateQuestEntryTitle()
	{
		int num = 0;
		for (int i = 0; i < ((Transform)QuestEntryContainer).childCount; i++)
		{
			if (((Component)((Transform)QuestEntryContainer).GetChild(i)).gameObject.activeSelf && ++num > 1)
			{
				((Behaviour)QuestEntryTitle).enabled = true;
				break;
			}
		}
	}

	private void RefreshFPS()
	{
		_previousFPS.Add(1f / Time.unscaledDeltaTime);
		if (_previousFPS.Count > SampleSize)
		{
			_previousFPS.RemoveAt(0);
		}
		fpsLabel.text = Mathf.Floor(GetAverageFPS()) + " FPS";
	}

	private float GetAverageFPS()
	{
		float num = 0f;
		for (int i = 0; i < _previousFPS.Count; i++)
		{
			num += _previousFPS[i];
		}
		return num / (float)_previousFPS.Count;
	}

	protected virtual void LateUpdate()
	{
		if (!radialIndicatorSetThisFrame)
		{
			((Behaviour)radialIndicator).enabled = false;
		}
		if ((Object)(object)Player.Local != (Object)null)
		{
			NotificationsCanvasGroup.alpha = Mathf.MoveTowards(NotificationsCanvasGroup.alpha, (Player.Local.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None) ? 1f : 0f, Time.unscaledDeltaTime * 0.6f);
		}
		radialIndicatorSetThisFrame = false;
	}

	protected IEnumerator FadeBlackOverlay(bool visible, float fadeTime)
	{
		if (visible)
		{
			((Behaviour)blackOverlay).enabled = true;
			PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement("Blackout");
		}
		float startAlpha = ((Graphic)blackOverlay).color.a;
		float endAlpha = 1f;
		if (!visible)
		{
			endAlpha = 0f;
		}
		for (float i = 0f; i < fadeTime; i += Time.unscaledDeltaTime)
		{
			((Graphic)blackOverlay).color = new Color(((Graphic)blackOverlay).color.r, ((Graphic)blackOverlay).color.g, ((Graphic)blackOverlay).color.b, Mathf.Lerp(startAlpha, endAlpha, i / fadeTime));
			yield return (object)new WaitForEndOfFrame();
		}
		((Graphic)blackOverlay).color = new Color(((Graphic)blackOverlay).color.r, ((Graphic)blackOverlay).color.g, ((Graphic)blackOverlay).color.b, endAlpha);
		blackOverlayFade = null;
		if (!visible)
		{
			((Behaviour)blackOverlay).enabled = false;
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement("Blackout");
		}
	}

	public void ShowRadialIndicator(float fill)
	{
		radialIndicatorSetThisFrame = true;
		radialIndicator.fillAmount = fill;
		((Behaviour)radialIndicator).enabled = true;
	}

	public void ShowTopScreenText(string t)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)topScreenText).text = t;
		topScreenText_Background.sizeDelta = new Vector2(((TMP_Text)topScreenText).preferredWidth + 30f, topScreenText_Background.sizeDelta.y);
		((Component)topScreenText_Background).gameObject.SetActive(true);
	}

	public void HideTopScreenText()
	{
		((Component)topScreenText_Background).gameObject.SetActive(false);
	}

	public void ShowFirearmReticle()
	{
		if (!_reticleController.IsActive)
		{
			_reticleController.ShowReticle();
		}
	}

	public void HideFirearmReticle()
	{
		if (_reticleController.IsActive)
		{
			_reticleController.HideReticle();
		}
	}

	public void SetFirearmReticle(float spreadAngle)
	{
		_reticleController.SetReticle(spreadAngle);
	}
}
