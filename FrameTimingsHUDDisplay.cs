using System;
using System.Collections.Generic;
using UnityEngine;

public class FrameTimingsHUDDisplay : MonoBehaviour
{
	public struct FrameTimingPoint
	{
		public double cpuFrameTime;

		public double cpuMainThreadFrameTime;

		public double cpuRenderThreadFrameTime;

		public double gpuFrameTime;
	}

	private GUIStyle m_Style;

	private readonly FrameTiming[] m_FrameTimings = (FrameTiming[])(object)new FrameTiming[1];

	public const int SAMPLE_SIZE = 200;

	public List<FrameTimingPoint> frameTimingsHistory = new List<FrameTimingPoint>();

	private void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		m_Style = new GUIStyle();
		m_Style.fontSize = 15;
		m_Style.normal.textColor = Color.white;
	}

	private void OnGUI()
	{
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		CaptureTimings();
		frameTimingsHistory.Add(new FrameTimingPoint
		{
			cpuFrameTime = m_FrameTimings[0].cpuFrameTime,
			cpuMainThreadFrameTime = m_FrameTimings[0].cpuMainThreadFrameTime,
			cpuRenderThreadFrameTime = m_FrameTimings[0].cpuRenderThreadFrameTime,
			gpuFrameTime = m_FrameTimings[0].gpuFrameTime
		});
		if (frameTimingsHistory.Count > 200)
		{
			frameTimingsHistory.RemoveAt(0);
		}
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		double num4 = 0.0;
		for (int i = 0; i < frameTimingsHistory.Count; i++)
		{
			num += frameTimingsHistory[i].cpuFrameTime;
			num2 += frameTimingsHistory[i].cpuMainThreadFrameTime;
			num3 += frameTimingsHistory[i].cpuRenderThreadFrameTime;
			num4 += frameTimingsHistory[i].gpuFrameTime;
		}
		num /= (double)frameTimingsHistory.Count;
		num2 /= (double)frameTimingsHistory.Count;
		num3 /= (double)frameTimingsHistory.Count;
		num4 /= (double)frameTimingsHistory.Count;
		string text = $"\nCPU: {num:00.00}" + $"\nMain Thread: {num2:00.00}" + $"\nRender Thread: {num3:00.00}" + $"\nGPU: {num4:00.00}";
		Color color = GUI.color;
		GUI.color = new Color(1f, 1f, 1f, 1f);
		float num5 = 300f;
		float num6 = 210f;
		GUILayout.BeginArea(new Rect(32f, 50f, num5, num6), "Frame Stats", GUI.skin.window);
		GUILayout.Label(text, m_Style, Array.Empty<GUILayoutOption>());
		GUILayout.EndArea();
		GUI.color = color;
	}

	private void CaptureTimings()
	{
		FrameTimingManager.CaptureFrameTimings();
		FrameTimingManager.GetLatestTimings((uint)m_FrameTimings.Length, m_FrameTimings);
	}
}
