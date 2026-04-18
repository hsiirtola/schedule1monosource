using ScheduleOne.Audio;
using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Growing;

public class GrowingMushroom : MonoBehaviour
{
	private const float CapExpansionThreshold = 0.5f;

	[HideInInspector]
	public float LateralScaleMultiplier = 1f;

	[HideInInspector]
	public float VerticalScaleMultiplier = 1f;

	[HideInInspector]
	public float MaxCapExpansion = 1f;

	[SerializeField]
	private Transform _modelContainer;

	[SerializeField]
	private SkinnedMeshRenderer[] _meshRenderers;

	[SerializeField]
	private AudioSourceController _harvestSound;

	private ShroomColony _parentColony;

	private int _alignmentIndex;

	private void Awake()
	{
	}

	public void Initialize(ShroomColony parentColony, int alignmentIndex)
	{
		_parentColony = parentColony;
		_alignmentIndex = alignmentIndex;
	}

	public void SetGrowthPercent(float percent)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01(percent);
		float num2 = (num - 0.5f) / 0.5f * MaxCapExpansion;
		SkinnedMeshRenderer[] meshRenderers = _meshRenderers;
		foreach (SkinnedMeshRenderer val in meshRenderers)
		{
			for (int j = 0; j < val.sharedMesh.blendShapeCount; j++)
			{
				val.SetBlendShapeWeight(j, 100f * (1f - num2));
			}
		}
		float num3 = Mathf.Lerp(0.1f, 1f, num);
		((Component)this).transform.localScale = new Vector3(LateralScaleMultiplier * num3, VerticalScaleMultiplier * num3, LateralScaleMultiplier * num3);
	}

	[Button]
	public void Harvest()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(_parentColony.GetHarvestedShroom());
		Object.Destroy((Object)(object)((Component)this).GetComponent<Collider>());
		_harvestSound.Play();
		_modelContainer.SetParent(NetworkSingleton<GameManager>.Instance.Temp);
		Rigidbody obj = ((Component)_modelContainer).gameObject.AddComponent<Rigidbody>();
		obj.AddForce(Vector3.up * 2f, (ForceMode)2);
		obj.AddTorque(new Vector3(Random.Range(-1f, 1f), Random.Range(1f, 1f), Random.Range(-1f, 1f)) * 4f, (ForceMode)2);
		Object.Destroy((Object)(object)((Component)_modelContainer).gameObject, 0.5f);
		_parentColony.RemoveShroom_Server(_alignmentIndex);
	}
}
