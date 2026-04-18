using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne;

public class UIToggle : UIOption
{
	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private Image toggleImage;

	private const string ONTEXT = "On";

	private const string OFFTEXT = "Off";

	public UnityEvent<bool> OnChanged;

	private bool state;

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.Awake();
		selectable.OnTrigger.AddListener((UnityAction)delegate
		{
			SetState(!state);
		});
		SetStateWithoutNotify(state: false);
	}

	protected override void OnUpdate()
	{
	}

	public void SetState(bool state)
	{
		SetStateInternal(state);
		OnChanged?.Invoke(state);
	}

	public void SetStateWithoutNotify(bool state)
	{
		SetStateInternal(state);
	}

	private void SetStateInternal(bool state)
	{
		this.state = state;
		SetButtonState(state);
	}

	private void SetButtonState(bool state)
	{
		((TMP_Text)buttonText).text = (state ? "On" : "Off");
		((Behaviour)toggleImage).enabled = state;
	}
}
