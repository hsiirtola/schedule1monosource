using UnityEngine;

namespace VLB;

public static class GlobalMeshSD
{
	private static Mesh ms_Mesh;

	private static bool ms_DoubleSided;

	public static Mesh Get()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		bool sD_requiresDoubleSidedMesh = Config.Instance.SD_requiresDoubleSidedMesh;
		if ((Object)(object)ms_Mesh == (Object)null || ms_DoubleSided != sD_requiresDoubleSidedMesh)
		{
			Destroy();
			ms_Mesh = MeshGenerator.GenerateConeZ_Radii(1f, 1f, 1f, Config.Instance.sharedMeshSides, Config.Instance.sharedMeshSegments, cap: true, sD_requiresDoubleSidedMesh);
			((Object)ms_Mesh).hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
			ms_DoubleSided = sD_requiresDoubleSidedMesh;
		}
		return ms_Mesh;
	}

	public static void Destroy()
	{
		if ((Object)(object)ms_Mesh != (Object)null)
		{
			Object.DestroyImmediate((Object)(object)ms_Mesh);
			ms_Mesh = null;
		}
	}
}
