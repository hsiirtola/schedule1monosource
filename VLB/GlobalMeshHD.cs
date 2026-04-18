using UnityEngine;

namespace VLB;

public static class GlobalMeshHD
{
	private static Mesh ms_Mesh;

	public static Mesh Get()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ms_Mesh == (Object)null)
		{
			Destroy();
			ms_Mesh = MeshGenerator.GenerateConeZ_Radii_DoubleCaps(1f, 1f, 1f, Config.Instance.sharedMeshSides, inverted: true);
			((Object)ms_Mesh).hideFlags = Consts.Internal.ProceduralObjectsHideFlags;
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
