using HSVPicker;
using UnityEngine;
using UnityEngine.Events;

public class SColorPicker : ColorPicker
{
	public int PropertyIndex;

	public UnityEvent<Color, int> onValueChangeWithIndex;

	private void Start()
	{
		((UnityEvent<Color>)(object)base.onValueChanged).AddListener((UnityAction<Color>)ValueChanged);
	}

	private void ValueChanged(Color col)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		onValueChangeWithIndex.Invoke(col, PropertyIndex);
	}
}
