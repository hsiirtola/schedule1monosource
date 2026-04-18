using UnityEngine;

namespace ScheduleOne.StationFramework;

public class MushroomSpawnStationItem : StationItem
{
	[SerializeField]
	private MeshRenderer[] _renderers;

	[SerializeField]
	private int _materialIndex;

	[SerializeField]
	private GameObject _injectionPortHighlight;

	[field: SerializeField]
	public Collider InjectionPortCollider { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		SetInocculationAmount(0f);
		SetInjectionPortHighlightActive(active: false);
		float num = Random.Range(0f, 360f);
		for (int i = 0; i < _renderers.Length; i++)
		{
			((Renderer)_renderers[i]).materials[_materialIndex].SetFloat("_AngleOffset", num);
		}
	}

	public void SetInocculationAmount(float amount)
	{
		for (int i = 0; i < _renderers.Length; i++)
		{
			((Renderer)_renderers[i]).materials[_materialIndex].SetFloat("_SpawnAmount", Mathf.Clamp01(amount));
		}
	}

	public void SetInjectionPortHighlightActive(bool active)
	{
		_injectionPortHighlight.SetActive(active);
	}
}
