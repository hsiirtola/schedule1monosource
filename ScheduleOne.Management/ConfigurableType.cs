namespace ScheduleOne.Management;

public static class ConfigurableType
{
	public static string GetTypeName(EConfigurableType type)
	{
		switch (type)
		{
		case EConfigurableType.Pot:
			return "Pot";
		case EConfigurableType.PackagingStation:
			return "Packaging Station";
		case EConfigurableType.LabOven:
			return "Lab Oven";
		case EConfigurableType.Botanist:
			return "Botanist";
		case EConfigurableType.Packager:
			return "Handler";
		case EConfigurableType.ChemistryStation:
			return "Chemistry Station";
		case EConfigurableType.Chemist:
			return "Chemist";
		case EConfigurableType.Cauldron:
			return "Cauldron";
		case EConfigurableType.Cleaner:
			return "Cleaner";
		case EConfigurableType.BrickPress:
			return "Brick Press";
		case EConfigurableType.MixingStation:
			return "Mixing Station";
		case EConfigurableType.DryingRack:
			return "Drying Rack";
		case EConfigurableType.SpawnStation:
			return "Mushroom Spawn Station";
		case EConfigurableType.MushroomBed:
			return "Mushroom Bed";
		case EConfigurableType.Storage:
			return "Storage";
		default:
			Console.LogError("Unknown Configurable Type: " + type);
			return string.Empty;
		}
	}
}
