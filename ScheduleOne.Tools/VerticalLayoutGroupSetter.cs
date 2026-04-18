using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Tools;

public class VerticalLayoutGroupSetter : MonoBehaviour
{
	public float LeftSpacing;

	public float RightSpacing;

	private VerticalLayoutGroup layoutGroup;

	private void Awake()
	{
		layoutGroup = ((Component)this).GetComponent<VerticalLayoutGroup>();
	}

	public void Update()
	{
		bool flag = false;
		if (((LayoutGroup)layoutGroup).padding.left != (int)LeftSpacing)
		{
			((LayoutGroup)layoutGroup).padding.left = (int)LeftSpacing;
			flag = true;
		}
		if (((LayoutGroup)layoutGroup).padding.right != (int)RightSpacing)
		{
			((LayoutGroup)layoutGroup).padding.right = (int)RightSpacing;
			flag = true;
		}
		if (flag)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)layoutGroup).GetComponent<RectTransform>());
		}
	}
}
