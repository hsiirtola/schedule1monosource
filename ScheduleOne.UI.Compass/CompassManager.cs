using System;
using System.Collections;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI.Compass;

public class CompassManager : Singleton<CompassManager>
{
	public class Notch
	{
		public RectTransform Rect;

		public CanvasGroup Group;
	}

	public class Element
	{
		public bool LastState;

		public bool Visible;

		public RectTransform Rect;

		public CanvasGroup Group;

		public TextMeshProUGUI DistanceLabel;

		public Transform Transform;
	}

	public const int NOTCH_COUNT = 24;

	public const float DISTANCE_LABEL_THRESHOLD = 50f;

	[Header("References")]
	public RectTransform Container;

	public RectTransform NotchUIContainer;

	public RectTransform ElementUIContainer;

	public Canvas Canvas;

	[Header("Prefabs")]
	public GameObject DirectionIndicatorPrefab;

	public GameObject NotchPrefab;

	public GameObject ElementPrefab;

	[Header("Settings")]
	public bool CompassEnabled = true;

	public Vector2 ElementContentSize = new Vector2(20f, 20f);

	public float CompassUIRange = 800f;

	public float FullAlphaRange = 40f;

	public float AngleDivisor = 60f;

	public float ClosedYPos = 30f;

	public float OpenYPos = -50f;

	private List<Vector3> notchPositions = new List<Vector3>();

	private List<Notch> notches = new List<Notch>();

	private List<Element> elements = new List<Element>();

	private Coroutine lerpContainerPositionCoroutine;

	private Transform cam => ((Component)PlayerSingleton<PlayerCamera>.Instance).transform;

	protected override void Awake()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		notchPositions = new List<Vector3>();
		for (int i = 0; i < 24; i++)
		{
			float num = i * 15;
			float num2 = Mathf.Cos((float)System.Math.PI / 180f * num);
			float num3 = Mathf.Sin((float)System.Math.PI / 180f * num);
			Vector3 item = new Vector3(num2, 0f, num3) * 10000f;
			notchPositions.Add(item);
		}
		for (int j = 0; j < notchPositions.Count; j++)
		{
			GameObject val = NotchPrefab;
			int num4 = Mathf.RoundToInt((float)j / (float)notchPositions.Count * 360f) + 90;
			if (num4 % 90 == 0)
			{
				val = DirectionIndicatorPrefab;
			}
			GameObject val2 = Object.Instantiate<GameObject>(val, (Transform)(object)NotchUIContainer);
			Notch notch = new Notch();
			notch.Rect = val2.GetComponent<RectTransform>();
			notch.Group = val2.GetComponent<CanvasGroup>();
			notches.Add(notch);
			if (num4 % 90 == 0)
			{
				string text = "S";
				switch (num4)
				{
				case 90:
					text = "E";
					break;
				case 180:
					text = "N";
					break;
				case 270:
					text = "W";
					break;
				}
				((TMP_Text)((Component)notch.Rect).GetComponentInChildren<TextMeshProUGUI>()).text = text;
			}
		}
	}

	private void LateUpdate()
	{
		if (PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			((Behaviour)Canvas).enabled = ((Behaviour)Singleton<HUD>.Instance.canvas).enabled && CompassEnabled;
		}
	}

	private void Update()
	{
		if (PlayerSingleton<PlayerCamera>.InstanceExists && ((Behaviour)Singleton<HUD>.Instance.canvas).enabled)
		{
			UpdateNotches();
			UpdateElements();
		}
	}

	public void SetCompassEnabled(bool enabled)
	{
		CompassEnabled = enabled;
	}

	public void SetVisible(bool visible)
	{
		if (lerpContainerPositionCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(lerpContainerPositionCoroutine);
		}
		lerpContainerPositionCoroutine = ((MonoBehaviour)this).StartCoroutine(LerpContainerPosition(visible ? OpenYPos : ClosedYPos, visible));
		IEnumerator LerpContainerPosition(float yPos, bool flag)
		{
			if (flag)
			{
				((Component)Container).gameObject.SetActive(true);
			}
			float t = 0f;
			Vector2 startPos = Container.anchoredPosition;
			Vector2 endPos = new Vector2(startPos.x, yPos);
			while (t < 1f)
			{
				t += Time.deltaTime * 7f;
				Container.anchoredPosition = new Vector2(0f, Mathf.Lerp(startPos.y, endPos.y, t));
				yield return null;
			}
			Container.anchoredPosition = endPos;
			((Component)Container).gameObject.SetActive(flag);
		}
	}

	private void UpdateNotches()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < notchPositions.Count; i++)
		{
			GetCompassData(notchPositions[i], out var xPos, out var _);
			notches[i].Rect.anchoredPosition = new Vector2(xPos, 0f);
		}
	}

	private void UpdateElements()
	{
		for (int i = 0; i < elements.Count; i++)
		{
			UpdateElement(elements[i]);
		}
	}

	private void UpdateElement(Element element)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (!element.Visible || ((Object)(object)element.Transform == (Object)null && element.LastState))
		{
			element.Group.alpha = 0f;
			element.LastState = false;
			return;
		}
		if (!element.LastState)
		{
			element.Group.alpha = 1f;
			element.LastState = true;
		}
		GetCompassData(element.Transform.position, out var xPos, out var _);
		element.Rect.anchoredPosition = new Vector2(xPos, 0f);
		float num = Vector3.Distance(cam.position, element.Transform.position);
		if (num <= 50f)
		{
			((TMP_Text)element.DistanceLabel).text = UnitsUtility.FormatShortDistance(num, UnitsUtility.ERoundingType.Up);
		}
		else
		{
			((TMP_Text)element.DistanceLabel).text = string.Empty;
		}
	}

	public void GetCompassData(Vector3 worldPosition, out float xPos, out float alpha)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		Transform val = cam;
		Vector3 forward = val.forward;
		forward.y = 0f;
		((Vector3)(ref forward)).Normalize();
		Vector3 val2 = worldPosition - val.position;
		val2.y = 0f;
		float num = Vector3.SignedAngle(forward, val2, Vector3.up);
		xPos = Mathf.Clamp(num / AngleDivisor, -1f, 1f) * CompassUIRange * 0.5f;
		alpha = 1f;
		if (Mathf.Abs(num) > FullAlphaRange)
		{
			alpha = 1f - (Mathf.Abs(num) - FullAlphaRange) / (AngleDivisor - FullAlphaRange);
		}
	}

	public Element AddElement(Transform transform, RectTransform contentPrefab, bool visible = true)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Element element = new Element();
		element.Transform = transform;
		element.Rect = Object.Instantiate<GameObject>(ElementPrefab, (Transform)(object)ElementUIContainer).GetComponent<RectTransform>();
		element.Group = ((Component)element.Rect).GetComponent<CanvasGroup>();
		element.DistanceLabel = ((Component)((Transform)element.Rect).Find("Text")).GetComponent<TextMeshProUGUI>();
		RectTransform component = ((Component)Object.Instantiate<RectTransform>(contentPrefab, (Transform)(object)element.Rect)).GetComponent<RectTransform>();
		component.anchoredPosition = Vector2.zero;
		component.sizeDelta = ElementContentSize;
		element.Visible = visible;
		element.LastState = !visible;
		elements.Add(element);
		UpdateElement(element);
		return element;
	}

	public void RemoveElement(Transform transform, bool alsoDestroyRect = true)
	{
		for (int i = 0; i < elements.Count; i++)
		{
			if ((Object)(object)elements[i].Transform == (Object)(object)transform)
			{
				RemoveElement(elements[i], alsoDestroyRect);
				break;
			}
		}
	}

	public void RemoveElement(Element el, bool alsoDestroyRect = true)
	{
		if (alsoDestroyRect)
		{
			Object.Destroy((Object)(object)((Component)el.Rect).gameObject);
		}
		elements.Remove(el);
	}
}
