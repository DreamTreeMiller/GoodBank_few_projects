using Interfaces;
using Interfaces_Data;
using Transaction_Class;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BankInside
{
	public partial class GoodBank : ITransactions
	{
		private List<Transaction> log;

		public void WriteLog(Transaction t)
		{
			log.Add(t);
		}

		/// <summary>
		/// Формирует список всех транзакций указанного счета
		/// </summary>
		/// <param name="account"></param>
		/// <returns></returns>
		public ObservableCollection<ITransaction> GetAccountTransactionsLog(uint accID)
		{
			ObservableCollection<ITransaction> accountLog = new ObservableCollection<ITransaction>();
			foreach (var t in log)
				if (t.TransactionAccountID == accID) accountLog.Add(t);
			return accountLog;
		}
	}
}
