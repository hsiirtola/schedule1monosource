using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Funly.SkyStudio;

public abstract class BaseSpriteInstancedRenderer : MonoBehaviour
{
	public const int kArrayMaxSprites = 1000;

	[Tooltip("Mesh used to render the instances onto. If empty, a quad will be used.")]
	public Mesh modelMesh;

	[Tooltip("Sky Studio sprite sheet animated shader material.")]
	public Material renderMaterial;

	protected Queue<BaseSpriteItemData> m_Available = new Queue<BaseSpriteItemData>();

	protected HashSet<BaseSpriteItemData> m_Active = new HashSet<BaseSpriteItemData>();

	private MaterialPropertyBlock m_PropertyBlock;

	private Matrix4x4[] m_ModelMatrices = (Matrix4x4[])(object)new Matrix4x4[1000];

	private float[] m_StartTimes = new float[1000];

	private float[] m_EndTimes = new float[1000];

	protected SpriteSheetData m_SpriteSheetLayout = new SpriteSheetData();

	protected Texture m_SpriteTexture;

	protected Color m_TintColor = Color.white;

	protected Mesh m_DefaltModelMesh;

	public int maxSprites { get; protected set; }

	protected Camera m_ViewerCamera { get; set; }

	private void Start()
	{
		if (!SystemInfo.supportsInstancing)
		{
			Debug.LogError((object)"Can't render since GPU instancing isn't supported on this device");
			((Behaviour)this).enabled = false;
		}
		else
		{
			m_ViewerCamera = Camera.main;
		}
	}

	protected abstract Bounds CalculateMeshBounds();

	protected abstract BaseSpriteItemData CreateSpriteItemData();

	protected abstract bool IsRenderingEnabled();

	protected abstract int GetNextSpawnCount();

	protected abstract void CalculateSpriteTRS(BaseSpriteItemData data, out Vector3 spritePosition, out Quaternion spriteRotation, out Vector3 spriteScale);

	protected abstract void ConfigureSpriteItemData(BaseSpriteItemData data);

	protected abstract void PrepareDataArraysForRendering(int instanceId, BaseSpriteItemData data);

	protected abstract void PopulatePropertyBlockForRendering(ref MaterialPropertyBlock propertyBlock);

	private BaseSpriteItemData DequeueNextSpriteItemData()
	{
		BaseSpriteItemData baseSpriteItemData = null;
		baseSpriteItemData = ((m_Available.Count != 0) ? m_Available.Dequeue() : CreateSpriteItemData());
		m_Active.Add(baseSpriteItemData);
		return baseSpriteItemData;
	}

	private void ReturnSpriteItemData(BaseSpriteItemData splash)
	{
		splash.Reset();
		m_Active.Remove(splash);
		m_Available.Enqueue(splash);
	}

	protected virtual void LateUpdate()
	{
		m_ViewerCamera = Camera.main;
		if (IsRenderingEnabled())
		{
			GenerateNewSprites();
			AdvanceAllSprites();
			RenderAllSprites();
		}
	}

	private void GenerateNewSprites()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		int nextSpawnCount = GetNextSpawnCount();
		for (int i = 0; i < nextSpawnCount; i++)
		{
			BaseSpriteItemData baseSpriteItemData = DequeueNextSpriteItemData();
			baseSpriteItemData.spriteSheetData = m_SpriteSheetLayout;
			ConfigureSpriteItemData(baseSpriteItemData);
			CalculateSpriteTRS(baseSpriteItemData, out var spritePosition, out var spriteRotation, out var spriteScale);
			baseSpriteItemData.SetTRSMatrix(spritePosition, spriteRotation, spriteScale);
			baseSpriteItemData.Start();
		}
	}

	private void AdvanceAllSprites()
	{
		foreach (BaseSpriteItemData item in new HashSet<BaseSpriteItemData>(m_Active))
		{
			item.Continue();
			if (item.state == BaseSpriteItemData.SpriteState.Complete)
			{
				ReturnSpriteItemData(item);
			}
		}
	}

	private void RenderAllSprites()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (m_Active.Count == 0)
		{
			return;
		}
		if ((Object)(object)renderMaterial == (Object)null)
		{
			Debug.LogError((object)"Can't render sprite without a material.");
			return;
		}
		if (m_PropertyBlock == null)
		{
			m_PropertyBlock = new MaterialPropertyBlock();
		}
		int num = 0;
		foreach (BaseSpriteItemData item in m_Active)
		{
			if (num >= 1000)
			{
				Debug.LogError((object)"Can't render any more sprites...");
				break;
			}
			if (item.state == BaseSpriteItemData.SpriteState.Animating && !(item.startTime > Time.time))
			{
				m_ModelMatrices[num] = item.modelMatrix;
				m_StartTimes[num] = item.startTime;
				m_EndTimes[num] = item.endTime;
				PrepareDataArraysForRendering(num, item);
				num++;
			}
		}
		if (num != 0)
		{
			m_PropertyBlock.Clear();
			m_PropertyBlock.SetFloatArray("_StartTime", m_StartTimes);
			m_PropertyBlock.SetFloatArray("_EndTime", m_EndTimes);
			m_PropertyBlock.SetFloat("_SpriteColumnCount", (float)m_SpriteSheetLayout.columns);
			m_PropertyBlock.SetFloat("_SpriteRowCount", (float)m_SpriteSheetLayout.rows);
			m_PropertyBlock.SetFloat("_SpriteItemCount", (float)m_SpriteSheetLayout.frameCount);
			m_PropertyBlock.SetFloat("_AnimationSpeed", (float)m_SpriteSheetLayout.frameRate);
			m_PropertyBlock.SetVector("_TintColor", Color.op_Implicit(m_TintColor));
			PopulatePropertyBlockForRendering(ref m_PropertyBlock);
			Mesh mesh = GetMesh();
			mesh.bounds = CalculateMeshBounds();
			Graphics.DrawMeshInstanced(mesh, 0, renderMaterial, m_ModelMatrices, num, m_PropertyBlock, (ShadowCastingMode)0, false, LayerMask.NameToLayer("TransparentFX"));
		}
	}

	protected Mesh GetMesh()
	{
		if (Object.op_Implicit((Object)(object)modelMesh))
		{
			return modelMesh;
		}
		if (Object.op_Implicit((Object)(object)m_DefaltModelMesh))
		{
			return m_DefaltModelMesh;
		}
		m_DefaltModelMesh = GenerateMesh();
		return m_DefaltModelMesh;
	}

	protected virtual Mesh GenerateMesh()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		Mesh val = new Mesh();
		Vector3[] vertices = (Vector3[])(object)new Vector3[4]
		{
			new Vector3(-1f, -1f, 0f),
			new Vector3(-1f, 1f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(1f, -1f, 0f)
		};
		Vector2[] uv = (Vector2[])(object)new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};
		int[] triangles = new int[6] { 0, 1, 2, 0, 2, 3 };
		val.vertices = vertices;
		val.uv = uv;
		val.triangles = triangles;
		val.bounds = new Bounds(Vector3.zero, new Vector3(500f, 500f, 500f));
		return val;
	}
}
