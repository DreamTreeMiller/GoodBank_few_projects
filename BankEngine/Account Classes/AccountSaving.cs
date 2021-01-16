using BankTime;
using Interfaces_Data;
using Transaction;
using System;

namespace Enumerables
{
	public class AccountSaving : Account
	{
		public override AccountType AccType { get => AccountType.Saving; }

		/// <summary>
		/// Создание счета на основе введенных данных
		/// </summary>
		/// <param name="acc">Данные для открытия счета</param>
		/// Напоминалка, что инициализируется в базовом классе
		/// ClientID	  = clientID;				--> из IAccountDTO acc
		/// ClientType	  = clientType;				--> из IAccountDTO acc
		/// ID			  = NextID();
		/// AccountNumber = $"{ID:000000000000}";	--> добавляется SAV
		/// Compounding	  = compounding;			--> из IAccountDTO acc
		/// CompoundAccID = compAccID;				--> из IAccountDTO acc
		/// Balance		  = 0;
		/// Interest	  = interest;				--> из IAccountDTO acc
		/// AccountStatus = AccountStatus.Opened;
		/// Opened		  = GoodBankTime.Today;
		/// Topupable	  =							--> true
		/// WithdrawalAllowed	=					--> ture
		/// RecalcPeriod  =							--> No recalc period
		/// EndDate		  =							--> null 
		public AccountSaving(IAccountDTO acc, Action<TransactionDTO> writeloghandler)
			: base( "SAV", acc.ClientID, acc.ClientType, 
					acc.Balance, acc.Compounding, acc.Interest,
					true, true, RecalcPeriod.NoRecalc, 0, writeloghandler)
		{
			TransactionDTO openAccountTransaction = new TransactionDTO(
				AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				"",
				"",
				OperationType.OpenAccount,
				Balance,
				"Текущий счет " + AccountNumber 
				+ " с начальной суммой " + Balance + " руб."
				+ " открыт."
				);
			OnWriteLog(openAccountTransaction);
		}

		/// <summary>
		/// Констркуктор для искусственной генерации счета. 
		/// Включает в себя поле даты открытия счета, 0 - кол-во месяцев с даты открытия
		/// </summary>
		/// <param name="acc"></param>
		/// <param name="opened"></param>
		public AccountSaving(IAccountDTO acc, DateTime opened, Action<TransactionDTO> writeloghandler)
			: base( "SAV", acc.ClientID, acc.ClientType, 
					acc.Balance, acc.Compounding, acc.Interest,
					opened,
					true, true, RecalcPeriod.NoRecalc, 0,
					0, // months elapsed
					writeloghandler)
		{
			TransactionDTO openAccountTransaction = new TransactionDTO(
				AccID,
				Opened,
				"",
				"",
				OperationType.OpenAccount,
				Balance,
				"Текущий счет " + AccountNumber
				+ " с начальной суммой " + Balance + " руб."
				+ " открыт."
				);
			OnWriteLog(openAccountTransaction);
		}

		public override decimal RecalculateInterest()
		{
			if (IsBlocked) return 0;

			NumberOfTopUpsInDay = 0;
			// Do nothing since no interest recalculation for Saving account
			return 0;
		}

		public override decimal CloseAccount()
		{
			return base.CloseAccount();
		}
	}
}
