using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core;
using UnityEngine;

namespace ScheduleOne.DevUtilities;

public class LODAdjuster : MonoBehaviour
{
	[Header("Components")]
	[SerializeField]
	private LODGroup _lodGroup;

	[Header("LOD Settings")]
	[SerializeField]
	private string _rendererName;

	[SerializeField]
	private int _lodLevel;

	[Button]
	public void AddToLodGroup()
	{
		List<MeshRenderer> list = (from r in ((Component)this).GetComponentsInChildren<MeshRenderer>()
			where ((Object)((Component)r).gameObject).name.Contains(_rendererName)
			select r).ToList();
		LOD[] lODs = _lodGroup.GetLODs();
		List<Renderer> list2 = lODs[_lodLevel].renderers.ToList();
		_ = list2.Count;
		foreach (MeshRenderer item in list)
		{
			list2.Add((Renderer)(object)item);
		}
		lODs[_lodLevel].renderers = list2.ToArray();
		_lodGroup.SetLODs(lODs);
		_lodGroup.RecalculateBounds();
		Debug.Log((object)$"Added {list.Count} renderers to LOD {_lodLevel} of LODGroup {((Object)((Component)_lodGroup).gameObject).name}.");
	}
}
