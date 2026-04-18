using System;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Persistence;

public class SaveRequest
{
	public ISaveable Saveable;

	public string ParentFolderPath;

	public string SaveString { get; private set; }

	public SaveRequest(ISaveable saveable, string parentFolderPath)
	{
		Saveable = saveable;
		ParentFolderPath = parentFolderPath;
		try
		{
			SaveString = saveable.GetSaveString();
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Error generating save string for " + saveable.GetType().ToString() + ": " + ex.Message));
			SaveString = string.Empty;
		}
		if (SaveString != string.Empty)
		{
			Singleton<SaveManager>.Instance.QueueSaveRequest(this);
		}
		else
		{
			saveable.CompleteSave(parentFolderPath, writeDataFile: false);
		}
	}

	public void Complete()
	{
		Singleton<SaveManager>.Instance.DequeueSaveRequest(this);
		Saveable.WriteBaseData(ParentFolderPath, SaveString);
	}
}
