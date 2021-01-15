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
		OpenAccount,
		CloseAccount,
		CashDeposit,
		CashWithdrawal,
		ReceiveWireFromAccount,
		SendWireToAccount,
		InterestAccrual,
		BlockAccount
	}

	public enum RecalcPeriod
	{
		Monthly		= 0,
		Annually	= 1,
		AtTheEnd	= 2,
		NoRecalc	= 3
	}
}
