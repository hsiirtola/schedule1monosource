using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LiquidVolumeFX;

public class LiquidVolumeDepthPrePassRenderFeature : ScriptableRendererFeature
{
	private static class ShaderParams
	{
		public const string RTBackBufferName = "_VLBackBufferTexture";

		public static int RTBackBuffer = Shader.PropertyToID("_VLBackBufferTexture");

		public const string RTFrontBufferName = "_VLFrontBufferTexture";

		public static int RTFrontBuffer = Shader.PropertyToID("_VLFrontBufferTexture");

		public static int FlaskThickness = Shader.PropertyToID("_FlaskThickness");

		public static int ForcedInvisible = Shader.PropertyToID("_LVForcedInvisible");

		public const string SKW_FP_RENDER_TEXTURE = "LIQUID_VOLUME_FP_RENDER_TEXTURES";
	}

	private enum Pass
	{
		BackBuffer,
		FrontBuffer
	}

	private class DepthPass : ScriptableRenderPass
	{
		private class PassData
		{
			public Camera cam;

			public CommandBuffer cmd;

			public DepthPass depthPass;

			public Material mat;

			public RTHandle source;

			public RTHandle depth;

			public RenderTextureDescriptor cameraTargetDescriptor;
		}

		private const string profilerTag = "LiquidVolumeDepthPrePass";

		private Material mat;

		private int targetNameId;

		private RTHandle targetRT;

		private int passId;

		private List<LiquidVolume> lvRenderers;

		public ScriptableRenderer renderer;

		public bool interleavedRendering;

		private static Vector3 currentCameraPosition;

		private readonly PassData passData = new PassData();

		public DepthPass(Material mat, Pass pass, RenderPassEvent renderPassEvent)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			((ScriptableRenderPass)this).renderPassEvent = renderPassEvent;
			this.mat = mat;
			passData.depthPass = this;
			switch (pass)
			{
			case Pass.BackBuffer:
			{
				targetNameId = ShaderParams.RTBackBuffer;
				RenderTargetIdentifier val2 = default(RenderTargetIdentifier);
				((RenderTargetIdentifier)(ref val2))._002Ector(targetNameId, 0, (CubemapFace)(-1), -1);
				targetRT = RTHandles.Alloc(val2, "_VLBackBufferTexture");
				passId = 0;
				lvRenderers = lvBackRenderers;
				break;
			}
			case Pass.FrontBuffer:
			{
				targetNameId = ShaderParams.RTFrontBuffer;
				RenderTargetIdentifier val = default(RenderTargetIdentifier);
				((RenderTargetIdentifier)(ref val))._002Ector(targetNameId, 0, (CubemapFace)(-1), -1);
				targetRT = RTHandles.Alloc(val, "_VLFrontBufferTexture");
				passId = 1;
				lvRenderers = lvFrontRenderers;
				break;
			}
			}
		}

		public void Setup(LiquidVolumeDepthPrePassRenderFeature feature, ScriptableRenderer renderer)
		{
			this.renderer = renderer;
			interleavedRendering = feature.interleavedRendering;
		}

		private int SortByDistanceToCamera(LiquidVolume lv1, LiquidVolume lv2)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			bool flag = (Object)(object)lv1 == (Object)null;
			bool flag2 = (Object)(object)lv2 == (Object)null;
			if (flag && flag2)
			{
				return 0;
			}
			if (flag2)
			{
				return 1;
			}
			if (flag)
			{
				return -1;
			}
			float num = Vector3.Distance(((Component)lv1).transform.position, currentCameraPosition);
			float num2 = Vector3.Distance(((Component)lv2).transform.position, currentCameraPosition);
			if (num < num2)
			{
				return 1;
			}
			if (num > num2)
			{
				return -1;
			}
			return 0;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			((RenderTextureDescriptor)(ref cameraTextureDescriptor)).colorFormat = (RenderTextureFormat)(LiquidVolume.useFPRenderTextures ? 15 : 0);
			((RenderTextureDescriptor)(ref cameraTextureDescriptor)).sRGB = false;
			((RenderTextureDescriptor)(ref cameraTextureDescriptor)).depthBufferBits = 16;
			((RenderTextureDescriptor)(ref cameraTextureDescriptor)).msaaSamples = 1;
			cmd.GetTemporaryRT(targetNameId, cameraTextureDescriptor);
			if (!interleavedRendering)
			{
				((ScriptableRenderPass)this).ConfigureTarget(targetRT);
			}
			((ScriptableRenderPass)this).ConfigureInput((ScriptableRenderPassInput)1);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			if (lvRenderers != null)
			{
				CommandBuffer val = CommandBufferPool.Get("LiquidVolumeDepthPrePass");
				val.Clear();
				passData.cam = renderingData.cameraData.camera;
				passData.cmd = val;
				passData.mat = mat;
				passData.cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
				passData.source = renderer.cameraColorTargetHandle;
				passData.depth = renderer.cameraDepthTargetHandle;
				ExecutePass(passData);
				((ScriptableRenderContext)(ref context)).ExecuteCommandBuffer(val);
				CommandBufferPool.Release(val);
			}
		}

		private static void ExecutePass(PassData passData)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			CommandBuffer cmd = passData.cmd;
			cmd.SetGlobalFloat(ShaderParams.ForcedInvisible, 0f);
			Camera cam = passData.cam;
			DepthPass depthPass = passData.depthPass;
			RenderTextureDescriptor cameraTargetDescriptor = passData.cameraTargetDescriptor;
			((RenderTextureDescriptor)(ref cameraTargetDescriptor)).colorFormat = (RenderTextureFormat)(LiquidVolume.useFPRenderTextures ? 15 : 0);
			((RenderTextureDescriptor)(ref cameraTargetDescriptor)).sRGB = false;
			((RenderTextureDescriptor)(ref cameraTargetDescriptor)).depthBufferBits = 16;
			((RenderTextureDescriptor)(ref cameraTargetDescriptor)).msaaSamples = 1;
			cmd.GetTemporaryRT(depthPass.targetNameId, cameraTargetDescriptor);
			int count = depthPass.lvRenderers.Count;
			if (depthPass.interleavedRendering)
			{
				RenderTargetIdentifier val = default(RenderTargetIdentifier);
				((RenderTargetIdentifier)(ref val))._002Ector(depthPass.targetNameId, 0, (CubemapFace)(-1), -1);
				currentCameraPosition = ((Component)cam).transform.position;
				depthPass.lvRenderers.Sort(depthPass.SortByDistanceToCamera);
				RenderTargetIdentifier val2 = default(RenderTargetIdentifier);
				RenderTargetIdentifier val3 = default(RenderTargetIdentifier);
				for (int i = 0; i < count; i++)
				{
					LiquidVolume liquidVolume = depthPass.lvRenderers[i];
					if (!((Object)(object)liquidVolume != (Object)null) || !((Behaviour)liquidVolume).isActiveAndEnabled)
					{
						continue;
					}
					if (liquidVolume.topology == TOPOLOGY.Irregular)
					{
						cmd.SetRenderTarget(val, (RenderBufferLoadAction)2, (RenderBufferStoreAction)0);
						if (LiquidVolume.useFPRenderTextures)
						{
							cmd.ClearRenderTarget(true, true, new Color(cam.farClipPlane, 0f, 0f, 0f), 1f);
							cmd.EnableShaderKeyword("LIQUID_VOLUME_FP_RENDER_TEXTURES");
						}
						else
						{
							cmd.ClearRenderTarget(true, true, new Color(84f / 85f, 0.4470558f, 0.75f, 0f), 1f);
							cmd.DisableShaderKeyword("LIQUID_VOLUME_FP_RENDER_TEXTURES");
						}
						cmd.SetGlobalFloat(ShaderParams.FlaskThickness, 1f - liquidVolume.flaskThickness);
						cmd.DrawRenderer(liquidVolume.mr, passData.mat, (liquidVolume.subMeshIndex >= 0) ? liquidVolume.subMeshIndex : 0, depthPass.passId);
					}
					((RenderTargetIdentifier)(ref val2))._002Ector(RTHandle.op_Implicit(passData.source), 0, (CubemapFace)(-1), -1);
					((RenderTargetIdentifier)(ref val3))._002Ector(RTHandle.op_Implicit(passData.depth), 0, (CubemapFace)(-1), -1);
					cmd.SetRenderTarget(val2, val3);
					cmd.DrawRenderer(liquidVolume.mr, liquidVolume.liqMat, (liquidVolume.subMeshIndex >= 0) ? liquidVolume.subMeshIndex : 0, 1);
				}
				cmd.SetGlobalFloat(ShaderParams.ForcedInvisible, 1f);
				return;
			}
			RenderTargetIdentifier val4 = default(RenderTargetIdentifier);
			((RenderTargetIdentifier)(ref val4))._002Ector(depthPass.targetNameId, 0, (CubemapFace)(-1), -1);
			cmd.SetRenderTarget(val4);
			cmd.SetGlobalTexture(depthPass.targetNameId, val4);
			if (LiquidVolume.useFPRenderTextures)
			{
				cmd.ClearRenderTarget(true, true, new Color(cam.farClipPlane, 0f, 0f, 0f), 1f);
				cmd.EnableShaderKeyword("LIQUID_VOLUME_FP_RENDER_TEXTURES");
			}
			else
			{
				cmd.ClearRenderTarget(true, true, new Color(84f / 85f, 0.4470558f, 0.75f, 0f), 1f);
				cmd.DisableShaderKeyword("LIQUID_VOLUME_FP_RENDER_TEXTURES");
			}
			for (int j = 0; j < count; j++)
			{
				LiquidVolume liquidVolume2 = depthPass.lvRenderers[j];
				if ((Object)(object)liquidVolume2 != (Object)null && ((Behaviour)liquidVolume2).isActiveAndEnabled)
				{
					cmd.SetGlobalFloat(ShaderParams.FlaskThickness, 1f - liquidVolume2.flaskThickness);
					cmd.DrawRenderer(liquidVolume2.mr, passData.mat, (liquidVolume2.subMeshIndex >= 0) ? liquidVolume2.subMeshIndex : 0, depthPass.passId);
				}
			}
		}

		public void CleanUp()
		{
			RTHandles.Release(targetRT);
		}
	}

	public static readonly List<LiquidVolume> lvBackRenderers = new List<LiquidVolume>();

	public static readonly List<LiquidVolume> lvFrontRenderers = new List<LiquidVolume>();

	[SerializeField]
	[HideInInspector]
	private Shader shader;

	public static bool installed;

	private Material mat;

	private DepthPass backPass;

	private DepthPass frontPass;

	[Tooltip("Renders each irregular liquid volume completely before rendering the next one.")]
	public bool interleavedRendering;

	public RenderPassEvent renderPassEvent = (RenderPassEvent)450;

	public static void AddLiquidToBackRenderers(LiquidVolume lv)
	{
		if (!((Object)(object)lv == (Object)null) && lv.topology == TOPOLOGY.Irregular && !lvBackRenderers.Contains(lv))
		{
			lvBackRenderers.Add(lv);
		}
	}

	public static void RemoveLiquidFromBackRenderers(LiquidVolume lv)
	{
		if (!((Object)(object)lv == (Object)null) && lvBackRenderers.Contains(lv))
		{
			lvBackRenderers.Remove(lv);
		}
	}

	public static void AddLiquidToFrontRenderers(LiquidVolume lv)
	{
		if (!((Object)(object)lv == (Object)null) && lv.topology == TOPOLOGY.Irregular && !lvFrontRenderers.Contains(lv))
		{
			lvFrontRenderers.Add(lv);
		}
	}

	public static void RemoveLiquidFromFrontRenderers(LiquidVolume lv)
	{
		if (!((Object)(object)lv == (Object)null) && lvFrontRenderers.Contains(lv))
		{
			lvFrontRenderers.Remove(lv);
		}
	}

	private void OnDestroy()
	{
		Shader.SetGlobalFloat(ShaderParams.ForcedInvisible, 0f);
		CoreUtils.Destroy((Object)(object)mat);
		if (backPass != null)
		{
			backPass.CleanUp();
		}
		if (frontPass != null)
		{
			frontPass.CleanUp();
		}
	}

	public override void Create()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		((Object)this).name = "Liquid Volume Depth PrePass";
		shader = Shader.Find("LiquidVolume/DepthPrePass");
		if (!((Object)(object)shader == (Object)null))
		{
			mat = CoreUtils.CreateEngineMaterial(shader);
			backPass = new DepthPass(mat, Pass.BackBuffer, renderPassEvent);
			frontPass = new DepthPass(mat, Pass.FrontBuffer, renderPassEvent);
		}
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		installed = true;
		if (backPass != null && lvBackRenderers.Count > 0)
		{
			backPass.Setup(this, renderer);
			renderer.EnqueuePass((ScriptableRenderPass)(object)backPass);
		}
		if (frontPass != null && lvFrontRenderers.Count > 0)
		{
			frontPass.Setup(this, renderer);
			frontPass.renderer = renderer;
			renderer.EnqueuePass((ScriptableRenderPass)(object)frontPass);
		}
	}
}
