using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using ScheduleOne.Audio;
using ScheduleOne.Combat;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.ItemFramework;
using ScheduleOne.Noise;
using ScheduleOne.NPCs;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Storage;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using ScheduleOne.Vision;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.Equipping;

public class Equippable_RangedWeapon : Equippable_AvatarViewmodel
{
	public enum EReloadType
	{
		Magazine,
		Incremental
	}

	public const float NPC_AIM_DETECTION_RANGE = 10f;

	public int MagazineSize = 7;

	[Header("Aim Settings")]
	public float AimDuration = 0.2f;

	public float MinAimFOVReduction = 5f;

	public float MaxAimFOVReduction = 10f;

	[Header("Firing")]
	public AudioSourceController FireSound;

	public AudioSourceController EmptySound;

	public float FireCooldown = 0.3f;

	public string[] FireAnimTriggers;

	public float AccuracyChangeDuration = 0.6f;

	public float AccuracyDropPerShot = 0.4f;

	[Header("Raycasting")]
	public float Range = 40f;

	public float RayRadius = 0.05f;

	[Header("Spread")]
	public float MinSpread = 5f;

	public float MaxSpread = 15f;

	[Header("Damage")]
	public float Damage = 60f;

	public float ImpactForce = 300f;

	public float HeadshotMultiplier = 1.75f;

	[Header("Reloading")]
	public bool CanReload = true;

	public EReloadType ReloadType;

	public StorableItemDefinition Magazine;

	public float ReloadStartTime = 1.5f;

	public float ReloadIndividalTime;

	public float ReloadEndTime;

	public string ReloadStartAnimTrigger = "MagazineReload";

	public string ReloadIndividualAnimTrigger = string.Empty;

	public string ReloadEndAnimTrigger = string.Empty;

	public TrashItem ReloadTrash;

	[Header("Cocking")]
	public bool MustBeCocked;

	public bool CockedByDefault;

	public bool AutoCockAfterReload;

	public float CockTime = 0.5f;

	public string CockAnimTrigger = "MagazineReload";

	[Header("Effects")]
	public float TracerSpeed = 50f;

	public UnityEvent onFire;

	public UnityEvent onReloadStart;

	public UnityEvent onReloadIndividual;

	public UnityEvent onReloadEnd;

	public UnityEvent onCockStart;

	protected IntegerItemInstance weaponItem;

	private bool aimStarted;

	private float aimVelocity;

	private Coroutine reloadRoutine;

	private bool shotQueued;

	private bool reloadQueued;

	private float timeSincePrimaryClick = 100f;

	private float timeSinceReloadStart;

	private float timeSinceAimStart;

	private bool interruptReload;

	public float Aim { get; private set; }

	public float Accuracy { get; private set; }

	public float TimeSinceFire { get; set; } = 1000f;

	public bool IsReloading { get; private set; }

	public bool IsCocked { get; private set; }

	public bool IsCocking { get; private set; }

	public int Ammo
	{
		get
		{
			if (weaponItem == null)
			{
				return 0;
			}
			return weaponItem.Value;
		}
	}

	private float fov => Singleton<Settings>.Instance.CameraFOV - (aimStarted ? Mathf.Lerp(MinAimFOVReduction, MaxAimFOVReduction, Mathf.Clamp01(timeSinceAimStart / AccuracyChangeDuration)) : 0f);

	public override void Equip(ItemInstance item)
	{
		base.Equip(item);
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		Singleton<InputPromptsCanvas>.Instance.LoadModule("gun");
		weaponItem = item as IntegerItemInstance;
		((MonoBehaviour)this).InvokeRepeating("CheckAimingAtNPC", 0f, 0.5f);
		if (CockedByDefault)
		{
			IsCocked = true;
		}
	}

	public override void Unequip()
	{
		base.Unequip();
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: true);
		Singleton<InputPromptsCanvas>.Instance.UnloadModule();
		if (aimStarted)
		{
			PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(AimDuration);
			PlayerSingleton<PlayerMovement>.Instance.RemoveSprintBlocker("Aiming");
			aimStarted = false;
		}
		Singleton<HUD>.Instance.HideFirearmReticle();
		if (reloadRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(reloadRoutine);
		}
	}

	protected override void Update()
	{
		base.Update();
		UpdateInput();
		UpdateAnim();
		Singleton<HUD>.Instance.SetCrosshairVisible(vis: false);
		TimeSinceFire += Time.deltaTime;
		if (IsReloading)
		{
			timeSinceReloadStart += Time.deltaTime;
		}
	}

	private void UpdateInput()
	{
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		if (Time.timeScale == 0f)
		{
			return;
		}
		if ((GameInput.GetButton(GameInput.ButtonCode.SecondaryClick) || timeSincePrimaryClick < 0.5f || IsCocking) && CanAim())
		{
			Aim = Mathf.SmoothDamp(Aim, 1f, ref aimVelocity, AimDuration / 2f);
			Accuracy = Mathf.MoveTowards(Accuracy, 1f, Time.deltaTime / AccuracyChangeDuration);
			timeSinceAimStart += Time.deltaTime;
			if (!aimStarted)
			{
				PlayerSingleton<PlayerMovement>.Instance.AddSprintBlocker("Aiming");
				aimStarted = true;
				timeSinceAimStart = 0f;
				Player.Local.SendEquippableMessage_Networked("Raise", Random.Range(int.MinValue, int.MaxValue));
			}
		}
		else
		{
			if (TimeSinceFire > FireCooldown)
			{
				Aim = Mathf.SmoothDamp(Aim, 0f, ref aimVelocity, AimDuration / 2f);
			}
			Accuracy = Mathf.MoveTowards(Accuracy, 0f, Time.deltaTime / AccuracyChangeDuration * 2f);
			if (aimStarted)
			{
				PlayerSingleton<PlayerCamera>.Instance.StopFOVOverride(AimDuration);
				PlayerSingleton<PlayerMovement>.Instance.RemoveSprintBlocker("Aiming");
				aimStarted = false;
				Player.Local.SendEquippableMessage_Networked("Lower", Random.Range(int.MinValue, int.MaxValue));
			}
		}
		Vector3 velocity = PlayerSingleton<PlayerMovement>.Instance.Controller.velocity;
		float num = Mathf.Clamp01(((Vector3)(ref velocity)).magnitude / 3.25f);
		float num2 = Mathf.Lerp(1f, 0f, num);
		if (Accuracy > num2)
		{
			Accuracy = Mathf.MoveTowards(Accuracy, num2, Time.deltaTime / AccuracyChangeDuration * 2f);
		}
		PlayerSingleton<PlayerCamera>.Instance.OverrideFOV(fov, AimDuration);
		if (Singleton<PauseMenu>.Instance.IsPaused)
		{
			return;
		}
		if (GameInput.GetButton(GameInput.ButtonCode.PrimaryClick))
		{
			timeSincePrimaryClick = 0f;
			if (IsReloading && ReloadType == EReloadType.Incremental && timeSinceReloadStart > ReloadStartTime + ReloadIndividalTime * 0.5f)
			{
				interruptReload = true;
			}
		}
		else
		{
			timeSincePrimaryClick += Time.deltaTime;
		}
		if (GameInput.GetButtonDown(GameInput.ButtonCode.PrimaryClick) || shotQueued)
		{
			if (CanFire(checkAmmo: false))
			{
				if (Ammo > 0)
				{
					if (!MustBeCocked || IsCocked)
					{
						Fire();
					}
					else
					{
						Cock();
					}
				}
				else if ((Object)(object)EmptySound != (Object)null)
				{
					EmptySound.Play();
					shotQueued = false;
					if (IsReloadReady(ignoreTiming: false))
					{
						Reload();
					}
				}
			}
			else if (TimeSinceFire < FireCooldown || IsCocking)
			{
				shotQueued = true;
			}
		}
		if (reloadQueued || GameInput.GetButtonDown(GameInput.ButtonCode.Reload))
		{
			if (IsReloadReady(ignoreTiming: false))
			{
				Reload();
			}
			else if (GameInput.GetButtonDown(GameInput.ButtonCode.Reload) && IsReloadReady(ignoreTiming: true) && TimeSinceFire > FireCooldown * 0.5f)
			{
				Console.Log("Reload qeueued");
				reloadQueued = true;
			}
		}
	}

	private void UpdateAnim()
	{
		Singleton<ViewmodelAvatar>.Instance.Animator.SetFloat("Aim", Aim);
		Singleton<HUD>.Instance.SetFirearmReticle(GetSpreadAngle());
		if (Aim > 0.5f)
		{
			Singleton<HUD>.Instance.ShowFirearmReticle();
		}
		else
		{
			Singleton<HUD>.Instance.HideFirearmReticle();
		}
	}

	private bool CanAim()
	{
		return true;
	}

	public virtual void Fire()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		IsCocked = false;
		shotQueued = false;
		TimeSinceFire = 0f;
		Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(FireAnimTriggers[Random.Range(0, FireAnimTriggers.Length)]);
		PlayerSingleton<PlayerCamera>.Instance.JoltCamera();
		FireSound.Play();
		weaponItem.ChangeValue(-1);
		Vector3[] bulletDirections = GetBulletDirections();
		Vector3 position = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position;
		position += ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward * 0.4f;
		position += ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.right * 0.1f;
		position += ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.up * -0.03f;
		NoiseUtility.EmitNoise(((Component)this).transform.position, ENoiseType.Gunshot, 25f, ((Component)Player.Local).gameObject);
		if ((Object)(object)Player.Local.CurrentProperty == (Object)null)
		{
			Player.Local.VisualState.ApplyState("shooting", EVisualState.DischargingWeapon, 4f);
		}
		Dictionary<IDamageable, List<RaycastHit>> dictionary = new Dictionary<IDamageable, List<RaycastHit>>();
		Vector3[] array = bulletDirections;
		foreach (Vector3 val in array)
		{
			Singleton<FXManager>.Instance.CreateBulletTrail(position, val, TracerSpeed, Range, NetworkSingleton<CombatManager>.Instance.RangedWeaponLayerMask);
			Vector3 data = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position + val * Range;
			RaycastHit[] array2 = Physics.SphereCastAll(position, RayRadius, val, Range, LayerMask.op_Implicit(NetworkSingleton<CombatManager>.Instance.RangedWeaponLayerMask));
			Array.Sort(array2, (RaycastHit a, RaycastHit b) => ((RaycastHit)(ref a)).distance.CompareTo(((RaycastHit)(ref b)).distance));
			RaycastHit[] array3 = array2;
			for (int num = 0; num < array3.Length; num++)
			{
				RaycastHit item = array3[num];
				if (((Component)((RaycastHit)(ref item)).collider).gameObject.CompareTag("CombatIgnore"))
				{
					continue;
				}
				IDamageable componentInParent = ((Component)((RaycastHit)(ref item)).collider).GetComponentInParent<IDamageable>();
				if (componentInParent != null && componentInParent == Player.Local)
				{
					continue;
				}
				if (componentInParent != null)
				{
					if (!dictionary.ContainsKey(componentInParent))
					{
						dictionary.Add(componentInParent, new List<RaycastHit>());
					}
					dictionary[componentInParent].Add(item);
				}
				data = ((RaycastHit)(ref item)).point;
				break;
			}
			Player.Local.SendEquippableMessage_Networked_Vector("Shoot", Random.Range(int.MinValue, int.MaxValue), data);
		}
		foreach (IDamageable key in dictionary.Keys)
		{
			List<RaycastHit> list = dictionary[key];
			Vector3 hitPoint = Vector3.zero;
			list.ForEach(delegate(RaycastHit hit)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				hitPoint += ((RaycastHit)(ref hit)).point;
			});
			hitPoint /= (float)list.Count;
			float impactForce = ImpactForce * (float)list.Count;
			float num2 = 0f;
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				float num4 = Damage;
				RaycastHit val2 = list[num3];
				if (((Component)((RaycastHit)(ref val2)).collider).CompareTag("Head"))
				{
					num4 *= HeadshotMultiplier;
					Debug.Log((object)("Headshot! Damage: " + num4));
				}
				num2 += num4;
			}
			Impact impact = new Impact(hitPoint, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward, impactForce, num2, EImpactType.Bullet, ((NetworkBehaviour)Player.Local).NetworkObject, Random.Range(int.MinValue, int.MaxValue));
			key.SendImpact(impact);
			Singleton<FXManager>.Instance.CreateImpactFX(impact, key);
		}
		Accuracy = Mathf.Max(Accuracy - AccuracyDropPerShot, 0f);
		if (onFire != null)
		{
			onFire.Invoke();
		}
	}

	protected virtual Vector3[] GetBulletDirections()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		return (Vector3[])(object)new Vector3[1] { SpreadDirection(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward, GetSpreadAngle()) };
	}

	protected static Vector3 SpreadDirection(Vector3 direction, float maxAngle)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		((Vector3)(ref direction)).Normalize();
		float num = maxAngle * ((float)System.Math.PI / 180f);
		float num2 = Random.Range(0f, num);
		float num3 = Random.Range(0f, (float)System.Math.PI * 2f);
		float num4 = Mathf.Sin(num2) * Mathf.Cos(num3);
		float num5 = Mathf.Sin(num2) * Mathf.Sin(num3);
		float num6 = Mathf.Cos(num2);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(num4, num5, num6);
		Vector3 val2 = Quaternion.FromToRotation(Vector3.forward, direction) * val;
		return ((Vector3)(ref val2)).normalized;
	}

	public virtual void Reload()
	{
		reloadQueued = false;
		IsReloading = true;
		interruptReload = false;
		timeSinceReloadStart = 0f;
		Console.Log("Reloading...");
		reloadRoutine = ((MonoBehaviour)this).StartCoroutine(ReloadRoutine());
		IEnumerator ReloadRoutine()
		{
			if (onReloadStart != null)
			{
				onReloadStart.Invoke();
			}
			Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(ReloadStartAnimTrigger);
			yield return (object)new WaitForSeconds(ReloadStartTime);
			StorableItemInstance mag2;
			if (ReloadType == EReloadType.Incremental)
			{
				StorableItemInstance mag;
				while (weaponItem.Value < MagazineSize && GetMagazine(out mag) && !interruptReload)
				{
					if (onReloadIndividual != null)
					{
						onReloadIndividual.Invoke();
					}
					Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(ReloadIndividualAnimTrigger);
					yield return (object)new WaitForSeconds(ReloadIndividalTime);
					weaponItem.ChangeValue(1);
					if (mag is IntegerItemInstance)
					{
						IntegerItemInstance obj = mag as IntegerItemInstance;
						obj.ChangeValue(-1);
						if (obj.Value <= 0)
						{
							((BaseItemInstance)mag).ChangeQuantity(-1);
							if ((Object)(object)ReloadTrash != (Object)null)
							{
								Vector3 posiiton = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.up * 0.4f;
								NetworkSingleton<TrashManager>.Instance.CreateTrashItem(ReloadTrash.ID, posiiton, Random.rotation);
							}
						}
					}
					else
					{
						((BaseItemInstance)mag).ChangeQuantity(-1);
					}
					NotifyIncrementalReload();
				}
				yield return (object)new WaitForSeconds(0.05f);
				if (onReloadEnd != null)
				{
					onReloadEnd.Invoke();
				}
				Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(ReloadEndAnimTrigger);
				yield return (object)new WaitForSeconds(ReloadEndTime);
			}
			else if (ReloadType == EReloadType.Magazine && GetMagazine(out mag2))
			{
				IntegerItemInstance obj2 = mag2 as IntegerItemInstance;
				obj2.ChangeValue(-(MagazineSize - weaponItem.Value));
				if (obj2.Value <= 0)
				{
					((BaseItemInstance)mag2).ChangeQuantity(-1);
					if ((Object)(object)ReloadTrash != (Object)null)
					{
						Vector3 posiiton2 = ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position - ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.up * 0.4f;
						NetworkSingleton<TrashManager>.Instance.CreateTrashItem(ReloadTrash.ID, posiiton2, Random.rotation);
					}
				}
				weaponItem.SetValue(MagazineSize);
			}
			if (MustBeCocked && !IsCocked && AutoCockAfterReload)
			{
				Cock();
			}
			Console.Log("Reloading done!");
			IsReloading = false;
			reloadRoutine = null;
		}
	}

	protected virtual void NotifyIncrementalReload()
	{
	}

	private bool IsReloadReady(bool ignoreTiming)
	{
		if (!CanReload)
		{
			return false;
		}
		if (IsReloading)
		{
			return false;
		}
		if (!GetMagazine(out var _))
		{
			return false;
		}
		if (weaponItem.Value >= MagazineSize)
		{
			return false;
		}
		if (TimeSinceFire < FireCooldown && !ignoreTiming)
		{
			return false;
		}
		if (!base.equipAnimDone && !ignoreTiming)
		{
			return false;
		}
		if (IsCocking)
		{
			return false;
		}
		return true;
	}

	protected virtual bool GetMagazine(out StorableItemInstance mag)
	{
		mag = null;
		for (int i = 0; i < PlayerSingleton<PlayerInventory>.Instance.hotbarSlots.Count; i++)
		{
			if (PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i].Quantity != 0 && ((BaseItemInstance)PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i].ItemInstance).ID == ((BaseItemDefinition)Magazine).ID)
			{
				mag = PlayerSingleton<PlayerInventory>.Instance.hotbarSlots[i].ItemInstance as StorableItemInstance;
				return true;
			}
		}
		return false;
	}

	private bool CanFire(bool checkAmmo = true)
	{
		if (TimeSinceFire < FireCooldown)
		{
			return false;
		}
		if (Aim < 0.1f)
		{
			return false;
		}
		if (!base.equipAnimDone)
		{
			return false;
		}
		if (checkAmmo && Ammo <= 0)
		{
			return false;
		}
		if (IsReloading)
		{
			return false;
		}
		if (IsCocking)
		{
			return false;
		}
		return true;
	}

	private bool CanCock()
	{
		if (IsCocked)
		{
			return false;
		}
		if (IsCocking)
		{
			return false;
		}
		if (weaponItem.Value <= 0)
		{
			return false;
		}
		if (!base.equipAnimDone)
		{
			return false;
		}
		if (IsReloading)
		{
			return false;
		}
		if (TimeSinceFire < FireCooldown)
		{
			return false;
		}
		return true;
	}

	private void Cock()
	{
		shotQueued = false;
		IsCocking = true;
		((MonoBehaviour)this).StartCoroutine(CockRoutine());
		IEnumerator CockRoutine()
		{
			if (onCockStart != null)
			{
				onCockStart.Invoke();
			}
			Singleton<ViewmodelAvatar>.Instance.Animator.SetTrigger(CockAnimTrigger);
			yield return (object)new WaitForSeconds(CockTime);
			IsCocked = true;
			IsCocking = false;
		}
	}

	protected float GetSpreadAngle()
	{
		return Mathf.Lerp(MaxSpread, MinSpread, Accuracy);
	}

	private void CheckAimingAtNPC()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (Aim < 0.5f)
		{
			return;
		}
		RaycastHit[] array = Physics.SphereCastAll(new Ray(((Component)PlayerSingleton<PlayerCamera>.Instance).transform.position, ((Component)PlayerSingleton<PlayerCamera>.Instance).transform.forward), 0.5f, 10f, LayerMask.op_Implicit(NetworkSingleton<CombatManager>.Instance.RangedWeaponLayerMask));
		List<NPC> list = new List<NPC>();
		RaycastHit[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			RaycastHit val = array2[i];
			NPC componentInParent = ((Component)((RaycastHit)(ref val)).collider).GetComponentInParent<NPC>();
			if ((Object)(object)componentInParent != (Object)null && !list.Contains(componentInParent))
			{
				list.Add(componentInParent);
				if (componentInParent.Awareness.VisionCone.IsPlayerVisible(Player.Local))
				{
					componentInParent.Responses.RespondToAimedAt(Player.Local);
				}
			}
		}
	}
}
