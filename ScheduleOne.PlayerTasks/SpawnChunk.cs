using System.Collections.Generic;
using ScheduleOne.Audio;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.PlayerTasks;

[RequireComponent(typeof(Clickable))]
[RequireComponent(typeof(Rigidbody))]
public class SpawnChunk : Clickable
{
	private MeshRenderer _meshRenderer;

	private Rigidbody _rb;

	private Collider _collider;

	private bool _isBroken;

	private List<SpawnChunk> _childChunks = new List<SpawnChunk>();

	public UnityEvent OnBreak;

	private bool hasChildChunks => _childChunks.Count > 0;

	private void Awake()
	{
		_meshRenderer = ((Component)this).GetComponent<MeshRenderer>();
		_rb = ((Component)this).GetComponent<Rigidbody>();
		_collider = ((Component)this).GetComponent<Collider>();
		SpawnChunk item = default(SpawnChunk);
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			if (((Component)((Component)this).transform.GetChild(i)).TryGetComponent<SpawnChunk>(ref item))
			{
				_childChunks.Add(item);
			}
		}
	}

	public void EnableChunk(Vector3 force, Vector3 torque)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		((Renderer)_meshRenderer).enabled = true;
		_collider.enabled = true;
		ClickableEnabled = hasChildChunks;
		if (!hasChildChunks)
		{
			_isBroken = true;
		}
		_rb.position = ((Component)this).transform.position;
		_rb.rotation = ((Component)this).transform.rotation;
		_rb.isKinematic = false;
		_rb.AddForce(force, (ForceMode)2);
		_rb.AddTorque(torque, (ForceMode)2);
	}

	public void DisableChunk(bool recursive)
	{
		((Renderer)_meshRenderer).enabled = false;
		_rb.isKinematic = true;
		_collider.enabled = false;
		ClickableEnabled = false;
		if (!recursive)
		{
			return;
		}
		foreach (SpawnChunk childChunk in _childChunks)
		{
			childChunk.DisableChunk(recursive: true);
		}
	}

	public void Break()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		_isBroken = true;
		DisableChunk(recursive: false);
		foreach (SpawnChunk childChunk in _childChunks)
		{
			Vector3 val = ((Component)childChunk).transform.position - _rb.worldCenterOfMass;
			Vector3 val2 = ((Vector3)(ref val)).normalized * Random.Range(0.5f, 0.8f);
			val2.y += 0.3f;
			val2 += Random.insideUnitSphere * 0.8f;
			childChunk.EnableChunk(val2, Random.insideUnitSphere * 3f);
		}
		if (OnBreak != null)
		{
			OnBreak.Invoke();
		}
	}

	public bool GetIsBroken(bool recursive = true)
	{
		if (recursive)
		{
			foreach (SpawnChunk childChunk in _childChunks)
			{
				if (!childChunk.GetIsBroken())
				{
					return false;
				}
			}
		}
		return _isBroken;
	}

	public override void StartClick(RaycastHit hit)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		base.StartClick(hit);
		if (ClickableEnabled)
		{
			Break();
		}
	}

	public void SetChunkOrder(int i)
	{
		AudioSourceController audioSourceController = default(AudioSourceController);
		if (((Component)this).TryGetComponent<AudioSourceController>(ref audioSourceController))
		{
			audioSourceController.VolumeMultiplier = 1f / (float)(i + 1);
		}
		foreach (SpawnChunk childChunk in _childChunks)
		{
			childChunk.SetChunkOrder(i + 1);
		}
	}
}
