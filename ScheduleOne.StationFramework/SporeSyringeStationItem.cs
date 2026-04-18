using System.Collections;
using ScheduleOne.Audio;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.PlayerTasks;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.StationFramework;

public class SporeSyringeStationItem : StationItem
{
	public const float MaxAngleDifferenceForInjection = 35f;

	public const float PlungerPushSpeed = 0.8f;

	public const float PlungerDragDistanceMultiplier = 2f;

	[SerializeField]
	private GameObject _capHighlight;

	[SerializeField]
	private Transform _capContainer;

	[SerializeField]
	private Clickable _capClickable;

	[SerializeField]
	private Draggable _syringeDraggable;

	[SerializeField]
	private GameObject _plungerHighlight;

	[SerializeField]
	private Transform _plungerTransform;

	[SerializeField]
	private Transform _plungerExtendedPosition;

	[SerializeField]
	private Transform _plungerCompressedPosition;

	[SerializeField]
	private Transform _liquidTransform;

	[SerializeField]
	private Clickable _plungerClickable;

	[SerializeField]
	private AudioSourceController _plungerSound;

	private Collider _injectionPortCollider;

	public UnityEvent onCapRemoved;

	public UnityEvent onInserted;

	public UnityEvent<float> onPlungerMoved;

	private bool _capRemoved;

	private Vector3 _initialPlungerHitPoint;

	private float timeOnPlungerClickStart;

	public float PlungerPosition { get; private set; }

	protected override void Awake()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		base.Awake();
		_capClickable.onClickStart.AddListener((UnityAction<RaycastHit>)delegate
		{
			RemoveCap();
		});
		_plungerClickable.onClickStart.AddListener((UnityAction<RaycastHit>)OnPlungerClickStart);
		_plungerClickable.onClickEnd.AddListener(new UnityAction(OnPlungerClickEnd));
		SetCapInteractable(interactable: false);
		SetSyringeDraggable(draggable: false);
		SetPlungerInteractable(interactable: false);
	}

	private void LateUpdate()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		_plungerHighlight.SetActive(_plungerClickable.ClickableEnabled && !_plungerClickable.IsHeld);
		float num = 0f;
		if (_plungerClickable.IsHeld)
		{
			Vector3 val = Vector3.ProjectOnPlane(GetPlungerPlaneHit() - _initialPlungerHitPoint, ((Component)_plungerClickable).transform.forward);
			float num2 = (0f - ((Component)_plungerClickable).transform.InverseTransformVector(val).x) / (Vector3.Distance(_plungerExtendedPosition.localPosition, _plungerCompressedPosition.localPosition) * 2f);
			if (num2 > PlungerPosition)
			{
				float num3 = Mathf.MoveTowards(PlungerPosition, num2, 0.8f * Time.deltaTime);
				num = num3 - PlungerPosition;
				SetPlungerPosition(num3);
			}
		}
		else if (_plungerClickable.ClickableEnabled && GameInput.GetButton(GameInput.ButtonCode.Jump))
		{
			float plungerPosition = Mathf.MoveTowards(PlungerPosition, 1f, 0.8f * Time.deltaTime);
			SetPlungerPosition(plungerPosition);
		}
		_plungerSound.VolumeMultiplier = Mathf.MoveTowards(_plungerSound.VolumeMultiplier, Mathf.Abs(num) * 100f, Time.deltaTime * 5f);
		_plungerSound.PitchMultiplier = Mathf.Lerp(0.8f, 1.2f, PlungerPosition);
	}

	public void SetCapInteractable(bool interactable)
	{
		_capHighlight.SetActive(interactable);
		_capClickable.ClickableEnabled = interactable;
	}

	public void SetInjectionPortCollider(Collider collider)
	{
		_injectionPortCollider = collider;
	}

	private void RemoveCap()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!_capRemoved)
		{
			_capRemoved = true;
			SetCapInteractable(interactable: false);
			((Component)_capContainer).transform.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
			((Component)_capContainer).gameObject.AddComponent<Rigidbody>().AddRelativeForce(-Vector3.right * 1.5f, (ForceMode)2);
			Object.Destroy((Object)(object)((Component)_capContainer).gameObject, 2f);
			if (onCapRemoved != null)
			{
				onCapRemoved.Invoke();
			}
		}
	}

	public void SetSyringeDraggable(bool draggable)
	{
		_syringeDraggable.ClickableEnabled = draggable;
		if ((Object)(object)_syringeDraggable.Rb != (Object)null)
		{
			_syringeDraggable.Rb.isKinematic = !draggable;
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (_syringeDraggable.ClickableEnabled && _syringeDraggable.IsHeld && (Object)(object)other == (Object)(object)_injectionPortCollider && Mathf.Abs(Vector3.SignedAngle(-((Component)_syringeDraggable).transform.right, ((Component)_injectionPortCollider).transform.forward, ((Component)_injectionPortCollider).transform.right)) < 35f)
		{
			InsertSyringe();
		}
	}

	private void InsertSyringe()
	{
		SetSyringeDraggable(draggable: false);
		if (onInserted != null)
		{
			onInserted.Invoke();
		}
		((MonoBehaviour)this).StartCoroutine(MoveSyringe());
		IEnumerator MoveSyringe()
		{
			float duration = 0.2f;
			float elapsed = 0f;
			Vector3 startPosition = ((Component)_syringeDraggable).transform.position;
			Vector3 targetPosition = ((Component)_syringeDraggable).transform.position + -((Component)_syringeDraggable).transform.right * 0.04f;
			while (elapsed < duration && !((Object)(object)_syringeDraggable == (Object)null))
			{
				elapsed += Time.deltaTime;
				float num = Mathf.Clamp01(elapsed / duration);
				((Component)_syringeDraggable).transform.position = Vector3.Lerp(startPosition, targetPosition, num);
				yield return (object)new WaitForEndOfFrame();
			}
		}
	}

	public void SetPlungerInteractable(bool interactable)
	{
		_plungerClickable.ClickableEnabled = interactable;
	}

	private void SetPlungerPosition(float position)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		PlungerPosition = Mathf.Clamp01(position);
		_plungerTransform.localPosition = Vector3.Lerp(_plungerExtendedPosition.localPosition, _plungerCompressedPosition.localPosition, PlungerPosition);
		_liquidTransform.localScale = new Vector3(1f - PlungerPosition, 1f, 1f);
		if (onPlungerMoved != null)
		{
			onPlungerMoved.Invoke(PlungerPosition);
		}
	}

	private void OnPlungerClickStart(RaycastHit hit)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_initialPlungerHitPoint = GetPlungerPlaneHit();
		timeOnPlungerClickStart = Time.timeSinceLevelLoad;
		_plungerSound.VolumeMultiplier = 0f;
		_plungerSound.Play();
	}

	private void OnPlungerClickEnd()
	{
		_plungerSound.Stop();
	}

	private Vector3 GetPlungerPlaneHit()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Plane val = default(Plane);
		((Plane)(ref val))._002Ector(((Component)_plungerClickable).transform.up, ((Component)_plungerClickable).transform.position);
		Ray val2 = PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
		float num = default(float);
		((Plane)(ref val)).Raycast(val2, ref num);
		return ((Ray)(ref val2)).GetPoint(num);
	}
}
