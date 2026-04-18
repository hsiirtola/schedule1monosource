using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing;

public sealed class BuiltinDebugViewsComponent : PostProcessingComponentCommandBuffer<BuiltinDebugViewsModel>
{
	private static class Uniforms
	{
		internal static readonly int _DepthScale = Shader.PropertyToID("_DepthScale");

		internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");

		internal static readonly int _Opacity = Shader.PropertyToID("_Opacity");

		internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");

		internal static readonly int _TempRT2 = Shader.PropertyToID("_TempRT2");

		internal static readonly int _Amplitude = Shader.PropertyToID("_Amplitude");

		internal static readonly int _Scale = Shader.PropertyToID("_Scale");
	}

	private enum Pass
	{
		Depth,
		Normals,
		MovecOpacity,
		MovecImaging,
		MovecArrows
	}

	private class ArrowArray
	{
		public Mesh mesh { get; private set; }

		public int columnCount { get; private set; }

		public int rowCount { get; private set; }

		public void BuildMesh(int columns, int rows)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Expected O, but got Unknown
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			Vector3[] array = (Vector3[])(object)new Vector3[6]
			{
				new Vector3(0f, 0f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(-1f, 1f, 0f),
				new Vector3(0f, 1f, 0f),
				new Vector3(1f, 1f, 0f)
			};
			int num = 6 * columns * rows;
			List<Vector3> list = new List<Vector3>(num);
			List<Vector2> list2 = new List<Vector2>(num);
			Vector2 item = default(Vector2);
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < columns; j++)
				{
					((Vector2)(ref item))._002Ector((0.5f + (float)j) / (float)columns, (0.5f + (float)i) / (float)rows);
					for (int k = 0; k < 6; k++)
					{
						list.Add(array[k]);
						list2.Add(item);
					}
				}
			}
			int[] array2 = new int[num];
			for (int l = 0; l < num; l++)
			{
				array2[l] = l;
			}
			mesh = new Mesh
			{
				hideFlags = (HideFlags)52
			};
			mesh.SetVertices(list);
			mesh.SetUVs(0, list2);
			mesh.SetIndices(array2, (MeshTopology)3, 0);
			mesh.UploadMeshData(true);
			columnCount = columns;
			rowCount = rows;
		}

		public void Release()
		{
			GraphicsUtils.Destroy((Object)(object)mesh);
			mesh = null;
		}
	}

	private const string k_ShaderString = "Hidden/Post FX/Builtin Debug Views";

	private ArrowArray m_Arrows;

	public override bool active
	{
		get
		{
			if (!base.model.IsModeActive(BuiltinDebugViewsModel.Mode.Depth) && !base.model.IsModeActive(BuiltinDebugViewsModel.Mode.Normals))
			{
				return base.model.IsModeActive(BuiltinDebugViewsModel.Mode.MotionVectors);
			}
			return true;
		}
	}

	public override DepthTextureMode GetCameraFlags()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		BuiltinDebugViewsModel.Mode mode = base.model.settings.mode;
		DepthTextureMode val = (DepthTextureMode)0;
		switch (mode)
		{
		case BuiltinDebugViewsModel.Mode.Normals:
			val = (DepthTextureMode)(val | 2);
			break;
		case BuiltinDebugViewsModel.Mode.MotionVectors:
			val = (DepthTextureMode)(val | 5);
			break;
		case BuiltinDebugViewsModel.Mode.Depth:
			val = (DepthTextureMode)(val | 1);
			break;
		}
		return val;
	}

	public override CameraEvent GetCameraEvent()
	{
		if (base.model.settings.mode == BuiltinDebugViewsModel.Mode.MotionVectors)
		{
			return (CameraEvent)18;
		}
		return (CameraEvent)12;
	}

	public override string GetName()
	{
		return "Builtin Debug Views";
	}

	public override void PopulateCommandBuffer(CommandBuffer cb)
	{
		BuiltinDebugViewsModel.Settings settings = base.model.settings;
		Material val = context.materialFactory.Get("Hidden/Post FX/Builtin Debug Views");
		val.shaderKeywords = null;
		if (context.isGBufferAvailable)
		{
			val.EnableKeyword("SOURCE_GBUFFER");
		}
		switch (settings.mode)
		{
		case BuiltinDebugViewsModel.Mode.Depth:
			DepthPass(cb);
			break;
		case BuiltinDebugViewsModel.Mode.Normals:
			DepthNormalsPass(cb);
			break;
		case BuiltinDebugViewsModel.Mode.MotionVectors:
			MotionVectorsPass(cb);
			break;
		}
		context.Interrupt();
	}

	private void DepthPass(CommandBuffer cb)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Material val = context.materialFactory.Get("Hidden/Post FX/Builtin Debug Views");
		BuiltinDebugViewsModel.DepthSettings depth = base.model.settings.depth;
		cb.SetGlobalFloat(Uniforms._DepthScale, 1f / depth.scale);
		cb.Blit((Texture)null, RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), val, 0);
	}

	private void DepthNormalsPass(CommandBuffer cb)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Material val = context.materialFactory.Get("Hidden/Post FX/Builtin Debug Views");
		cb.Blit((Texture)null, RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), val, 1);
	}

	private void MotionVectorsPass(CommandBuffer cb)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		Material val = context.materialFactory.Get("Hidden/Post FX/Builtin Debug Views");
		BuiltinDebugViewsModel.MotionVectorsSettings motionVectors = base.model.settings.motionVectors;
		int num = Uniforms._TempRT;
		cb.GetTemporaryRT(num, context.width, context.height, 0, (FilterMode)1);
		cb.SetGlobalFloat(Uniforms._Opacity, motionVectors.sourceOpacity);
		cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2));
		cb.Blit(RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2), RenderTargetIdentifier.op_Implicit(num), val, 2);
		if (motionVectors.motionImageOpacity > 0f && motionVectors.motionImageAmplitude > 0f)
		{
			int tempRT = Uniforms._TempRT2;
			cb.GetTemporaryRT(tempRT, context.width, context.height, 0, (FilterMode)1);
			cb.SetGlobalFloat(Uniforms._Opacity, motionVectors.motionImageOpacity);
			cb.SetGlobalFloat(Uniforms._Amplitude, motionVectors.motionImageAmplitude);
			cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(num));
			cb.Blit(RenderTargetIdentifier.op_Implicit(num), RenderTargetIdentifier.op_Implicit(tempRT), val, 3);
			cb.ReleaseTemporaryRT(num);
			num = tempRT;
		}
		if (motionVectors.motionVectorsOpacity > 0f && motionVectors.motionVectorsAmplitude > 0f)
		{
			PrepareArrows();
			float num2 = 1f / (float)motionVectors.motionVectorsResolution;
			float num3 = num2 * (float)context.height / (float)context.width;
			cb.SetGlobalVector(Uniforms._Scale, Vector4.op_Implicit(new Vector2(num3, num2)));
			cb.SetGlobalFloat(Uniforms._Opacity, motionVectors.motionVectorsOpacity);
			cb.SetGlobalFloat(Uniforms._Amplitude, motionVectors.motionVectorsAmplitude);
			cb.DrawMesh(m_Arrows.mesh, Matrix4x4.identity, val, 0, 4);
		}
		cb.SetGlobalTexture(Uniforms._MainTex, RenderTargetIdentifier.op_Implicit(num));
		cb.Blit(RenderTargetIdentifier.op_Implicit(num), RenderTargetIdentifier.op_Implicit((BuiltinRenderTextureType)2));
		cb.ReleaseTemporaryRT(num);
	}

	private void PrepareArrows()
	{
		int motionVectorsResolution = base.model.settings.motionVectors.motionVectorsResolution;
		int num = motionVectorsResolution * Screen.width / Screen.height;
		if (m_Arrows == null)
		{
			m_Arrows = new ArrowArray();
		}
		if (m_Arrows.columnCount != num || m_Arrows.rowCount != motionVectorsResolution)
		{
			m_Arrows.Release();
			m_Arrows.BuildMesh(num, motionVectorsResolution);
		}
	}

	public override void OnDisable()
	{
		if (m_Arrows != null)
		{
			m_Arrows.Release();
		}
		m_Arrows = null;
	}
}
