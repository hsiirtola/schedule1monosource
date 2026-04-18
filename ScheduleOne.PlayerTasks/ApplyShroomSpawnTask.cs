using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ScheduleOne.PlayerTasks;

public class ApplyShroomSpawnTask : Task
{
	private enum EStage
	{
		BreakUpChunks,
		MixIntoSoil
	}

	private const float DistanceBetweenMixes = 0.08f;

	private const float MixRadius = 0.1f;

	private const int MaskTextureSize = 128;

	private const int SmallChunkCount = 16;

	private ShroomSpawnDefinition _spawnDefinition;

	private MushroomBed _mushroomBed;

	private SpawnChunk _baseSpawnChunk;

	private EStage _currentStage;

	private DecalProjector _mixProjector;

	private Vector3 _lastMixPosition;

	private Texture2D _maskingTexture;

	private List<SpawnChunk> _mixedChunks = new List<SpawnChunk>();

	private bool _mixMouseUp;

	public ApplyShroomSpawnTask(MushroomBed mushroomBed, ShroomSpawnDefinition spawnDefinition)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mushroomBed == (Object)null)
		{
			Console.LogWarning("ApplyShroomSpawnTask: mushroomBed null");
			StopTask();
			return;
		}
		ClickDetectionEnabled = true;
		ClickDetectionRadius = 0.02f;
		_spawnDefinition = spawnDefinition;
		_mushroomBed = mushroomBed;
		_mushroomBed.SetPlayerUser(((NetworkBehaviour)Player.Local).NetworkObject);
		Transform cameraPosition = mushroomBed.CameraHandler.GetCameraPosition(GrowContainerCameraHandler.ECameraPosition.Midshot);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cameraPosition.position, cameraPosition.rotation, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("applyshroomspawn_breakchunks");
		_baseSpawnChunk = ((Component)Object.Instantiate<SpawnChunk>(spawnDefinition.ChunkPrefab, NetworkSingleton<GameManager>.Instance.Temp)).GetComponent<SpawnChunk>();
		_baseSpawnChunk.DisableChunk(recursive: true);
		((Component)_baseSpawnChunk).transform.position = mushroomBed.PourableStartPoint.position;
		((Component)_baseSpawnChunk).transform.rotation = Quaternion.LookRotation(((Component)_baseSpawnChunk).transform.position - cameraPosition.position, Vector3.up);
		_baseSpawnChunk.EnableChunk(Vector3.zero, Vector3.zero);
		_baseSpawnChunk.SetChunkOrder(0);
		_mixProjector = Object.Instantiate<DecalProjector>(spawnDefinition.MixTaskProjectorPrefab, NetworkSingleton<GameManager>.Instance.Temp);
		((Component)_mixProjector).gameObject.SetActive(false);
		_mixProjector.size = new Vector3(0.25f, 0.25f, _mixProjector.size.z);
		_maskingTexture = CreateMaskTexture();
		UpdateInstructionText();
	}

	public override void StopTask()
	{
		base.StopTask();
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.25f);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.25f);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		_mushroomBed.SetPlayerUser(null);
		if (Outcome != EOutcome.Success)
		{
			_mushroomBed.ConfigureSoilAppearance(MushroomBed.EMushroomBedSoilAppearance.NoSpores);
		}
		Object.Destroy((Object)(object)((Component)_baseSpawnChunk).gameObject);
		Object.Destroy((Object)(object)((Component)_mixProjector).gameObject);
		Object.Destroy((Object)(object)_maskingTexture);
		Singleton<CursorManager>.Instance.SetCursorAppearance(CursorManager.ECursorType.Default);
	}

	public override void Success()
	{
		base.Success();
		if (NetworkSingleton<VariableDatabase>.InstanceExists)
		{
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("AppliedShroomSpawnCount", (NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("AppliedShroomSpawnCount") + 1f).ToString());
		}
		PlayerSingleton<PlayerInventory>.Instance.RemoveAmountOfItem(((BaseItemDefinition)_spawnDefinition).ID);
		_mushroomBed.ConfigureSoilAppearance(MushroomBed.EMushroomBedSoilAppearance.FullSpores);
		_mushroomBed.CreateAndAssignColony_Server(((BaseItemDefinition)_spawnDefinition).ID);
		_mushroomBed.CheckShowTemperatureHint();
	}

	public override void Update()
	{
		base.Update();
		if ((Object)(object)_mushroomBed == (Object)null)
		{
			StopTask();
			return;
		}
		UpdateProgression();
		UpdateInstructionText();
	}

	public override void LateUpdate()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		base.LateUpdate();
		if (_currentStage != EStage.MixIntoSoil)
		{
			return;
		}
		if (!GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
		{
			_mixMouseUp = true;
		}
		if (GetCursorHoverOnSoil(out var hitPoint))
		{
			((Component)_mixProjector).transform.position = hitPoint;
			((Component)_mixProjector).transform.forward = Vector3.down;
			_mixProjector.fadeFactor = (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) ? 0.4f : 0.08f);
			((Component)_mixProjector).gameObject.SetActive(true);
			if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick) && _mixMouseUp && (Vector3.Distance(hitPoint, _lastMixPosition) >= 0.08f || GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick)))
			{
				TriggerMix(hitPoint);
			}
		}
		else
		{
			((Component)_mixProjector).gameObject.SetActive(false);
		}
	}

	private void UpdateInstructionText()
	{
		if (!((Object)(object)_mushroomBed == (Object)null))
		{
			if (_currentStage == EStage.BreakUpChunks)
			{
				base.CurrentInstruction = "Break shroom spawn into small chunks";
			}
			else if (_currentStage == EStage.MixIntoSoil)
			{
				base.CurrentInstruction = "Mix the chunks into the soil (" + _mixedChunks.Count + "/" + 16 + ")";
			}
		}
	}

	private void UpdateProgression()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (_currentStage == EStage.BreakUpChunks)
		{
			if (_baseSpawnChunk.GetIsBroken())
			{
				_currentStage = EStage.MixIntoSoil;
				Transform cameraPosition = _mushroomBed.CameraHandler.GetCameraPosition(GrowContainerCameraHandler.ECameraPosition.BirdsEye);
				PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cameraPosition.position, cameraPosition.rotation, 0.2f);
				GetCursorHoverOnSoil(out _lastMixPosition);
				_mushroomBed.ConfigureSoilAppearance(MushroomBed.EMushroomBedSoilAppearance.MaskedSpores, _maskingTexture);
				Singleton<InputPromptsCanvas>.Instance.LoadModule("applyshroomspawn_mix");
			}
		}
		else if (_currentStage == EStage.MixIntoSoil && _mixedChunks.Count >= 16)
		{
			Success();
		}
	}

	private bool GetCursorHoverOnSoil(out Vector3 hitPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		hitPoint = Vector3.zero;
		Plane val = default(Plane);
		((Plane)(ref val))._002Ector(((Component)_mushroomBed.SoilContainer).transform.up, ((Component)_mushroomBed.SoilContainer).transform.position);
		Ray mouseRay = PlayerSingleton<PlayerCamera>.Instance.GetMouseRay();
		float num = default(float);
		if (((Plane)(ref val)).Raycast(mouseRay, ref num))
		{
			hitPoint = ((Ray)(ref mouseRay)).GetPoint(num);
		}
		return _mushroomBed.IsPointAboveGrowSurface(hitPoint);
	}

	private void TriggerMix(Vector3 mixPoint)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		_lastMixPosition = mixPoint;
		_mushroomBed.PlayMixFXAtPoint(mixPoint);
		Collider[] array = Physics.OverlapSphere(mixPoint, 0.1f, LayerMask.GetMask(new string[1] { "Task" }));
		SpawnChunk spawnChunk = default(SpawnChunk);
		for (int i = 0; i < array.Length; i++)
		{
			if (((Component)array[i]).TryGetComponent<SpawnChunk>(ref spawnChunk))
			{
				((Component)spawnChunk).GetComponent<Collider>().enabled = false;
				Object.Destroy((Object)(object)((Component)spawnChunk).gameObject, 0.5f);
				if (!_mixedChunks.Contains(spawnChunk))
				{
					_mixedChunks.Add(spawnChunk);
				}
			}
		}
		Vector3 val = ((Component)_mushroomBed.SoilContainer).transform.InverseTransformPoint(mixPoint);
		float num = Mathf.InverseLerp(_mushroomBed.GetGrowSurfaceSideLength() / 2f, (0f - _mushroomBed.GetGrowSurfaceSideLength()) / 2f, val.x);
		float num2 = Mathf.InverseLerp(_mushroomBed.GetGrowSurfaceSideLength() / 2f, (0f - _mushroomBed.GetGrowSurfaceSideLength()) / 2f, val.z);
		int x = Mathf.FloorToInt(num * 128f);
		int y = Mathf.FloorToInt(num2 * 128f);
		PaintMask(x, y);
	}

	private void PaintMask(int x, int y)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_maskingTexture == (Object)null)
		{
			return;
		}
		int num = Mathf.FloorToInt(0.1f / _mushroomBed.GetGrowSurfaceSideLength() * 128f);
		for (int i = -num; i <= num; i++)
		{
			for (int j = -num; j <= num; j++)
			{
				int num2 = x + i;
				int num3 = y + j;
				if (num2 >= 0 && num2 < 128 && num3 >= 0 && num3 < 128 && Mathf.Sqrt((float)(i * i + j * j)) <= (float)num)
				{
					_maskingTexture.SetPixel(num2, num3, Color.white);
				}
			}
		}
		_maskingTexture.Apply();
	}

	private Texture2D CreateMaskTexture()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(128, 128);
		Color[] array = (Color[])(object)new Color[16384];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Color.black;
		}
		val.SetPixels(array);
		val.Apply();
		return val;
	}
}
