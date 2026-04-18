using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI.Phone.Map;

public class MapApp : App<MapApp>
{
	public const float KeyMoveSpeed = 1.25f;

	public RectTransform ContentRect;

	public RectTransform PoIContainer;

	public Scrollbar HorizontalScrollbar;

	public Scrollbar VerticalScrollbar;

	public Image BackgroundImage;

	public CanvasGroup LabelGroup;

	[Header("Settings")]
	public Sprite DemoMapSprite;

	public Sprite MainMapSprite;

	public Sprite TutorialMapSprite;

	public float LabelScrollMin = 1.2f;

	public float LabelScrollMax = 1.5f;

	[Header("Custom UI")]
	[SerializeField]
	protected UIScreen uiScreen;

	[SerializeField]
	protected UIMapPanel uiPanel;

	[HideInInspector]
	public bool SkipFocusPlayer;

	private Coroutine contentMoveRoutine;

	private bool opened;

	protected override void Start()
	{
		base.Start();
		BackgroundImage.sprite = (NetworkSingleton<GameManager>.Instance.IsTutorial ? TutorialMapSprite : MainMapSprite);
	}

	public override void SetOpen(bool open)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		base.SetOpen(open);
		if (NetworkSingleton<VariableDatabase>.InstanceExists)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("MapAppOpen", open.ToString(), network: false);
		}
		if (open)
		{
			if (!opened && !SkipFocusPlayer)
			{
				opened = true;
				Player.Local.PoI.UpdatePosition();
				FocusPosition(Player.Local.PoI.UI.anchoredPosition);
			}
			if ((Object)(object)Player.Local != (Object)null && (Object)(object)Player.Local.PoI.UI != (Object)null)
			{
				((Component)Player.Local.PoI.UI).GetComponentInChildren<Animation>().Play();
			}
			uiScreen.SetCurrentSelectedPanel(uiPanel);
		}
	}

	protected override void Update()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (base.isOpen)
		{
			GameInput.GetButton(GameInput.ButtonCode.Right);
			GameInput.GetButton(GameInput.ButtonCode.Left);
			GameInput.GetButton(GameInput.ButtonCode.Forward);
			GameInput.GetButton(GameInput.ButtonCode.Backward);
			float x = ((Transform)ContentRect).localScale.x;
			if (x >= LabelScrollMin)
			{
				LabelGroup.alpha = Mathf.Clamp01((x - LabelScrollMin) / (LabelScrollMax - LabelScrollMin));
			}
			else
			{
				LabelGroup.alpha = 0f;
			}
		}
	}

	public void FocusPosition(Vector2 anchoredPosition)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		ContentRect.pivot = new Vector2(0f, 1f);
		float num = 1.3f;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((0f - ContentRect.sizeDelta.x) / 2f, ContentRect.sizeDelta.y / 2f);
		val.x -= anchoredPosition.x;
		val.y -= anchoredPosition.y;
		((Transform)ContentRect).localScale = new Vector3(num, num, num);
		ContentRect.anchoredPosition = val * num;
	}

	public void SetupMapItem(GameObject gameObject)
	{
		uiPanel.RegisterMapItem(gameObject.GetComponent<UIMapItem>());
	}

	public void TeardownMapItem(GameObject gameObject)
	{
		uiPanel.DeregisterMapItem(gameObject.GetComponent<UIMapItem>());
	}
}
