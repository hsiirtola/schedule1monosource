using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrabScreenFeature : ScriptableRendererFeature
{
	[Serializable]
	public class Settings
	{
		public string TextureName = "_GrabPassTransparent";

		public RenderPassEvent RenderPassEvent = (RenderPassEvent)500;

		public LayerMask LayerMask;
	}

	public class GrabPass : ScriptableRenderPass
	{
		private Settings settings;

		private RTHandle m_GrabbedTextureHandle;

		private RTHandle m_CameraColorHandle;

		public GrabPass(Settings s)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			settings = s;
			((ScriptableRenderPass)this).renderPassEvent = settings.RenderPassEvent;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			ScriptableRenderer renderer = renderingData.cameraData.renderer;
			m_CameraColorHandle = renderer.cameraColorTargetHandle;
			RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			((RenderTextureDescriptor)(ref cameraTargetDescriptor)).depthBufferBits = 0;
			RenderingUtils.ReAllocateIfNeeded(ref m_GrabbedTextureHandle, ref cameraTargetDescriptor, (FilterMode)1, (TextureWrapMode)1, false, 1, 0f, settings.TextureName);
			cmd.SetGlobalTexture(settings.TextureName, RTHandle.op_Implicit(m_GrabbedTextureHandle));
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Invalid comparison between Unknown and I4
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Invalid comparison between Unknown and I4
			Camera camera = renderingData.cameraData.camera;
			if (m_CameraColorHandle != null && m_GrabbedTextureHandle != null && ((int)camera.cameraType == 1 || (int)camera.cameraType == 2))
			{
				CommandBuffer val = CommandBufferPool.Get("Grab Screen Pass");
				Blitter.BlitCameraTexture(val, m_CameraColorHandle, m_GrabbedTextureHandle, 0f, false);
				((ScriptableRenderContext)(ref context)).ExecuteCommandBuffer(val);
				CommandBufferPool.Release(val);
			}
		}

		public void Dispose()
		{
			m_GrabbedTextureHandle = null;
		}
	}

	private class RenderPass : ScriptableRenderPass
	{
		private Settings settings;

		private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

		private FilteringSettings m_FilteringSettings;

		private RenderStateBlock m_RenderStateBlock;

		public RenderPass(Settings settings)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			this.settings = settings;
			((ScriptableRenderPass)this).renderPassEvent = (RenderPassEvent)501;
			m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
			m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
			m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
			m_FilteringSettings = new FilteringSettings((RenderQueueRange?)RenderQueueRange.all, LayerMask.op_Implicit(settings.LayerMask), uint.MaxValue, 0);
			m_RenderStateBlock = new RenderStateBlock((RenderStateMask)0);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			CommandBuffer val = CommandBufferPool.Get("Render Grabbed Objects");
			((ScriptableRenderContext)(ref context)).ExecuteCommandBuffer(val);
			val.Clear();
			DrawingSettings val2 = ((ScriptableRenderPass)this).CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, (SortingCriteria)23);
			((ScriptableRenderContext)(ref context)).DrawRenderers(renderingData.cullResults, ref val2, ref m_FilteringSettings, ref m_RenderStateBlock);
			((ScriptableRenderContext)(ref context)).ExecuteCommandBuffer(val);
			CommandBufferPool.Release(val);
		}
	}

	private GrabPass grabPass;

	private RenderPass renderPass;

	[SerializeField]
	private Settings settings = new Settings();

	public override void Create()
	{
		grabPass = new GrabPass(settings);
		renderPass = new RenderPass(settings);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass((ScriptableRenderPass)(object)grabPass);
		renderer.EnqueuePass((ScriptableRenderPass)(object)renderPass);
	}

	protected override void Dispose(bool disposing)
	{
		grabPass?.Dispose();
	}
}
