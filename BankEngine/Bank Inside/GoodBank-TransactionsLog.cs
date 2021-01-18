using Interfaces_Actions;
using System.Data;

namespace BankInside
{
	public partial class GoodBank : ITransactions
	{

		/// <summary>
		/// Формирует список всех транзакций указанного счета
		/// </summary>
		/// <param name="account"></param>
		/// <returns></returns>
		public DataView GetAccountTransactionsLog(int accID)
		{
			return TransactionAction.GetAccountTransactionsLog(accID);
		}
	}
}
