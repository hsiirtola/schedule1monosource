using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.PlayerScripts;
using UnityEngine;

namespace ScheduleOne.Vision;

[RequireComponent(typeof(Light))]
public class LightVisibilityAffector : MonoBehaviour
{
	public const float PointLightEffect = 15f;

	public const float SpotLightEffect = 10f;

	[Header("Settings")]
	public float EffectMultiplier = 1f;

	public string uniquenessCode = "Light";

	[Tooltip("How far does the player have to move for visibility to be recalculated?")]
	public int updateDistanceThreshold = 1;

	protected Light light;

	protected VisibilityAttribute attribute;

	protected virtual void Awake()
	{
		light = ((Component)this).GetComponent<Light>();
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
	}

	private void PlayerSpawned()
	{
		Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
		PlayerSingleton<PlayerMovement>.Instance.RegisterMovementEvent(updateDistanceThreshold, UpdateVisibility);
	}

	private void OnDestroy()
	{
		if ((Object)(object)PlayerSingleton<PlayerMovement>.Instance != (Object)null)
		{
			PlayerSingleton<PlayerMovement>.Instance.DeregisterMovementEvent(UpdateVisibility);
		}
		ClearAttribute();
	}

	protected virtual void UpdateVisibility()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)light == (Object)null || (Object)(object)((Component)this).gameObject == (Object)null)
		{
			return;
		}
		if (!((Behaviour)light).enabled || !((Component)this).gameObject.activeInHierarchy)
		{
			ClearAttribute();
		}
		else
		{
			if ((Object)(object)Player.Local == (Object)null)
			{
				return;
			}
			float num = Player.Local.Visibility.CalculateExposureToPoint(((Component)this).transform.position, light.range);
			if (num == 0f)
			{
				ClearAttribute();
				return;
			}
			float num2 = Mathf.Pow(1f - Mathf.Clamp(Vector3.Distance(((Component)this).transform.position, Player.Local.Avatar.CenterPoint) / light.range, 0f, 1f), 2f);
			float num3 = 1f - Singleton<EnvironmentFX>.Instance.normalizedEnvironmentalBrightness;
			float num4 = 1f;
			if ((int)light.type == 0)
			{
				Vector3 forward = ((Component)this).transform.forward;
				Vector3 val = Player.Local.Avatar.CenterPoint - ((Component)this).transform.position;
				float num5 = Vector3.Angle(forward, ((Vector3)(ref val)).normalized);
				if (num5 > light.spotAngle * 0.5f)
				{
					num4 = 0f;
				}
				else
				{
					float num6 = light.spotAngle * 0.5f - num5;
					float num7 = light.spotAngle * 0.5f - light.innerSpotAngle * 0.5f;
					num4 = Mathf.Clamp(num6 / num7, 0f, 1f);
				}
			}
			float visibity = num * num2 * light.intensity * num3 * num4 * (((int)light.type == 0) ? 10f : 15f) * EffectMultiplier;
			UpdateAttribute(visibity);
		}
	}

	private void UpdateAttribute(float visibity)
	{
		if (visibity <= 0f)
		{
			ClearAttribute();
		}
		else if (attribute == null)
		{
			if (uniquenessCode != string.Empty)
			{
				attribute = new UniqueVisibilityAttribute("Light Exposure (" + ((Object)((Component)this).gameObject).name + ")", visibity, uniquenessCode);
			}
			else
			{
				attribute = new VisibilityAttribute("Light Exposure (" + ((Object)((Component)this).gameObject).name + ")", visibity);
			}
		}
		else
		{
			attribute.pointsChange = visibity;
		}
	}

	private void ClearAttribute()
	{
		if (attribute != null)
		{
			attribute.Delete();
			attribute = null;
		}
	}
}
