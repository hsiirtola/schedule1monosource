using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;
using TMPro;
using UnityEngine;

namespace ScheduleOne.Misc;

public class DigitalAlarm : MonoBehaviour
{
	public const float FLASH_FREQUENCY = 4f;

	public MeshRenderer ScreenMesh;

	public int ScreenMeshMaterialIndex;

	public TextMeshPro ScreenText;

	public bool FlashScreen;

	[Header("Settings")]
	public bool DisplayCurrentTime;

	public Material ScreenOffMat;

	public Material ScreenOnMat;

	private bool isLit;

	private void Start()
	{
		NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		NetworkSingleton<TimeManager>.Instance.onMinutePass += new Action(MinPass);
	}

	private void OnDestroy()
	{
		if ((Object)(object)NetworkSingleton<TimeManager>.Instance != (Object)null)
		{
			NetworkSingleton<TimeManager>.Instance.onMinutePass -= new Action(MinPass);
		}
	}

	public void SetScreenLit(bool lit)
	{
		if (lit != isLit)
		{
			Material[] materials = ((Renderer)ScreenMesh).materials;
			materials[ScreenMeshMaterialIndex] = (lit ? ScreenOnMat : ScreenOffMat);
			((Renderer)ScreenMesh).materials = materials;
			isLit = lit;
		}
	}

	public void DisplayText(string text)
	{
		((TMP_Text)ScreenText).text = text;
	}

	public void DisplayMinutes(int mins)
	{
		int num = mins / 60;
		mins %= 60;
		DisplayText($"{num:D2}:{mins:D2}");
	}

	private void MinPass()
	{
		if (DisplayCurrentTime)
		{
			DisplayText(TimeManager.Get12HourTime(NetworkSingleton<TimeManager>.Instance.CurrentTime, appendDesignator: false));
		}
	}

	private void FixedUpdate()
	{
		if (FlashScreen)
		{
			float num = Mathf.Sin(Time.timeSinceLevelLoad * 4f);
			SetScreenLit(num > 0f);
		}
	}
}
