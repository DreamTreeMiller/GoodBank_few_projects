using BankTime;
using Interfaces_Data;
using Transaction;
using System;

namespace Enumerables
{
	public class AccountCredit : Account
	{
		public override AccountType AccType { get => AccountType.Credit; }
		public decimal	AccumulatedInterest { get; set; }

		/// <summary>
		/// Создание счета на основе введенных данных
		/// </summary>
		/// <param name="acc">Данные для открытия счета</param>
		/// Напоминалка, что инициализируется в базовом классе
		/// ClientID	  = clientID;				--> из IAccountDTO acc
		/// ClientType	  = clientType;				--> из IAccountDTO acc
		/// ID			  = NextID();
		/// AccountNumber = $"{ID:000000000000}";	--> добавляется CRE
		/// Compounding	  = compounding;			--> из IAccountDTO acc
		/// CompoundAccID = compAccID;				--> из IAccountDTO acc
		/// Balance		  = 0;
		/// Interest	  = interest;				--> из IAccountDTO acc
		/// AccountStatus = AccountStatus.Opened;
		/// Opened		  = GoodBankTime.Today;
		/// Topupable	  =							--> false
		/// WithdrawalAllowed	=					--> false
		/// RecalcPeriod  =							--> monthly
		/// EndDate		  =							--> из IAccountDTO acc 
		public AccountCredit(IAccountDTO acc, Action<TransactionDTO> writeloghandler)
			: base( "CRE", acc.ClientID, acc.ClientType,
					acc.Balance, acc.Compounding, acc.Interest,
					true, false, RecalcPeriod.Monthly, acc.Duration, writeloghandler)
		{
			TransactionDTO openAccountTransaction = new TransactionDTO(
				AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				"",
				"",
				OperationType.OpenAccount,
				Balance,
				"Кредитный счет " + AccountNumber
				+ " на сумму " + Balance + " руб."
				+ " открыт."
				);
			OnWriteLog(openAccountTransaction);
		}

		/// <summary>
		/// Констркуктор для искусственной генерации счета. 
		/// Включает в себя поле даты открытия счета
		/// </summary>
		/// <param name="acc"></param>
		/// <param name="opened"></param>
		public AccountCredit(IAccountDTO acc, DateTime opened, Action<TransactionDTO> writeloghandler)
			: base( "CRE", acc.ClientID, acc.ClientType,
					acc.Balance, acc.Compounding, acc.Interest,
					opened,
					true, false, RecalcPeriod.Monthly, acc.Duration,
					acc.MonthsElapsed,
					writeloghandler)
		{
			TransactionDTO openAccountTransaction = new TransactionDTO(
				AccID,
				Opened,
				"",
				"",
				OperationType.OpenAccount,
				Balance,
				"Кредитный счет " + AccountNumber
				+ " на сумму " + Balance + " руб."
				+ " открыт."
				);
			OnWriteLog(openAccountTransaction);
		}

		/// <summary>
		/// Пересчет процентов для кредитов. Происходит только раз в месяц,
		/// т.е. не раз в год, и не один раз в конце
		/// </summary>
		/// <param name="date"></param>
		public override decimal RecalculateInterest()
		{
			if (IsBlocked) return 0;

			if (Closed != null) return 0;

			NumberOfTopUpsInDay = 0;

			// Пересчёт не нужен. 
			// Клиент должен пополнить счет до 0 и закрыть
			if (Duration == MonthsElapsed) return 0;
			MonthsElapsed++;

			decimal calculatedInterest = Balance * (decimal)Interest / 12;
			AccumulatedInterest		 += calculatedInterest;
			Balance					 += calculatedInterest;

			TransactionDTO interestAccrualTransaction = new TransactionDTO(
				AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				"",
				AccountNumber,
				OperationType.InterestAccrual,
				calculatedInterest,
				"Начисление процентов на счет " + AccountNumber
				+ $" на сумму {calculatedInterest:N2} руб."
				);
			OnWriteLog(interestAccrualTransaction);
			return calculatedInterest;
		}

		public override decimal CloseAccount()
		{
			return base.CloseAccount();
		}
	}
}
