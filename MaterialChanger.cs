using UnityEngine;

[ExecuteAlways]
public class MaterialChanger : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 5f)]
	private float _value = 1f;

	[SerializeField]
	private string _changeMaterialSetting = "_Worn_Level";

	private Renderer[] _renderers;

	private MaterialPropertyBlock _propBlock;

	private void OnEnable()
	{
		FindAllMaterialInChild();
	}

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		_propBlock = new MaterialPropertyBlock();
		SetNewValueForAllMaterial(_value);
	}

	private void FindAllMaterialInChild()
	{
		_renderers = ((Component)((Component)this).transform).GetComponentsInChildren<Renderer>();
	}

	private void SetNewValueForAllMaterial(float value)
	{
		FindAllMaterialInChild();
		for (int i = 0; i < _renderers.Length; i++)
		{
			_renderers[i].GetPropertyBlock(_propBlock);
			_propBlock.SetFloat(_changeMaterialSetting, value);
			_renderers[i].SetPropertyBlock(_propBlock);
		}
	}
}
