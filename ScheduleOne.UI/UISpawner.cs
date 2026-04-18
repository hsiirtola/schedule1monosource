using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI;

public class UISpawner : MonoBehaviour
{
	public RectTransform SpawnArea;

	public GameObject[] Prefabs;

	public float MinInterval = 1f;

	public float MaxInterval = 5f;

	public float SpawnRateMultiplier = 1f;

	public Vector2 MinScale = Vector2.one;

	public Vector2 MaxScale = Vector2.one;

	public bool UniformScale = true;

	private float nextSpawnTime;

	public UnityEvent<GameObject> OnSpawn;

	private void Start()
	{
		nextSpawnTime = Time.time + Random.Range(MinInterval, MaxInterval);
	}

	private void Update()
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		if (SpawnRateMultiplier == 0f || !(Time.time > nextSpawnTime))
		{
			return;
		}
		nextSpawnTime = Time.time + Random.Range(MinInterval, MaxInterval) / SpawnRateMultiplier;
		if (Prefabs.Length != 0)
		{
			GameObject val = Object.Instantiate<GameObject>(Prefabs[Random.Range(0, Prefabs.Length)], ((Component)this).transform);
			if (UniformScale)
			{
				float num = Random.Range(MinScale.x, MaxScale.x);
				val.transform.localScale = new Vector3(num, num, 1f);
			}
			else
			{
				val.transform.localScale = new Vector3(Random.Range(MinScale.x, MaxScale.x), Random.Range(MinScale.y, MaxScale.y), 1f);
			}
			Transform transform = val.transform;
			Rect rect = SpawnArea.rect;
			float num2 = (0f - ((Rect)(ref rect)).width) / 2f;
			rect = SpawnArea.rect;
			float num3 = Random.Range(num2, ((Rect)(ref rect)).width / 2f);
			rect = SpawnArea.rect;
			float num4 = (0f - ((Rect)(ref rect)).height) / 2f;
			rect = SpawnArea.rect;
			transform.localPosition = new Vector3(num3, Random.Range(num4, ((Rect)(ref rect)).height / 2f), 0f);
			if (OnSpawn != null)
			{
				OnSpawn.Invoke(val);
			}
		}
	}
}
