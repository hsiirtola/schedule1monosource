using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Cutscenes;

[RequireComponent(typeof(Animation))]
public class Cutscene : MonoBehaviour
{
	[Header("Settings")]
	public string Name = "Cutscene";

	public bool DisablePlayerControl = true;

	public bool OverrideFOV;

	public float CameraFOV = 70f;

	[Header("References")]
	public Transform CameraControl;

	[Header("Events")]
	public UnityEvent onPlay;

	public UnityEvent onEnd;

	private Animation animation;

	public bool IsPlaying { get; private set; }

	protected virtual void Awake()
	{
		animation = ((Component)this).GetComponent<Animation>();
	}

	private void LateUpdate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (IsPlaying)
		{
			PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraControl.position, CameraControl.rotation, 0f);
		}
	}

	public virtual void Play()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Console.Log("Playing cutscene: " + Name);
		animation.Play();
		IsPlaying = true;
		if (onPlay != null)
		{
			onPlay.Invoke();
		}
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement("Cutscene (" + Name + ")");
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraControl.position, CameraControl.rotation, 0f);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		((Behaviour)Singleton<HUD>.Instance.canvas).enabled = false;
		if (DisablePlayerControl)
		{
			PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
			PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		}
		if (OverrideFOV)
		{
			PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(CameraFOV, 0f);
		}
	}

	public void InvokeEnd()
	{
		Console.Log("Cutscene ended: " + Name);
		animation.Stop();
		IsPlaying = false;
		if (onEnd != null)
		{
			onEnd.Invoke();
		}
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement("Cutscene (" + Name + ")");
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.25f);
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0f, reenableCameraLook: true, returnToOriginalRotation: false);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		((Behaviour)Singleton<HUD>.Instance.canvas).enabled = true;
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
	}
}
