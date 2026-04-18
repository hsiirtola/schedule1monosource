using System;
using System.Collections.Generic;

namespace UnityEngine.PostProcessing;

public sealed class MaterialFactory : IDisposable
{
	private Dictionary<string, Material> m_Materials;

	public MaterialFactory()
	{
		m_Materials = new Dictionary<string, Material>();
	}

	public Material Get(string shaderName)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		if (!m_Materials.TryGetValue(shaderName, out var value))
		{
			Shader obj = Shader.Find(shaderName);
			if ((Object)(object)obj == (Object)null)
			{
				throw new ArgumentException($"Shader not found ({shaderName})");
			}
			value = new Material(obj)
			{
				name = string.Format("PostFX - {0}", shaderName.Substring(shaderName.LastIndexOf("/") + 1)),
				hideFlags = (HideFlags)52
			};
			m_Materials.Add(shaderName, value);
		}
		return value;
	}

	public void Dispose()
	{
		Dictionary<string, Material>.Enumerator enumerator = m_Materials.GetEnumerator();
		while (enumerator.MoveNext())
		{
			GraphicsUtils.Destroy((Object)(object)enumerator.Current.Value);
		}
		m_Materials.Clear();
	}
}
