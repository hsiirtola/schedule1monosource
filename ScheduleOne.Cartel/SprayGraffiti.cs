using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using GameKit.Utilities;
using ScheduleOne.DevUtilities;
using ScheduleOne.Graffiti;
using ScheduleOne.Map;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Cartel;

public class SprayGraffiti : CartelActivity
{
	[Header("Settings")]
	[SerializeField]
	private float _minimumDistanceFromPlayers = 20f;

	private WorldSpraySurface _validSpraySurface;

	public override bool IsRegionValidForActivity(EMapRegion region)
	{
		SetSpraySurface(region);
		if ((Object)(object)_validSpraySurface != (Object)null)
		{
			return base.IsRegionValidForActivity(region);
		}
		return false;
	}

	public void SetSpraySurface(EMapRegion region, bool overrideExisting = true)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!overrideExisting && (Object)(object)_validSpraySurface != (Object)null)
		{
			return;
		}
		List<WorldSpraySurface> list = NetworkSingleton<GraffitiManager>.Instance.WorldSpraySurfaces.Where((WorldSpraySurface s) => s.Region == region && s.CanBeSprayedByNPCs && s.CanBeEdited(checkEditor: true)).ToList();
		Arrays.Shuffle<WorldSpraySurface>(list);
		_validSpraySurface = null;
		foreach (WorldSpraySurface item in list)
		{
			Player.GetClosestPlayer(item.CenterPoint, out var distance);
			if (distance > _minimumDistanceFromPlayers)
			{
				_validSpraySurface = item;
			}
		}
	}

	public override void Activate(EMapRegion region)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.Activate(region);
		if (!InstanceFinder.IsServer)
		{
			return;
		}
		SetSpraySurface(region, overrideExisting: false);
		if ((Object)(object)_validSpraySurface == (Object)null)
		{
			Debug.LogError((object)$"GraffitiRegion: No valid spray surface found in region {region} on activity activation!");
			return;
		}
		CartelGoon cartelGoon = NetworkSingleton<Cartel>.Instance.GoonPool.SpawnGoon(_validSpraySurface.NPCStandPoint.position);
		Debug.Log((object)$"GraffitiRegion: acitvity activated in region {region} using spray surface {((Object)_validSpraySurface).name} by goon {((Object)cartelGoon).name}");
		GraffitiBehaviour graffitiBehaviour = cartelGoon.Behaviour.GetBehaviour("Graffiti") as GraffitiBehaviour;
		if ((Object)(object)graffitiBehaviour == (Object)null)
		{
			Debug.LogError((object)("GraffitiRegion: Goon " + ((Object)cartelGoon).name + " does not have Graffiti behaviour!"));
			return;
		}
		graffitiBehaviour.SetSpraySurface_Client(null, ((NetworkBehaviour)_validSpraySurface).NetworkObject);
		graffitiBehaviour.Enable_Server();
		_validSpraySurface = null;
	}
}
