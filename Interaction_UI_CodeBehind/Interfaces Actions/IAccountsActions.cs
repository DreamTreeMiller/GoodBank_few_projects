using Enumerables;
using Interfaces_Data;
using System.Collections.ObjectModel;

namespace Interfaces_Actions
{
	public interface IAccountsActions
	{
		IAccount GetAccountByID(int id);

		/// <summary>
		/// Находит список всех счетов, принадлежащих клиентам данного типа
		/// </summary>
		/// <param name="clientType">ВИП, обычный клиент или организация</param>
		/// <returns>
		/// Коллекцию счетов, принадлежащих клиентам данного типа
		/// </returns>
		(ObservableCollection<IAccountDTO> accList, double totalCurr, double totalDeposit, double totalCredit)
			GetAccountsList(ClientType clientType);

		(ObservableCollection<IAccountDTO> accList, double totalCurr, double totalDeposit, double totalCredit)
			GetClientAccounts(int ID);

		ObservableCollection<IAccountDTO> GetClientAccounts(int clientID, AccountType accType);

		ObservableCollection<IAccountDTO> GetClientAccountsToAccumulateInterest(int ID);

		ObservableCollection<IAccount> GetTopupableAccountsToWireFrom(int sourceAccID);


		IAccountDTO AddAccount(IAccountDTO acc); 

		IAccountDTO GenerateAccount(IAccountDTO acc);

		IAccount TopUpCash(int accID, double cashAmount);

		IAccount WithdrawCash(int accID, double amount);

		IAccount CloseAccount(int accID, out double accumulatedAmount);

		void Wire(int sourceAccID, int destAccID, double amount);

		/// <summary>
		/// Увеличивает внутреннюю дату на 1 месяц и пересчитывает проценты у всех счетов
		/// </summary>
		void AddOneMonth();
	}
}
