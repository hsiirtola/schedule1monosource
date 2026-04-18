using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VFXRainPass : ScriptableRenderPass
{
	private RTHandle _cameraColorTarget;

	private RTHandle _cameraDepthTarget;

	private LayerMask _layerMask;

	private FilteringSettings _filteringSettings;

	private static readonly ShaderTagId[] _shaderTagIds = (ShaderTagId[])(object)new ShaderTagId[3]
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("UniversalForward"),
		new ShaderTagId("UniversalForwardOnly")
	};

	public void Setup(VFXRainFeature.Settings settings, RTHandle cameraColorTarget, RTHandle cameraDepthTarget)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		_layerMask = settings.LayerMask;
		_cameraColorTarget = cameraColorTarget;
		_cameraDepthTarget = cameraDepthTarget;
		_filteringSettings = new FilteringSettings((RenderQueueRange?)RenderQueueRange.transparent, LayerMask.op_Implicit(_layerMask), uint.MaxValue, 0);
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		((ScriptableRenderPass)this).ConfigureTarget(_cameraColorTarget, _cameraDepthTarget);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Camera camera = renderingData.cameraData.camera;
		if ((int)camera.cameraType == 1 || (int)camera.cameraType == 2)
		{
			CommandBuffer val = CommandBufferPool.Get("VFXRainPass");
			DrawingSettings val2 = ((ScriptableRenderPass)this).CreateDrawingSettings(_shaderTagIds[0], ref renderingData, (SortingCriteria)23);
			for (int i = 1; i < _shaderTagIds.Length; i++)
			{
				((DrawingSettings)(ref val2)).SetShaderPassName(i, _shaderTagIds[i]);
			}
			((ScriptableRenderContext)(ref context)).ExecuteCommandBuffer(val);
			val.Clear();
			((ScriptableRenderContext)(ref context)).DrawRenderers(renderingData.cullResults, ref val2, ref _filteringSettings);
			CommandBufferPool.Release(val);
		}
	}

	public void Dispose()
	{
	}
}
