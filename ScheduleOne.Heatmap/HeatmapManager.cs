using System;
using System.Collections.Generic;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.Property;
using ScheduleOne.Temperature;
using ScheduleOne.Tiles;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace ScheduleOne.Heatmap;

public class HeatmapManager : Singleton<HeatmapManager>
{
	[Serializable]
	public class PropertyData
	{
		public int[] MaskData;

		public Matrix4x4[] Matrices;

		public List<HeatmapRegion> Regions;

		public ScheduleOne.Property.Property Property;

		public bool InitialDispatched;
	}

	private struct PropertyRegionReference
	{
		public string PropertyCode;

		public int RegionAmount;
	}

	public Action<ScheduleOne.Property.Property, bool> onHeatmapVisibilityChanged;

	[Header("Components")]
	[SerializeField]
	private ComputeShader _shader;

	[SerializeField]
	private RenderTexture _heatmaps;

	[SerializeField]
	private HeatmapRegion _heatmapRegionPrefab;

	[SerializeField]
	private Material _heatmapMat;

	[Header("Settings")]
	[SerializeField]
	private Texture2D _gradientTexture;

	[Header("Debugging & Testing")]
	[SerializeField]
	private string _propertyCodeToTest;

	private Dictionary<string, PropertyData> _propertyGridMasks;

	private List<PropertyRegionReference> _propertyRegionReferences;

	private int _kernal;

	private int _textureDepth;

	public const int TEXTURE_SIZE = 128;

	public const int MAX_REGIONS = 16;

	protected override void Awake()
	{
		base.Awake();
		Initialise();
	}

	protected override void Start()
	{
		base.Start();
		SetShader();
		SetPropertyData();
		SetAllHeatmapsActive(isActive: false);
	}

	private void Initialise()
	{
		_propertyRegionReferences = new List<PropertyRegionReference>();
		_propertyGridMasks = new Dictionary<string, PropertyData>();
	}

	private void SetShader()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		_kernal = _shader.FindKernel("CSMain");
		int count = ScheduleOne.Property.Property.Properties.Count;
		_textureDepth = 16 * count;
		_heatmaps = new RenderTexture(128, 128, 0);
		((Texture)_heatmaps).dimension = (TextureDimension)5;
		_heatmaps.volumeDepth = _textureDepth;
		_heatmaps.enableRandomWrite = true;
		((Texture)_heatmaps).filterMode = (FilterMode)1;
		_heatmaps.graphicsFormat = (GraphicsFormat)52;
		_heatmaps.Create();
		_heatmapMat.SetTexture("_Heatmaps", (Texture)(object)_heatmaps);
		_shader.SetFloat("TileSize", 0.5f);
		_shader.SetFloat("MinTemperature", 0f);
		_shader.SetFloat("MaxTemperature", 40f);
		_shader.SetInt("GradientSize", ((Texture)_gradientTexture).width);
		_shader.SetTexture(_kernal, "Heatmaps", (Texture)(object)_heatmaps);
		_shader.SetTexture(_kernal, "Gradient", (Texture)(object)_gradientTexture);
	}

	private void SetPropertyData()
	{
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		foreach (ScheduleOne.Property.Property property in ScheduleOne.Property.Property.Properties)
		{
			PropertyData propertyData = new PropertyData
			{
				MaskData = new int[16384 * property.Grids.Count],
				Matrices = (Matrix4x4[])(object)new Matrix4x4[property.Grids.Count],
				Regions = new List<HeatmapRegion>(),
				Property = property
			};
			_propertyRegionReferences.Add(new PropertyRegionReference
			{
				PropertyCode = property.PropertyCode,
				RegionAmount = property.Grids.Count
			});
			int num = Mathf.Min(property.Grids.Count, 16);
			for (int i = 0; i < num; i++)
			{
				Grid grid = property.Grids[i];
				HeatmapRegion heatmapRegion = Object.Instantiate<HeatmapRegion>(_heatmapRegionPrefab, ((Component)grid).transform);
				heatmapRegion.Create(grid, (_propertyRegionReferences.Count - 1) * 16 + i, _heatmapMat);
				propertyData.Regions.Add(heatmapRegion);
				propertyData.Matrices[i] = ((Component)grid).transform.localToWorldMatrix;
				grid.OnCosmeticTemperatureEmittersChanged = (Action<string, TemperatureEmitterInfo[]>)Delegate.Combine(grid.OnCosmeticTemperatureEmittersChanged, new Action<string, TemperatureEmitterInfo[]>(OnEmitterUpdate));
				for (int j = 0; j < grid.Width; j++)
				{
					for (int k = 0; k < grid.Height; k++)
					{
						int num2 = (((Object)(object)grid.GetTile(new Coordinate(j, k)) != (Object)null) ? 1 : 0);
						int num3 = j + 128 * (k + 128 * i);
						propertyData.MaskData[num3] = num2;
					}
				}
			}
			_propertyGridMasks.Add(property.PropertyCode, propertyData);
		}
	}

	private void OnEmitterUpdate(string propertyCode, TemperatureEmitterInfo[] emitterInfos)
	{
		DispatchHeatmap(propertyCode, emitterInfos);
	}

	private void DispatchHeatmap(string propertyCode, TemperatureEmitterInfo[] emitterInfos)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		PropertyData propertyData = _propertyGridMasks[propertyCode];
		Vector2Int propertyRegionStartAndEndIndex = GetPropertyRegionStartAndEndIndex(propertyCode);
		propertyData.InitialDispatched = true;
		if (emitterInfos == null || emitterInfos.Length == 0)
		{
			Debug.Log((object)("[Heatmap] No emitters found for property: " + propertyCode + ". Using default ambient emitter."));
			emitterInfos = new TemperatureEmitterInfo[1]
			{
				new TemperatureEmitterInfo
				{
					Position = Vector3.zero,
					SqrRange = 1f,
					Temperature = propertyData.Property.AmbientTemperature
				}
			};
		}
		ComputeBuffer val = new ComputeBuffer(propertyData.MaskData.Length, 4);
		val.SetData((Array)propertyData.MaskData);
		ComputeBuffer val2 = new ComputeBuffer(propertyData.Matrices.Length, 64);
		val2.SetData((Array)propertyData.Matrices);
		ComputeBuffer val3 = new ComputeBuffer(emitterInfos.Length, TemperatureEmitterInfo.SizeOf);
		val3.SetData((Array)emitterInfos);
		_shader.SetFloat("AmbientTemperature", propertyData.Property.AmbientTemperature);
		_shader.SetInt("RegionStartIndex", ((Vector2Int)(ref propertyRegionStartAndEndIndex)).x);
		_shader.SetInt("RegionEndIndex", ((Vector2Int)(ref propertyRegionStartAndEndIndex)).y);
		_shader.SetInt("TextureSize", 128);
		_shader.SetBuffer(_kernal, "MaskData", val);
		_shader.SetBuffer(_kernal, "Matrices", val2);
		_shader.SetBuffer(_kernal, "Emitters", val3);
		int num = Mathf.CeilToInt(16f);
		_shader.Dispatch(_kernal, num, num, 2);
	}

	private Vector2Int GetPropertyRegionStartAndEndIndex(string propertyCode)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _propertyRegionReferences.Count; i++)
		{
			if (_propertyRegionReferences[i].PropertyCode == propertyCode)
			{
				int num = 16 * i;
				int num2 = num + _propertyRegionReferences[i].RegionAmount - 1;
				return new Vector2Int(num, num2);
			}
		}
		return new Vector2Int(-1, -1);
	}

	public void SetHeatmapActive(string propertyCode, bool isActive)
	{
		SetHeatmapActive(Singleton<PropertyManager>.Instance.GetProperty(propertyCode), isActive);
	}

	public void SetHeatmapActive(ScheduleOne.Property.Property property, bool isActive)
	{
		if (!_propertyGridMasks.ContainsKey(property.PropertyCode))
		{
			return;
		}
		PropertyData propertyData = _propertyGridMasks[property.PropertyCode];
		if (isActive && !propertyData.InitialDispatched)
		{
			DispatchHeatmap(property.PropertyCode, null);
		}
		foreach (HeatmapRegion region in propertyData.Regions)
		{
			((Component)region).gameObject.SetActive(isActive);
		}
		if (onHeatmapVisibilityChanged != null)
		{
			onHeatmapVisibilityChanged(property, isActive);
		}
	}

	public void ToggleHeatmapActive(ScheduleOne.Property.Property property)
	{
		SetHeatmapActive(property, !((Component)_propertyGridMasks[property.PropertyCode].Regions[0]).gameObject.activeSelf);
	}

	public void SetAllHeatmapsActive(bool isActive)
	{
		foreach (ScheduleOne.Property.Property property in ScheduleOne.Property.Property.Properties)
		{
			SetHeatmapActive(property, isActive);
		}
	}

	public bool IsHeatmapActive(ScheduleOne.Property.Property property)
	{
		if ((Object)(object)property == (Object)null)
		{
			return false;
		}
		if (_propertyGridMasks == null)
		{
			return false;
		}
		if (_propertyGridMasks.ContainsKey(property.PropertyCode))
		{
			return ((Component)_propertyGridMasks[property.PropertyCode].Regions[0]).gameObject.activeSelf;
		}
		return false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RenderTexture heatmaps = _heatmaps;
		if (heatmaps != null)
		{
			heatmaps.Release();
		}
	}

	[Button]
	public void TurnOnAllHeatmaps()
	{
		foreach (ScheduleOne.Property.Property property in ScheduleOne.Property.Property.Properties)
		{
			SetHeatmapActive(property, isActive: true);
		}
	}

	[Button]
	public void TurnOffAllHeatmaps()
	{
		foreach (ScheduleOne.Property.Property property in ScheduleOne.Property.Property.Properties)
		{
			SetHeatmapActive(property, isActive: false);
		}
	}

	[Button]
	public void RunDispatchHeatmap()
	{
		ScheduleOne.Property.Property property = ScheduleOne.Property.Property.Properties.Find((ScheduleOne.Property.Property p) => p.PropertyCode == _propertyCodeToTest);
		if ((Object)(object)property != (Object)null)
		{
			DispatchHeatmap(_propertyCodeToTest, property.Grids[0].TemperatureEmitterInfos);
		}
	}
}
