using Interfaces_Data;
using System.Collections.ObjectModel;

namespace Interfaces_Actions
{
	public interface ITransactions
	{
		ObservableCollection<ITransaction> GetAccountTransactionsLog(int accID);
	}
}
