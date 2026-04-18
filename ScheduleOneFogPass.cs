using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScheduleOneFogPass : ScriptableRenderPass
{
	private Material _material;

	private RTHandle _cameraColorTarget;

	private RTHandle _tempTexture;

	private Color _color;

	private float _start;

	private float _end;

	private float _density;

	private float _blurStrength;

	private float _startHeightFade;

	private float _endHeightFade;

	public ScheduleOneFogPass(Material material)
	{
		_material = material;
	}

	public void Setup(ScheduleOneFogFeature.Settings settings, RTHandle cameraColorTarget)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		_color = settings.Color;
		_start = settings.Start;
		_end = settings.End;
		_density = settings.Density;
		_blurStrength = settings.BlurStrength;
		_startHeightFade = settings.StartHeightFade;
		_endHeightFade = settings.EndHeightFade;
		_cameraColorTarget = cameraColorTarget;
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
		((RenderTextureDescriptor)(ref cameraTargetDescriptor)).depthBufferBits = 0;
		RenderingUtils.ReAllocateIfNeeded(ref _tempTexture, ref cameraTargetDescriptor, (FilterMode)1, (TextureWrapMode)1, false, 1, 0f, "_TempFogTexture");
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		Camera camera = renderingData.cameraData.camera;
		if ((int)camera.cameraType != 1 && (int)camera.cameraType != 2)
		{
			return;
		}
		if ((Object)(object)_material == (Object)null)
		{
			Debug.LogError((object)"Fog material is null!");
		}
		else if (_cameraColorTarget != null && _tempTexture != null)
		{
			CommandBuffer val = CommandBufferPool.Get("ScheduleOneFog");
			if (!Application.isPlaying)
			{
				_material.SetColor("_FogColor", _color);
				_material.SetFloat("_FogStart", _start);
				_material.SetFloat("_FogEnd", _end);
				_material.SetFloat("_FogDensity", _density);
				_material.SetFloat("_BlurStrength", _blurStrength);
				_material.SetFloat("_StartFogHeightFade", _startHeightFade);
				_material.SetFloat("_EndFogHeightFade", _endHeightFade);
			}
			else
			{
				_material.SetFloat("_FogStart", _start);
				_material.SetFloat("_FogEnd", _end);
				_material.SetFloat("_BlurStrength", _blurStrength);
			}
			Blitter.BlitCameraTexture(val, _cameraColorTarget, _tempTexture, _material, 0);
			Blitter.BlitCameraTexture(val, _tempTexture, _cameraColorTarget, 0f, false);
			((ScriptableRenderContext)(ref context)).ExecuteCommandBuffer(val);
			CommandBufferPool.Release(val);
		}
	}

	public void Dispose()
	{
		_tempTexture = null;
	}
}
