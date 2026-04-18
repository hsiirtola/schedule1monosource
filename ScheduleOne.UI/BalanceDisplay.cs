using ScheduleOne.Money;
using TMPro;
using UnityEngine;

namespace ScheduleOne.UI;

public class BalanceDisplay : MonoBehaviour
{
	[Header("References")]
	public TextMeshProUGUI BalanceLabel;

	public void SetBalance(float balance)
	{
		((TMP_Text)BalanceLabel).text = MoneyManager.FormatAmount(balance);
	}
}
