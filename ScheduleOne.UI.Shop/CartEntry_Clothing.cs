using ScheduleOne.Clothing;
using TMPro;

namespace ScheduleOne.UI.Shop;

public class CartEntry_Clothing : CartEntry
{
	protected override void UpdateTitle()
	{
		base.UpdateTitle();
		if ((base.Listing.Item as ClothingDefinition).Colorable)
		{
			TextMeshProUGUI nameLabel = NameLabel;
			((TMP_Text)nameLabel).text = ((TMP_Text)nameLabel).text + " (" + (base.Listing as ClothingShopListing).Color.GetLabel() + ")";
		}
	}
}
