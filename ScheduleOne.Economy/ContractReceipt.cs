using System;
using ScheduleOne.DevUtilities;
using ScheduleOne.GameTime;

namespace ScheduleOne.Economy;

[Serializable]
public class ContractReceipt
{
	public int ReceiptId;

	public EContractParty CompletedBy;

	public string CustomerId;

	public GameDateTime CompletionTime;

	public StringIntPair[] Items;

	public float AmountPaid;

	public ContractReceipt(int receiptId, EContractParty completedBy, string customerID, GameDateTime completionTime, StringIntPair[] items, float amountPaid)
	{
		ReceiptId = receiptId;
		CompletedBy = completedBy;
		CustomerId = customerID;
		CompletionTime = completionTime;
		Items = items;
		AmountPaid = amountPaid;
	}

	public ContractReceipt()
	{
		ReceiptId = -1;
		CompletedBy = EContractParty.Player;
		CustomerId = string.Empty;
		CompletionTime = new GameDateTime(0, 0);
		Items = new StringIntPair[0];
		AmountPaid = 0f;
	}
}
