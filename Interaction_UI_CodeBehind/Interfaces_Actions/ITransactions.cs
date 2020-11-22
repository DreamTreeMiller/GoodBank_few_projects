using Interfaces_Data;
using System.Collections.ObjectModel;

namespace Interfaces
{
	public interface ITransactions
	{
		ObservableCollection<ITransaction> GetAccountTransactionsLog(uint accID);
	}
}
