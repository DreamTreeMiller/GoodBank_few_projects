namespace Enumerables
{
	public enum AccountType
	{
		Saving  = 0,
		Deposit = 1,
		Credit  = 2,
		Total   = 3
	}

	public enum OperationType
	{
		OpenAccount				= 0,
		CloseAccount			= 1,
		CashDeposit				= 2,
		CashWithdrawal			= 3,
		ReceiveWireFromAccount	= 4,
		SendWireToAccount		= 5,
		InterestAccrual			= 6,
		BlockAccount			= 7
	}

	public enum RecalcPeriod
	{
		Monthly		= 0,
		Annually	= 1,
		AtTheEnd	= 2,
		NoRecalc	= 3
	}
}
