using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using ScheduleOne.DevUtilities;
using ScheduleOne.Levelling;
using ScheduleOne.Networking;
using ScheduleOne.Persistence;
using ScheduleOne.Persistence.Datas;
using ScheduleOne.Persistence.Loaders;
using ScheduleOne.Variables;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Graffiti;

public class GraffitiManager : NetworkSingleton<GraffitiManager>, IBaseSaveable, ISaveable
{
	private const string SPRAY_PAINT_STOCK_VARIABLE = "SprayPaintStock";

	private const string SPRAY_PAINTS_PURCHASED_VARIABLE = "SprayPaintsPurchased";

	[SerializeField]
	private AnimationCurve _falloffCurve;

	private Dictionary<byte, float[]> _falloffTableCache = new Dictionary<byte, float[]>();

	private GraffitiLoader loader = new GraffitiLoader();

	private bool NetworkInitialize___EarlyScheduleOne_002EGraffiti_002EGraffitiManagerAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002EGraffiti_002EGraffitiManagerAssembly_002DCSharp_002Edll_Excuted;

	public List<WorldSpraySurface> WorldSpraySurfaces { get; private set; } = new List<WorldSpraySurface>();

	public string SaveFolderName => "Graffiti";

	public string SaveFileName => "Graffiti";

	public Loader Loader => loader;

	public bool ShouldSaveUnderFolder => false;

	public List<string> LocalExtraFiles { get; set; } = new List<string>();

	public List<string> LocalExtraFolders { get; set; } = new List<string>();

	public bool HasChanged { get; set; } = true;

	public int LoadOrder { get; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002EGraffiti_002EGraffitiManager_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		(NetworkSingleton<VariableDatabase>.Instance.GetVariable("SprayPaintsPurchased") as NumberVariable).OnValueChanged.AddListener((UnityAction<float>)SprayPaintPurchaseCountChanged);
		LevelManager levelManager = NetworkSingleton<LevelManager>.Instance;
		levelManager.onRankUp = (Action<FullRank, FullRank>)Delegate.Combine(levelManager.onRankUp, new Action<FullRank, FullRank>(RankChange));
		LevelManager levelManager2 = NetworkSingleton<LevelManager>.Instance;
		levelManager2.onRankChanged = (Action<FullRank, FullRank>)Delegate.Combine(levelManager2.onRankChanged, new Action<FullRank, FullRank>(RankChange));
		UpdateSprayPaintStockVariable();
	}

	public virtual void InitializeSaveable()
	{
		Singleton<SaveManager>.Instance.RegisterSaveable(this);
	}

	private void SprayPaintPurchaseCountChanged(float newValue)
	{
		UpdateSprayPaintStockVariable();
	}

	private void RankChange(FullRank oldRank, FullRank newRank)
	{
		UpdateSprayPaintStockVariable();
	}

	private void UpdateSprayPaintStockVariable()
	{
		if (InstanceFinder.IsServer)
		{
			int num = NetworkSingleton<LevelManager>.Instance.GetFullRank().GetRankIndex() + 1;
			int num2 = (int)(NetworkSingleton<VariableDatabase>.Instance.GetVariable("SprayPaintsPurchased") as NumberVariable).Value;
			int num3 = Mathf.Max(0, num - num2);
			NetworkSingleton<VariableDatabase>.Instance.SetVariableValue("SprayPaintStock", ((float)num3).ToString());
		}
	}

	public virtual string GetSaveString()
	{
		List<WorldSpraySurfaceData> list = new List<WorldSpraySurfaceData>();
		foreach (WorldSpraySurface worldSpraySurface in WorldSpraySurfaces)
		{
			if ((Object)(object)worldSpraySurface == (Object)null)
			{
				continue;
			}
			try
			{
				if (worldSpraySurface.ShouldSave())
				{
					list.Add(worldSpraySurface.GetSaveData());
				}
			}
			catch (Exception ex)
			{
				Console.LogError("Save error for spray surface " + worldSpraySurface.GUID.ToString() + ": " + ex.Message);
			}
		}
		return new GraffitiData(list).GetJson(prettyPrint: false);
	}

	public void QueueSurfaceToReplicate(SpraySurface surface, NetworkConnection conn)
	{
		ReplicationQueue.Enqueue(((object)this).GetType().Name, conn, ReplicateSpraySurface, 100 + surface.DrawingStrokeCount * 25);
		void ReplicateSpraySurface(NetworkConnection conn2)
		{
			surface.ReplicateTo(conn2);
		}
	}

	public float GetPixelStrength(byte strokeSize, int pixelIndex)
	{
		if (!_falloffTableCache.ContainsKey(strokeSize))
		{
			_falloffTableCache.Add(strokeSize, GetFalloffTable(strokeSize));
		}
		return _falloffTableCache[strokeSize][pixelIndex];
	}

	private float[] GetFalloffTable(int strokeSize)
	{
		int num = Mathf.FloorToInt((float)strokeSize / 2f);
		int num2 = Mathf.CeilToInt((float)strokeSize / 2f);
		float[] array = new float[strokeSize * strokeSize];
		int num3 = 0;
		for (int i = -num; i < num2; i++)
		{
			for (int j = -num; j < num2; j++)
			{
				float num4 = Mathf.Sqrt(Mathf.Pow((float)i, 2f) + Mathf.Pow((float)j, 2f)) / (float)num2;
				float num5 = _falloffCurve.Evaluate(num4);
				if (num5 < 0.01f)
				{
					num5 = 0f;
				}
				array[num3] = num5 * 0.9999f;
				num3++;
			}
		}
		return array;
	}

	public override void NetworkInitialize___Early()
	{
		if (!NetworkInitialize___EarlyScheduleOne_002EGraffiti_002EGraffitiManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002EGraffiti_002EGraffitiManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002EGraffiti_002EGraffitiManagerAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002EGraffiti_002EGraffitiManagerAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	protected override void Awake_UserLogic_ScheduleOne_002EGraffiti_002EGraffitiManager_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		InitializeSaveable();
		byte[] strokeSizePresets = SprayStroke.StrokeSizePresets;
		foreach (byte strokeSize in strokeSizePresets)
		{
			GetFalloffTable(strokeSize);
		}
	}
}
