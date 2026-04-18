using System;
using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Interaction;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.UI.Compass;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScheduleOne.Graffiti;

[RequireComponent(typeof(SpraySurface))]
public class SpraySurfaceInteraction : MonoBehaviour
{
	private const float CameraLerpTime = 0.15f;

	private const int MaxPixelsBeforeNewStroke = 1000;

	private const int ManhattanDistanceBetweenPaintedPixels = 3;

	private const int FixedPaintedPixelLimit = 25000;

	private const int CanvasPadding = 12;

	public SpraySurface SpraySurface;

	public InteractableObject IntObj;

	public Transform CameraPosition;

	public Canvas Canvas;

	public Image SprayImg;

	public AudioSourceController SpraySound;

	public AudioSourceController CleanSound;

	public bool _allowDraw = true;

	[Header("Settings")]
	[SerializeField]
	public float PaintedPixelLimitMultiplier = 1f;

	private ESprayColor selectedColor = ESprayColor.Black;

	private byte selectedStrokeSize = 24;

	private UShort2 lastPaintedPixelCoord;

	private bool paintedLastFrame;

	private List<UShort2> currentStrokePixels = new List<UShort2>();

	private bool isPaintingStroke;

	private float timeSinceStrokeStart;

	private int _startPaintedPixelCount;

	public bool IsOpen { get; private set; }

	private bool confirmationPanelOpen => ((Component)Singleton<GraffitiMenu>.Instance.ConfirmPanel).gameObject.activeSelf;

	private int _paintedPixelLimit => Mathf.RoundToInt(25000f * PaintedPixelLimitMultiplier);

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		IntObj.onHovered.AddListener(new UnityAction(Hovered));
		IntObj.onInteractStart.AddListener(new UnityAction(Interacted));
		GameInput.RegisterExitListener(Exit, 2);
		((Component)Canvas).gameObject.SetActive(false);
		ResizeCanvas();
	}

	private void Start()
	{
		if ((Object)(object)Player.Local != (Object)null)
		{
			PlayerSpawned();
		}
		else
		{
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		}
	}

	private void PlayerSpawned()
	{
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		PlayerInventory instance = PlayerSingleton<PlayerInventory>.Instance;
		instance.onEquippedSlotChanged = (Action<int>)Delegate.Combine(instance.onEquippedSlotChanged, new Action<int>(EquippedSlotChanged));
	}

	private void OnDestroy()
	{
		if (PlayerSingleton<PlayerInventory>.InstanceExists)
		{
			PlayerInventory instance = PlayerSingleton<PlayerInventory>.Instance;
			instance.onEquippedSlotChanged = (Action<int>)Delegate.Remove(instance.onEquippedSlotChanged, new Action<int>(EquippedSlotChanged));
		}
	}

	private void ResizeCanvas()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		((Component)Canvas).GetComponent<RectTransform>().sizeDelta = new Vector2((float)(SpraySurface.Width - 12) * 0.006666671f, (float)(SpraySurface.Height - 12) * 0.006666671f) * 100f;
		((Component)Canvas).transform.localPosition = new Vector3(SpraySurface.BottomLeftPoint.localPosition.x - (float)SpraySurface.Width * 0.006666671f / 2f, SpraySurface.BottomLeftPoint.localPosition.y + (float)SpraySurface.Height * 0.006666671f / 2f, ((Component)Canvas).transform.localPosition.z);
		IntObj.displayLocationPoint.position = ((Component)Canvas).transform.position - ((Component)this).transform.up * 0.25f;
		float num = Mathf.Max((float)SpraySurface.Width, (float)SpraySurface.Height * 1.6f) * 0.006666671f / 2f + 0.5f;
		((Component)CameraPosition).transform.localPosition = ((Component)Canvas).transform.localPosition + new Vector3(0f, -0.1f, num);
		((Component)IntObj).transform.localScale = new Vector3((float)SpraySurface.Width * 0.006666671f, (float)SpraySurface.Height * 0.006666671f, ((Component)IntObj).transform.localScale.z);
		((Component)IntObj).transform.position = ((Component)Canvas).transform.position;
	}

	private void Update()
	{
		if (!IsOpen)
		{
			return;
		}
		CheckCameraInBounds();
		if (IsOpen)
		{
			UpdateCursor();
			if (isPaintingStroke)
			{
				UpdateSpraySound();
				UpdateRemainingPaintIndicator();
			}
		}
	}

	private void UpdateCursor()
	{
		if (IsOpen)
		{
			ushort pixelX;
			ushort pixelY;
			bool cursorPositionOnSurface = GetCursorPositionOnSurface(out pixelX, out pixelY);
			Singleton<CursorManager>.Instance.SetCursorAppearance((cursorPositionOnSurface && !confirmationPanelOpen) ? CursorManager.ECursorType.Spray : CursorManager.ECursorType.Default);
		}
	}

	private void UpdateSpraySound()
	{
		SpraySound.VolumeMultiplier = Mathf.Lerp(0.3f, 1f, Mathf.Clamp01(timeSinceStrokeStart / 0.2f));
	}

	private void CheckCameraInBounds()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3.Distance(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, SpraySurface.BottomLeftPoint.position) > (float)Mathf.Max(SpraySurface.Width, SpraySurface.Height) * 0.006666671f * 2f)
		{
			Console.LogWarning("SpraySurfaceInteraction: Player camera moved too far from spray surface, closing interaction.");
			Clear();
			Close();
		}
	}

	private void FixedUpdate()
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		if (!IsOpen)
		{
			return;
		}
		bool flag = false;
		if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && GetCursorPositionOnSurface(out var pixelX, out var pixelY) && !confirmationPanelOpen && !EventSystem.current.IsPointerOverGameObject() && SpraySurface.DrawingPaintedPixelCount < _paintedPixelLimit)
		{
			timeSinceStrokeStart += Time.fixedDeltaTime;
			if (!isPaintingStroke)
			{
				StartStroke();
				UpdateSpraySound();
			}
			int padding = SpraySurface.GetPadding(selectedStrokeSize);
			pixelX = (ushort)Mathf.Clamp((int)pixelX, padding, SpraySurface.Width - padding);
			pixelY = (ushort)Mathf.Clamp((int)pixelY, padding, SpraySurface.Height - padding);
			((Component)SpraySound).transform.position = SpraySurface.ToWorldPosition(new UShort2(pixelX, pixelY));
			int num = Mathf.Abs(lastPaintedPixelCoord.X - pixelX);
			int num2 = Mathf.Abs(lastPaintedPixelCoord.Y - pixelY);
			if (num + num2 < 3)
			{
				return;
			}
			flag = true;
			if (paintedLastFrame)
			{
				int num3 = pixelX - lastPaintedPixelCoord.X;
				int num4 = pixelY - lastPaintedPixelCoord.Y;
				int num5 = Mathf.Max(Mathf.Abs(num3), Mathf.Abs(num4));
				for (int i = 0; i <= num5; i++)
				{
					float num6 = (float)i / (float)num5;
					int num7 = Mathf.RoundToInt(Mathf.Lerp((float)(int)lastPaintedPixelCoord.X, (float)(int)pixelX, num6));
					int num8 = Mathf.RoundToInt(Mathf.Lerp((float)(int)lastPaintedPixelCoord.Y, (float)(int)pixelY, num6));
					if (num7 > 0 || num8 > 0)
					{
						num7 = Mathf.Clamp(num7, padding, SpraySurface.Width - padding);
						num8 = Mathf.Clamp(num8, padding, SpraySurface.Height - padding);
						UShort2 uShort = new UShort2((ushort)num7, (ushort)num8);
						PixelData data = new PixelData(uShort, selectedColor, selectedStrokeSize);
						currentStrokePixels.Add(uShort);
						if (_allowDraw)
						{
							SpraySurface.DrawPaintedPixel(data, applyTexture: false);
						}
					}
				}
			}
			PixelData data2 = new PixelData(new UShort2(pixelX, pixelY), selectedColor, selectedStrokeSize);
			currentStrokePixels.Add(new UShort2(pixelX, pixelY));
			if (_allowDraw)
			{
				SpraySurface.DrawPaintedPixel(data2, applyTexture: true);
			}
			lastPaintedPixelCoord = new UShort2(pixelX, pixelY);
			if (currentStrokePixels.Count > 1000)
			{
				float num9 = timeSinceStrokeStart;
				EndStroke(stopSpraySound: false);
				StartStroke(recordHistory: false);
				lastPaintedPixelCoord = new UShort2(pixelX, pixelY);
				timeSinceStrokeStart = num9;
			}
		}
		if (isPaintingStroke && !flag)
		{
			EndStroke(stopSpraySound: true);
		}
		paintedLastFrame = flag;
	}

	private void StartStroke(bool recordHistory = true)
	{
		if (recordHistory)
		{
			SpraySurface.AddTextureToHistory_Server(Random.Range(int.MinValue, int.MaxValue));
		}
		else
		{
			SpraySurface.CacheDrawing();
		}
		lastPaintedPixelCoord = new UShort2(0, 0);
		isPaintingStroke = true;
		timeSinceStrokeStart = 0f;
		SpraySound.Play();
	}

	private void EndStroke(bool stopSpraySound)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		isPaintingStroke = false;
		List<SprayStroke> strokesFromPixels = SprayStroke.GetStrokesFromPixels(currentStrokePixels, selectedColor, selectedStrokeSize, SpraySurface);
		currentStrokePixels.Clear();
		SpraySurface.RestoreFromCache();
		SpraySurface.AddStrokes_Server(strokesFromPixels, Random.Range(int.MinValue, int.MaxValue));
		for (int i = 0; i < strokesFromPixels.Count; i++)
		{
			UShort2 start = strokesFromPixels[i].Start;
			UShort2 end = strokesFromPixels[i].End;
			Debug.DrawLine(SpraySurface.ToWorldPosition(start), SpraySurface.ToWorldPosition(end), (i % 2 == 0) ? Color.red : Color.green, 10f);
		}
		if (stopSpraySound)
		{
			SpraySound.Stop();
		}
	}

	private bool GetCursorPositionOnSurface(out ushort pixelX, out ushort pixelY)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		pixelX = 0;
		pixelY = 0;
		Plane val = default(Plane);
		((Plane)(ref val))._002Ector(SpraySurface.BottomLeftPoint.forward, SpraySurface.BottomLeftPoint.position);
		Ray cursorRay = GetCursorRay();
		float num = 0f;
		float num2 = 0f;
		bool result = false;
		float num3 = default(float);
		if (((Plane)(ref val)).Raycast(cursorRay, ref num3))
		{
			Vector3 point = ((Ray)(ref cursorRay)).GetPoint(num3);
			Vector2 val2 = Vector2.op_Implicit(SpraySurface.BottomLeftPoint.InverseTransformPoint(point));
			Vector3 val3 = SpraySurface.BottomLeftPoint.InverseTransformPoint(SpraySurface.TopRightPoint);
			num = val2.x / val3.x;
			num2 = val2.y / val3.y;
			num = Mathf.Clamp01(num);
			num2 = Mathf.Clamp01(num2);
			pixelX = (ushort)Mathf.RoundToInt(num * (float)SpraySurface.Width);
			pixelY = (ushort)Mathf.RoundToInt(num2 * (float)SpraySurface.Height);
			int padding = SpraySurface.GetPadding(selectedStrokeSize);
			if (pixelX > padding && pixelX < SpraySurface.Width - padding && pixelY > padding && pixelY < SpraySurface.Height - padding)
			{
				result = true;
			}
		}
		return result;
	}

	private Ray GetCursorRay()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		return PlayerSingleton<PlayerCamera>.Instance.Camera.ScreenPointToRay(Input.mousePosition);
	}

	private void Hovered()
	{
		if (SpraySurface.CanBeEdited(checkEditor: true) && IsSprayCanEquipped())
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			IntObj.SetMessage("Use spray can");
		}
		else if (SpraySurface.DrawingStrokeCount > 0 && IsGraffitiCleanerEquipped())
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Default);
			IntObj.SetMessage("Clean graffiti");
		}
		else
		{
			IntObj.SetInteractableState(InteractableObject.EInteractableState.Disabled);
		}
	}

	private void Interacted()
	{
		if (IsSprayCanEquipped())
		{
			if (SpraySurface.CanBeEdited(checkEditor: true))
			{
				Open();
			}
		}
		else if (IsGraffitiCleanerEquipped() && SpraySurface.DrawingStrokeCount != 0)
		{
			UseGraffitiCleaner();
		}
	}

	private void UseGraffitiCleaner()
	{
		SpraySurface.CleanGraffiti();
		PlayerSingleton<PlayerInventory>.Instance.RemoveAmountOfItem("graffiticleaner");
		PlayerSingleton<PlayerInventory>.Instance.Reequip();
		CleanSound.Play();
	}

	private void Exit(ExitAction action)
	{
		if (action.Used || !IsOpen || action.exitType != ExitType.Escape)
		{
			return;
		}
		action.Use();
		if (SpraySurface.DrawingStrokeCount > 0)
		{
			if (confirmationPanelOpen)
			{
				Close();
			}
			else
			{
				Singleton<GraffitiMenu>.Instance.ShowConfirmPanel();
			}
		}
		else
		{
			Close();
		}
	}

	private void Open()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		IsOpen = true;
		SpraySurface.SetCurrentEditor_Server(((NetworkBehaviour)Player.Local).NetworkObject);
		SpraySurface.EnsureDrawingExists();
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(CameraPosition.position, CameraPosition.rotation, 0.15f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(70f, 0.15f);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement("SpraySurfaceInteraction");
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		if (SpraySurface.IsVandalismSurface)
		{
			Player.Local.VisualState.ApplyState("graffiti", EVisualState.Vandalizing);
		}
		((Behaviour)SprayImg).enabled = false;
		Singleton<GraffitiMenu>.Instance.Open();
		Singleton<GraffitiMenu>.Instance.UpdateRemainingPaintIndicator(1f);
		GraffitiMenu instance = Singleton<GraffitiMenu>.Instance;
		instance.onColorSelected = (Action<ESprayColor>)Delegate.Combine(instance.onColorSelected, new Action<ESprayColor>(SetColor));
		GraffitiMenu instance2 = Singleton<GraffitiMenu>.Instance;
		instance2.onWeightSelected = (Action<byte>)Delegate.Combine(instance2.onWeightSelected, new Action<byte>(SetStrokeSize));
		GraffitiMenu instance3 = Singleton<GraffitiMenu>.Instance;
		instance3.onClearClicked = (Action)Delegate.Combine(instance3.onClearClicked, new Action(Clear));
		GraffitiMenu instance4 = Singleton<GraffitiMenu>.Instance;
		instance4.onDone = (Action)Delegate.Combine(instance4.onDone, new Action(Close));
		GraffitiMenu instance5 = Singleton<GraffitiMenu>.Instance;
		instance5.onUndoClicked = (Action)Delegate.Combine(instance5.onUndoClicked, new Action(Undo));
		Singleton<GraffitiMenu>.Instance.SetActiveSurface(SpraySurface);
		Singleton<CompassManager>.Instance.SetVisible(visible: false);
	}

	private void Close()
	{
		IsOpen = false;
		SpraySurface.OnEditingFinished();
		if (SpraySurface.DrawingStrokeCount > 0)
		{
			PlayerSingleton<PlayerInventory>.Instance.RemoveAmountOfItem("spraypaint");
		}
		if (isPaintingStroke)
		{
			EndStroke(stopSpraySound: true);
		}
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.15f);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.15f);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement("SpraySurfaceInteraction");
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		Player.Local.VisualState.RemoveState("graffiti");
		((Behaviour)SprayImg).enabled = true;
		Singleton<GraffitiMenu>.Instance.Close();
		GraffitiMenu instance = Singleton<GraffitiMenu>.Instance;
		instance.onColorSelected = (Action<ESprayColor>)Delegate.Remove(instance.onColorSelected, new Action<ESprayColor>(SetColor));
		GraffitiMenu instance2 = Singleton<GraffitiMenu>.Instance;
		instance2.onClearClicked = (Action)Delegate.Remove(instance2.onClearClicked, new Action(Clear));
		GraffitiMenu instance3 = Singleton<GraffitiMenu>.Instance;
		instance3.onDone = (Action)Delegate.Remove(instance3.onDone, new Action(Close));
		GraffitiMenu instance4 = Singleton<GraffitiMenu>.Instance;
		instance4.onWeightSelected = (Action<byte>)Delegate.Remove(instance4.onWeightSelected, new Action<byte>(SetStrokeSize));
		GraffitiMenu instance5 = Singleton<GraffitiMenu>.Instance;
		instance5.onUndoClicked = (Action)Delegate.Remove(instance5.onUndoClicked, new Action(Undo));
		Singleton<CompassManager>.Instance.SetVisible(visible: true);
	}

	private void EquippedSlotChanged(int equippedSlotIndex)
	{
		((Component)Canvas).gameObject.SetActive((IsOpen || IsSprayCanEquipped()) && SpraySurface.DrawingStrokeCount == 0 && SpraySurface.CanBeEdited(checkEditor: false));
	}

	private void SetColor(ESprayColor color)
	{
		selectedColor = color;
	}

	private void SetStrokeSize(byte strokeSize)
	{
		selectedStrokeSize = (byte)Mathf.Clamp((int)strokeSize, 10, 32);
		Debug.Log((object)("Stroke size set to: " + selectedStrokeSize));
	}

	private void UpdateRemainingPaintIndicator()
	{
		Singleton<GraffitiMenu>.Instance.UpdateRemainingPaintIndicator(1f - (float)SpraySurface.DrawingPaintedPixelCount / (float)_paintedPixelLimit);
	}

	public void Undo()
	{
		SpraySurface.Undo_Server(Random.Range(int.MinValue, int.MaxValue));
		UpdateRemainingPaintIndicator();
	}

	private void Clear()
	{
		SpraySurface.ClearDrawing();
		Singleton<GraffitiMenu>.Instance.UpdateRemainingPaintIndicator(1f);
	}

	private static bool IsSprayCanEquipped()
	{
		if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped)
		{
			return ((BaseItemInstance)PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance).ID == "spraypaint";
		}
		return false;
	}

	private static bool IsGraffitiCleanerEquipped()
	{
		if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped)
		{
			return ((BaseItemInstance)PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance).ID == "graffiticleaner";
		}
		return false;
	}
}
