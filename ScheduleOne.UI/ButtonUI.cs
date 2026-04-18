using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class ButtonUI : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private Button _button;

	private int _id;

	public Action<int> OnSelect;

	public Button Button => _button;

	public void Initialize(int id)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		_id = id;
		((UnityEventBase)_button.onClick).RemoveAllListeners();
		((UnityEvent)_button.onClick).AddListener((UnityAction)delegate
		{
			OnSelect?.Invoke(_id);
		});
	}
}
