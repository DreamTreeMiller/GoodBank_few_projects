using Enumerables;
using Interfaces_Data;
using System.Data;

namespace Interfaces_Actions
{
	public interface IAccountsActions
	{
		/// <summary>
		/// Извлекает из базы (AccountsParent, SavingAccounts, DepositAccounts, CreditAccounts)
		/// данные о счёте и помещает их в Data Transfer Object
		/// </summary>
		/// <param name="accountRow"></param>
		/// <returns></returns>
		IAccountDTO GetAccountByID(int accID);

		/// <summary>
		/// Находит список всех счетов, принадлежащих клиентам данного типа
		/// </summary>
		/// <param name="clientType">ВИП, обычный клиент или организация</param>
		/// <returns>
		/// Коллекцию счетов, принадлежащих клиентам данного типа
		/// </returns>
		(DataView accountsViewTable, decimal totalSaving, decimal totalDeposit, decimal totalCredit)
			GetAccountsList(ClientType clientType);

		(DataView accountsViewTable, decimal totalSaving, decimal totalDeposit, decimal totalCredit)
			GetClientAccounts(int ID);

		DataView GetClientSavingAccounts(int clientID);

		DataView GetClientAccountsToAccumulateInterest(int ID);

		DataView GetTopupableAccountsToWireFrom(int sourceAccID);

		IAccountDTO AddAccount(IAccountDTO acc); 

		IAccountDTO TopUpCash(int accID, decimal cashAmount);

		IAccountDTO WithdrawCash(int accID, decimal amount);

		IAccountDTO CloseAccount(int accID, out decimal accumulatedAmount);

		(IAccountDTO senderAcc, IAccountDTO recipientAcc) Wire(int sourceAccID, int destAccID, decimal amount);

		/// <summary>
		/// Увеличивает внутреннюю дату на 1 месяц и пересчитывает проценты у всех счетов
		/// </summary>
		void AddOneMonth();
	}
}
