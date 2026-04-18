using System;
using System.Collections.Generic;

namespace UnityEngine.PostProcessing;

public sealed class RenderTextureFactory : IDisposable
{
	private HashSet<RenderTexture> m_TemporaryRTs;

	public RenderTextureFactory()
	{
		m_TemporaryRTs = new HashSet<RenderTexture>();
	}

	public RenderTexture Get(RenderTexture baseRenderTexture)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return Get(((Texture)baseRenderTexture).width, ((Texture)baseRenderTexture).height, baseRenderTexture.depth, baseRenderTexture.format, (RenderTextureReadWrite)((!baseRenderTexture.sRGB) ? 1 : 2), ((Texture)baseRenderTexture).filterMode, ((Texture)baseRenderTexture).wrapMode);
	}

	public RenderTexture Get(int width, int height, int depthBuffer = 0, RenderTextureFormat format = (RenderTextureFormat)2, RenderTextureReadWrite rw = (RenderTextureReadWrite)0, FilterMode filterMode = (FilterMode)1, TextureWrapMode wrapMode = (TextureWrapMode)1, string name = "FactoryTempTexture")
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		RenderTexture temporary = RenderTexture.GetTemporary(width, height, depthBuffer, format, rw);
		((Texture)temporary).filterMode = filterMode;
		((Texture)temporary).wrapMode = wrapMode;
		((Object)temporary).name = name;
		m_TemporaryRTs.Add(temporary);
		return temporary;
	}

	public void Release(RenderTexture rt)
	{
		if (!((Object)(object)rt == (Object)null))
		{
			if (!m_TemporaryRTs.Contains(rt))
			{
				throw new ArgumentException($"Attempting to remove a RenderTexture that was not allocated: {rt}");
			}
			m_TemporaryRTs.Remove(rt);
			RenderTexture.ReleaseTemporary(rt);
		}
	}

	public void ReleaseAll()
	{
		HashSet<RenderTexture>.Enumerator enumerator = m_TemporaryRTs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			RenderTexture.ReleaseTemporary(enumerator.Current);
		}
		m_TemporaryRTs.Clear();
	}

	public void Dispose()
	{
		ReleaseAll();
	}
}
