using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.UI;

public class TabController : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private RectTransform _tabIndicator;

	[SerializeField]
	private List<TabItemUI> _tabItems;

	[Header("Settings")]
	[SerializeField]
	private float _indicatorMoveTime = 0.25f;

	[SerializeField]
	private AnimationCurve _indicatorMoveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[Header("Fonts")]
	[SerializeField]
	private ColorFont _tabColorFont;

	private int _currentTabIndex = -1;

	private Vector2 _indicatorPosition;

	private Coroutine _moveIndicatorCo;

	private TabSelectedEvent _onTabSelected;

	public int CurrentTabIndex => _currentTabIndex;

	public void Start()
	{
		for (int i = 0; i < _tabItems.Count; i++)
		{
			TabItemUI tabItemUI = _tabItems[i];
			tabItemUI.Button.Initialize(i);
			ButtonUI button = tabItemUI.Button;
			button.OnSelect = (Action<int>)Delegate.Combine(button.OnSelect, new Action<int>(SetTab));
		}
	}

	private void SetTab(int index)
	{
		SetTab(index, false);
	}

	public void SetToSelectedTab(bool instantIndicatorMove = false)
	{
		if (_currentTabIndex != -1)
		{
			SetTab(_currentTabIndex, instantIndicatorMove);
		}
	}

	public void SetTab(int index, bool instantIndicatorMove = false)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		_currentTabIndex = index;
		for (int i = 0; i < _tabItems.Count; i++)
		{
			TabItemUI tabItemUI = _tabItems[i];
			bool flag = i == index;
			if ((Object)(object)tabItemUI.Label != (Object)null)
			{
				((Graphic)tabItemUI.Label).color = (flag ? _tabColorFont.GetColour("Selected") : _tabColorFont.GetColour("Deselected"));
			}
			if ((Object)(object)tabItemUI.Content != (Object)null)
			{
				tabItemUI.Content.SetActive(flag);
			}
		}
		_onTabSelected?.Invoke(index);
		if (!((Object)(object)_tabIndicator != (Object)null))
		{
			return;
		}
		if (instantIndicatorMove)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(DoDelayRoutine(0.1f, delegate
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_0066: Unknown result type (might be due to invalid IL or missing references)
				_indicatorPosition = _tabIndicator.anchoredPosition;
				_indicatorPosition.x = ((Component)_tabItems[index].Button).GetComponent<RectTransform>().anchoredPosition.x;
				_tabIndicator.anchoredPosition = _indicatorPosition;
			}));
			return;
		}
		if (_moveIndicatorCo != null)
		{
			((MonoBehaviour)Singleton<CoroutineService>.Instance).StopCoroutine(_moveIndicatorCo);
		}
		_indicatorPosition = _tabIndicator.anchoredPosition;
		_indicatorPosition.x = ((Component)_tabItems[index].Button).GetComponent<RectTransform>().anchoredPosition.x;
		_moveIndicatorCo = ((MonoBehaviour)Singleton<CoroutineService>.Instance).StartCoroutine(DoMoveTabIndicatorRoutine());
	}

	private IEnumerator DoMoveTabIndicatorRoutine()
	{
		float elapsed = 0f;
		Vector2 startingPosition = _tabIndicator.anchoredPosition;
		while (elapsed < _indicatorMoveTime)
		{
			elapsed += Time.deltaTime;
			float num = Mathf.Clamp01(elapsed / _indicatorMoveTime);
			_tabIndicator.anchoredPosition = Vector2.Lerp(startingPosition, _indicatorPosition, _indicatorMoveCurve.Evaluate(num));
			yield return null;
		}
		_tabIndicator.anchoredPosition = _indicatorPosition;
		_moveIndicatorCo = null;
	}

	public void SetTabIndicatorText(int index, string text)
	{
		if (index < 0 || index >= _tabItems.Count)
		{
			Debug.LogError((object)("Invalid tab index: " + index));
		}
		else
		{
			_tabItems[index].SetIndicator(text);
		}
	}

	public void HideTabIndicator(int index)
	{
		if (index < 0 || index >= _tabItems.Count)
		{
			Debug.LogError((object)("Invalid tab index: " + index));
		}
		else
		{
			_tabItems[index].HideIndicator();
		}
	}

	public void SubscribeToTabSelected(TabSelectedEvent handler)
	{
		_onTabSelected = (TabSelectedEvent)Delegate.Combine(_onTabSelected, handler);
	}

	public void UnsubscribeFromTabSelected(TabSelectedEvent handler)
	{
		_onTabSelected = (TabSelectedEvent)Delegate.Remove(_onTabSelected, handler);
	}

	private IEnumerator DoDelayRoutine(float delay, Action onComplete)
	{
		yield return (object)new WaitForSeconds(delay);
		onComplete?.Invoke();
	}
}
