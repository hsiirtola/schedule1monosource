using UnityEngine;

namespace ScheduleOne.Tools;

public class CopyTransform : MonoBehaviour
{
	public enum EUpdateMode
	{
		Update,
		LateUpdate,
		FixedUpdate
	}

	public Transform Target;

	public EUpdateMode UpdateMode;

	public bool CopyPosition = true;

	public bool CopyRotation = true;

	public bool CopyScale;

	public Vector3 GlobalPositionOffset;

	public Vector3 LocalPositionOffset;

	public Vector3 RotationOffset;

	private void FixedUpdate()
	{
		if (UpdateMode == EUpdateMode.FixedUpdate)
		{
			Copy();
		}
	}

	private void Update()
	{
		if (UpdateMode == EUpdateMode.Update)
		{
			Copy();
		}
	}

	private void LateUpdate()
	{
		if (UpdateMode == EUpdateMode.LateUpdate)
		{
			Copy();
		}
	}

	private void Copy()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)Target == (Object)null))
		{
			if (CopyPosition)
			{
				((Component)this).transform.position = Target.position + GlobalPositionOffset + Target.TransformVector(LocalPositionOffset);
			}
			if (CopyRotation)
			{
				((Component)this).transform.rotation = Target.rotation * Quaternion.Euler(RotationOffset);
			}
			if (CopyScale)
			{
				((Component)this).transform.localScale = Target.localScale;
			}
		}
	}
}
