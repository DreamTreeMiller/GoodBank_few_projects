using System.Data;

namespace Interfaces_Actions
{
	public interface ITransactions
	{
		DataView GetAccountTransactionsLog(int accID);
	}
}
