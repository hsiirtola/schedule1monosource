using ScheduleOne.Misc;
using ScheduleOne.PlayerTasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.ObjectScripts;

public class LabOvenButton : MonoBehaviour
{
	private const float ANIMATION_TIME = 0.2f;

	public Transform Button;

	public Transform PressedTransform;

	public Transform DepressedTransform;

	public ToggleableLight Light;

	public Clickable Clickable;

	private float animationTimer;

	private Vector3 animationStartPos;

	private Vector3 animationEndPos;

	public bool Pressed { get; private set; }

	private void Start()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		SetInteractable(interactable: false);
		Clickable.onClickStart.AddListener((UnityAction<RaycastHit>)Press);
		animationStartPos = DepressedTransform.localPosition;
		animationEndPos = DepressedTransform.localPosition;
	}

	public void SetInteractable(bool interactable)
	{
		Clickable.ClickableEnabled = interactable;
	}

	public void Press(RaycastHit hit)
	{
		SetPressed(pressed: true);
	}

	public void SetPressed(bool pressed)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (Pressed != pressed)
		{
			Pressed = pressed;
			Light.isOn = pressed;
			animationTimer = 0f;
			animationStartPos = Button.localPosition;
			animationEndPos = (pressed ? PressedTransform.localPosition : DepressedTransform.localPosition);
		}
	}

	private void Update()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (animationTimer < 0.2f)
		{
			animationTimer += Time.deltaTime;
			float num = animationTimer / 0.2f;
			if (num >= 1f)
			{
				Button.localPosition = animationEndPos;
			}
			else
			{
				Button.localPosition = Vector3.Lerp(animationStartPos, animationEndPos, num);
			}
		}
	}
}
