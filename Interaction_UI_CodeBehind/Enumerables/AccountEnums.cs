namespace Enumerables
{
	public enum AccountType
	{
		Current,
		Deposit,
		Credit,
		Total
	}

	public enum AccountStatus
	{
		Opened,
		Closed
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
		Monthly,
		Annually,
		AtTheEnd,
		NoRecalc
	}
}
