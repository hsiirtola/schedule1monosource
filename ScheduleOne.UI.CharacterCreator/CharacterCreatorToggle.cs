using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI.CharacterCreator;

public class CharacterCreatorToggle : CharacterCreatorField<int>
{
	[Header("References")]
	public Button Button1;

	public Button Button2;

	protected override void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		base.Awake();
		((UnityEvent)Button1.onClick).AddListener(new UnityAction(OnButton1));
		((UnityEvent)Button2.onClick).AddListener(new UnityAction(OnButton2));
	}

	public override void ApplyValue()
	{
		base.ApplyValue();
		((Selectable)Button1).interactable = base.value != 0;
		((Selectable)Button2).interactable = base.value == 0;
	}

	public void OnButton1()
	{
		base.value = 0;
		WriteValue();
	}

	public void OnButton2()
	{
		base.value = 1;
		WriteValue();
	}
}
