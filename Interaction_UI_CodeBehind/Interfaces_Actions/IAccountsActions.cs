using Enumerables;
using Interfaces_Data;
using System.Collections.ObjectModel;

namespace Interfaces
{
	public interface IAccountsActions
	{
		IAccount GetAccountByID(uint id);

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
			GetClientAccounts(uint ID);

		ObservableCollection<IAccountDTO> GetClientAccounts(uint clientID, AccountType accType);

		ObservableCollection<IAccountDTO> GetClientAccountsToAccumulateInterest(uint ID);

		ObservableCollection<IAccount> GetTopupableAccountsToWireFrom(uint sourceAccID);


		IAccountDTO AddAccount(IAccountDTO acc); 

		IAccountDTO GenerateAccount(IAccountDTO acc);

		IAccount TopUpCash(uint accID, double cashAmount);

		IAccount WithdrawCash(uint accID, double amount);

		IAccount CloseAccount(uint accID, out double accumulatedAmount);

		void Wire(uint sourceAccID, uint destAccID, double amount);

		/// <summary>
		/// Увеличивает внутреннюю дату на 1 месяц и пересчитывает проценты у всех счетов
		/// </summary>
		void AddOneMonth();
	}
}
