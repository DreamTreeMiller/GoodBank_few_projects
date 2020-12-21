using BankTime;
using Interfaces_Data;
using Transaction_Class;
using System;
using DTO;

namespace Enumerables
{
	public abstract class Account : IAccount
	{
		#region Статическая часть для генерации уникального ID

		/// <summary>
		/// Текущий ID счета
		/// </summary>
		private static uint		staticID;

		/// <summary>
		/// Статический конструктор. Обнуляет счетчик ID
		/// </summary>
		static Account()
		{
			staticID = 0;
		}

		/// <summary>
		/// Герерирует следующий ID
		/// </summary>
		/// <returns>New unique ID</returns>
		private static uint		NextID()
		{
			staticID++;
			return staticID;
		}

		#endregion

		#region Общие поля для всех счетов
		
		/// <summary>
		/// Тип клиента, которому принадлежит счет.
		/// Это избыточное поле, но благодаря ему делается всего один проход по базе 
		/// при показе счетов одного типа клиентов
		/// </summary>
		public ClientType		ClientType			{ get; set; }

		/// <summary>
		/// ID владельца счета. 
		/// </summary>
		public uint				ClientID			{ get; set; }

		/// <summary>
		/// Тип счета текущий, вклад или кредит
		/// Дублирование, т.к. тип счета определяется его классом
		/// </summary>
		public abstract AccountType		AccType		{ get; }

		/// <summary>
		/// Уникальный ID счёта - используем для базы
		/// </summary>
		public uint				AccID				{ get; }

		/// <summary>
		/// Уникальный номер счёта. 
		/// Числовая часть совпадает с ID. 
		/// Есть префикс, указывающий тип счета
		/// </summary>
		public string			AccountNumber		{ get; set; }

		/// <summary>
		/// Баланс счёта. Для разных типов разный
		/// Текущий - остаток
		/// Вклад	- сумма вклада
		/// Кредит	- сумма долга
		/// </summary>
		public double			Balance				{ get; set; }

		/// <summary>
		/// Процент. 0 для текущего, прирорст для вклада, минус для долга
		/// </summary>
		public double			Interest			{ get; set; }

		/// <summary>
		/// С капитализацией или без
		/// </summary>
		public bool				Compounding			{ get; set; }

		/// <summary>
		/// Дата открытия счета
		/// </summary>
		public DateTime			Opened				{ get; set; }

		/// <summary>
		/// Количество месяцев, на который открыт вклад, выдан кредит.
		/// 0 - бессрочно
		/// </summary>
		public int				Duration			{ get; set; }

		/// <summary>
		/// Количество месяцев, прошедших с открытия вклада
		/// </summary>
		public int				MonthsElapsed		{ get; set; }

		/// <summary>
		/// Дата окончания вклада/кредита. 
		/// null - бессрочно
		/// </summary>
		public DateTime?		EndDate				{ get; }

		/// <summary>
		/// Дата закрытия счета. Только для закрытых
		/// Если счет открыт, то равен null
		/// </summary>
		public DateTime?		Closed				{ get; set; }

		/// <summary>
		/// Пополняемый счет или нет
		/// У закрытого счета - false
		/// </summary>
		public bool				Topupable			{ get; set; }

		/// <summary>
		/// С правом частичного снятия или нет
		/// У закрытого счета - false
		/// </summary>
		public bool				WithdrawalAllowed	{ get; set; }

		/// <summary>
		/// Период пересчета процентов - ежемесячно, ежегодно, один раз в конце
		/// </summary>
		public RecalcPeriod		RecalcPeriod		{ get; set; }

		#endregion

		#region Поля противодействия отмыванию денег

		protected int			NumberOfTopUpsInDay = 0;

		public bool				IsBlocked			{ get; set; } = false;

		#endregion

		#region Конструктор

		/// <summary>
		/// Создание счета
		/// </summary>
		/// <param name="clientID"></param>
		/// <param name="clientType"></param>
		/// <param name="compounding"></param>
		/// <param name="compAccID"></param>
		/// <param name="interest"></param>
		public Account( uint clientID, ClientType clientType, bool compounding, double interest,
						bool topup, bool withdrawal, RecalcPeriod recalc, int duration,
						Action<Transaction> writeloghandler)
		{
			ClientID			= clientID;
			ClientType			= clientType;
			AccID				= NextID();
			AccountNumber		= $"{AccID:000000000000}";
			Compounding			= compounding;
			Balance				= 0;
			Interest			= interest;
			Opened				= GoodBankTime.Today;
			Topupable			= topup;
			WithdrawalAllowed	= withdrawal;
			RecalcPeriod		= recalc;
			Duration			= duration;
			MonthsElapsed		= 0;
			EndDate				= Duration == 0 ? null : (DateTime?)Opened.AddMonths(Duration);
			WriteLog		   += writeloghandler;
		}

		/// <summary>
		/// Конструктор для генератора случайных счетов. Добавляется дата
		/// </summary>
		/// <param name="clientID"></param>
		/// <param name="clientType"></param>
		/// <param name="compounding"></param>
		/// <param name="compAccID"></param>
		/// <param name="interest"></param>
		public Account(uint clientID, ClientType clientType, bool compounding, double interest,
						DateTime opened,
						bool topup, bool withdrawal, RecalcPeriod recalc, int duration,
						Action<Transaction> writeloghandler)
		{
			ClientID			= clientID;
			ClientType			= clientType;
			AccID				= NextID();
			AccountNumber		= $"{AccID:000000000000}";
			Compounding			= compounding;
			Balance				= 0;
			Interest			= interest;
			Opened				= opened;
			Topupable			= topup;
			WithdrawalAllowed	= withdrawal;
			RecalcPeriod		= recalc;
			Duration			= duration;
			EndDate = Duration == 0 ? null : (DateTime?)Opened.AddMonths(Duration);
			WriteLog		   += writeloghandler;
		}

		#endregion

		#region События

		/// <summary>
		/// Запись транзакции в журнал
		/// </summary>
		public event Action<Transaction> WriteLog;

		protected virtual void OnWriteLog(Transaction tr)
		{
			WriteLog?.Invoke(tr);
		}

		#endregion

		#region Общие методы для всех типов счетов

		/// <summary>
		/// Пополнение счета наличкой
		/// </summary>
		/// <param name="cashAmount"></param>
		public void TopUpCash(double cashAmount)
		{
			if (cashAmount >= 1000)
			{
				if (++NumberOfTopUpsInDay > 3)
				{
					Topupable = false;
					WithdrawalAllowed = false;
					IsBlocked = true;

					Transaction blockAccountTransaction = new Transaction(
						AccID,
						GoodBankTime.GetBanksTodayWithCurrentTime(),
						"",
						AccountNumber,
						OperationType.BlockAccount,
						0,
						"Счет заблокирован, количество пополнений больше или равных 1000 руб. превысило 3."
						);
					WriteLog?.Invoke(blockAccountTransaction);

					throw new AccountOperationException(ExceptionErrorCodes.AccountIsBlcoked);
				}
			}
			Balance += cashAmount;
			Transaction topUpCashTransaction = new Transaction(
				AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				"",
				AccountNumber,
				OperationType.CashDeposit,
				cashAmount,
				"Пополнение счета " + AccountNumber + $" наличными средствами на сумму {cashAmount:N2} руб."
				);
			WriteLog?.Invoke(topUpCashTransaction);
		}

		/// <summary>
		/// Снятие налички со счета
		/// </summary>
		/// <param name="cashAmount"></param>
		public double WithdrawCash(double cashAmount)
		{
			Balance -= cashAmount;
			Transaction withdrawCashTransaction = new Transaction(
				AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				AccountNumber,
				"",
				OperationType.CashWithdrawal,
				cashAmount,
				"Снятие со счета " + AccountNumber + $" наличных средств на сумму {cashAmount:N2} руб."
				);
			WriteLog?.Invoke(withdrawCashTransaction);
			return cashAmount;
		}

		/// <summary>
		/// Получение перевода на счет денег со счета-источника
		/// </summary>
		/// <param name="sourceAcc">Счет-источник</param>
		/// <param name="wireAmount">сумма перевода</param>
		public void ReceiveFromAccount(IAccount sourceAcc, double wireAmount)
		{
			Balance += wireAmount;
			Transaction DepositFromAccountTransaction = new Transaction(
				AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				sourceAcc.AccountNumber,
				AccountNumber,
				OperationType.ReceiveWireFromAccount,
				wireAmount,
				"Получение со счета " + sourceAcc.AccountNumber
				+ " на счет " + AccountNumber 
				+ $" средств на сумму {wireAmount:N2} руб."
				);
			WriteLog?.Invoke(DepositFromAccountTransaction);
		}

		/// <summary>
		/// Перевод средств со счета на счет-получатель
		/// </summary>
		/// <param name="destAcc">Счет-получатель</param>
		/// <param name="wireAmount">Сумма перевода</param>
		public void SendToAccount(IAccount destAcc, double wireAmount)
		{
			Balance -= wireAmount;
			Transaction withdrawCashTransaction = new Transaction(
				AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				AccountNumber,
				destAcc.AccountNumber,
				OperationType.SendWireToAccount,
				wireAmount,
				"Перевод со счета " + AccountNumber
				+ " на счет " + destAcc.AccountNumber
				+ $" средств на сумму {wireAmount:N2} руб."
				);
			WriteLog?.Invoke(withdrawCashTransaction);
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
		public abstract double RecalculateInterest();

		/// <summary>
		/// Закрывает счет: обнуляет баланс и накопленный процент
		/// ставит запрет на пополнение и снятие.
		/// Устанавливает дату закрытия счета
		/// </summary>
		/// <returns>
		/// Накопленную сумму
		/// </returns>
		public virtual double CloseAccount()
		{
			double tmp			= WithdrawCash(Balance);
			Topupable			= false;
			WithdrawalAllowed	= false;
			Closed				= GoodBankTime.Today;

			Transaction closeTransaction = new Transaction(
				AccID,
				GoodBankTime.GetBanksTodayWithCurrentTime(),
				"",
				"",
				OperationType.CloseAccount,
				0,
				"Счет " + AccountNumber + " закрыт."
				);
			WriteLog?.Invoke(closeTransaction);

			return tmp;
		}

		#endregion
	}
}
