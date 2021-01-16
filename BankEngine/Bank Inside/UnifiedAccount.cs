using BankTime;
using Interfaces_Data;
using Transaction;
using System;
using DTO;
using Enumerables;

namespace BankInside
{
	public class UnifiedAccount
	{
		#region События

		/// <summary>
		/// Запись транзакции в журнал
		/// </summary>
		public event Action<TransactionDTO> WriteLog;


		#endregion

		#region Общие методы для всех типов счетов

		/// <summary>
		/// Пополнение счета наличкой
		/// </summary>
		/// <param name="cashAmount"></param>
		public IAccountDTO TopUpCash(IAccountDTO acc, decimal cashAmount)
		{
			if (cashAmount >= 1000)
			{
				if (++acc.NumberOfTopUpsInDay > 3)
				{
					acc.Topupable			= false;
					acc.WithdrawalAllowed	= false;
					acc.IsBlocked			= true;

					TransactionDTO blockAccountTransaction = new TransactionDTO(
						acc.AccID,
						GoodBankTime.GetBanksTodayWithCurrentTime(),
						"",
						acc.AccountNumber,
						OperationType.BlockAccount,
						0,
						"Счет заблокирован, количество пополнений больше или равных 1000 руб. превысило 3."
						);
					WriteLog?.Invoke(blockAccountTransaction);

					throw new AccountOperationException(ExceptionErrorCodes.AccountIsBlcoked);
				}
			}
			acc.Balance += cashAmount;
			TransactionDTO topUpCashTransaction = new TransactionDTO(
				acc.AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				"",
				acc.AccountNumber,
				OperationType.CashDeposit,
				cashAmount,
				"Пополнение счета " + acc.AccountNumber + $" наличными средствами на сумму {cashAmount:N2} руб."
				);
			WriteLog?.Invoke(topUpCashTransaction);
			return acc;
		}

		/// <summary>
		/// Снятие налички со счета
		/// </summary>
		/// <param name="cashAmount"></param>
		public IAccountDTO WithdrawCash(IAccountDTO acc, decimal cashAmount)
		{
			acc.Balance -= cashAmount;
			TransactionDTO withdrawCashTransaction = new TransactionDTO(
				acc.AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				acc.AccountNumber,
				"",
				OperationType.CashWithdrawal,
				cashAmount,
				"Снятие со счета " + acc.AccountNumber + $" наличных средств на сумму {cashAmount:N2} руб."
				);
			WriteLog?.Invoke(withdrawCashTransaction);
			return acc;
		}

		/// <summary>
		/// Получение перевода на счет денег со счета-источника
		/// </summary>
		/// <param name="senderAcc">Счет-источник</param>
		/// <param name="wireAmount">сумма перевода</param>
		public IAccountDTO ReceiveFromAccount(string senderAccNumber, IAccountDTO recipientAcc, decimal wireAmount)
		{
			recipientAcc.Balance += wireAmount;
			TransactionDTO DepositFromAccountTransaction = new TransactionDTO(
				recipientAcc.AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				senderAccNumber,
				recipientAcc.AccountNumber,
				OperationType.ReceiveWireFromAccount,
				wireAmount,
				"Получение со счета " + senderAccNumber
				+ " на счет " + recipientAcc.AccountNumber 
				+ $" средств на сумму {wireAmount:N2} руб."
				);
			WriteLog?.Invoke(DepositFromAccountTransaction);
			return recipientAcc;
		}

		/// <summary>
		/// Перевод средств со счета на счет-получатель
		/// </summary>
		/// <param name="recipientAcc">Счет-получатель</param>
		/// <param name="wireAmount">Сумма перевода</param>
		public IAccountDTO SendToAccount(IAccountDTO senderAcc, string recipientAccNumber, decimal wireAmount)
		{
			senderAcc.Balance -= wireAmount;
			TransactionDTO withdrawCashTransaction = new TransactionDTO(
				senderAcc.AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				senderAcc.AccountNumber,
				recipientAccNumber,
				OperationType.SendWireToAccount,
				wireAmount,
				"Перевод со счета " + senderAcc.AccountNumber
				+ " на счет " + recipientAccNumber
				+ $" средств на сумму {wireAmount:N2} руб."
				);
			WriteLog?.Invoke(withdrawCashTransaction);
			return senderAcc;
		}

		#endregion

		#region Абстрактные и виртуальные методы

		/// <summary>
		/// Делает пересчет процентов на указанную дату
		/// Вызывается извне при изменении даты
		/// </summary>
		/// <returns>
		/// Сумму начисленных процентов, если её надо перевести на другой счет
		/// </returns>
		//public decimal RecalculateInterest();

		/// <summary>
		/// Закрывает счет: обнуляет баланс и накопленный процент
		/// ставит запрет на пополнение и снятие.
		/// Устанавливает дату закрытия счета
		/// </summary>
		/// <returns>
		/// Накопленную сумму
		/// </returns>
		public IAccountDTO CloseAccount(IAccountDTO acc, out decimal accumulatedAmount)
		{
			accumulatedAmount = acc.Balance;
			acc = WithdrawCash(acc, acc.Balance);
			acc.Topupable			= false;
			acc.WithdrawalAllowed	= false;
			acc.Closed				= GoodBankTime.Today;

			TransactionDTO closeTransaction = new TransactionDTO(
				acc.AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				"",
				"",
				OperationType.CloseAccount,
				0,
				"Счет " + acc.AccountNumber + " закрыт."
				);
			WriteLog?.Invoke(closeTransaction);

			return acc;
		}

		#endregion
	}
}
