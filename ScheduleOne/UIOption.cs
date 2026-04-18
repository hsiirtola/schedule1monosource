using TMPro;
using UnityEngine;

namespace ScheduleOne;

public abstract class UIOption : MonoBehaviour
{
	public struct OptionInfo
	{
		public string OptionName;

		public int OptionIndex;
	}

	[SerializeField]
	protected UISelectable selectable;

	[SerializeField]
	protected TextMeshProUGUI nameText;

	[SerializeField]
	protected string optionName = "Option Name";

	private const float MoveThreshold = 0.25f;

	private bool wasNavPressedLastFrame;

	private float navTimer;

	protected virtual float NavigationRepeatRateMult => 1f;

	protected virtual void Awake()
	{
		((TMP_Text)nameText).text = optionName;
	}

	private void OnValidate()
	{
		if ((Object)(object)nameText != (Object)null)
		{
			((TMP_Text)nameText).text = optionName;
		}
	}

	private void Update()
	{
		if (selectable.IsSelected())
		{
			OnUpdate();
		}
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void MoveLeft()
	{
	}

	protected virtual void MoveRight()
	{
	}

	protected virtual void DetectInput()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Vector2 uINavigationDirection = GameInput.UINavigationDirection;
		if (uINavigationDirection.y > 0.25f || uINavigationDirection.y < -0.25f)
		{
			uINavigationDirection.x = 0f;
		}
		uINavigationDirection.y = 0f;
		bool flag = uINavigationDirection != Vector2.zero;
		if (flag)
		{
			if (!wasNavPressedLastFrame)
			{
				if (Navigate(uINavigationDirection))
				{
					navTimer = 0.5f;
				}
			}
			else
			{
				navTimer -= Time.unscaledDeltaTime;
				if (navTimer <= 0f && Navigate(uINavigationDirection))
				{
					navTimer = 0.125f * NavigationRepeatRateMult;
				}
			}
		}
		else
		{
			navTimer = 0f;
		}
		wasNavPressedLastFrame = flag;
	}

	protected virtual bool Navigate(Vector2 navDir)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (navDir.x < -0.25f)
		{
			MoveLeft();
			return true;
		}
		if (navDir.x > 0.25f)
		{
			MoveRight();
			return true;
		}
		return false;
	}
}
