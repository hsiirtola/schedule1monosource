using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Funly.SkyStudio;

public class NearbyStarRenderer : BaseStarDataRenderer
{
	private const int kMaxStars = 2000;

	private const int kStarPointTextureWidth = 2048;

	private const float kStarPaddingRadiusMultipler = 2.1f;

	private RenderTexture CreateRenderTexture(string name, int renderTextureSize, RenderTextureFormat format)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		RenderTexture temporary = RenderTexture.GetTemporary(renderTextureSize, renderTextureSize, 0, format, (RenderTextureReadWrite)1);
		((Texture)temporary).filterMode = (FilterMode)0;
		((Texture)temporary).wrapMode = (TextureWrapMode)1;
		((Object)temporary).name = name;
		return temporary;
	}

	private Material GetNearbyStarMaterial(Vector4 randomSeed, int starCount)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		Material val = new Material(new Material(Shader.Find("Hidden/Funly/Sky Studio/Computation/StarCalcNearby")))
		{
			hideFlags = (HideFlags)61
		};
		val.SetFloat("_StarDensity", density);
		val.SetFloat("_NumStarPoints", (float)starCount);
		val.SetVector("_RandomSeed", randomSeed);
		val.SetFloat("_TextureSize", 2048f);
		return val;
	}

	private void WriteDebugTexture(RenderTexture rt, string path)
	{
		Texture2D val = ConvertToTexture2D(rt);
		File.WriteAllBytes(path, ImageConversion.EncodeToPNG(val));
	}

	private Texture2D GetStarListTexture(string starTexKey, out int validStarPixelCount)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(2048, 1, (TextureFormat)20, false, true);
		((Texture)val).filterMode = (FilterMode)0;
		int num = 0;
		float num2 = maxRadius * 2.1f;
		List<Vector4> list = new List<Vector4>();
		bool flag = maxRadius > 0.0015f;
		for (int i = 0; i < 2000; i++)
		{
			Vector3 onUnitSphere = Random.onUnitSphere;
			if (flag)
			{
				bool flag2 = false;
				for (int j = 0; j < list.Count; j++)
				{
					if (Vector3.Distance(onUnitSphere, Vector4.op_Implicit(list[j])) < num2)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					continue;
				}
			}
			list.Add(Vector4.op_Implicit(onUnitSphere));
			val.SetPixel(num, 0, new Color(onUnitSphere.x, onUnitSphere.y, onUnitSphere.z, 0f));
			num++;
		}
		val.Apply();
		validStarPixelCount = num;
		return val;
	}

	public override IEnumerator ComputeStarData()
	{
		SendProgress(0f);
		RenderTexture val = CreateRenderTexture("Nearby Star " + layerId, (int)imageSize, (RenderTextureFormat)0);
		RenderTexture active = RenderTexture.active;
		State state = Random.state;
		Random.InitState(layerId.GetHashCode());
		Vector4 randomSeed = default(Vector4);
		((Vector4)(ref randomSeed))._002Ector(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		int validStarPixelCount;
		Texture2D starListTexture = GetStarListTexture(layerId, out validStarPixelCount);
		int starCount = Math.Min(Mathf.FloorToInt(Mathf.Clamp01(density) * 2000f), validStarPixelCount);
		RenderTexture.active = val;
		Material nearbyStarMaterial = GetNearbyStarMaterial(randomSeed, starCount);
		Graphics.Blit((Texture)(object)starListTexture, nearbyStarMaterial);
		Texture2D texture = ConvertToTexture2D(val);
		RenderTexture.active = active;
		val.Release();
		Random.state = state;
		SendCompletion(texture, success: true);
		yield break;
	}

	private Texture2D ConvertToTexture2D(RenderTexture rt)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		Texture2D val = new Texture2D(((Texture)rt).width, ((Texture)rt).height, (TextureFormat)4, false)
		{
			name = layerId,
			filterMode = (FilterMode)0,
			wrapMode = (TextureWrapMode)1
		};
		val.ReadPixels(new Rect(0f, 0f, (float)((Texture)rt).width, (float)((Texture)rt).height), 0, 0, false);
		val.Apply(false);
		return val;
	}
}
