using UnityEngine;

namespace Funly.SkyStudio;

public class LightningSpawnArea : MonoBehaviour
{
	[Tooltip("Dimensions of the lightning area where lightning bolts will be spawned inside randomly.")]
	public Vector3 lightningArea = new Vector3(40f, 20f, 20f);

	public void OnDrawGizmosSelected()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		_ = ((Component)this).transform.localScale;
		Gizmos.color = Color.yellow;
		_ = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS(((Component)this).transform.position, ((Component)this).transform.rotation, lightningArea);
		Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
	}

	private void OnEnable()
	{
		LightningRenderer.AddSpawnArea(this);
	}

	private void OnDisable()
	{
		LightningRenderer.RemoveSpawnArea(this);
	}
}
