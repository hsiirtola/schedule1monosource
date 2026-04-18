using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public class LightOptimizer : MonoBehaviour
{
	public bool LightsEnabled = true;

	[Header("References")]
	[SerializeField]
	protected BoxCollider[] activationZones;

	[SerializeField]
	protected Transform[] viewPoints;

	[Header("Settings")]
	public float checkRange = 50f;

	protected OptimizedLight[] lights;

	public void Awake()
	{
		lights = ((Component)this).GetComponentsInChildren<OptimizedLight>();
		((Behaviour)this).enabled = false;
	}

	public void FixedUpdate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if (!PlayerSingleton<PlayerCamera>.InstanceExists)
		{
			return;
		}
		OptimizedLight[] array;
		if (Vector3.Distance(((Component)this).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position) > checkRange)
		{
			array = lights;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].DisabledForOptimization = true;
			}
			return;
		}
		if (activationZones.Length == 0 && viewPoints.Length == 0)
		{
			ApplyLights();
			return;
		}
		BoxCollider[] array2 = activationZones;
		for (int i = 0; i < array2.Length; i++)
		{
			Bounds bounds = ((Collider)array2[i]).bounds;
			if (((Bounds)(ref bounds)).Contains(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position))
			{
				ApplyLights();
				return;
			}
		}
		Transform[] array3 = viewPoints;
		foreach (Transform val in array3)
		{
			if (PointInCameraView(val.position))
			{
				ApplyLights();
				return;
			}
		}
		array = lights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DisabledForOptimization = true;
		}
	}

	public void ApplyLights()
	{
		OptimizedLight[] array = lights;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DisabledForOptimization = false;
		}
	}

	public bool PointInCameraView(Vector3 point)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Camera camera = PlayerSingleton<PlayerCamera>.Instance.Camera;
		bool num = camera.WorldToViewportPoint(point).z > -1f;
		bool flag = false;
		Vector3 val = point - ((Component)camera).transform.position;
		val = ((Vector3)(ref val)).normalized;
		float num2 = Vector3.Distance(((Component)camera).transform.position, point);
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(((Component)camera).transform.position, val, ref val2, num2 + 0.05f, 1 << LayerMask.NameToLayer("Default")) && ((RaycastHit)(ref val2)).point != point)
		{
			flag = true;
		}
		if (num)
		{
			return !flag;
		}
		return false;
	}

	public bool Is01(float a)
	{
		if (a > 0f)
		{
			return a < 1f;
		}
		return false;
	}

	public void LightsEnabled_True()
	{
		LightsEnabled = true;
	}

	public void LightsEnabled_False()
	{
		LightsEnabled = false;
	}
}
