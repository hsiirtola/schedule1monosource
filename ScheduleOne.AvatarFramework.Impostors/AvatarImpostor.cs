using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.AvatarFramework.Impostors;

public class AvatarImpostor : MonoBehaviour
{
	public MeshRenderer meshRenderer;

	public Transform AnchorBone;

	private Transform cachedCamera;

	private Vector3 anchorBoneOffset = Vector3.zero;

	public bool HasTexture { get; private set; }

	private Transform Camera
	{
		get
		{
			if ((Object)(object)cachedCamera == (Object)null)
			{
				PlayerCamera instance = PlayerSingleton<PlayerCamera>.Instance;
				cachedCamera = ((instance != null) ? ((Component)instance).transform : null);
			}
			return cachedCamera;
		}
	}

	private void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		anchorBoneOffset = AnchorBone.InverseTransformPoint(((Component)this).transform.position);
	}

	public void SetAvatarSettings(AvatarSettings settings)
	{
		Texture2D impostorTexture = settings.ImpostorTexture;
		if ((Object)(object)impostorTexture != (Object)null)
		{
			((Renderer)meshRenderer).material.mainTexture = (Texture)(object)impostorTexture;
			HasTexture = true;
		}
		else
		{
			HasTexture = false;
		}
	}

	private void LateUpdate()
	{
		Realign();
	}

	private void Realign()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = AnchorBone.TransformPoint(anchorBoneOffset);
		if ((Object)(object)Camera != (Object)null)
		{
			Vector3 val = ((Component)this).transform.position - Camera.position;
			((Component)this).transform.rotation = Quaternion.LookRotation(val, AnchorBone.up);
		}
	}

	public void EnableImpostor()
	{
		((Component)this).gameObject.SetActive(true);
		Realign();
	}

	public void DisableImpostor()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
