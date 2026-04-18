using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponTrail : MonoBehaviour
{
	[Serializable]
	public class Point
	{
		public float timeCreated;

		public Vector3 basePosition;

		public Vector3 tipPosition;
	}

	[SerializeField]
	private bool _emit = true;

	private bool _use = true;

	[SerializeField]
	private float _emitTime;

	[SerializeField]
	private Material _material;

	[SerializeField]
	private float _lifeTime = 1f;

	[SerializeField]
	private Color[] _colors;

	[SerializeField]
	private float[] _sizes;

	[SerializeField]
	private float _minVertexDistance = 0.1f;

	[SerializeField]
	private float _maxVertexDistance = 10f;

	private float _minVertexDistanceSqr;

	private float _maxVertexDistanceSqr;

	[SerializeField]
	private float _maxAngle = 3f;

	[SerializeField]
	private bool _autoDestruct;

	[SerializeField]
	private int subdivisions = 4;

	[SerializeField]
	private Transform _base;

	[SerializeField]
	private Transform _tip;

	private List<Point> _points = new List<Point>();

	private List<Point> _smoothedPoints = new List<Point>();

	private GameObject _trailObject;

	private Mesh _trailMesh;

	private Vector3 _lastPosition;

	public bool Emit
	{
		set
		{
			_emit = value;
		}
	}

	public bool Use
	{
		set
		{
			_use = value;
		}
	}

	private void Start()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		_lastPosition = ((Component)this).transform.position;
		_trailObject = new GameObject("Trail");
		_trailObject.transform.parent = null;
		_trailObject.transform.position = Vector3.zero;
		_trailObject.transform.rotation = Quaternion.identity;
		_trailObject.transform.localScale = Vector3.one;
		_trailObject.AddComponent(typeof(MeshFilter));
		_trailObject.AddComponent(typeof(MeshRenderer));
		_trailObject.GetComponent<Renderer>().material = _material;
		_trailMesh = new Mesh();
		((Object)_trailMesh).name = ((Object)this).name + "TrailMesh";
		_trailObject.GetComponent<MeshFilter>().mesh = _trailMesh;
		_minVertexDistanceSqr = _minVertexDistance * _minVertexDistance;
		_maxVertexDistanceSqr = _maxVertexDistance * _maxVertexDistance;
	}

	private void OnDisable()
	{
		Object.Destroy((Object)(object)_trailObject);
	}

	private void Update()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_069f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_084f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0856: Unknown result type (might be due to invalid IL or missing references)
		//IL_085b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0860: Unknown result type (might be due to invalid IL or missing references)
		//IL_086a: Unknown result type (might be due to invalid IL or missing references)
		//IL_086f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0879: Unknown result type (might be due to invalid IL or missing references)
		//IL_087e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0883: Unknown result type (might be due to invalid IL or missing references)
		//IL_0892: Unknown result type (might be due to invalid IL or missing references)
		//IL_0897: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_08be: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0902: Unknown result type (might be due to invalid IL or missing references)
		//IL_0907: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_075e: Unknown result type (might be due to invalid IL or missing references)
		//IL_076c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0773: Unknown result type (might be due to invalid IL or missing references)
		//IL_0778: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0418: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		if (!_use)
		{
			return;
		}
		if (_emit && _emitTime != 0f)
		{
			_emitTime -= Time.deltaTime;
			if (_emitTime == 0f)
			{
				_emitTime = -1f;
			}
			if (_emitTime < 0f)
			{
				_emit = false;
			}
		}
		if (!_emit && _points.Count == 0 && _autoDestruct)
		{
			Object.Destroy((Object)(object)_trailObject);
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		if (!Object.op_Implicit((Object)(object)Camera.main))
		{
			return;
		}
		Vector3 val = _lastPosition - ((Component)this).transform.position;
		float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
		if (_emit)
		{
			if (sqrMagnitude > _minVertexDistanceSqr)
			{
				bool flag = false;
				if (_points.Count < 3)
				{
					flag = true;
				}
				else
				{
					Vector3 val2 = _points[_points.Count - 2].tipPosition - _points[_points.Count - 3].tipPosition;
					Vector3 val3 = _points[_points.Count - 1].tipPosition - _points[_points.Count - 2].tipPosition;
					if (Vector3.Angle(val2, val3) > _maxAngle || sqrMagnitude > _maxVertexDistanceSqr)
					{
						flag = true;
					}
				}
				if (flag)
				{
					Point point = new Point();
					point.basePosition = _base.position;
					point.tipPosition = _tip.position;
					point.timeCreated = Time.time;
					_points.Add(point);
					_lastPosition = ((Component)this).transform.position;
					if (_points.Count == 1)
					{
						_smoothedPoints.Add(point);
					}
					else if (_points.Count > 1)
					{
						for (int i = 0; i < 1 + subdivisions; i++)
						{
							_smoothedPoints.Add(point);
						}
					}
					if (_points.Count >= 4)
					{
						IEnumerable<Vector3> collection = Interpolate.NewCatmullRom((Vector3[])(object)new Vector3[4]
						{
							_points[_points.Count - 4].tipPosition,
							_points[_points.Count - 3].tipPosition,
							_points[_points.Count - 2].tipPosition,
							_points[_points.Count - 1].tipPosition
						}, subdivisions, loop: false);
						IEnumerable<Vector3> collection2 = Interpolate.NewCatmullRom((Vector3[])(object)new Vector3[4]
						{
							_points[_points.Count - 4].basePosition,
							_points[_points.Count - 3].basePosition,
							_points[_points.Count - 2].basePosition,
							_points[_points.Count - 1].basePosition
						}, subdivisions, loop: false);
						List<Vector3> list = new List<Vector3>(collection);
						List<Vector3> list2 = new List<Vector3>(collection2);
						float timeCreated = _points[_points.Count - 4].timeCreated;
						float timeCreated2 = _points[_points.Count - 1].timeCreated;
						for (int j = 0; j < list.Count; j++)
						{
							int num = _smoothedPoints.Count - (list.Count - j);
							if (num > -1 && num < _smoothedPoints.Count)
							{
								Point point2 = new Point();
								point2.basePosition = list2[j];
								point2.tipPosition = list[j];
								point2.timeCreated = Mathf.Lerp(timeCreated, timeCreated2, (float)j / (float)list.Count);
								_smoothedPoints[num] = point2;
							}
						}
					}
				}
				else
				{
					_points[_points.Count - 1].basePosition = _base.position;
					_points[_points.Count - 1].tipPosition = _tip.position;
					_smoothedPoints[_smoothedPoints.Count - 1].basePosition = _base.position;
					_smoothedPoints[_smoothedPoints.Count - 1].tipPosition = _tip.position;
				}
			}
			else
			{
				if (_points.Count > 0)
				{
					_points[_points.Count - 1].basePosition = _base.position;
					_points[_points.Count - 1].tipPosition = _tip.position;
				}
				if (_smoothedPoints.Count > 0)
				{
					_smoothedPoints[_smoothedPoints.Count - 1].basePosition = _base.position;
					_smoothedPoints[_smoothedPoints.Count - 1].tipPosition = _tip.position;
				}
			}
		}
		RemoveOldPoints(_points);
		if (_points.Count == 0)
		{
			_trailMesh.Clear();
		}
		RemoveOldPoints(_smoothedPoints);
		if (_smoothedPoints.Count == 0)
		{
			_trailMesh.Clear();
		}
		List<Point> smoothedPoints = _smoothedPoints;
		if (smoothedPoints.Count <= 1)
		{
			return;
		}
		Vector3[] array = (Vector3[])(object)new Vector3[smoothedPoints.Count * 2];
		Vector2[] array2 = (Vector2[])(object)new Vector2[smoothedPoints.Count * 2];
		int[] array3 = new int[(smoothedPoints.Count - 1) * 6];
		Color[] array4 = (Color[])(object)new Color[smoothedPoints.Count * 2];
		for (int k = 0; k < smoothedPoints.Count; k++)
		{
			Point point3 = smoothedPoints[k];
			float num2 = (Time.time - point3.timeCreated) / _lifeTime;
			Color val4 = Color.Lerp(Color.white, Color.clear, num2);
			if (_colors != null && _colors.Length != 0)
			{
				float num3 = num2 * (float)(_colors.Length - 1);
				float num4 = Mathf.Floor(num3);
				float num5 = Mathf.Clamp(Mathf.Ceil(num3), 1f, (float)(_colors.Length - 1));
				float num6 = Mathf.InverseLerp(num4, num5, num3);
				if (num4 >= (float)_colors.Length)
				{
					num4 = _colors.Length - 1;
				}
				if (num4 < 0f)
				{
					num4 = 0f;
				}
				if (num5 >= (float)_colors.Length)
				{
					num5 = _colors.Length - 1;
				}
				if (num5 < 0f)
				{
					num5 = 0f;
				}
				val4 = Color.Lerp(_colors[(int)num4], _colors[(int)num5], num6);
			}
			float num7 = 0f;
			if (_sizes != null && _sizes.Length != 0)
			{
				float num8 = num2 * (float)(_sizes.Length - 1);
				float num9 = Mathf.Floor(num8);
				float num10 = Mathf.Clamp(Mathf.Ceil(num8), 1f, (float)(_sizes.Length - 1));
				float num11 = Mathf.InverseLerp(num9, num10, num8);
				if (num9 >= (float)_sizes.Length)
				{
					num9 = _sizes.Length - 1;
				}
				if (num9 < 0f)
				{
					num9 = 0f;
				}
				if (num10 >= (float)_sizes.Length)
				{
					num10 = _sizes.Length - 1;
				}
				if (num10 < 0f)
				{
					num10 = 0f;
				}
				num7 = Mathf.Lerp(_sizes[(int)num9], _sizes[(int)num10], num11);
			}
			Vector3 val5 = point3.tipPosition - point3.basePosition;
			array[k * 2] = point3.basePosition - val5 * (num7 * 0.5f);
			array[k * 2 + 1] = point3.tipPosition + val5 * (num7 * 0.5f);
			array4[k * 2] = (array4[k * 2 + 1] = val4);
			float num12 = (float)k / (float)smoothedPoints.Count;
			array2[k * 2] = new Vector2(num12, 0f);
			array2[k * 2 + 1] = new Vector2(num12, 1f);
			if (k > 0)
			{
				array3[(k - 1) * 6] = k * 2 - 2;
				array3[(k - 1) * 6 + 1] = k * 2 - 1;
				array3[(k - 1) * 6 + 2] = k * 2;
				array3[(k - 1) * 6 + 3] = k * 2 + 1;
				array3[(k - 1) * 6 + 4] = k * 2;
				array3[(k - 1) * 6 + 5] = k * 2 - 1;
			}
		}
		_trailMesh.Clear();
		_trailMesh.vertices = array;
		_trailMesh.colors = array4;
		_trailMesh.uv = array2;
		_trailMesh.triangles = array3;
	}

	private void RemoveOldPoints(List<Point> pointList)
	{
		List<Point> list = new List<Point>();
		foreach (Point point in pointList)
		{
			if (Time.time - point.timeCreated > _lifeTime)
			{
				list.Add(point);
			}
		}
		foreach (Point item in list)
		{
			pointList.Remove(item);
		}
	}
}
