using Interfaces_Actions;
using Interfaces_Data;
using Transaction;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BankInside
{
	public partial class GoodBank : ITransactions
	{
		private List<TransactionDTO> log = new List<TransactionDTO>();

		public void WriteLog(TransactionDTO t)
		{
			log.Add(t);
		}

		/// <summary>
		/// Формирует список всех транзакций указанного счета
		/// </summary>
		/// <param name="account"></param>
		/// <returns></returns>
		public ObservableCollection<ITransaction> GetAccountTransactionsLog(int accID)
		{
			ObservableCollection<ITransaction> accountLog = new ObservableCollection<ITransaction>();
			foreach (var t in log)
				if (t.TransactionAccountID == accID) accountLog.Add(t);
			return accountLog;
		}
	}
}
