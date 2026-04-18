using System.IO;
using FishNet;
using UnityEngine;

namespace ScheduleOne.Property;

public class SewerOffice : Property
{
	private const string DefaultSaveFilePath = "DefaultSave\\Properties\\Sewer Office.json";

	private bool NetworkInitialize___EarlyScheduleOne_002EProperty_002ESewerOfficeAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EProperty_002ESewerOfficeAssembly_002DCSharp_002Edll_Excuted;

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EProperty_002ESewerOffice_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public void OnPasscodeCorrect()
	{
		if (InstanceFinder.IsServer)
		{
			SetOwned_Server();
		}
	}

	public override bool ShouldSave()
	{
		return true;
	}

	public string GetDefaultSaveFileFullPath()
	{
		return Path.Combine(Application.streamingAssetsPath, "DefaultSave\\Properties\\Sewer Office.json");
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EProperty_002ESewerOfficeAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EProperty_002ESewerOfficeAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EProperty_002ESewerOfficeAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EProperty_002ESewerOfficeAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002EProperty_002ESewerOffice_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		if (!File.Exists(GetDefaultSaveFileFullPath()))
		{
			Console.LogError("SewerOffice: Default save file is missing!");
		}
	}
}
