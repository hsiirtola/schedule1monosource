using System;
using System.Collections.Generic;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.PlayerScripts;

public class CursorManager : Singleton<CursorManager>
{
	public enum ECursorType
	{
		Default,
		Finger,
		OpenHand,
		Grab,
		Scissors,
		Spray
	}

	[Serializable]
	public class CursorConfig
	{
		public ECursorType CursorType;

		public Texture2D Texture;

		public Vector2 HotSpot;
	}

	[Header("References")]
	public List<CursorConfig> Cursors = new List<CursorConfig>();

	protected override void Awake()
	{
		base.Awake();
		foreach (ECursorType type in Enum.GetValues(typeof(ECursorType)))
		{
			if (!Cursors.Exists((CursorConfig x) => x.CursorType == type))
			{
				Debug.LogError((object)("Cursor type " + type.ToString() + " not found in CursorManager. Please add it to the Cursors list."));
			}
		}
		SetCursorAppearance(ECursorType.Default);
	}

	public void SetCursorAppearance(ECursorType type)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		CursorConfig cursorConfig = Cursors.Find((CursorConfig x) => x.CursorType == type);
		if (cursorConfig == null)
		{
			Debug.LogError((object)("Cursor type " + type.ToString() + " not found in CursorManager."));
			return;
		}
		Cursor.SetCursor(cursorConfig.Texture, cursorConfig.HotSpot, (CursorMode)0);
		if (Singleton<OnScreenMouse>.InstanceExists)
		{
			Singleton<OnScreenMouse>.Instance.SetTexture((Texture)(object)cursorConfig.Texture, cursorConfig.HotSpot);
		}
	}
}
