using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class VolumetricFire : MonoBehaviour
{
	private Mesh mesh;

	private Material material;

	[SerializeField]
	[Range(1f, 20f)]
	[Tooltip("Controls the number of additional meshes to render in front of and behind the original mesh")]
	private int thickness = 1;

	[SerializeField]
	[Range(0.01f, 1f)]
	[Tooltip("Controls the total distance between the frontmost mesh and the backmost mesh")]
	private float spread = 0.2f;

	[SerializeField]
	private bool billboard = true;

	private MaterialPropertyBlock materialPropertyBlock;

	private int internalCount;

	private float randomStatic;

	private Collider boundaryCollider;

	private void Start()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		materialPropertyBlock = new MaterialPropertyBlock();
		MeshRenderer component = ((Component)this).GetComponent<MeshRenderer>();
		((Renderer)component).enabled = false;
		material = ((Renderer)component).sharedMaterial;
		mesh = ((Component)this).GetComponent<MeshFilter>().sharedMesh;
		boundaryCollider = ((Component)this).GetComponent<Collider>();
		randomStatic = Random.Range(0f, 1f);
	}

	private void OnEnable()
	{
		RenderPipelineManager.beginCameraRendering += RenderFlames;
	}

	private void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= RenderFlames;
	}

	private static bool IsVisible(Camera camera, Bounds bounds)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(camera), bounds);
	}

	private void RenderFlames(ScriptableRenderContext context, Camera camera)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		IsVisible(camera, boundaryCollider.bounds);
		internalCount = (thickness - 1) * 2;
		float spacing = 0f;
		if (internalCount > 0)
		{
			spacing = spread / (float)internalCount;
		}
		for (int i = 0; i <= internalCount; i++)
		{
			float item = (float)i - (float)internalCount * 0.5f;
			SetupMaterialPropertyBlock(item);
			CreateItem(spacing, item, camera);
		}
	}

	private void SetupMaterialPropertyBlock(float item)
	{
		if (materialPropertyBlock != null)
		{
			materialPropertyBlock.SetFloat("_ITEMNUMBER", item);
			materialPropertyBlock.SetFloat("_INTERNALCOUNT", (float)internalCount);
			materialPropertyBlock.SetFloat("_INITIALPOSITIONINT", randomStatic);
		}
	}

	private void CreateItem(float spacing, float item, Camera camera)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		Quaternion identity = Quaternion.identity;
		Vector3 zero = Vector3.zero;
		if (billboard)
		{
			identity *= ((Component)camera).transform.rotation;
			Vector3 val = ((Component)this).transform.position - ((Component)camera).transform.position;
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			zero = ((Component)this).transform.position - normalized * item * spacing;
		}
		else
		{
			identity = ((Component)this).transform.rotation;
			zero = ((Component)this).transform.position - ((Component)this).transform.forward * item * spacing;
		}
		Matrix4x4 val2 = Matrix4x4.TRS(zero, identity, ((Component)this).transform.localScale);
		Graphics.DrawMesh(mesh, val2, material, 0, camera, 0, materialPropertyBlock, false, false, false);
	}
}
