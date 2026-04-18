using ScheduleOne.Core;
using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Growing;

public class GrowContainerCameraHandler : MonoBehaviour
{
	public enum ECameraPosition
	{
		Closeup,
		Midshot,
		Fullshot,
		BirdsEye
	}

	[SerializeField]
	private bool RotateCameraContainerToFacePlayer = true;

	[SerializeField]
	private bool SnapRotationToRightAngles;

	[SerializeField]
	private Transform _midshotCamera;

	[SerializeField]
	private Transform _closeupCamera;

	[SerializeField]
	private Transform _fullshotContainer;

	[SerializeField]
	private Transform _birdsEyeCamera;

	[Header("Debug & Development")]
	[SerializeField]
	private ECameraPosition _debugCameraPosition = ECameraPosition.Midshot;

	public void PositionCameraContainer()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (RotateCameraContainerToFacePlayer)
		{
			Vector3 val = Vector3.ProjectOnPlane(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward, Vector3.up);
			Vector3 normalized = ((Vector3)(ref val)).normalized;
			((Component)this).transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
			if (SnapRotationToRightAngles)
			{
				Quaternion localRotation = ((Component)this).transform.localRotation;
				Vector3 eulerAngles = ((Quaternion)(ref localRotation)).eulerAngles;
				eulerAngles.y = Mathf.Round(eulerAngles.y / 90f) * 90f;
				((Component)this).transform.localRotation = Quaternion.Euler(eulerAngles);
			}
		}
	}

	public Transform GetCameraPosition(ECameraPosition pos, bool autoPosition = true)
	{
		if (autoPosition)
		{
			PositionCameraContainer();
		}
		return (Transform)(pos switch
		{
			ECameraPosition.Closeup => _closeupCamera, 
			ECameraPosition.Midshot => _midshotCamera, 
			ECameraPosition.Fullshot => _fullshotContainer, 
			ECameraPosition.BirdsEye => _birdsEyeCamera, 
			_ => null, 
		});
	}

	[Button]
	private void SetCameraPosition()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Transform cameraPosition = GetCameraPosition(_debugCameraPosition);
		PlayerSingleton<PlayerCamera>.Instance.OverrideTransform(cameraPosition.position, cameraPosition.rotation, 0.25f);
	}
}
