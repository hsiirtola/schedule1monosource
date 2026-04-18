using System;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using UnityEngine;

namespace ScheduleOne.Weather;

public class HeightMaskGenerator : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private ComputeShader _maskShader;

	[Header("Settings")]
	[SerializeField]
	private float _size;

	[SerializeField]
	private int _resolution = 128;

	[SerializeField]
	private Vector2 _minMaxHeight = new Vector2(0f, 100f);

	[SerializeField]
	private LayerMask _heightmapLayerMask;

	[Header("Debugging & Development")]
	[SerializeField]
	private float _debugTileSize;

	[SerializeField]
	private RenderTexture _heightTexture;

	[SerializeField]
	private Material _debugMaterial;

	private int _kernal;

	private float _tileSize;

	private float _tileHalfSize;

	private Vector3 _origin;

	private ComputeBuffer _heightBuffer;

	public void InitialiseMaskMap()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(_size, 0f, _size);
		_tileSize = _size / (float)_resolution;
		_tileHalfSize = _tileSize / 2f;
		_origin = ((Component)this).transform.position - val / 2f + new Vector3(_tileHalfSize, 0f, _tileHalfSize);
		_kernal = _maskShader.FindKernel("CSMain");
		((Texture)_heightTexture).width = _resolution;
		((Texture)_heightTexture).height = _resolution;
		_maskShader.SetInt("_Resolution", _resolution);
		_maskShader.SetTexture(_kernal, "_MaskMap", (Texture)(object)_heightTexture);
		Shader.SetGlobalTexture("_HeightMap", (Texture)(object)_heightTexture);
		Shader.SetGlobalVector("_HeightMapOrigin", Vector4.op_Implicit(_origin.XZ()));
		Shader.SetGlobalFloat("_MinHeight", _minMaxHeight.x);
		Shader.SetGlobalFloat("_MaxHeight", _minMaxHeight.y);
		Shader.SetGlobalFloat("_HeightMapTileSize", _tileSize);
		Shader.SetGlobalFloat("_HeightMapResolution", (float)_resolution);
	}

	private void GenerateMaskMap()
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		float[] array = new float[_resolution * _resolution];
		RaycastHit val2 = default(RaycastHit);
		for (int i = 0; i < _resolution; i++)
		{
			for (int j = 0; j < _resolution; j++)
			{
				Vector3 val = _origin + new Vector3((float)i * _tileSize, 0f, (float)j * _tileSize);
				float num = 0f;
				if (Physics.Raycast(val + Vector3.up * _minMaxHeight.y, Vector3.down, ref val2, _minMaxHeight.y - _minMaxHeight.x, LayerMask.op_Implicit(_heightmapLayerMask)))
				{
					num = Mathf.InverseLerp(_minMaxHeight.x, _minMaxHeight.y, ((RaycastHit)(ref val2)).point.y);
				}
				array[j * _resolution + i] = num;
			}
		}
		_heightBuffer = new ComputeBuffer(array.Length, 4);
		_heightBuffer.SetData((Array)array);
		_maskShader.SetBuffer(_kernal, "_HeightBuffer", _heightBuffer);
		_maskShader.Dispatch(_kernal, _resolution / 8, _resolution / 8, 1);
	}

	private void OnDestroy()
	{
		if ((Object)(object)_heightTexture != (Object)null)
		{
			_heightTexture.Release();
		}
		_heightTexture = null;
	}

	[Button]
	private void GenerateHeightMapDebug()
	{
		InitialiseMaskMap();
		GenerateMaskMap();
	}

	[Button]
	private void Dispose()
	{
		Object.DestroyImmediate((Object)(object)_heightTexture);
		_heightTexture = null;
	}

	private void OnDrawGizmos()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(_size, 0f, _size);
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(((Component)this).transform.position, val);
		_debugTileSize = _size / (float)_resolution;
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(((Component)this).transform.position - val / 2f + new Vector3(_debugTileSize / 2f, 0f, _debugTileSize / 2f), new Vector3(_debugTileSize, 0f, _debugTileSize));
	}
}
