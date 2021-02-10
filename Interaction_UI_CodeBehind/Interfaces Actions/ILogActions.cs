using System.Data;

namespace Interfaces_Actions
{
	public interface ILogActions
	{
		DataView GetAccountTransactionsLog(int accID);
	}
}
