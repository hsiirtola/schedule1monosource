using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne.Tools;

public class SetTerrainObstacles : MonoBehaviour
{
	public BoxCollider Bounds;

	private TreeInstance[] Obstacle;

	private Terrain terrain;

	private float width;

	private float lenght;

	private float hight;

	private bool isError;

	private void Start()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isEditor || Debug.isDebugBuild)
		{
			Console.Log("Skipping SetTerrainObstacles in Editor");
			return;
		}
		terrain = Terrain.activeTerrain;
		Obstacle = terrain.terrainData.treeInstances;
		lenght = terrain.terrainData.size.z;
		width = terrain.terrainData.size.x;
		hight = terrain.terrainData.size.y;
		int num = 0;
		GameObject val = new GameObject("Tree_Obstacles");
		val.transform.SetParent(((Component)this).transform);
		TreeInstance[] obstacle = Obstacle;
		foreach (TreeInstance val2 in obstacle)
		{
			Vector3 val3 = Vector3.Scale(val2.position, terrain.terrainData.size) + ((Component)terrain).transform.position;
			float x = val3.x;
			Bounds bounds = ((Collider)Bounds).bounds;
			if (x < ((Bounds)(ref bounds)).min.x)
			{
				continue;
			}
			float x2 = val3.x;
			bounds = ((Collider)Bounds).bounds;
			if (x2 > ((Bounds)(ref bounds)).max.x)
			{
				continue;
			}
			float z = val3.z;
			bounds = ((Collider)Bounds).bounds;
			if (z < ((Bounds)(ref bounds)).min.z)
			{
				continue;
			}
			float z2 = val3.z;
			bounds = ((Collider)Bounds).bounds;
			if (!(z2 > ((Bounds)(ref bounds)).max.z))
			{
				Quaternion rotation = Quaternion.AngleAxis(val2.rotation * 57.29578f, Vector3.up);
				GameObject val4 = new GameObject("Obstacle" + num);
				val4.transform.SetParent(val.transform);
				val4.transform.position = val3;
				val4.transform.rotation = rotation;
				val4.AddComponent<NavMeshObstacle>();
				NavMeshObstacle component = val4.GetComponent<NavMeshObstacle>();
				component.carving = true;
				component.carveOnlyStationary = true;
				if ((Object)(object)terrain.terrainData.treePrototypes[val2.prototypeIndex].prefab.GetComponent<Collider>() == (Object)null)
				{
					isError = true;
					Debug.LogError((object)("ERROR  There is no CapsuleCollider or BoxCollider attached to ''" + ((Object)terrain.terrainData.treePrototypes[val2.prototypeIndex].prefab).name + "'' please add one of them."));
					break;
				}
				Collider component2 = terrain.terrainData.treePrototypes[val2.prototypeIndex].prefab.GetComponent<Collider>();
				if (!(((object)component2).GetType() == typeof(CapsuleCollider)) && !(((object)component2).GetType() == typeof(BoxCollider)))
				{
					isError = true;
					Debug.LogError((object)("ERROR  There is no CapsuleCollider or BoxCollider attached to ''" + ((Object)terrain.terrainData.treePrototypes[val2.prototypeIndex].prefab).name + "'' please add one of them."));
					break;
				}
				if (((object)component2).GetType() == typeof(CapsuleCollider))
				{
					CapsuleCollider component3 = terrain.terrainData.treePrototypes[val2.prototypeIndex].prefab.GetComponent<CapsuleCollider>();
					component.shape = (NavMeshObstacleShape)0;
					component.center = component3.center;
					component.radius = component3.radius;
					component.height = component3.height;
				}
				else if (((object)component2).GetType() == typeof(BoxCollider))
				{
					BoxCollider component4 = terrain.terrainData.treePrototypes[val2.prototypeIndex].prefab.GetComponent<BoxCollider>();
					component.shape = (NavMeshObstacleShape)1;
					component.center = component4.center;
					component.size = component4.size;
				}
				num++;
			}
		}
		if (!isError)
		{
			Debug.Log((object)(Obstacle.Length + " NavMeshObstacles were succesfully added to scene"));
		}
	}
}
