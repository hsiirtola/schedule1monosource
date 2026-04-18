using ScheduleOne.DevUtilities;
using ScheduleOne.PlayerScripts;
using UnityEngine;
using UnityEngine.UI;

namespace ScheduleOne.Growing;

public class GrowContainerMoistureDisplay : MonoBehaviour
{
	public const float MaxCameraDistance = 2.5f;

	public const float MinCameraDistance = 0.5f;

	public const float FadeInDistance = 0.7f;

	public const float FadeOutDistance = 0.25f;

	public bool SnapToRightAngles;

	[Header("References")]
	public GrowContainer GrowContainer;

	public Transform WaterCanvasContainer;

	public Canvas WaterLevelCanvas;

	public CanvasGroup WaterLevelCanvasGroup;

	public Slider WaterLevelSlider;

	public GameObject NoWaterIcon;

	protected virtual void Awake()
	{
		((Component)WaterLevelCanvas).gameObject.SetActive(false);
	}

	private void LateUpdate()
	{
		UpdateCanvas();
	}

	private void UpdateCanvas()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Player.Local == (Object)null)
		{
			return;
		}
		if ((Object)(object)Player.Local.CurrentProperty != (Object)(object)GrowContainer.ParentProperty)
		{
			((Component)WaterLevelCanvas).gameObject.SetActive(false);
			return;
		}
		if (!GrowContainer.IsFullyFilledWithSoil)
		{
			((Component)WaterLevelCanvas).gameObject.SetActive(false);
			return;
		}
		float num = Vector3.Distance(((Component)WaterLevelCanvas).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position);
		if (num > 2.5f)
		{
			((Component)WaterLevelCanvas).gameObject.SetActive(false);
			return;
		}
		UpdateCanvasContents();
		Vector3 val = Vector3.ProjectOnPlane(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - WaterCanvasContainer.position, Vector3.up);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		WaterCanvasContainer.forward = normalized;
		if (SnapToRightAngles)
		{
			Quaternion localRotation = WaterCanvasContainer.localRotation;
			Vector3 eulerAngles = ((Quaternion)(ref localRotation)).eulerAngles;
			eulerAngles.y = Mathf.Round(eulerAngles.y / 90f) * 90f;
			WaterCanvasContainer.localRotation = Quaternion.Euler(eulerAngles);
		}
		Transform transform = ((Component)WaterLevelCanvas).transform;
		val = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - ((Component)WaterLevelCanvas).transform.position;
		transform.rotation = Quaternion.LookRotation(((Vector3)(ref val)).normalized, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.up);
		float num2 = 1f - Mathf.Clamp01(Mathf.InverseLerp(1.8f, 2.5f, num));
		float num3 = Mathf.Clamp01(Mathf.InverseLerp(0.5f, 0.75f, num));
		WaterLevelCanvasGroup.alpha = Mathf.Min(num2, num3);
		((Component)WaterLevelCanvas).gameObject.SetActive(true);
	}

	protected virtual void UpdateCanvasContents()
	{
		WaterLevelSlider.value = GrowContainer.NormalizedMoistureAmount;
		NoWaterIcon.gameObject.SetActive(GrowContainer.NormalizedMoistureAmount <= 0f);
	}
}
