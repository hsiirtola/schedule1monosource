namespace UnityEngine.PostProcessing;

public static class GraphicsUtils
{
	private static Texture2D s_WhiteTexture;

	private static Mesh s_Quad;

	public static bool isLinearColorSpace => (int)QualitySettings.activeColorSpace == 1;

	public static bool supportsDX11
	{
		get
		{
			if (SystemInfo.graphicsShaderLevel >= 50)
			{
				return SystemInfo.supportsComputeShaders;
			}
			return false;
		}
	}

	public static Texture2D whiteTexture
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)s_WhiteTexture != (Object)null)
			{
				return s_WhiteTexture;
			}
			s_WhiteTexture = new Texture2D(1, 1, (TextureFormat)5, false);
			s_WhiteTexture.SetPixel(0, 0, new Color(1f, 1f, 1f, 1f));
			s_WhiteTexture.Apply();
			return s_WhiteTexture;
		}
	}

	public static Mesh quad
	{
		get
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Expected O, but got Unknown
			if ((Object)(object)s_Quad != (Object)null)
			{
				return s_Quad;
			}
			Vector3[] vertices = (Vector3[])(object)new Vector3[4]
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(1f, 1f, 0f),
				new Vector3(1f, -1f, 0f),
				new Vector3(-1f, 1f, 0f)
			};
			Vector2[] uv = (Vector2[])(object)new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f),
				new Vector2(0f, 1f)
			};
			int[] triangles = new int[6] { 0, 1, 2, 1, 0, 3 };
			s_Quad = new Mesh
			{
				vertices = vertices,
				uv = uv,
				triangles = triangles
			};
			s_Quad.RecalculateNormals();
			s_Quad.RecalculateBounds();
			return s_Quad;
		}
	}

	public static void Blit(Material material, int pass)
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		material.SetPass(pass);
		GL.Begin(5);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.End();
		GL.PopMatrix();
	}

	public static void ClearAndBlit(Texture source, RenderTexture destination, Material material, int pass, bool clearColor = true, bool clearDepth = false)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = destination;
		GL.Clear(false, clearColor, Color.clear);
		GL.PushMatrix();
		GL.LoadOrtho();
		material.SetTexture("_MainTex", source);
		material.SetPass(pass);
		GL.Begin(5);
		GL.TexCoord2(0f, 0f);
		GL.Vertex3(0f, 0f, 0.1f);
		GL.TexCoord2(1f, 0f);
		GL.Vertex3(1f, 0f, 0.1f);
		GL.TexCoord2(0f, 1f);
		GL.Vertex3(0f, 1f, 0.1f);
		GL.TexCoord2(1f, 1f);
		GL.Vertex3(1f, 1f, 0.1f);
		GL.End();
		GL.PopMatrix();
		RenderTexture.active = active;
	}

	public static void Destroy(Object obj)
	{
		if (obj != (Object)null)
		{
			Object.Destroy(obj);
		}
	}

	public static void Dispose()
	{
		Destroy((Object)(object)s_Quad);
	}
}
