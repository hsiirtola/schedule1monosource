using System;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing;

public sealed class MotionBlurComponent : PostProcessingComponentCommandBuffer<MotionBlurModel>
{
	private static class Uniforms
	{
		internal static readonly int _VelocityScale = Shader.PropertyToID("_VelocityScale");

		internal static readonly int _MaxBlurRadius = Shader.PropertyToID("_MaxBlurRadius");

		internal static readonly int _RcpMaxBlurRadius = Shader.PropertyToID("_RcpMaxBlurRadius");

		internal static readonly int _VelocityTex = Shader.PropertyToID("_VelocityTex");

		internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int _Tile2RT = Shader.PropertyToID("_Tile2RT");

		internal static readonly int _Tile4RT = Shader.PropertyToID("_Tile4RT");

		internal static readonly int _Tile8RT = Shader.PropertyToID("_Tile8RT");

		internal static readonly int _TileMaxOffs = Shader.PropertyToID("_TileMaxOffs");

		internal static readonly int _TileMaxLoop = Shader.PropertyToID("_TileMaxLoop");

		internal static readonly int _TileVRT = Shader.PropertyToID("_TileVRT");

		internal static readonly int _NeighborMaxTex = Shader.PropertyToID("_NeighborMaxTex");

		internal static readonly int _LoopCount = Shader.PropertyToID("_LoopCount");

		internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");

		internal static readonly int _History1LumaTex = Shader.PropertyToID("_History1LumaTex");

		internal static readonly int _History2LumaTex = Shader.PropertyToID("_History2LumaTex");

		internal static readonly int _History3LumaTex = Shader.PropertyToID("_History3LumaTex");

		internal static readonly int _History4LumaTex = Shader.PropertyToID("_History4LumaTex");

		internal static readonly int _History1ChromaTex = Shader.PropertyToID("_History1ChromaTex");

		internal static readonly int _History2ChromaTex = Shader.PropertyToID("_History2ChromaTex");

		internal static readonly int _History3ChromaTex = Shader.PropertyToID("_History3ChromaTex");

		internal static readonly int _History4ChromaTex = Shader.PropertyToID("_History4ChromaTex");

		internal static readonly int _History1Weight = Shader.PropertyToID("_History1Weight");

		internal static readonly int _History2Weight = Shader.PropertyToID("_History2Weight");

		internal static readonly int _History3Weight = Shader.PropertyToID("_History3Weight");

		internal static readonly int _History4Weight = Shader.PropertyToID("_History4Weight");
	}

	private enum Pass
	{
		VelocitySetup,
		TileMax1,
		TileMax2,
		TileMaxV,
		NeighborMax,
		Reconstruction,
		FrameCompression,
		FrameBlendingChroma,
		FrameBlendingRaw
	}

	public class ReconstructionFilter
	{
		private RenderTextureFormat m_VectorRTFormat = (RenderTextureFormat)13;

		private RenderTextureFormat m_PackedRTFormat = (RenderTextureFormat)8;

		public ReconstructionFilter()
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			CheckTextureFormatSupport();
		}

		private void CheckTextureFormatSupport()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (!SystemInfo.SupportsRenderTextureFormat(m_PackedRTFormat))
			{
				m_PackedRTFormat = (RenderTextureFormat)0;
			}
		}

		public bool IsSupported()
		{
			return SystemInfo.supportsMotionVectors;
		}

		public void ProcessImage(PostProcessingContext context, CommandBuffer cb, ref MotionBlurModel.Settings settings, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_0267: Unknown result type (might be due to invalid IL or missing references)
			//IL_0274: Unknown result type (might be due to invalid IL or missing references)
			//IL_027b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			int num = (int)(5f * (float)context.height / 100f);
			int num2 = ((num - 1) / 8 + 1) * 8;
			float num3 = settings.shutterAngle / 360f;
			cb.SetGlobalFloat(Uniforms._VelocityScale, num3);
			cb.SetGlobalFloat(Uniforms._MaxBlurRadius, (float)num);
			cb.SetGlobalFloat(Uniforms._RcpMaxBlurRadius, 1f / (float)num);
			int velocityTex = Uniforms._VelocityTex;
			cb.GetTemporaryRT(velocityTex, context.width, context.height, 0, (FilterMode)0, m_PackedRTFormat, (RenderTextureReadWrite)1);
			cb.Blit((Texture)null, RenderTargetIdentifier.op_Implicit(velocityTex), material, 0);
			int tile2RT = Uniforms._Tile2RT;
			cb.GetTemporaryRT(tile2RT, context.width / 2, context.height / 2, 0, (FilterMode)0, m_VectorRTFormat, (RenderTextureReadWrite)1);
			cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(velocityTex));
			cb.Blit(RenderTargetIdentifier.op_Implicit(velocityTex), RenderTargetIdentifier.op_Implicit(tile2RT), material, 1);
			int tile4RT = Uniforms._Tile4RT;
			cb.GetTemporaryRT(tile4RT, context.width / 4, context.height / 4, 0, (FilterMode)0, m_VectorRTFormat, (RenderTextureReadWrite)1);
			cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(tile2RT));
			cb.Blit(RenderTargetIdentifier.op_Implicit(tile2RT), RenderTargetIdentifier.op_Implicit(tile4RT), material, 2);
			cb.ReleaseTemporaryRT(tile2RT);
			int tile8RT = Uniforms._Tile8RT;
			cb.GetTemporaryRT(tile8RT, context.width / 8, context.height / 8, 0, (FilterMode)0, m_VectorRTFormat, (RenderTextureReadWrite)1);
			cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(tile4RT));
			cb.Blit(RenderTargetIdentifier.op_Implicit(tile4RT), RenderTargetIdentifier.op_Implicit(tile8RT), material, 2);
			cb.ReleaseTemporaryRT(tile4RT);
			Vector2 val = Vector2.one * ((float)num2 / 8f - 1f) * -0.5f;
			cb.SetGlobalVector(Uniforms._TileMaxOffs, Vector4.op_Implicit(val));
			cb.SetGlobalFloat(Uniforms._TileMaxLoop, (float)(int)((float)num2 / 8f));
			int tileVRT = Uniforms._TileVRT;
			cb.GetTemporaryRT(tileVRT, context.width / num2, context.height / num2, 0, (FilterMode)0, m_VectorRTFormat, (RenderTextureReadWrite)1);
			cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(tile8RT));
			cb.Blit(RenderTargetIdentifier.op_Implicit(tile8RT), RenderTargetIdentifier.op_Implicit(tileVRT), material, 3);
			cb.ReleaseTemporaryRT(tile8RT);
			int neighborMaxTex = Uniforms._NeighborMaxTex;
			int num4 = context.width / num2;
			int num5 = context.height / num2;
			cb.GetTemporaryRT(neighborMaxTex, num4, num5, 0, (FilterMode)0, m_VectorRTFormat, (RenderTextureReadWrite)1);
			cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(tileVRT));
			cb.Blit(RenderTargetIdentifier.op_Implicit(tileVRT), RenderTargetIdentifier.op_Implicit(neighborMaxTex), material, 4);
			cb.ReleaseTemporaryRT(tileVRT);
			cb.SetGlobalFloat(Uniforms._LoopCount, (float)Mathf.Clamp(settings.sampleCount / 2, 1, 64));
			cb.SetGlobalTexture(Uniforms._MainTex, source);
			cb.Blit(source, destination, material, 5);
			cb.ReleaseTemporaryRT(velocityTex);
			cb.ReleaseTemporaryRT(neighborMaxTex);
		}
	}

	public class FrameBlendingFilter
	{
		private struct Frame
		{
			public RenderTexture lumaTexture;

			public RenderTexture chromaTexture;

			private float m_Time;

			private RenderTargetIdentifier[] m_MRT;

			public float CalculateWeight(float strength, float currentTime)
			{
				if (Mathf.Approximately(m_Time, 0f))
				{
					return 0f;
				}
				float num = Mathf.Lerp(80f, 16f, strength);
				return Mathf.Exp((m_Time - currentTime) * num);
			}

			public void Release()
			{
				if ((Object)(object)lumaTexture != (Object)null)
				{
					RenderTexture.ReleaseTemporary(lumaTexture);
				}
				if ((Object)(object)chromaTexture != (Object)null)
				{
					RenderTexture.ReleaseTemporary(chromaTexture);
				}
				lumaTexture = null;
				chromaTexture = null;
			}

			public void MakeRecord(CommandBuffer cb, RenderTargetIdentifier source, int width, int height, Material material)
			{
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				//IL_007a: Unknown result type (might be due to invalid IL or missing references)
				//IL_007f: Unknown result type (might be due to invalid IL or missing references)
				//IL_008a: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
				Release();
				lumaTexture = RenderTexture.GetTemporary(width, height, 0, (RenderTextureFormat)16, (RenderTextureReadWrite)1);
				chromaTexture = RenderTexture.GetTemporary(width, height, 0, (RenderTextureFormat)16, (RenderTextureReadWrite)1);
				((Texture)lumaTexture).filterMode = (FilterMode)0;
				((Texture)chromaTexture).filterMode = (FilterMode)0;
				if (m_MRT == null)
				{
					m_MRT = (RenderTargetIdentifier[])(object)new RenderTargetIdentifier[2];
				}
				m_MRT[0] = RenderTargetIdentifier.op_Implicit((Texture)(object)lumaTexture);
				m_MRT[1] = RenderTargetIdentifier.op_Implicit((Texture)(object)chromaTexture);
				cb.SetGlobalTexture(Uniforms._MainTex, source);
				cb.SetRenderTarget(m_MRT, RenderTargetIdentifier.op_Implicit((Texture)(object)lumaTexture));
				cb.DrawMesh(GraphicsUtils.quad, Matrix4x4.identity, material, 0, 6);
				m_Time = Time.time;
			}

			public void MakeRecordRaw(CommandBuffer cb, RenderTargetIdentifier source, int width, int height, RenderTextureFormat format)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				Release();
				lumaTexture = RenderTexture.GetTemporary(width, height, 0, format);
				((Texture)lumaTexture).filterMode = (FilterMode)0;
				cb.SetGlobalTexture(Uniforms._MainTex, source);
				cb.Blit(source, RenderTargetIdentifier.op_Implicit((Texture)(object)lumaTexture));
				m_Time = Time.time;
			}
		}

		private bool m_UseCompression;

		private RenderTextureFormat m_RawTextureFormat;

		private Frame[] m_FrameList;

		private int m_LastFrameCount;

		public FrameBlendingFilter()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			m_UseCompression = CheckSupportCompression();
			m_RawTextureFormat = GetPreferredRenderTextureFormat();
			m_FrameList = new Frame[4];
		}

		public void Dispose()
		{
			Frame[] frameList = m_FrameList;
			foreach (Frame frame in frameList)
			{
				frame.Release();
			}
		}

		public void PushFrame(CommandBuffer cb, RenderTargetIdentifier source, int width, int height, Material material)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			int frameCount = Time.frameCount;
			if (frameCount != m_LastFrameCount)
			{
				int num = frameCount % m_FrameList.Length;
				if (m_UseCompression)
				{
					m_FrameList[num].MakeRecord(cb, source, width, height, material);
				}
				else
				{
					m_FrameList[num].MakeRecordRaw(cb, source, width, height, m_RawTextureFormat);
				}
				m_LastFrameCount = frameCount;
			}
		}

		public void BlendFrames(CommandBuffer cb, float strength, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			float time = Time.time;
			Frame frameRelative = GetFrameRelative(-1);
			Frame frameRelative2 = GetFrameRelative(-2);
			Frame frameRelative3 = GetFrameRelative(-3);
			Frame frameRelative4 = GetFrameRelative(-4);
			cb.SetGlobalTexture(Uniforms._History1LumaTex, RenderTargetIdentifier.op_Implicit((Texture)(object)frameRelative.lumaTexture));
			cb.SetGlobalTexture(Uniforms._History2LumaTex, RenderTargetIdentifier.op_Implicit((Texture)(object)frameRelative2.lumaTexture));
			cb.SetGlobalTexture(Uniforms._History3LumaTex, RenderTargetIdentifier.op_Implicit((Texture)(object)frameRelative3.lumaTexture));
			cb.SetGlobalTexture(Uniforms._History4LumaTex, RenderTargetIdentifier.op_Implicit((Texture)(object)frameRelative4.lumaTexture));
			cb.SetGlobalTexture(Uniforms._History1ChromaTex, RenderTargetIdentifier.op_Implicit((Texture)(object)frameRelative.chromaTexture));
			cb.SetGlobalTexture(Uniforms._History2ChromaTex, RenderTargetIdentifier.op_Implicit((Texture)(object)frameRelative2.chromaTexture));
			cb.SetGlobalTexture(Uniforms._History3ChromaTex, RenderTargetIdentifier.op_Implicit((Texture)(object)frameRelative3.chromaTexture));
			cb.SetGlobalTexture(Uniforms._History4ChromaTex, RenderTargetIdentifier.op_Implicit((Texture)(object)frameRelative4.chromaTexture));
			cb.SetGlobalFloat(Uniforms._History1Weight, frameRelative.CalculateWeight(strength, time));
			cb.SetGlobalFloat(Uniforms._History2Weight, frameRelative2.CalculateWeight(strength, time));
			cb.SetGlobalFloat(Uniforms._History3Weight, frameRelative3.CalculateWeight(strength, time));
			cb.SetGlobalFloat(Uniforms._History4Weight, frameRelative4.CalculateWeight(strength, time));
			cb.SetGlobalTexture(Uniforms._MainTex, source);
			cb.Blit(source, destination, material, m_UseCompression ? 7 : 8);
		}

		private static bool CheckSupportCompression()
		{
			if (SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)16))
			{
				return SystemInfo.supportedRenderTargetCount > 1;
			}
			return false;
		}

		private static RenderTextureFormat GetPreferredRenderTextureFormat()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			RenderTextureFormat[] array = new RenderTextureFormat[3];
			RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
			RenderTextureFormat[] array2 = (RenderTextureFormat[])(object)array;
			foreach (RenderTextureFormat val in array2)
			{
				if (SystemInfo.SupportsRenderTextureFormat(val))
				{
					return val;
				}
			}
			return (RenderTextureFormat)7;
		}

		private Frame GetFrameRelative(int offset)
		{
			int num = (Time.frameCount + m_FrameList.Length + offset) % m_FrameList.Length;
			return m_FrameList[num];
		}
	}

	private ReconstructionFilter m_ReconstructionFilter;

	private FrameBlendingFilter m_FrameBlendingFilter;

	private bool m_FirstFrame = true;

	public ReconstructionFilter reconstructionFilter
	{
		get
		{
			if (m_ReconstructionFilter == null)
			{
				m_ReconstructionFilter = new ReconstructionFilter();
			}
			return m_ReconstructionFilter;
		}
	}

	public FrameBlendingFilter frameBlendingFilter
	{
		get
		{
			if (m_FrameBlendingFilter == null)
			{
				m_FrameBlendingFilter = new FrameBlendingFilter();
			}
			return m_FrameBlendingFilter;
		}
	}

	public override bool active
	{
		get
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Invalid comparison between Unknown and I4
			MotionBlurModel.Settings settings = base.model.settings;
			if (base.model.enabled && ((settings.shutterAngle > 0f && reconstructionFilter.IsSupported()) || settings.frameBlending > 0f) && (int)SystemInfo.graphicsDeviceType != 8)
			{
				return !context.interrupted;
			}
			return false;
		}
	}

	public override string GetName()
	{
		return "Motion Blur";
	}

	public void ResetHistory()
	{
		if (m_FrameBlendingFilter != null)
		{
			m_FrameBlendingFilter.Dispose();
		}
		m_FrameBlendingFilter = null;
	}

	public override DepthTextureMode GetCameraFlags()
	{
		return (DepthTextureMode)5;
	}

	public override CameraEvent GetCameraEvent()
	{
		return (CameraEvent)18;
	}

	public override void OnEnable()
	{
		m_FirstFrame = true;
	}

	public override void PopulateCommandBuffer(CommandBuffer cb)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		if (m_FirstFrame)
		{
			m_FirstFrame = false;
			return;
		}
		Material material = context.materialFactory.Get("Hidden/Post FX/Motion Blur");
		Material val = context.materialFactory.Get("Hidden/Post FX/Blit");
		MotionBlurModel.Settings settings = base.model.settings;
		RenderTextureFormat val2 = (RenderTextureFormat)(context.isHdr ? 9 : 7);
		int tempRT = Uniforms._TempRT;
		cb.GetTemporaryRT(tempRT, context.width, context.height, 0, (FilterMode)0, val2);
		if (settings.shutterAngle > 0f && settings.frameBlending > 0f)
		{
			reconstructionFilter.ProcessImage(context, cb, ref settings, RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), RenderTargetIdentifier.op_Implicit(tempRT), material);
			frameBlendingFilter.BlendFrames(cb, settings.frameBlending, RenderTargetIdentifier.op_Implicit(tempRT), RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), material);
			frameBlendingFilter.PushFrame(cb, RenderTargetIdentifier.op_Implicit(tempRT), context.width, context.height, material);
		}
		else if (settings.shutterAngle > 0f)
		{
			cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2));
			cb.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), RenderTargetIdentifier.op_Implicit(tempRT), val, 0);
			reconstructionFilter.ProcessImage(context, cb, ref settings, RenderTargetIdentifier.op_Implicit(tempRT), RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), material);
		}
		else if (settings.frameBlending > 0f)
		{
			cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2));
			cb.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), RenderTargetIdentifier.op_Implicit(tempRT), val, 0);
			frameBlendingFilter.BlendFrames(cb, settings.frameBlending, RenderTargetIdentifier.op_Implicit(tempRT), RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), material);
			frameBlendingFilter.PushFrame(cb, RenderTargetIdentifier.op_Implicit(tempRT), context.width, context.height, material);
		}
		cb.ReleaseTemporaryRT(tempRT);
	}

	public override void OnDisable()
	{
		if (m_FrameBlendingFilter != null)
		{
			m_FrameBlendingFilter.Dispose();
		}
	}
}
