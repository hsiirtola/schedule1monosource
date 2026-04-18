using System;
using FishNet.Object;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Growing;
using ScheduleOne.ObjectScripts;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using UnityEngine;

namespace ScheduleOne.PlayerTasks;

public class SowSeedTask : Task
{
	protected Pot pot;

	protected SeedDefinition definition;

	protected FunctionalSeed seed;

	private bool seedExitedVial;

	private bool seedReachedDestination;

	private bool successfullyPlanted;

	private float weedSeedStationaryTime;

	private bool capRemoved;

	public override string TaskName { get; protected set; } = "Sow seed";

	public SowSeedTask(Pot _pot, SeedDefinition def)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_pot == (Object)null)
		{
			Console.LogWarning("PourIntoPotTask: pot null");
			StopTask();
			return;
		}
		if ((Object)(object)def == (Object)null)
		{
			Console.LogWarning("SowSeedTask: seed definition null");
			StopTask();
			return;
		}
		ClickDetectionEnabled = true;
		pot = _pot;
		((Component)pot.TaskBounds).gameObject.SetActive(true);
		definition = def;
		pot.SetPlayerUser(((NetworkBehaviour)Player.Local).NetworkObject);
		base.CurrentInstruction = "Click cap to remove";
		Transform cameraPosition = pot.CameraHandler.GetCameraPosition(GrowContainerCameraHandler.ECameraPosition.Closeup);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cameraPosition.position, cameraPosition.rotation, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(60f, 0.25f);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		seed = ((Component)Object.Instantiate<FunctionalSeed>(def.FunctionSeedPrefab, GameObject.Find("_Temp").transform)).GetComponent<FunctionalSeed>();
		((Component)seed).transform.position = pot.SeedStartPoint.position;
		Vector3 val = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - ((Component)seed).transform.position;
		val.y = 0f;
		Vector3 position = pot.SeedStartPoint.position;
		Quaternion rotation = Quaternion.LookRotation(val, Vector3.up);
		((Component)seed.Vial).transform.position = position;
		((Component)seed.Vial).transform.rotation = rotation;
		seed.Vial.Rb.position = position;
		seed.Vial.Rb.rotation = rotation;
		Vector3 position2 = pot.SeedStartPoint.position + Vector3.down * 0.05337f;
		((Component)seed.SeedRigidbody).transform.position = position2;
		seed.SeedRigidbody.position = position2;
		FunctionalSeed functionalSeed = seed;
		functionalSeed.onSeedExitVial = (Action)Delegate.Combine(functionalSeed.onSeedExitVial, new Action(OnSeedExitVial));
		seed.Vial.Rb.isKinematic = false;
		seed.SeedRigidbody.isKinematic = false;
		Singleton<InputPromptsCanvas>.Instance.LoadModule("pourable");
		pot.SetSoilState(Pot.ESoilState.Parted);
		SoilChunk[] soilChunks = pot.SoilChunks;
		for (int i = 0; i < soilChunks.Length; i++)
		{
			soilChunks[i].ClickableEnabled = false;
		}
		Singleton<OnScreenMouse>.Instance.Activate();
	}

	public override void Update()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (seedExitedVial && !seedReachedDestination && capRemoved)
		{
			seed.Vial.idleUpForce = 0f;
			Vector3 velocity = seed.SeedRigidbody.velocity;
			if (((Vector3)(ref velocity)).magnitude < 0.08f)
			{
				weedSeedStationaryTime += Time.deltaTime;
			}
			else
			{
				weedSeedStationaryTime = 0f;
			}
			if (weedSeedStationaryTime > 0.2f && Vector3.Distance(((Component)seed.SeedCollider).transform.position, pot.SeedRestingPoint.position) < 0.1f)
			{
				OnSeedReachedDestination();
			}
		}
		if (!capRemoved)
		{
			if (seed.Cap.Removed)
			{
				capRemoved = true;
			}
		}
		else
		{
			base.CurrentInstruction = "Drop seed into hole";
		}
		seed.SeedBlocker.enabled = !capRemoved;
		if (!seedReachedDestination)
		{
			return;
		}
		int num = 0;
		SoilChunk[] soilChunks = pot.SoilChunks;
		for (int i = 0; i < soilChunks.Length; i++)
		{
			if (soilChunks[i].CurrentLerp > 0f)
			{
				num++;
			}
		}
		base.CurrentInstruction = "Click soil chunks to bury seed (" + num + "/" + pot.SoilChunks.Length + ")";
		if (num == pot.SoilChunks.Length)
		{
			Success();
		}
	}

	public override void Success()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		successfullyPlanted = true;
		PlayerSingleton<PlayerInventory>.Instance.RemoveAmountOfItem(((BaseItemDefinition)definition).ID);
		if ((Object)(object)seed.TrashPrefab != (Object)null)
		{
			NetworkSingleton<TrashManager>.Instance.CreateTrashItem(seed.TrashPrefab.ID, Player.Local.Avatar.CenterPoint, Random.rotation);
		}
		pot.PlantSeed_Server(((BaseItemDefinition)definition).ID, 0f);
		pot.SetSoilState(Pot.ESoilState.Packed);
		float value = NetworkSingleton<VariableDatabase>.Instance.GetValue<float>("SownSeedsCount");
		NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("SownSeedsCount", (value + 1f).ToString());
		base.Success();
	}

	public override void StopTask()
	{
		base.StopTask();
		PlayerSingleton<PlayerCamera>.Instance.StopTransformOverride(0.25f);
		PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(0.25f);
		PlayerSingleton<PlayerCamera>.Instance.LockMouse();
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: true);
		PlayerSingleton<PlayerMovement>.Instance.CanMove = true;
		Object.Destroy((Object)(object)((Component)seed).gameObject);
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		if (!successfullyPlanted)
		{
			pot.SetSoilState(Pot.ESoilState.Flat);
		}
		SoilChunk[] soilChunks = pot.SoilChunks;
		foreach (SoilChunk obj in soilChunks)
		{
			obj.StopLerp();
			obj.ClickableEnabled = false;
		}
		((Component)pot.TaskBounds).gameObject.SetActive(false);
		pot.SetPlayerUser(null);
		Singleton<OnScreenMouse>.Instance.Deactivate();
	}

	private void OnSeedExitVial()
	{
		seedExitedVial = true;
	}

	private void OnSeedReachedDestination()
	{
		seedReachedDestination = true;
		((Component)seed.SeedCollider).GetComponent<Rigidbody>().isKinematic = true;
		((Behaviour)((Component)seed.SeedCollider).GetComponent<Draggable>()).enabled = false;
		((Component)seed.Vial).gameObject.SetActive(false);
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(50f, 0.25f);
		SoilChunk[] soilChunks = pot.SoilChunks;
		for (int i = 0; i < soilChunks.Length; i++)
		{
			soilChunks[i].ClickableEnabled = true;
		}
	}
}
