using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using ScheduleOne.UI;
using UnityEngine;

namespace ScheduleOne.Vehicles.Modification;

public class VehicleModStation : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	protected Transform vehiclePosition;

	[SerializeField]
	protected OrbitCamera orbitCam;

	public LandVehicle currentVehicle { get; protected set; }

	public bool isOpen => (Object)(object)currentVehicle != (Object)null;

	public void Open(LandVehicle vehicle)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		orbitCam.Enable();
		PlayerSingleton<PlayerCamera>.Instance.SetCanLook(c: false);
		PlayerSingleton<PlayerCamera>.Instance.FreeMouse();
		PlayerSingleton<PlayerMovement>.Instance.CanMove = false;
		PlayerSingleton<PlayerInventory>.Instance.SetInventoryEnabled(enabled: false);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		currentVehicle = vehicle;
		((Component)vehicle).transform.rotation = vehiclePosition.rotation;
		((Component)vehicle).transform.position = vehiclePosition.position;
		Transform transform = ((Component)vehicle).transform;
		transform.position -= ((Component)vehicle).transform.InverseTransformPoint(((Component)vehicle.boundingBox).transform.position);
		Transform transform2 = ((Component)vehicle).transform;
		transform2.position += Vector3.up * ((Component)vehicle.boundingBox).transform.localScale.y * 0.5f;
		Singleton<VehicleModMenu>.Instance.Open(currentVehicle);
	}

	protected virtual void Update()
	{
		if (isOpen && GameInput.GetButtonDown(GameInput.ButtonCode.Escape))
		{
			Close();
		}
	}

	public void Close()
	{
		PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
		orbitCam.Disable();
		Singleton<VehicleModMenu>.Instance.Close();
		currentVehicle = null;
	}
}
