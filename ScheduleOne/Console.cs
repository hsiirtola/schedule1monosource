using System;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.Cutscenes;
using ScheduleOne.DevUtilities;
using ScheduleOne.Employees;
using ScheduleOne.GamePhysics;
using ScheduleOne.GameTime;
using ScheduleOne.Growing;
using ScheduleOne.ItemFramework;
using ScheduleOne.Law;
using ScheduleOne.Levelling;
using ScheduleOne.Map;
using ScheduleOne.Money;
using ScheduleOne.NPCs;
using ScheduleOne.NPCs.Relation;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Police;
using ScheduleOne.Product;
using ScheduleOne.Product.Packaging;
using ScheduleOne.Property;
using ScheduleOne.Quests;
using ScheduleOne.Trash;
using ScheduleOne.UI;
using ScheduleOne.Variables;
using ScheduleOne.Vehicles;
using ScheduleOne.Weather;
using UnityEngine;
using UnityEngine.AI;

namespace ScheduleOne;

public class Console : Singleton<Console>
{
	public abstract class ConsoleCommand
	{
		public abstract string CommandWord { get; }

		public abstract string CommandDescription { get; }

		public abstract string ExampleUsage { get; }

		public abstract void Execute(List<string> args);
	}

	public class SetTimeCommand : ConsoleCommand
	{
		public override string CommandWord => "settime";

		public override string CommandDescription => "Sets the time of day to the specified 24-hour time";

		public override string ExampleUsage => "settime 1530";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0 && TimeManager.IsValid24HourTime(args[0]))
			{
				if (Player.Local.IsSleeping)
				{
					LogWarning("Can't set time whilst sleeping");
					return;
				}
				Log("Time set to " + args[0]);
				NetworkSingleton<TimeManager>.Instance.SetTimeAndSync(int.Parse(args[0]));
			}
			else
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'settime 1530'");
			}
		}
	}

	public class SpawnVehicleCommand : ConsoleCommand
	{
		public override string CommandWord => "spawnvehicle";

		public override string CommandDescription => "Spawns a vehicle at the player's location";

		public override string ExampleUsage => "spawnvehicle shitbox";

		public override void Execute(List<string> args)
		{
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			bool flag = false;
			if (args.Count > 0 && (Object)(object)NetworkSingleton<VehicleManager>.Instance.GetVehiclePrefab(args[0]) != (Object)null)
			{
				flag = true;
				Log("Spawning '" + args[0] + "'...");
				Vector3 position = ((Component)player).transform.position + ((Component)player).transform.forward * 4f + ((Component)player).transform.up * 1f;
				Quaternion rotation = ((Component)player).transform.rotation;
				NetworkSingleton<VehicleManager>.Instance.SpawnAndReturnVehicle(args[0], position, rotation, playerOwned: true);
			}
			if (!flag)
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'spawnvehicle shitbox'");
			}
		}
	}

	public class AddItemToInventoryCommand : ConsoleCommand
	{
		public override string CommandWord => "give";

		public override string CommandDescription => "Gives the player the specified item. Optionally specify a quantity.";

		public override string ExampleUsage => "give ogkush 5";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				ItemDefinition item = Registry.GetItem(args[0]);
				if ((Object)(object)item != (Object)null)
				{
					ItemInstance defaultInstance = item.GetDefaultInstance();
					if (args[0] == "cash")
					{
						LogWarning("Unrecognized item code '" + args[0] + "'");
					}
					else if (PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(defaultInstance))
					{
						int result = 1;
						if (args.Count > 1)
						{
							bool flag = false;
							if (int.TryParse(args[1], out result) && result > 0)
							{
								flag = true;
							}
							if (!flag)
							{
								LogWarning("Unrecognized quantity '" + args[1] + "'. Please provide a positive integer");
							}
						}
						int num = 0;
						while (result > 0 && PlayerSingleton<PlayerInventory>.Instance.CanItemFitInInventory(defaultInstance))
						{
							PlayerSingleton<PlayerInventory>.Instance.AddItemToInventory(defaultInstance);
							result--;
							num++;
						}
						Log("Added " + num + " " + ((BaseItemDefinition)item).Name + " to inventory");
					}
					else
					{
						LogWarning("Insufficient inventory space");
					}
				}
				else
				{
					LogWarning("Unrecognized item code '" + args[0] + "'");
				}
			}
			else
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'give watering_can', 'give watering_can 5'");
			}
		}
	}

	public class ClearInventoryCommand : ConsoleCommand
	{
		public override string CommandWord => "clearinventory";

		public override string CommandDescription => "Clears the player's inventory";

		public override string ExampleUsage => "clearinventory";

		public override void Execute(List<string> args)
		{
			Log("Clearing player inventory...");
			PlayerSingleton<PlayerInventory>.Instance.ClearInventory();
		}
	}

	public class ChangeCashCommand : ConsoleCommand
	{
		public override string CommandWord => "changecash";

		public override string CommandDescription => "Changes the player's cash balance by the specified amount";

		public override string ExampleUsage => "changecash 5000";

		public override void Execute(List<string> args)
		{
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result))
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'changecash 5000', 'changecash -5000'");
			}
			else if (result > 0f)
			{
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(result);
				Log("Gave player " + MoneyManager.FormatAmount(result) + " cash");
			}
			else if (result < 0f)
			{
				result = Mathf.Clamp(result, 0f - NetworkSingleton<MoneyManager>.Instance.cashBalance, 0f);
				NetworkSingleton<MoneyManager>.Instance.ChangeCashBalance(result);
				Log("Removed " + MoneyManager.FormatAmount(result) + " cash from player");
			}
		}
	}

	public class ChangeOnlineBalanceCommand : ConsoleCommand
	{
		public override string CommandWord => "changebalance";

		public override string CommandDescription => "Changes the player's online balance by the specified amount";

		public override string ExampleUsage => "changebalance 5000";

		public override void Execute(List<string> args)
		{
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result))
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'changebalance 5000', 'changebalance -5000'");
			}
			else if (result > 0f)
			{
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Added online balance", result, 1f, "Added by developer console");
				Log("Increased online balance by " + MoneyManager.FormatAmount(result));
			}
			else if (result < 0f)
			{
				result = Mathf.Clamp(result, 0f - NetworkSingleton<MoneyManager>.Instance.SyncAccessor_onlineBalance, 0f);
				NetworkSingleton<MoneyManager>.Instance.CreateOnlineTransaction("Removed online balance", result, 1f, "Removed by developer console");
				Log("Decreased online balance by " + MoneyManager.FormatAmount(result));
			}
		}
	}

	public class SetMoveSpeedCommand : ConsoleCommand
	{
		public override string CommandWord => "setmovespeed";

		public override string CommandDescription => "Sets the player's move speed multiplier";

		public override string ExampleUsage => "setmovespeed 1";

		public override void Execute(List<string> args)
		{
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'setmovespeed 1'");
				return;
			}
			Log("Setting player move speed multiplier to " + result);
			PlayerMovement.StaticMoveSpeedMultiplier = result;
		}
	}

	public class SetJumpMultiplier : ConsoleCommand
	{
		public override string CommandWord => "setjumpforce";

		public override string CommandDescription => "Sets the player's jump force multiplier";

		public override string ExampleUsage => "setjumpforce 1";

		public override void Execute(List<string> args)
		{
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'setjumpforce 1'");
				return;
			}
			Log("Setting player jump force multiplier to " + result);
			PlayerMovement.JumpMultiplier = result;
		}
	}

	public class SetPropertyOwned : ConsoleCommand
	{
		public override string CommandWord => "setowned";

		public override string CommandDescription => "Sets the specified property or business as owned";

		public override string ExampleUsage => "setowned barn, setowned laundromat";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				string code = args[0].ToLower();
				ScheduleOne.Property.Property property = ScheduleOne.Property.Property.UnownedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == code);
				Business business = Business.UnownedBusinesses.Find((Business x) => x.PropertyCode.ToLower() == code);
				if ((Object)(object)property == (Object)null && (Object)(object)business == (Object)null)
				{
					LogCommandError("Could not find unowned property with code '" + code + "'");
					return;
				}
				if ((Object)(object)property != (Object)null)
				{
					property.SetOwned();
				}
				if ((Object)(object)business != (Object)null)
				{
					business.SetOwned();
				}
				Log("Property with code '" + code + "' is now owned");
			}
			else
			{
				LogUnrecognizedFormat(new string[2] { "setowned barn", "setowned manor" });
			}
		}
	}

	public class Teleport : ConsoleCommand
	{
		public override string CommandWord => "teleport";

		public override string CommandDescription => "Teleports the player to the specified location, property, or NPC.";

		public override string ExampleUsage => "teleport townhall, teleport barn, teleport jessi_waters";

		public override void Execute(List<string> args)
		{
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			if (args.Count > 0)
			{
				string text = args[0].ToLower();
				bool flag = false;
				TransformData transformData = default(TransformData);
				for (int i = 0; i < Singleton<Console>.Instance.TeleportPointsContainer.childCount; i++)
				{
					if (((Object)Singleton<Console>.Instance.TeleportPointsContainer.GetChild(i)).name.ToLower() == text)
					{
						flag = true;
						transformData = Singleton<Console>.Instance.TeleportPointsContainer.GetChild(i).GetWorldTransformData();
						break;
					}
				}
				if (!flag)
				{
					for (int j = 0; j < ScheduleOne.Property.Property.Properties.Count; j++)
					{
						if (ScheduleOne.Property.Property.Properties[j].PropertyCode.ToLower() == text)
						{
							flag = true;
							transformData = ScheduleOne.Property.Property.Properties[j].SpawnPoint.GetWorldTransformData();
							ref Vector3 position = ref transformData.Position;
							position += Vector3.up * 1f;
							break;
						}
					}
				}
				if (!flag)
				{
					NPC nPC = NPCManager.GetNPC(text);
					if ((Object)(object)nPC != (Object)null && NavMeshUtility.SamplePosition(nPC.CenterPoint + ((Component)nPC.Avatar).transform.forward * 1.5f, out var hit, 10f, -1))
					{
						flag = true;
						transformData.Position = ((NavMeshHit)(ref hit)).position + Vector3.up * 1f;
						Vector3 val = nPC.CenterPoint - transformData.Position;
						transformData.Rotation = Quaternion.LookRotation(((Vector3)(ref val)).normalized, Vector3.up);
					}
				}
				if (!flag)
				{
					LogCommandError("Unrecognized destination");
					return;
				}
				PlayerSingleton<PlayerMovement>.Instance.Teleport(transformData.Position);
				PlayerSingleton<PlayerMovement>.Instance.SetPlayerRotation(transformData.Rotation);
				Log("Teleported to '" + text + "'");
			}
			else
			{
				LogUnrecognizedFormat(new string[2] { "teleport docks", "teleport barn" });
			}
		}
	}

	public class PackageProduct : ConsoleCommand
	{
		public override string CommandWord => "packageproduct";

		public override string CommandDescription => "Packages the equipped product with the specified packaging";

		public override string ExampleUsage => "packageproduct jar, packageproduct baggie";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				PackagingDefinition packagingDefinition = Registry.GetItem(args[0].ToLower()) as PackagingDefinition;
				if ((Object)(object)packagingDefinition == (Object)null)
				{
					LogCommandError("Unrecognized packaging ID");
				}
				else if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance is ProductItemInstance)
				{
					(PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance as ProductItemInstance).SetPackaging(packagingDefinition);
					PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
					PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
					Log("Applied packaging '" + ((BaseItemDefinition)packagingDefinition).Name + "' to equipped product");
				}
				else
				{
					LogCommandError("No product equipped");
				}
			}
			else
			{
				LogUnrecognizedFormat(new string[2] { "packageproduct jar", "packageproduct baggie" });
			}
		}
	}

	public class SetStaminaReserve : ConsoleCommand
	{
		public override string CommandWord => "setstaminareserve";

		public override string CommandDescription => "Sets the player's stamina reserve (default 100) to the specified amount.";

		public override string ExampleUsage => "setstaminareserve 200";

		public override void Execute(List<string> args)
		{
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'setstaminareserve 200'");
				return;
			}
			Log("Setting player stamina reserve to " + result);
			PlayerMovement.StaminaReserveMax = result;
			PlayerSingleton<PlayerMovement>.Instance.SetStamina(result);
		}
	}

	public class SetWeather : ConsoleCommand
	{
		public override string CommandWord => "setweather";

		public override string CommandDescription => "Sets the weather to the specified type";

		public override string ExampleUsage => "setweather clear, setweather lightrain, setweather heavyrain";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				Log("Setting weather to " + ((args.Count > 0) ? args[0] : "unknown") + "...");
				string weather = args[0].ToLower();
				NetworkSingleton<EnvironmentManager>.Instance.SetWeather(weather);
			}
		}
	}

	public class SetWeatherSpeed : ConsoleCommand
	{
		public override string CommandWord => "setweatherspeed";

		public override string CommandDescription => "Sets the speed at which weather volumes move. Default is 1.";

		public override string ExampleUsage => "setweatherspeed 2";

		public override void Execute(List<string> args)
		{
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'setweatherspeed 2'");
				return;
			}
			Log("Setting weather volume move speed to " + result);
			NetworkSingleton<EnvironmentManager>.Instance.SetVolumeMoveSpeed(result);
		}
	}

	public class TriggerLightning : ConsoleCommand
	{
		public override string CommandWord => "triggerlightning";

		public override string CommandDescription => "Triggers a lightning event. You can specify a target (player or npc) or leave it empty for a random location.";

		public override string ExampleUsage => "triggerlightning, triggerlightning cranky_frank, triggerlightning playername";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				string text = string.Join(" ", args).ToLower();
				if ((Object)(object)Player.GetPlayerByName(text) != (Object)null)
				{
					NetworkSingleton<EnvironmentManager>.Instance.TriggerPlayerLightningEvent(Player.GetPlayerByName(text));
				}
				else if ((Object)(object)NPCManager.GetNPC(text) != (Object)null)
				{
					NetworkSingleton<EnvironmentManager>.Instance.TriggerNpcLightningEvent(NPCManager.GetNPC(text));
				}
				else
				{
					LogWarning("Unrecognized target '" + text + "'");
				}
			}
			else
			{
				NetworkSingleton<EnvironmentManager>.Instance.TriggerLightningEvent();
			}
		}
	}

	public class TriggerDistantThunder : ConsoleCommand
	{
		public override string CommandWord => "triggerdistantthunder";

		public override string CommandDescription => "Triggers distant thunder.";

		public override string ExampleUsage => "triggerdistantthunder";

		public override void Execute(List<string> args)
		{
			NetworkSingleton<EnvironmentManager>.Instance.TriggerDistantThunder();
		}
	}

	public class RaisedWanted : ConsoleCommand
	{
		public override string CommandWord => "raisewanted";

		public override string CommandDescription => "Raises the player's wanted level";

		public override string ExampleUsage => "raisewanted";

		public override void Execute(List<string> args)
		{
			Log("Raising wanted level...");
			if (player.CrimeData.CurrentPursuitLevel == PlayerCrimeData.EPursuitLevel.None)
			{
				Singleton<LawManager>.Instance.PoliceCalled(player, new Crime());
			}
			player.CrimeData.Escalate();
		}
	}

	public class LowerWanted : ConsoleCommand
	{
		public override string CommandWord => "lowerwanted";

		public override string CommandDescription => "Lowers the player's wanted level";

		public override string ExampleUsage => "lowerwanted";

		public override void Execute(List<string> args)
		{
			Log("Lowering wanted level...");
			player.CrimeData.Deescalate();
		}
	}

	public class ClearWanted : ConsoleCommand
	{
		public override string CommandWord => "clearwanted";

		public override string CommandDescription => "Clears the player's wanted level";

		public override string ExampleUsage => "clearwanted";

		public override void Execute(List<string> args)
		{
			Log("Clearing wanted level...");
			player.CrimeData.SetPursuitLevel(PlayerCrimeData.EPursuitLevel.None);
			player.CrimeData.ClearCrimes();
		}
	}

	public class SetHealth : ConsoleCommand
	{
		public override string CommandWord => "sethealth";

		public override string CommandDescription => "Sets the player's health to the specified amount";

		public override string ExampleUsage => "sethealth 100";

		public override void Execute(List<string> args)
		{
			if (!player.Health.IsAlive)
			{
				LogWarning("Can't set health whilst dead");
				return;
			}
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'sethealth 100'");
				return;
			}
			Log("Setting player health to " + result);
			player.Health.SetHealth(result);
			if (result < 0f)
			{
				PlayerSingleton<PlayerCamera>.Instance.JoltCamera();
			}
		}
	}

	public class SetEnergy : ConsoleCommand
	{
		public override string CommandWord => "setenergy";

		public override string CommandDescription => "Sets the player's energy to the specified amount";

		public override string ExampleUsage => "setenergy 100";

		public override void Execute(List<string> args)
		{
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'setenergy 100'");
				return;
			}
			result = Mathf.Clamp(result, 0f, 100f);
			Log("Setting player energy to " + result);
			Player.Local.Energy.SetEnergy(result);
		}
	}

	public class FreeCamCommand : ConsoleCommand
	{
		public override string CommandWord => "freecam";

		public override string CommandDescription => "Toggles free cam mode";

		public override string ExampleUsage => "freecam";

		public override void Execute(List<string> args)
		{
			if (PlayerSingleton<PlayerCamera>.Instance.FreeCamEnabled)
			{
				PlayerSingleton<PlayerCamera>.Instance.SetFreeCam(enable: false);
			}
			else
			{
				PlayerSingleton<PlayerCamera>.Instance.SetFreeCam(enable: true);
			}
		}
	}

	public class Save : ConsoleCommand
	{
		public override string CommandWord => "save";

		public override string CommandDescription => "Forces a save";

		public override string ExampleUsage => "save";

		public override void Execute(List<string> args)
		{
			Log("Forcing save...");
			Singleton<SaveManager>.Instance.Save();
		}
	}

	public class SetTimeScale : ConsoleCommand
	{
		public override string CommandWord => "settimescale";

		public override string CommandDescription => "Sets the time scale. Default 1";

		public override string ExampleUsage => "settimescale 1";

		public override void Execute(List<string> args)
		{
			if (!Singleton<Settings>.Instance.PausingFreezesTime)
			{
				LogWarning("Can't set time scale right now.");
				return;
			}
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
			{
				LogWarning("Unrecognized command format. Correct format example(s): 'settimescale 1'");
				return;
			}
			result = Mathf.Clamp(result, 0f, 20f);
			Log("Setting time scale to " + result);
			Time.timeScale = result;
		}
	}

	public class SetVariableValue : ConsoleCommand
	{
		public override string CommandWord => "setvar";

		public override string CommandDescription => "Sets the value of the specified variable";

		public override string ExampleUsage => "setvar <variable> <value>";

		public override void Execute(List<string> args)
		{
			if (args.Count >= 2)
			{
				string variableName = args[0].ToLower();
				string value = args[1];
				NetworkSingleton<VariableDatabase>.Instance.SetVariableValue(variableName, value);
			}
			else
			{
				LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
			}
		}
	}

	public class SetQuestState : ConsoleCommand
	{
		public override string CommandWord => "setqueststate";

		public override string CommandDescription => "Sets the state of the specified quest";

		public override string ExampleUsage => "setqueststate <quest name> <state>";

		public override void Execute(List<string> args)
		{
			if (args.Count >= 2)
			{
				string text = args[0].ToLower();
				string text2 = args[1];
				text = text.Replace("_", " ");
				Quest quest = Quest.GetQuest(text);
				if ((Object)(object)quest == (Object)null)
				{
					LogWarning("Failed to find quest with name '" + text + "'");
					return;
				}
				EQuestState result = EQuestState.Inactive;
				if (!Enum.TryParse<EQuestState>(text2, ignoreCase: true, out result))
				{
					LogWarning("Failed to parse quest state '" + text2 + "'");
					return;
				}
				Log("Setting quest '" + text + "' state to " + result);
				quest.SetQuestState(result);
			}
			else
			{
				LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
			}
		}
	}

	public class SetQuestEntryState : ConsoleCommand
	{
		public override string CommandWord => "setquestentrystate";

		public override string CommandDescription => "Sets the state of the specified quest entry";

		public override string ExampleUsage => "setquestentrystate <quest name> <entry index> <state>";

		public override void Execute(List<string> args)
		{
			if (args.Count >= 3)
			{
				string text = args[0].ToLower();
				int num = (int.TryParse(args[1], out num) ? num : (-1));
				string text2 = args[2];
				text = text.Replace("_", " ");
				Quest quest = Quest.GetQuest(text);
				if ((Object)(object)quest == (Object)null)
				{
					LogWarning("Failed to find quest with name '" + text + "'");
					return;
				}
				if (num < 0 || num >= quest.Entries.Count)
				{
					LogWarning("Invalid entry index");
					return;
				}
				EQuestState result = EQuestState.Inactive;
				if (!Enum.TryParse<EQuestState>(text2, ignoreCase: true, out result))
				{
					LogWarning("Failed to parse quest state '" + text2 + "'");
					return;
				}
				Log("Setting quest '" + text + "' entry " + num + " state to " + result);
				quest.SetQuestEntryState(num, result);
			}
			else
			{
				LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
			}
		}
	}

	public class SetEmotion : ConsoleCommand
	{
		public override string CommandWord => "setemotion";

		public override string CommandDescription => "Sets the facial expression of the player's avatar.";

		public override string ExampleUsage => "setemotion cheery";

		public override void Execute(List<string> args)
		{
			if (!Singleton<Settings>.Instance.PausingFreezesTime)
			{
				LogWarning("Can't set time scale right now.");
				return;
			}
			if (args.Count == 0)
			{
				LogWarning("Unrecognized command format. Correct format example(s): " + ExampleUsage);
				return;
			}
			string text = args[0].ToLower();
			if (!Player.Local.Avatar.EmotionManager.HasEmotion(text))
			{
				LogWarning("Unrecognized emotion '" + text + "'");
				return;
			}
			Log("Setting emotion to " + text);
			Player.Local.Avatar.EmotionManager.AddEmotionOverride(text, "console");
		}
	}

	public class SetUnlocked : ConsoleCommand
	{
		public override string CommandWord => "setunlocked";

		public override string CommandDescription => "Unlocks the given NPC";

		public override string ExampleUsage => "setunlocked <npc_id>";

		public override void Execute(List<string> args)
		{
			if (args.Count >= 1)
			{
				string text = args[0].ToLower();
				NPC nPC = NPCManager.GetNPC(text);
				if ((Object)(object)nPC == (Object)null)
				{
					LogWarning("Failed to find NPC with ID '" + text + "'");
				}
				else
				{
					nPC.RelationData.Unlock(NPCRelationData.EUnlockType.DirectApproach);
				}
			}
			else
			{
				LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
			}
		}
	}

	public class SetRelationship : ConsoleCommand
	{
		public override string CommandWord => "setrelationship";

		public override string CommandDescription => "Sets the relationship scalar of the given NPC. Range is 0-5.";

		public override string ExampleUsage => "setrelationship <npc_id> 5";

		public override void Execute(List<string> args)
		{
			if (args.Count >= 2)
			{
				string text = args[0].ToLower();
				NPC nPC = NPCManager.GetNPC(text);
				if ((Object)(object)nPC == (Object)null)
				{
					LogWarning("Failed to find NPC with ID '" + text + "'");
					return;
				}
				float result = 0f;
				if (!float.TryParse(args[1], out result) || result < 0f || result > 5f)
				{
					LogWarning("Invalid scalar value. Must be between 0 and 5.");
				}
				else
				{
					nPC.RelationData.SetRelationship(result);
				}
			}
			else
			{
				LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
			}
		}
	}

	public class AddEmployeeCommand : ConsoleCommand
	{
		public override string CommandWord => "addemployee";

		public override string CommandDescription => "Adds an employee of the specified type to the given property.";

		public override string ExampleUsage => "addemployee botanist barn";

		public override void Execute(List<string> args)
		{
			if (args.Count >= 2)
			{
				args[0].ToLower();
				EEmployeeType result = EEmployeeType.Botanist;
				if (!Enum.TryParse<EEmployeeType>(args[0], ignoreCase: true, out result))
				{
					LogCommandError("Unrecognized employee type '" + args[0] + "'");
					return;
				}
				string code = args[1].ToLower();
				ScheduleOne.Property.Property property = ScheduleOne.Property.Property.OwnedProperties.Find((ScheduleOne.Property.Property x) => x.PropertyCode.ToLower() == code);
				if ((Object)(object)property == (Object)null)
				{
					LogCommandError("Could not find property with code '" + code + "'");
					return;
				}
				NetworkSingleton<EmployeeManager>.Instance.CreateNewEmployee(property, result);
				Log("Adding employee of type '" + result.ToString() + "' to property '" + property.PropertyCode + "'");
			}
			else
			{
				LogUnrecognizedFormat(new string[2] { "setowned barn", "setowned manor" });
			}
		}
	}

	public class SetDiscovered : ConsoleCommand
	{
		public override string CommandWord => "setdiscovered";

		public override string CommandDescription => "Sets the specified product as discovered";

		public override string ExampleUsage => "setdiscovered ogkush";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				string text = args[0].ToLower();
				ProductDefinition productDefinition = Registry.GetItem(text) as ProductDefinition;
				if ((Object)(object)productDefinition == (Object)null)
				{
					LogCommandError("Unrecognized product code '" + text + "'");
					return;
				}
				NetworkSingleton<ProductManager>.Instance.DiscoverProduct(((BaseItemDefinition)productDefinition).ID);
				Log(((BaseItemDefinition)productDefinition).Name + " now discovered");
			}
			else
			{
				LogUnrecognizedFormat(new string[1] { ExampleUsage });
			}
		}
	}

	public class GrowPlants : ConsoleCommand
	{
		public override string CommandWord => "growplants";

		public override string CommandDescription => "Sets ALL plants in the world fully grown";

		public override string ExampleUsage => "growplants";

		public override void Execute(List<string> args)
		{
			Plant[] array = Object.FindObjectsOfType<Plant>();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Pot.SetGrowthProgress_Server(1f);
			}
			ShroomColony[] array2 = Object.FindObjectsOfType<ShroomColony>();
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].SetFullyGrown();
			}
		}
	}

	public class SetLawIntensity : ConsoleCommand
	{
		public override string CommandWord => "setlawintensity";

		public override string CommandDescription => "Sets the intensity of law enforcement activity on a scale of 0-10.";

		public override string ExampleUsage => "setlawintensity 6";

		public override void Execute(List<string> args)
		{
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result) || result < 0f)
			{
				LogWarning("Unrecognized command format. Correct format example(s): " + ExampleUsage);
				return;
			}
			float num = Mathf.Clamp(result, 0f, 10f);
			Log("Setting law enforcement intensity to " + num);
			Singleton<LawController>.Instance.SetInternalIntensity(num / 10f);
		}
	}

	public class SetQuality : ConsoleCommand
	{
		public override string CommandWord => "setquality";

		public override string CommandDescription => "Sets the quality of the currently equipped item.";

		public override string ExampleUsage => "setquality standard, setquality heavenly";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				string text = args[0].ToLower();
				if (!Enum.TryParse<EQuality>(text, ignoreCase: true, out var result))
				{
					LogCommandError("Unrecognized quality '" + text + "'");
				}
				if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped && PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance is QualityItemInstance)
				{
					(PlayerSingleton<PlayerInventory>.Instance.equippedSlot.ItemInstance as QualityItemInstance).SetQuality(result);
					PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: false);
					PlayerSingleton<PlayerInventory>.Instance.SetEquippingEnabled(enabled: true);
					Log("Set quality to " + result);
				}
				else
				{
					LogCommandError("No quality item equipped");
				}
			}
			else
			{
				LogUnrecognizedFormat(new string[1] { ExampleUsage });
			}
		}
	}

	public class SetQuantity : ConsoleCommand
	{
		public override string CommandWord => "setquantity";

		public override string CommandDescription => "Sets the quantity of the currently equipped item.";

		public override string ExampleUsage => "setquantity 5";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				if (!int.TryParse(args[0], out var result))
				{
					LogCommandError("Unrecognized quantity '" + args[0] + "'");
				}
				if (PlayerSingleton<PlayerInventory>.Instance.isAnythingEquipped)
				{
					PlayerSingleton<PlayerInventory>.Instance.equippedSlot.SetQuantity(result);
				}
				else
				{
					LogCommandError("Nothing equipped");
				}
			}
			else
			{
				LogUnrecognizedFormat(new string[1] { ExampleUsage });
			}
		}
	}

	public class Bind : ConsoleCommand
	{
		public override string CommandWord => "bind";

		public override string CommandDescription => "Binds the given key to the given command.";

		public override string ExampleUsage => "bind t 'settime 1200'";

		public override void Execute(List<string> args)
		{
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			if (args.Count > 1)
			{
				string text = args[0].ToLower();
				if (!Enum.TryParse<KeyCode>(text, true, out KeyCode result))
				{
					LogCommandError("Unrecognized keycode '" + text + "'");
				}
				string command = string.Join(" ", args.ToArray()).Substring(text.Length + 1);
				Singleton<Console>.Instance.AddBinding(result, command);
			}
			else
			{
				LogUnrecognizedFormat(new string[1] { ExampleUsage });
			}
		}
	}

	public class Unbind : ConsoleCommand
	{
		public override string CommandWord => "unbind";

		public override string CommandDescription => "Removes the given bind.";

		public override string ExampleUsage => "unbind t";

		public override void Execute(List<string> args)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			if (args.Count > 0)
			{
				string text = args[0].ToLower();
				if (!Enum.TryParse<KeyCode>(text, true, out KeyCode result))
				{
					LogCommandError("Unrecognized keycode '" + text + "'");
				}
				Singleton<Console>.Instance.RemoveBinding(result);
			}
			else
			{
				LogUnrecognizedFormat(new string[1] { ExampleUsage });
			}
		}
	}

	public class ClearBinds : ConsoleCommand
	{
		public override string CommandWord => "clearbinds";

		public override string CommandDescription => "Clears ALL binds.";

		public override string ExampleUsage => "clearbinds";

		public override void Execute(List<string> args)
		{
			Singleton<Console>.Instance.ClearBindings();
		}
	}

	public class HideUI : ConsoleCommand
	{
		public override string CommandWord => "hideui";

		public override string CommandDescription => "Hides all on-screen UI.";

		public override string ExampleUsage => "hideui";

		public override void Execute(List<string> args)
		{
			((Behaviour)Singleton<HUD>.Instance.canvas).enabled = false;
		}
	}

	public class GiveXP : ConsoleCommand
	{
		public override string CommandWord => "addxp";

		public override string CommandDescription => "Adds the specified amount of experience points.";

		public override string ExampleUsage => "addxp 100";

		public override void Execute(List<string> args)
		{
			int result = 0;
			if (args.Count == 0 || !int.TryParse(args[0], out result) || result < 0)
			{
				LogWarning("Unrecognized command format. Correct format example(s): " + ExampleUsage);
				return;
			}
			Log("Giving " + result + " experience points");
			NetworkSingleton<LevelManager>.Instance.AddXP(result);
		}
	}

	public class Disable : ConsoleCommand
	{
		public override string CommandWord => "disable";

		public override string CommandDescription => "Disables the specified GameObject";

		public override string ExampleUsage => "disable pp";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				string code = args[0].ToLower();
				LabelledGameObject labelledGameObject = Singleton<Console>.Instance.LabelledGameObjectList.Find((LabelledGameObject x) => x.Label.ToLower() == code);
				if (labelledGameObject == null)
				{
					LogCommandError("Could not find GameObject with label '" + code + "'");
				}
				else if ((Object)(object)labelledGameObject.GameObject == (Object)null)
				{
					LogCommandError("GameObject with label '" + code + "' is null");
				}
				else
				{
					labelledGameObject.GameObject.SetActive(false);
				}
			}
			else
			{
				LogUnrecognizedFormat(new string[1] { ExampleUsage });
			}
		}
	}

	public class Enable : ConsoleCommand
	{
		public override string CommandWord => "enable";

		public override string CommandDescription => "Enables the specified GameObject";

		public override string ExampleUsage => "enable pp";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				string code = args[0].ToLower();
				LabelledGameObject labelledGameObject = Singleton<Console>.Instance.LabelledGameObjectList.Find((LabelledGameObject x) => x.Label.ToLower() == code);
				if (labelledGameObject == null)
				{
					LogCommandError("Could not find GameObject with label '" + code + "'");
				}
				else
				{
					labelledGameObject.GameObject.SetActive(true);
				}
			}
			else
			{
				LogUnrecognizedFormat(new string[1] { ExampleUsage });
			}
		}
	}

	public class EndTutorial : ConsoleCommand
	{
		public override string CommandWord => "endtutorial";

		public override string CommandDescription => "Forces the tutorial to end immediately (only if the player is actually in the tutorial).";

		public override string ExampleUsage => "endtutorial";

		public override void Execute(List<string> args)
		{
			NetworkSingleton<GameManager>.Instance.EndTutorial(natural: false);
		}
	}

	public class DisableNPCAsset : ConsoleCommand
	{
		public override string CommandWord => "disablenpcasset";

		public override string CommandDescription => "Disabled the given asset under all NPCs";

		public override string ExampleUsage => "disablenpcasset avatar";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				string text = args[0];
				{
					foreach (NPC item in NPCManager.NPCRegistry)
					{
						for (int i = 0; i < ((Component)item).transform.childCount; i++)
						{
							Transform child = ((Component)item).transform.GetChild(i);
							if (text == "all" || ((Object)child).name.ToLower() == text.ToLower())
							{
								((Component)child).gameObject.SetActive(false);
							}
						}
					}
					return;
				}
			}
			LogUnrecognizedFormat(new string[1] { ExampleUsage });
		}
	}

	public class ShowFPS : ConsoleCommand
	{
		public override string CommandWord => "showfps";

		public override string CommandDescription => "Shows FPS label.";

		public override string ExampleUsage => "showfps";

		public override void Execute(List<string> args)
		{
			((Component)Singleton<HUD>.Instance.fpsLabel).gameObject.SetActive(true);
		}
	}

	public class HideFPS : ConsoleCommand
	{
		public override string CommandWord => "hidefps";

		public override string CommandDescription => "Hides FPS label.";

		public override string ExampleUsage => "hidefps";

		public override void Execute(List<string> args)
		{
			((Component)Singleton<HUD>.Instance.fpsLabel).gameObject.SetActive(false);
		}
	}

	public class ClearTrash : ConsoleCommand
	{
		public override string CommandWord => "cleartrash";

		public override string CommandDescription => "Instantly removes all trash from the world.";

		public override string ExampleUsage => "cleartrash";

		public override void Execute(List<string> args)
		{
			NetworkSingleton<TrashManager>.Instance.DestroyAllTrash();
		}
	}

	public class PlayCutscene : ConsoleCommand
	{
		public override string CommandWord => "playcutscene";

		public override string CommandDescription => "Plays the cutscene with the given name";

		public override string ExampleUsage => "playcutscene Tutorial end";

		public override void Execute(List<string> args)
		{
			if (args.Count > 0)
			{
				string name = string.Join(" ", args).ToLower();
				if (Singleton<CutsceneManager>.InstanceExists)
				{
					Singleton<CutsceneManager>.Instance.Play(name);
				}
			}
			else
			{
				LogUnrecognizedFormat(new string[1] { ExampleUsage });
			}
		}
	}

	public class SetGravityMultiplier : ConsoleCommand
	{
		public override string CommandWord => "setgravitymultiplier";

		public override string CommandDescription => "Sets the multiplier of the gravity strength.";

		public override string ExampleUsage => "setgravitymultiplier 0.5";

		public override void Execute(List<string> args)
		{
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result))
			{
				LogWarning("Unrecognized command format. Correct format example(s): " + ExampleUsage);
				return;
			}
			float gravity = Mathf.Clamp(result, -10f, 10f);
			NetworkSingleton<PhysicsManager>.Instance.SetGravityMultiplier(null, gravity);
		}
	}

	public class SetRegionUnlocked : ConsoleCommand
	{
		public override string CommandWord => "setregionunlocked";

		public override string CommandDescription => "Unlocks the given region";

		public override string ExampleUsage => "setregionunlocked downtown";

		public override void Execute(List<string> args)
		{
			if (args.Count >= 1 && Enum.TryParse<EMapRegion>(args[0], ignoreCase: true, out var result))
			{
				Singleton<ScheduleOne.Map.Map>.Instance.GetRegionData(result).SetUnlocked();
			}
			else
			{
				LogWarning("Unrecognized command format. Example usage: " + ExampleUsage);
			}
		}
	}

	public class ForceSleep : ConsoleCommand
	{
		public override string CommandWord => "forcesleep";

		public override string CommandDescription => "Forces all players to immediately sleep.";

		public override string ExampleUsage => "forcesleep";

		public override void Execute(List<string> args)
		{
			NetworkSingleton<TimeManager>.Instance.StartSleep();
		}
	}

	public class DestroyNPCs : ConsoleCommand
	{
		public override string CommandWord => "destroynpcs";

		public override string CommandDescription => "Destroys all NPCs in the scene, including employees and dealers.";

		public override string ExampleUsage => "destroynpcs";

		public override void Execute(List<string> args)
		{
			NPC[] array = Object.FindObjectsOfType<NPC>();
			foreach (NPC obj in array)
			{
				((NetworkBehaviour)obj).Despawn((DespawnType?)null);
				Object.Destroy((Object)(object)((Component)obj).gameObject);
			}
		}
	}

	public class SetDayDuration : ConsoleCommand
	{
		public override string CommandWord => "setdayduration";

		public override string CommandDescription => "Sets the (real life) duration of an in-game 24-hour cycle. Measured in real minutes.";

		public override string ExampleUsage => "setdayduration 24";

		public override void Execute(List<string> args)
		{
			float result = 0f;
			if (args.Count == 0 || !float.TryParse(args[0], out result))
			{
				LogWarning("Unrecognized command format. Correct format example(s): " + ExampleUsage);
			}
			else
			{
				NetworkSingleton<TimeManager>.Instance.SetCycleDuration(result);
			}
		}
	}

	public class SetPoliceIgnorePlayers : ConsoleCommand
	{
		public override string CommandWord => "setpoliceignoreplayers";

		public override string CommandDescription => "Sets whether police ignore players.";

		public override string ExampleUsage => "setpoliceignoreplayers true, setpoliceignoreplayers false";

		public override void Execute(List<string> args)
		{
			if (args.Count == 0 || !bool.TryParse(args[0], out var result))
			{
				LogWarning("Unrecognized command format. Correct format example(s): " + ExampleUsage);
				return;
			}
			PoliceOfficer[] array = Object.FindObjectsOfType<PoliceOfficer>(true);
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetIgnorePlayers(result);
			}
		}
	}

	[Serializable]
	public class LabelledGameObject
	{
		public string Label;

		public GameObject GameObject;
	}

	public Transform TeleportPointsContainer;

	public List<LabelledGameObject> LabelledGameObjectList;

	[Tooltip("Commands that run on startup (Editor only)")]
	public List<string> startupCommands = new List<string>();

	public static List<ConsoleCommand> Commands = new List<ConsoleCommand>();

	private static Dictionary<string, ConsoleCommand> commands = new Dictionary<string, ConsoleCommand>();

	private Dictionary<KeyCode, string> keyBindings = new Dictionary<KeyCode, string>();

	private static Player player => Player.Local;

	private static void LogCommandError(string error)
	{
		LogWarning(error);
	}

	private static void LogUnrecognizedFormat(string[] correctExamples)
	{
		string text = string.Empty;
		for (int i = 0; i < correctExamples.Length; i++)
		{
			if (i > 0)
			{
				text += ",";
			}
			text = text + "'" + correctExamples[i] + "'";
		}
		LogWarning("Unrecognized command format. Correct format example(s): " + text);
	}

	protected override void Awake()
	{
		base.Awake();
		if (!((Object)(object)Singleton<Console>.Instance != (Object)(object)this))
		{
			if (commands.Count == 0)
			{
				AddCommand(new FreeCamCommand());
				AddCommand(new Save());
				AddCommand(new SetTimeCommand());
				AddCommand(new AddItemToInventoryCommand());
				AddCommand(new ClearInventoryCommand());
				AddCommand(new ChangeCashCommand());
				AddCommand(new ChangeOnlineBalanceCommand());
				AddCommand(new GiveXP());
				AddCommand(new SpawnVehicleCommand());
				AddCommand(new SetMoveSpeedCommand());
				AddCommand(new SetJumpMultiplier());
				AddCommand(new Teleport());
				AddCommand(new SetPropertyOwned());
				AddCommand(new PackageProduct());
				AddCommand(new SetStaminaReserve());
				AddCommand(new RaisedWanted());
				AddCommand(new LowerWanted());
				AddCommand(new ClearWanted());
				AddCommand(new SetHealth());
				AddCommand(new SetTimeScale());
				AddCommand(new SetVariableValue());
				AddCommand(new SetQuestState());
				AddCommand(new SetQuestEntryState());
				AddCommand(new SetEmotion());
				AddCommand(new SetUnlocked());
				AddCommand(new SetRelationship());
				AddCommand(new AddEmployeeCommand());
				AddCommand(new SetDiscovered());
				AddCommand(new GrowPlants());
				AddCommand(new SetLawIntensity());
				AddCommand(new SetQuality());
				AddCommand(new Bind());
				AddCommand(new Unbind());
				AddCommand(new ClearBinds());
				AddCommand(new HideUI());
				AddCommand(new Disable());
				AddCommand(new Enable());
				AddCommand(new EndTutorial());
				AddCommand(new DisableNPCAsset());
				AddCommand(new ShowFPS());
				AddCommand(new HideFPS());
				AddCommand(new ClearTrash());
				AddCommand(new PlayCutscene());
				AddCommand(new SetGravityMultiplier());
				AddCommand(new SetRegionUnlocked());
				AddCommand(new ForceSleep());
				AddCommand(new DestroyNPCs());
				AddCommand(new SetDayDuration());
				AddCommand(new SetPoliceIgnorePlayers());
				AddCommand(new SetQuantity());
				AddCommand(new SetWeather());
				AddCommand(new SetWeatherSpeed());
				AddCommand(new TriggerLightning());
				AddCommand(new TriggerDistantThunder());
			}
			Player.onLocalPlayerSpawned = (Action)Delegate.Remove(Player.onLocalPlayerSpawned, new Action(RunStartupCommands));
			Player.onLocalPlayerSpawned = (Action)Delegate.Combine(Player.onLocalPlayerSpawned, new Action(RunStartupCommands));
		}
	}

	private void AddCommand(ConsoleCommand command)
	{
		if (!commands.ContainsKey(command.CommandWord))
		{
			commands.Add(command.CommandWord, command);
			Commands.Add(command);
		}
	}

	private void RunStartupCommands()
	{
		if (!Application.isEditor && !Debug.isDebugBuild)
		{
			return;
		}
		Log($"Running {startupCommands.Count} startup console commands. (Editor/Debug build only)");
		foreach (string startupCommand in startupCommands)
		{
			SubmitCommand(startupCommand);
		}
	}

	[HideInCallstack]
	public static void Log(object message, Object context = null)
	{
		Debug.Log(message, context);
	}

	[HideInCallstack]
	public static void LogWarning(object message, Object context = null)
	{
		Debug.LogWarning(message, context);
	}

	[HideInCallstack]
	public static void LogError(object message, Object context = null)
	{
		Debug.LogError(message, context);
	}

	public static void SubmitCommand(List<string> args)
	{
		if (args.Count != 0 && (InstanceFinder.IsHost || Application.isEditor || Debug.isDebugBuild))
		{
			for (int i = 0; i < args.Count; i++)
			{
				args[i] = args[i].ToLower();
			}
			string text = args[0];
			if (commands.TryGetValue(text, out var value))
			{
				args.RemoveAt(0);
				value.Execute(args);
			}
			else
			{
				LogWarning("Command '" + text + "' not found.");
			}
		}
	}

	public static void SubmitCommand(string args)
	{
		SubmitCommand(new List<string>(args.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
	}

	public unsafe void AddBinding(KeyCode key, string command)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		Log("Binding " + ((object)(*(KeyCode*)(&key))/*cast due to .constrained prefix*/).ToString() + " to " + command);
		if (keyBindings.ContainsKey(key))
		{
			keyBindings[key] = command;
		}
		else
		{
			keyBindings.Add(key, command);
		}
	}

	public unsafe void RemoveBinding(KeyCode key)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		Log("Unbinding " + ((object)(*(KeyCode*)(&key))/*cast due to .constrained prefix*/).ToString());
		keyBindings.Remove(key);
	}

	public void ClearBindings()
	{
		Log("Clearing all key bindings");
		keyBindings.Clear();
	}

	private void Update()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (GameInput.IsTyping || Singleton<PauseMenu>.Instance.IsPaused)
		{
			return;
		}
		foreach (KeyValuePair<KeyCode, string> keyBinding in keyBindings)
		{
			if (Input.GetKeyDown(keyBinding.Key))
			{
				SubmitCommand(keyBinding.Value);
			}
		}
	}
}
