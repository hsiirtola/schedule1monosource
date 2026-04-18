using UnityEngine;

namespace ScheduleOne.AvatarFramework;

public class Accessory : MonoBehaviour
{
	[Header("Settings")]
	public string Name;

	public string AssetPath;

	public bool ReduceFootSize;

	[Range(0f, 1f)]
	public float FootSizeReduction = 1f;

	public bool ShouldBlockHair;

	public bool ColorAllMeshes = true;

	[Header("References")]
	public MeshRenderer[] meshesToColor;

	public SkinnedMeshRenderer[] skinnedMeshesToColor;

	public SkinnedMeshRenderer[] skinnedMeshesToBind;

	public SkinnedMeshRenderer[] shapeKeyMeshRends;

	private void Awake()
	{
		for (int i = 0; i < skinnedMeshesToBind.Length; i++)
		{
			skinnedMeshesToBind[i].updateWhenOffscreen = true;
		}
	}

	public void ApplyColor(Color col)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		MeshRenderer[] array = meshesToColor;
		foreach (MeshRenderer val in array)
		{
			for (int j = 0; j < ((Renderer)val).materials.Length; j++)
			{
				((Renderer)val).materials[j].color = col;
				if (!ColorAllMeshes)
				{
					break;
				}
			}
		}
		SkinnedMeshRenderer[] array2 = skinnedMeshesToColor;
		foreach (SkinnedMeshRenderer val2 in array2)
		{
			for (int k = 0; k < ((Renderer)val2).materials.Length; k++)
			{
				((Renderer)val2).materials[k].color = col;
				if (!ColorAllMeshes)
				{
					break;
				}
			}
		}
	}

	public void ApplyShapeKeys(float gender, float weight)
	{
		SkinnedMeshRenderer[] array = shapeKeyMeshRends;
		foreach (SkinnedMeshRenderer val in array)
		{
			if (val.sharedMesh.blendShapeCount >= 2)
			{
				val.SetBlendShapeWeight(0, gender);
				val.SetBlendShapeWeight(1, weight);
			}
		}
	}

	public void BindBones(Transform[] bones)
	{
		SkinnedMeshRenderer[] array = skinnedMeshesToBind;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].bones = bones;
		}
	}
}
