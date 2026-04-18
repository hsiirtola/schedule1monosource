using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace UnityEngine.PostProcessing;

[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
[AddComponentMenu("Effects/Post-Processing Behaviour", -1)]
public class PostProcessingBehaviour : MonoBehaviour
{
	public PostProcessingProfile profile;

	public Func<Vector2, Matrix4x4> jitteredMatrixFunc;

	private Dictionary<Type, KeyValuePair<CameraEvent, CommandBuffer>> m_CommandBuffers;

	private List<PostProcessingComponentBase> m_Components;

	private Dictionary<PostProcessingComponentBase, bool> m_ComponentStates;

	private MaterialFactory m_MaterialFactory;

	private RenderTextureFactory m_RenderTextureFactory;

	private PostProcessingContext m_Context;

	private Camera m_Camera;

	private PostProcessingProfile m_PreviousProfile;

	private bool m_RenderingInSceneView;

	private BuiltinDebugViewsComponent m_DebugViews;

	private AmbientOcclusionComponent m_AmbientOcclusion;

	private ScreenSpaceReflectionComponent m_ScreenSpaceReflection;

	private FogComponent m_FogComponent;

	private MotionBlurComponent m_MotionBlur;

	private TaaComponent m_Taa;

	private EyeAdaptationComponent m_EyeAdaptation;

	private DepthOfFieldComponent m_DepthOfField;

	private BloomComponent m_Bloom;

	private ChromaticAberrationComponent m_ChromaticAberration;

	private ColorGradingComponent m_ColorGrading;

	private UserLutComponent m_UserLut;

	private GrainComponent m_Grain;

	private VignetteComponent m_Vignette;

	private DitheringComponent m_Dithering;

	private FxaaComponent m_Fxaa;

	private List<PostProcessingComponentBase> m_ComponentsToEnable = new List<PostProcessingComponentBase>();

	private List<PostProcessingComponentBase> m_ComponentsToDisable = new List<PostProcessingComponentBase>();

	private void OnEnable()
	{
		m_CommandBuffers = new Dictionary<Type, KeyValuePair<CameraEvent, CommandBuffer>>();
		m_MaterialFactory = new MaterialFactory();
		m_RenderTextureFactory = new RenderTextureFactory();
		m_Context = new PostProcessingContext();
		m_Components = new List<PostProcessingComponentBase>();
		m_DebugViews = AddComponent(new BuiltinDebugViewsComponent());
		m_AmbientOcclusion = AddComponent(new AmbientOcclusionComponent());
		m_ScreenSpaceReflection = AddComponent(new ScreenSpaceReflectionComponent());
		m_FogComponent = AddComponent(new FogComponent());
		m_MotionBlur = AddComponent(new MotionBlurComponent());
		m_Taa = AddComponent(new TaaComponent());
		m_EyeAdaptation = AddComponent(new EyeAdaptationComponent());
		m_DepthOfField = AddComponent(new DepthOfFieldComponent());
		m_Bloom = AddComponent(new BloomComponent());
		m_ChromaticAberration = AddComponent(new ChromaticAberrationComponent());
		m_ColorGrading = AddComponent(new ColorGradingComponent());
		m_UserLut = AddComponent(new UserLutComponent());
		m_Grain = AddComponent(new GrainComponent());
		m_Vignette = AddComponent(new VignetteComponent());
		m_Dithering = AddComponent(new DitheringComponent());
		m_Fxaa = AddComponent(new FxaaComponent());
		m_ComponentStates = new Dictionary<PostProcessingComponentBase, bool>();
		foreach (PostProcessingComponentBase component in m_Components)
		{
			m_ComponentStates.Add(component, value: false);
		}
		((MonoBehaviour)this).useGUILayout = false;
	}

	private void OnPreCull()
	{
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		m_Camera = ((Component)this).GetComponent<Camera>();
		if ((Object)(object)profile == (Object)null || (Object)(object)m_Camera == (Object)null)
		{
			return;
		}
		PostProcessingContext postProcessingContext = m_Context.Reset();
		postProcessingContext.profile = profile;
		postProcessingContext.renderTextureFactory = m_RenderTextureFactory;
		postProcessingContext.materialFactory = m_MaterialFactory;
		postProcessingContext.camera = m_Camera;
		m_DebugViews.Init(postProcessingContext, profile.debugViews);
		m_AmbientOcclusion.Init(postProcessingContext, profile.ambientOcclusion);
		m_ScreenSpaceReflection.Init(postProcessingContext, profile.screenSpaceReflection);
		m_FogComponent.Init(postProcessingContext, profile.fog);
		m_MotionBlur.Init(postProcessingContext, profile.motionBlur);
		m_Taa.Init(postProcessingContext, profile.antialiasing);
		m_EyeAdaptation.Init(postProcessingContext, profile.eyeAdaptation);
		m_DepthOfField.Init(postProcessingContext, profile.depthOfField);
		m_Bloom.Init(postProcessingContext, profile.bloom);
		m_ChromaticAberration.Init(postProcessingContext, profile.chromaticAberration);
		m_ColorGrading.Init(postProcessingContext, profile.colorGrading);
		m_UserLut.Init(postProcessingContext, profile.userLut);
		m_Grain.Init(postProcessingContext, profile.grain);
		m_Vignette.Init(postProcessingContext, profile.vignette);
		m_Dithering.Init(postProcessingContext, profile.dithering);
		m_Fxaa.Init(postProcessingContext, profile.antialiasing);
		if ((Object)(object)m_PreviousProfile != (Object)(object)profile)
		{
			DisableComponents();
			m_PreviousProfile = profile;
		}
		CheckObservers();
		DepthTextureMode val = postProcessingContext.camera.depthTextureMode;
		foreach (PostProcessingComponentBase component in m_Components)
		{
			if (component.active)
			{
				val |= component.GetCameraFlags();
			}
		}
		postProcessingContext.camera.depthTextureMode = val;
		if (!m_RenderingInSceneView && m_Taa.active && !profile.debugViews.willInterrupt)
		{
			m_Taa.SetProjectionMatrix(jitteredMatrixFunc);
		}
	}

	private void OnPreRender()
	{
		if (!((Object)(object)profile == (Object)null))
		{
			TryExecuteCommandBuffer(m_DebugViews);
			TryExecuteCommandBuffer(m_AmbientOcclusion);
			TryExecuteCommandBuffer(m_ScreenSpaceReflection);
			TryExecuteCommandBuffer(m_FogComponent);
			if (!m_RenderingInSceneView)
			{
				TryExecuteCommandBuffer(m_MotionBlur);
			}
		}
	}

	private void OnPostRender()
	{
		if (!((Object)(object)profile == (Object)null) && !((Object)(object)m_Camera == (Object)null) && !m_RenderingInSceneView && m_Taa.active && !profile.debugViews.willInterrupt)
		{
			m_Context.camera.ResetProjectionMatrix();
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)profile == (Object)null || (Object)(object)m_Camera == (Object)null)
		{
			Graphics.Blit((Texture)(object)source, destination);
			return;
		}
		bool flag = false;
		bool active = m_Fxaa.active;
		bool flag2 = m_Taa.active && !m_RenderingInSceneView;
		bool num = m_DepthOfField.active && !m_RenderingInSceneView;
		Material val = m_MaterialFactory.Get("Hidden/Post FX/Uber Shader");
		val.shaderKeywords = null;
		RenderTexture val2 = source;
		if (flag2)
		{
			RenderTexture val3 = m_RenderTextureFactory.Get(val2);
			m_Taa.Render(val2, val3);
			val2 = val3;
		}
		Texture val4 = (Texture)(object)GraphicsUtils.whiteTexture;
		if (m_EyeAdaptation.active)
		{
			flag = true;
			val4 = m_EyeAdaptation.Prepare(val2, val);
		}
		val.SetTexture("_AutoExposure", val4);
		if (num)
		{
			flag = true;
			m_DepthOfField.Prepare(val2, val, flag2, m_Taa.jitterVector, m_Taa.model.settings.taaSettings.motionBlending);
		}
		if (m_Bloom.active)
		{
			flag = true;
			m_Bloom.Prepare(val2, val, val4);
		}
		flag |= TryPrepareUberImageEffect(m_ChromaticAberration, val);
		flag |= TryPrepareUberImageEffect(m_ColorGrading, val);
		flag |= TryPrepareUberImageEffect(m_Vignette, val);
		flag |= TryPrepareUberImageEffect(m_UserLut, val);
		Material val5 = (active ? m_MaterialFactory.Get("Hidden/Post FX/FXAA") : null);
		if (active)
		{
			val5.shaderKeywords = null;
			TryPrepareUberImageEffect(m_Grain, val5);
			TryPrepareUberImageEffect(m_Dithering, val5);
			if (flag)
			{
				RenderTexture val6 = m_RenderTextureFactory.Get(val2);
				Graphics.Blit((Texture)(object)val2, val6, val, 0);
				val2 = val6;
			}
			m_Fxaa.Render(val2, destination);
		}
		else
		{
			flag |= TryPrepareUberImageEffect(m_Grain, val);
			flag |= TryPrepareUberImageEffect(m_Dithering, val);
			if (flag)
			{
				if (!GraphicsUtils.isLinearColorSpace)
				{
					val.EnableKeyword("UNITY_COLORSPACE_GAMMA");
				}
				Graphics.Blit((Texture)(object)val2, destination, val, 0);
			}
		}
		if (!flag && !active)
		{
			Graphics.Blit((Texture)(object)val2, destination);
		}
		m_RenderTextureFactory.ReleaseAll();
	}

	private void OnGUI()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)Event.current.type == 7 && !((Object)(object)profile == (Object)null) && !((Object)(object)m_Camera == (Object)null))
		{
			if (m_EyeAdaptation.active && profile.debugViews.IsModeActive(BuiltinDebugViewsModel.Mode.EyeAdaptation))
			{
				m_EyeAdaptation.OnGUI();
			}
			else if (m_ColorGrading.active && profile.debugViews.IsModeActive(BuiltinDebugViewsModel.Mode.LogLut))
			{
				m_ColorGrading.OnGUI();
			}
			else if (m_UserLut.active && profile.debugViews.IsModeActive(BuiltinDebugViewsModel.Mode.UserLut))
			{
				m_UserLut.OnGUI();
			}
		}
	}

	private void OnDisable()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<CameraEvent, CommandBuffer> value in m_CommandBuffers.Values)
		{
			m_Camera.RemoveCommandBuffer(value.Key, value.Value);
			value.Value.Dispose();
		}
		m_CommandBuffers.Clear();
		if ((Object)(object)profile != (Object)null)
		{
			DisableComponents();
		}
		m_Components.Clear();
		m_MaterialFactory.Dispose();
		m_RenderTextureFactory.Dispose();
		GraphicsUtils.Dispose();
	}

	public void ResetTemporalEffects()
	{
		m_Taa.ResetHistory();
		m_MotionBlur.ResetHistory();
		m_EyeAdaptation.ResetHistory();
	}

	private void CheckObservers()
	{
		foreach (KeyValuePair<PostProcessingComponentBase, bool> componentState in m_ComponentStates)
		{
			PostProcessingComponentBase key = componentState.Key;
			bool enabled = key.GetModel().enabled;
			if (enabled != componentState.Value)
			{
				if (enabled)
				{
					m_ComponentsToEnable.Add(key);
				}
				else
				{
					m_ComponentsToDisable.Add(key);
				}
			}
		}
		for (int i = 0; i < m_ComponentsToDisable.Count; i++)
		{
			PostProcessingComponentBase postProcessingComponentBase = m_ComponentsToDisable[i];
			m_ComponentStates[postProcessingComponentBase] = false;
			postProcessingComponentBase.OnDisable();
		}
		for (int j = 0; j < m_ComponentsToEnable.Count; j++)
		{
			PostProcessingComponentBase postProcessingComponentBase2 = m_ComponentsToEnable[j];
			m_ComponentStates[postProcessingComponentBase2] = true;
			postProcessingComponentBase2.OnEnable();
		}
		m_ComponentsToDisable.Clear();
		m_ComponentsToEnable.Clear();
	}

	private void DisableComponents()
	{
		foreach (PostProcessingComponentBase component in m_Components)
		{
			PostProcessingModel model = component.GetModel();
			if (model != null && model.enabled)
			{
				component.OnDisable();
			}
		}
	}

	private CommandBuffer AddCommandBuffer<T>(CameraEvent evt, string name) where T : PostProcessingModel
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		CommandBuffer value = new CommandBuffer
		{
			name = name
		};
		KeyValuePair<CameraEvent, CommandBuffer> value2 = new KeyValuePair<CameraEvent, CommandBuffer>(evt, value);
		m_CommandBuffers.Add(typeof(T), value2);
		m_Camera.AddCommandBuffer(evt, value2.Value);
		return value2.Value;
	}

	private void RemoveCommandBuffer<T>() where T : PostProcessingModel
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Type typeFromHandle = typeof(T);
		if (m_CommandBuffers.TryGetValue(typeFromHandle, out var value))
		{
			m_Camera.RemoveCommandBuffer(value.Key, value.Value);
			m_CommandBuffers.Remove(typeFromHandle);
			value.Value.Dispose();
		}
	}

	private CommandBuffer GetCommandBuffer<T>(CameraEvent evt, string name) where T : PostProcessingModel
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!m_CommandBuffers.TryGetValue(typeof(T), out var value))
		{
			return AddCommandBuffer<T>(evt, name);
		}
		if (value.Key != evt)
		{
			RemoveCommandBuffer<T>();
			return AddCommandBuffer<T>(evt, name);
		}
		return value.Value;
	}

	private void TryExecuteCommandBuffer<T>(PostProcessingComponentCommandBuffer<T> component) where T : PostProcessingModel
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (component.active)
		{
			CommandBuffer commandBuffer = GetCommandBuffer<T>(component.GetCameraEvent(), component.GetName());
			commandBuffer.Clear();
			component.PopulateCommandBuffer(commandBuffer);
		}
		else
		{
			RemoveCommandBuffer<T>();
		}
	}

	private bool TryPrepareUberImageEffect<T>(PostProcessingComponentRenderTexture<T> component, Material material) where T : PostProcessingModel
	{
		if (!component.active)
		{
			return false;
		}
		component.Prepare(material);
		return true;
	}

	private T AddComponent<T>(T component) where T : PostProcessingComponentBase
	{
		m_Components.Add(component);
		return component;
	}
}
