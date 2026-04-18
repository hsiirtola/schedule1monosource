using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Employees;
using ScheduleOne.ItemFramework;
using ScheduleOne.Management;
using ScheduleOne.ObjectScripts;
using ScheduleOne.StationFramework;
using UnityEngine;

namespace ScheduleOne.NPCs.Behaviour;

public class StartChemistryStationBehaviour : Behaviour
{
	public const float PLACE_INGREDIENTS_TIME = 8f;

	public const float STIR_TIME = 6f;

	public const float BURNER_TIME = 6f;

	private Chemist chemist;

	private Coroutine cookRoutine;

	private Beaker beaker;

	private bool NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted;

	private bool NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted;

	public ChemistryStation targetStation { get; private set; }

	public override void Awake()
	{
		NetworkInitialize___Early();
		Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviour_Assembly_002DCSharp_002Edll();
		NetworkInitialize__Late();
	}

	public void SetTargetStation(ChemistryStation station)
	{
		targetStation = station;
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if ((Object)(object)beaker != (Object)null)
		{
			beaker.Destroy();
			beaker = null;
		}
		if ((Object)(object)targetStation != (Object)null)
		{
			((Component)targetStation.StaticBeaker).gameObject.SetActive(true);
		}
		if (cookRoutine != null)
		{
			StopCook();
		}
		Disable();
	}

	public override void OnActiveTick()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		base.OnActiveTick();
		if (cookRoutine == null && InstanceFinder.IsServer && !base.Npc.Movement.IsMoving)
		{
			if (IsAtStation())
			{
				StartCook();
			}
			else
			{
				SetDestination(GetStationAccessPoint());
			}
		}
	}

	public override void BehaviourUpdate()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.BehaviourUpdate();
		if (cookRoutine != null)
		{
			base.Npc.Avatar.LookController.OverrideLookTarget(targetStation.UIPoint.position, 5);
		}
	}

	[ObserversRpc(RunLocally = true)]
	private void StartCook()
	{
		RpcWriter___Observers_StartCook_2166136261();
		RpcLogic___StartCook_2166136261();
	}

	private void SetupBeaker()
	{
		if ((Object)(object)beaker != (Object)null)
		{
			Console.LogWarning("Beaker already exists!");
			return;
		}
		beaker = targetStation.CreateBeaker();
		((Component)targetStation.StaticBeaker).gameObject.SetActive(false);
	}

	private void FillBeaker(StationRecipe recipe, Beaker beaker)
	{
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < recipe.Ingredients.Count; i++)
		{
			StorableItemDefinition storableItemDefinition = null;
			foreach (ItemDefinition item in recipe.Ingredients[i].Items)
			{
				StorableItemDefinition storableItemDefinition2 = item as StorableItemDefinition;
				for (int j = 0; j < targetStation.IngredientSlots.Length; j++)
				{
					if (targetStation.IngredientSlots[j].ItemInstance != null && ((BaseItemDefinition)targetStation.IngredientSlots[j].ItemInstance.Definition).ID == ((BaseItemDefinition)storableItemDefinition2).ID)
					{
						storableItemDefinition = storableItemDefinition2;
						break;
					}
				}
			}
			if ((Object)(object)storableItemDefinition.StationItem == (Object)null)
			{
				Console.LogError("Ingredient '" + ((BaseItemDefinition)storableItemDefinition).Name + "' does not have a station item");
				continue;
			}
			StationItem stationItem = storableItemDefinition.StationItem;
			if (!stationItem.HasModule<IngredientModule>())
			{
				if (stationItem.HasModule<PourableModule>())
				{
					PourableModule module = stationItem.GetModule<PourableModule>();
					beaker.Fillable.AddLiquid(module.LiquidType, module.LiquidCapacity_L, module.LiquidColor);
				}
				else
				{
					Console.LogError("Ingredient '" + ((BaseItemDefinition)storableItemDefinition).Name + "' does not have an ingredient or pourable module");
				}
			}
		}
	}

	private bool CanCookStart()
	{
		if ((Object)(object)targetStation == (Object)null)
		{
			return false;
		}
		if (((IUsable)targetStation).IsInUse && (Object)(object)((IUsable)targetStation).NPCUserObject != (Object)(object)((NetworkBehaviour)base.Npc).NetworkObject)
		{
			return false;
		}
		ChemistryStationConfiguration chemistryStationConfiguration = targetStation.Configuration as ChemistryStationConfiguration;
		if ((Object)(object)chemistryStationConfiguration.Recipe.SelectedRecipe == (Object)null)
		{
			return false;
		}
		if (!targetStation.HasIngredientsForRecipe(chemistryStationConfiguration.Recipe.SelectedRecipe))
		{
			return false;
		}
		return true;
	}

	private void StopCook()
	{
		targetStation.SetNPCUser(null);
		base.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", value: false);
		if (cookRoutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(cookRoutine);
			cookRoutine = null;
		}
	}

	private Vector3 GetStationAccessPoint()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetStation == (Object)null)
		{
			return ((Component)base.Npc).transform.position;
		}
		return ((ITransitEntity)targetStation).AccessPoints[0].position;
	}

	private bool IsAtStation()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetStation == (Object)null)
		{
			return false;
		}
		return Vector3.Distance(((Component)base.Npc).transform.position, GetStationAccessPoint()) < 1f;
	}

	public override void NetworkInitialize___Early()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize___EarlyScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize___Early();
			((NetworkBehaviour)this).RegisterObserversRpc(0u, new ClientRpcDelegate(RpcReader___Observers_StartCook_2166136261));
		}
	}

	public override void NetworkInitialize__Late()
	{
		if (!NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted)
		{
			NetworkInitialize__LateScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviourAssembly_002DCSharp_002Edll_Excuted = true;
			base.NetworkInitialize__Late();
		}
	}

	public override void NetworkInitializeIfDisabled()
	{
		NetworkInitialize___Early();
		NetworkInitialize__Late();
	}

	private void RpcWriter___Observers_StartCook_2166136261()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkBehaviour)this).IsServerInitialized)
		{
			NetworkManager networkManager = ((NetworkBehaviour)this).NetworkManager;
			if (networkManager == null)
			{
				networkManager = InstanceFinder.NetworkManager;
			}
			if (networkManager != null)
			{
				networkManager.LogWarning("Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
			else
			{
				Debug.LogWarning((object)"Cannot complete action because server is not active. This may also occur if the object is not yet initialized, has deinitialized, or if it does not contain a NetworkObject component.");
			}
		}
		else
		{
			Channel val = (Channel)0;
			PooledWriter writer = WriterPool.GetWriter();
			((NetworkBehaviour)this).SendObserversRpc(0u, writer, val, (DataOrderType)0, false, false, false);
			writer.Store();
		}
	}

	private void RpcLogic___StartCook_2166136261()
	{
		if (cookRoutine == null && !((Object)(object)targetStation == (Object)null))
		{
			cookRoutine = ((MonoBehaviour)this).StartCoroutine(CookRoutine());
		}
		IEnumerator CookRoutine()
		{
			base.Npc.Movement.FacePoint(((Component)targetStation).transform.position);
			yield return (object)new WaitForSeconds(0.5f);
			base.Npc.SetAnimationBool_Networked(null, "UseChemistryStation", value: true);
			if (!CanCookStart())
			{
				StopCook();
				Deactivate_Networked(null);
			}
			else
			{
				targetStation.SetNPCUser(((NetworkBehaviour)base.Npc).NetworkObject);
				StationRecipe recipe = (targetStation.Configuration as ChemistryStationConfiguration).Recipe.SelectedRecipe;
				SetupBeaker();
				yield return (object)new WaitForSeconds(1f);
				FillBeaker(recipe, beaker);
				float speedMultiplier = 1f / (base.Npc as Employee).CurrentWorkSpeed;
				yield return (object)new WaitForSeconds(8f * speedMultiplier);
				yield return (object)new WaitForSeconds(6f * speedMultiplier);
				yield return (object)new WaitForSeconds(6f * speedMultiplier);
				List<ItemInstance> list = new List<ItemInstance>();
				for (int i = 0; i < recipe.Ingredients.Count; i++)
				{
					foreach (ItemDefinition item in recipe.Ingredients[i].Items)
					{
						StorableItemDefinition storableItemDefinition = item as StorableItemDefinition;
						for (int j = 0; j < targetStation.IngredientSlots.Length; j++)
						{
							if (targetStation.IngredientSlots[j].ItemInstance != null && ((BaseItemDefinition)targetStation.IngredientSlots[j].ItemInstance.Definition).ID == ((BaseItemDefinition)storableItemDefinition).ID)
							{
								list.Add(targetStation.IngredientSlots[j].ItemInstance.GetCopy(recipe.Ingredients[i].Quantity));
								targetStation.IngredientSlots[j].ChangeQuantity(-recipe.Ingredients[i].Quantity);
								break;
							}
						}
					}
				}
				EQuality productQuality = recipe.CalculateQuality(list);
				targetStation.SendCookOperation(new ChemistryCookOperation(recipe, productQuality, beaker.Container.LiquidColor, beaker.Fillable.LiquidContainer.CurrentLiquidLevel));
				beaker.Destroy();
				beaker = null;
				StopCook();
				Deactivate_Networked(null);
			}
		}
	}

	private void RpcReader___Observers_StartCook_2166136261(PooledReader PooledReader0, Channel channel)
	{
		if (((NetworkBehaviour)this).IsClientInitialized && !((NetworkBehaviour)this).IsHost)
		{
			RpcLogic___StartCook_2166136261();
		}
	}

	protected override void Awake_UserLogic_ScheduleOne_002ENPCs_002EBehaviour_002EStartChemistryStationBehaviour_Assembly_002DCSharp_002Edll()
	{
		base.Awake();
		chemist = base.Npc as Chemist;
	}
}
