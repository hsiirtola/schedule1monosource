using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using UnityEngine;

namespace ScheduleOne.StationFramework;

public class LiquidMeth_StationItem : StationItem
{
	public LiquidMethVisuals Visuals;

	public override void Initialize(StorableItemDefinition itemDefinition)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		base.Initialize(itemDefinition);
		LiquidMethDefinition liquidMethDefinition = itemDefinition as LiquidMethDefinition;
		if ((Object)(object)Visuals != (Object)null)
		{
			Visuals.Setup(liquidMethDefinition);
		}
		GetModule<CookableModule>().LiquidColor = liquidMethDefinition.CookableLiquidColor;
		GetModule<CookableModule>().SolidColor = liquidMethDefinition.CookableSolidColor;
		GetModule<PourableModule>().LiquidColor = liquidMethDefinition.LiquidVolumeColor;
		GetModule<PourableModule>().PourParticlesColor = liquidMethDefinition.PourParticlesColor;
	}
}
