using System.Collections;
using UnityEngine;

namespace LiquidVolumeFX;

public class SpillController : MonoBehaviour
{
	public GameObject spill;

	private LiquidVolume lv;

	private GameObject[] dropTemplates;

	private const int DROP_TEMPLATES_COUNT = 10;

	private void Start()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		lv = ((Component)this).GetComponent<LiquidVolume>();
		dropTemplates = (GameObject[])(object)new GameObject[10];
		for (int i = 0; i < 10; i++)
		{
			GameObject val = Object.Instantiate<GameObject>(spill);
			Transform transform = val.transform;
			transform.localScale *= Random.Range(0.45f, 0.65f);
			val.GetComponent<Renderer>().material.color = Color.Lerp(lv.liquidColor1, lv.liquidColor2, Random.value);
			val.SetActive(false);
			dropTemplates[i] = val;
		}
	}

	private void Update()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKey((KeyCode)276))
		{
			((Component)this).transform.Rotate(Vector3.forward * Time.deltaTime * 10f);
		}
		if (Input.GetKey((KeyCode)275))
		{
			((Component)this).transform.Rotate(-Vector3.forward * Time.deltaTime * 10f);
		}
	}

	private void FixedUpdate()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (lv.GetSpillPoint(out var spillPosition, out var spillAmount))
		{
			for (int i = 0; i < 15; i++)
			{
				int num = Random.Range(0, 10);
				GameObject val = Object.Instantiate<GameObject>(dropTemplates[num]);
				val.SetActive(true);
				Rigidbody component = val.GetComponent<Rigidbody>();
				((Component)component).transform.position = spillPosition + Random.insideUnitSphere * 0.01f;
				component.AddForce(new Vector3(Random.value - 0.5f, Random.value * 0.1f - 0.2f, Random.value - 0.5f));
				((MonoBehaviour)this).StartCoroutine(DestroySpill(val));
			}
			lv.level -= spillAmount / 10f + 0.001f;
		}
	}

	private IEnumerator DestroySpill(GameObject spill)
	{
		yield return (object)new WaitForSeconds(1f);
		Object.Destroy((Object)(object)spill);
	}
}
