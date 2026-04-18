using ScheduleOne.Core.Items.Framework;

namespace ScheduleOne.ItemFramework;

public class ItemFilter_LegalStatus : ItemFilter
{
	public ELegalStatus RequiredLegalStatus;

	public ItemFilter_LegalStatus(ELegalStatus requiredLegalStatus)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		RequiredLegalStatus = requiredLegalStatus;
	}

	public override bool DoesItemMatchFilter(ItemInstance instance)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (instance == null)
		{
			return false;
		}
		if (((BaseItemDefinition)instance.Definition).legalStatus != RequiredLegalStatus)
		{
			return false;
		}
		return base.DoesItemMatchFilter(instance);
	}
}
