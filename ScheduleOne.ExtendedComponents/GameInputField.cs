using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.ExtendedComponents;

public class GameInputField : TMP_InputField
{
	protected override void Awake()
	{
		((Selectable)this).Awake();
		((UnityEvent<string>)(object)((TMP_InputField)this).onSelect).AddListener((UnityAction<string>)EditStart);
		((UnityEvent<string>)(object)((TMP_InputField)this).onEndEdit).AddListener((UnityAction<string>)EndEdit);
	}

	private void EditStart(string newVal)
	{
		GameInput.IsTyping = true;
	}

	private void EndEdit(string newVal)
	{
		GameInput.IsTyping = false;
	}
}
