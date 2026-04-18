using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.FX;
using ScheduleOne.ItemFramework;
using ScheduleOne.Law;
using ScheduleOne.Map;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Vehicles;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ScheduleOne.UI;

public class ArrestNoticeScreen : Singleton<ArrestNoticeScreen>
{
	public const float VEHICLE_POSSESSION_TIMEOUT = 30f;

	[Header("References")]
	public Canvas Canvas;

	public CanvasGroup CanvasGroup;

	public RectTransform CrimeEntryContainer;

	public RectTransform PenaltyEntryContainer;

	[Header("Prefabs")]
	public RectTransform CrimeEntryPrefab;

	public RectTransform PenaltyEntryPrefab;

	private Dictionary<Crime, int> recordedCrimes = new Dictionary<Crime, int>();

	private LandVehicle vehicle;

	public bool isOpen { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		isOpen = false;
		((Behaviour)Canvas).enabled = false;
		CanvasGroup.alpha = 0f;
		GameInput.RegisterExitListener(Exit, 20);
		Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(PlayerSpawned));
	}

	private void PlayerSpawned()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		Player.Local.onArrested.AddListener(new UnityAction(RecordCrimes));
	}

	private void Exit(ExitAction action)
	{
		if (!action.Used && isOpen && action.exitType == ExitType.Escape)
		{
			action.Used = true;
			Close();
		}
	}

	public void Open()
	{
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		ClearEntries();
		isOpen = true;
		((Behaviour)Canvas).enabled = true;
		CanvasGroup.alpha = 1f;
		CanvasGroup.interactable = true;
		Singleton<PostProcessingManager>.Instance.SetBlur(1f);
		Crime[] array = recordedCrimes.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			((TMP_Text)((Component)Object.Instantiate<RectTransform>(CrimeEntryPrefab, (Transform)(object)CrimeEntryContainer)).GetComponentInChildren<TextMeshProUGUI>()).text = recordedCrimes[array[i]] + "x " + array[i].CrimeName.ToLower();
		}
		List<string> list = PenaltyHandler.ProcessCrimeList(recordedCrimes);
		ConfiscateItems(EStealthLevel.None);
		for (int j = 0; j < list.Count; j++)
		{
			((TMP_Text)((Component)Object.Instantiate<RectTransform>(PenaltyEntryPrefab, (Transform)(object)PenaltyEntryContainer)).GetComponentInChildren<TextMeshProUGUI>()).text = list[j];
		}
		if ((Object)(object)vehicle != (Object)null && !vehicle.IsOccupied)
		{
			Transform[] possessedVehicleSpawnPoints = Singleton<ScheduleOne.Map.Map>.Instance.PoliceStation.PossessedVehicleSpawnPoints;
			Transform target = possessedVehicleSpawnPoints[Random.Range(0, possessedVehicleSpawnPoints.Length - 1)];
			Tuple<Vector3, Quaternion> alignmentTransform = vehicle.GetAlignmentTransform(target, EParkingAlignment.RearToKerb);
			vehicle.SetTransform_Server(alignmentTransform.Item1, alignmentTransform.Item2);
		}
		PlayerSingleton<PlayerCamera>.Instance.AddActiveUIElement(((Object)this).name);
		Player.Deactivate(freeMouse: true);
	}

	public void Close()
	{
		if (CanvasGroup.interactable && isOpen)
		{
			CanvasGroup.interactable = false;
			((MonoBehaviour)this).StartCoroutine(CloseRoutine());
		}
		IEnumerator CloseRoutine()
		{
			float lerpTime = 0.3f;
			for (float i = 0f; i < lerpTime; i += Time.deltaTime)
			{
				CanvasGroup.alpha = Mathf.Lerp(1f, 0f, i / lerpTime);
				Singleton<PostProcessingManager>.Instance.SetBlur(CanvasGroup.alpha);
				yield return (object)new WaitForEndOfFrame();
			}
			CanvasGroup.alpha = 0f;
			((Behaviour)Canvas).enabled = false;
			Singleton<PostProcessingManager>.Instance.SetBlur(0f);
			PlayerSingleton<PlayerCamera>.Instance.RemoveActiveUIElement(((Object)this).name);
			Player.Activate();
			ClearEntries();
			isOpen = false;
		}
	}

	public void RecordCrimes()
	{
		Debug.Log((object)"Crimes recorded");
		recordedCrimes.Clear();
		if ((Object)(object)Player.Local.LastDrivenVehicle != (Object)null && (Player.Local.TimeSinceVehicleExit < 30f || Player.Local.CrimeData.IsCrimeOnRecord(typeof(TransportingIllicitItems))))
		{
			vehicle = Player.Local.LastDrivenVehicle;
		}
		for (int i = 0; i < Player.Local.CrimeData.Crimes.Keys.Count; i++)
		{
			recordedCrimes.Add(Player.Local.CrimeData.Crimes.Keys.ElementAt(i), Player.Local.CrimeData.Crimes.Values.ElementAt(i));
		}
		if (Player.Local.CrimeData.EvadedArrest)
		{
			recordedCrimes.Add(new Evading(), 1);
		}
		RecordPossession(EStealthLevel.None);
	}

	private void RecordPossession(EStealthLevel maxStealthLevel)
	{
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected I4, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected I4, but got Unknown
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		List<ItemSlot> allInventorySlots = PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots();
		if ((Object)(object)Player.Local.LastDrivenVehicle != (Object)null && Player.Local.TimeSinceVehicleExit < 30f && (Object)(object)Player.Local.LastDrivenVehicle.Storage != (Object)null)
		{
			allInventorySlots.AddRange(Player.Local.LastDrivenVehicle.Storage.ItemSlots);
		}
		foreach (ItemSlot item in allInventorySlots)
		{
			if (item.ItemInstance == null)
			{
				continue;
			}
			if (item.ItemInstance is ProductItemInstance)
			{
				ProductItemInstance productItemInstance = item.ItemInstance as ProductItemInstance;
				if ((Object)(object)productItemInstance.AppliedPackaging == (Object)null || productItemInstance.AppliedPackaging.StealthLevel <= maxStealthLevel)
				{
					ELegalStatus legalStatus = ((BaseItemDefinition)item.ItemInstance.Definition).legalStatus;
					switch (legalStatus - 1)
					{
					case 0:
						num += ((BaseItemInstance)productItemInstance).Quantity;
						break;
					case 1:
						num2 += ((BaseItemInstance)productItemInstance).Quantity;
						break;
					case 2:
						num3 += ((BaseItemInstance)productItemInstance).Quantity;
						break;
					case 3:
						num4 += ((BaseItemInstance)productItemInstance).Quantity;
						break;
					}
				}
			}
			else
			{
				ELegalStatus legalStatus = ((BaseItemDefinition)item.ItemInstance.Definition).legalStatus;
				switch (legalStatus - 1)
				{
				case 0:
					num += ((BaseItemInstance)item.ItemInstance).Quantity;
					break;
				case 1:
					num2 += ((BaseItemInstance)item.ItemInstance).Quantity;
					break;
				case 2:
					num3 += ((BaseItemInstance)item.ItemInstance).Quantity;
					break;
				case 3:
					num4 += ((BaseItemInstance)item.ItemInstance).Quantity;
					break;
				}
			}
		}
		if (num > 0)
		{
			recordedCrimes.Add(new PossessingControlledSubstances(), num);
		}
		if (num2 > 0)
		{
			recordedCrimes.Add(new PossessingLowSeverityDrug(), num2);
		}
		if (num3 > 0)
		{
			recordedCrimes.Add(new PossessingModerateSeverityDrug(), num3);
		}
		if (num4 > 0)
		{
			recordedCrimes.Add(new PossessingHighSeverityDrug(), num4);
		}
	}

	private void ConfiscateItems(EStealthLevel maxStealthLevel)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		List<ItemSlot> allInventorySlots = PlayerSingleton<PlayerInventory>.Instance.GetAllInventorySlots();
		if ((Object)(object)Player.Local.LastDrivenVehicle != (Object)null && Player.Local.TimeSinceVehicleExit < 30f && (Object)(object)Player.Local.LastDrivenVehicle.Storage != (Object)null)
		{
			allInventorySlots.AddRange(Player.Local.LastDrivenVehicle.Storage.ItemSlots);
		}
		foreach (ItemSlot item in allInventorySlots)
		{
			if (item.ItemInstance == null)
			{
				continue;
			}
			if (item.ItemInstance is ProductItemInstance)
			{
				ProductItemInstance productItemInstance = item.ItemInstance as ProductItemInstance;
				if ((Object)(object)productItemInstance.AppliedPackaging == (Object)null || productItemInstance.AppliedPackaging.StealthLevel <= maxStealthLevel)
				{
					item.ClearStoredInstance();
				}
			}
			else if ((int)((BaseItemDefinition)item.ItemInstance.Definition).legalStatus != 0)
			{
				item.ClearStoredInstance();
			}
		}
	}

	private void ClearEntries()
	{
		int childCount = ((Transform)CrimeEntryContainer).childCount;
		for (int i = 0; i < childCount; i++)
		{
			Object.Destroy((Object)(object)((Component)((Transform)CrimeEntryContainer).GetChild(i)).gameObject);
		}
		childCount = ((Transform)PenaltyEntryContainer).childCount;
		for (int j = 0; j < childCount; j++)
		{
			Object.Destroy((Object)(object)((Component)((Transform)PenaltyEntryContainer).GetChild(j)).gameObject);
		}
	}
}
