using System;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.UI;

public class ReticleUI : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private ReticleLineUI[] _lineUI;

	[SerializeField]
	private CanvasGroup _canvas;

	[Header("Settings")]
	[SerializeField]
	private float _lineLength = 20f;

	[SerializeField]
	private float _lineThickness = 3f;

	[SerializeField]
	private float _borderThickness = 3f;

	[SerializeField]
	private Color _lineColor = Color.white;

	[SerializeField]
	private Color _borderColor = Color.black;

	[SerializeField]
	private float _minGap = 5f;

	[SerializeField]
	private float _lerpSpeed = 15f;

	private float _radius;

	private float _currentRadius;

	private float _lastSpreadAngle;

	public float Alpha
	{
		get
		{
			return _canvas.alpha;
		}
		set
		{
			_canvas.alpha = value;
		}
	}

	private void Awake()
	{
		_canvas.interactable = false;
		_canvas.blocksRaycasts = false;
		ApplyLineSizes();
		ApplyColors();
	}

	private void OnValidate()
	{
		ApplyLineSizes();
		ApplyColors();
	}

	public void Set(float spreadAngle)
	{
		if (!MathUtility.NearlyEqual(spreadAngle, _lastSpreadAngle, 0.001f))
		{
			_lastSpreadAngle = spreadAngle;
			float num = spreadAngle * 0.5f * ((float)System.Math.PI / 180f);
			float num2 = Camera.main.fieldOfView * ((float)System.Math.PI / 180f);
			float num3 = Mathf.Tan(num) / Mathf.Tan(num2 / 2f) * ((float)Screen.height / 2f);
			_radius = Mathf.Max(num3, _minGap);
		}
	}

	private void Update()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		_currentRadius = Mathf.Lerp(_currentRadius, _radius, Time.deltaTime * _lerpSpeed);
		float currentRadius = _currentRadius;
		_lineUI[0].SetPosition(new Vector2(0f, currentRadius));
		_lineUI[1].SetPosition(new Vector2(0f, 0f - currentRadius));
		_lineUI[2].SetPosition(new Vector2(0f - currentRadius, 0f));
		_lineUI[3].SetPosition(new Vector2(currentRadius, 0f));
	}

	private void ApplyLineSizes()
	{
		if (_lineUI.Length >= 4)
		{
			_lineUI[0].SetSize(_lineThickness, _lineLength, _borderThickness);
			_lineUI[1].SetSize(_lineThickness, _lineLength, _borderThickness);
			_lineUI[2].SetSize(_lineLength, _lineThickness, _borderThickness);
			_lineUI[3].SetSize(_lineLength, _lineThickness, _borderThickness);
		}
	}

	private void ApplyColors()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		ReticleLineUI[] lineUI = _lineUI;
		for (int i = 0; i < lineUI.Length; i++)
		{
			lineUI[i].SetColor(_lineColor, _borderColor);
		}
	}
}
