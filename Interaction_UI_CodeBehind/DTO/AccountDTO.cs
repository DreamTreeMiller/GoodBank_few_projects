using Enumerables;
using Interfaces_Data;
using System;
using System.Data.SqlClient;

namespace DTO
{
	/// <summary>
	/// Структура для показа данных о любом счете
	/// Либо для передачи данных при открытии счета
	/// Не меняется в процессе, поэтому все поля заполняются на этапе создания
	/// через конструктор
	/// </summary>
	public class AccountDTO : IAccountDTO
	{
		public ClientType	ClientType		{ get; set; }
		public string		ClientTypeTag	{ get; set; }
		public int			ClientID		{ get; set; }
		public string		ClientName		{ get; set; }
		public AccountType	AccType			{ get; set; }
		public int			AccID			{ get; set; } = 0;
		public string		AccountNumber	{ get; set; }
		public decimal		Balance			{ get; set; }

		public decimal		CurrentAmount	
		{
			get => AccType == AccountType.Saving ? Balance : 0;
		}

		public decimal		DepositAmount
		{
			get => AccType == AccountType.Deposit ? Balance : 0; 
		}

		public decimal		DebtAmount
		{
			get => AccType == AccountType.Credit ? Balance : 0; 
		}

		public double		Interest		{ get; set; }

		/// <summary>
		/// С капитализацией или без
		/// </summary>
		public bool			Compounding	{ get; set; } = true;

		#region поля только для депозитов
		/// <summary>
		/// ID счета, куда перечислять проценты.
		/// При капитализации, совпадает с ИД счета депозита
		/// 0 - если внутренний счет
		/// </summary>
		public int			InterestAccumulationAccID	{ get; set; }


		public string		InterestAccumulationAccNum	{ get; set; }
		public decimal		AccumulatedInterest			{ get; set; } = 0;

		#endregion

		public DateTime		Opened			{ get; set; }

		/// <summary>
		/// Количество месяцев, на который открыт вклад, выдан кредит.
		/// 0 - бессрочно
		/// </summary>
		public int			Duration		{ get; set; }

		/// <summary>
		/// Количество месяцев, прошедших с открытия вклада
		/// </summary>
		public int			MonthsElapsed	{ get; set; }

		/// <summary>
		/// Дата окончания вклада/кредита. 
		/// null - бессрочно
		/// </summary>
		public DateTime?	EndDate =>
			Duration == 0 ? null : (DateTime?)Opened.AddMonths(Duration);

		/// <summary>
		/// Закончился ли период вклада/кредита, чтобы больше не пересчитывать проценты
		/// </summary>
		public bool			StopRecalculate { get; set; } = false;

		/// <summary>
		/// Дата фактического закрытия вклада
		/// </summary>
		public DateTime?	Closed			{ get; set; }

		/// <summary>
		/// Пополняемый счет или нет
		/// </summary>
		public bool			Topupable		{ get; set; }

		/// <summary>
		/// С правом частичного снятия или нет
		/// </summary>
		public bool		WithdrawalAllowed	{ get; set; }

		/// <summary>
		/// Период пересчета процентов - ежедневно, ежемесячно, ежегодно, один раз в конце
		/// </summary>
		public RecalcPeriod	RecalcPeriod	{ get; set; }

		public int		NumberOfTopUpsInDay { get; set; }

		public bool			IsBlocked		{ get; set; }

		/// <summary>
		/// Конструктор для создания заглушек в разных списках счетов
		/// </summary>
		public AccountDTO() { }

		/// <summary>
		/// Конструктор для создания счета и записи счета в базу
		/// Данные получены от ручного ввода
		/// 14 полей!!! ужас!!!
		/// </summary>
		public AccountDTO(ClientType ct, int clientID, AccountType accType,
						  decimal balance, double interest, 
						  bool compounding, int interestAccumAccID, string interestAccumAccNum, DateTime opened, 
						  bool topup, bool withdraw, RecalcPeriod recalc, int duration, int monthsElapsed)

		{
			ClientType			= ct;
			ClientID			= clientID;
			AccType				= accType;
			Balance				= balance;
			Interest			= interest;
			Compounding			= compounding;
			InterestAccumulationAccID  = interestAccumAccID;
			InterestAccumulationAccNum = interestAccumAccNum;
			Opened				= opened;
			Topupable			= topup;
			WithdrawalAllowed	= withdraw;
			RecalcPeriod		= recalc;
			Duration			= duration;
			MonthsElapsed		= monthsElapsed;

		}

		public AccountDTO(SqlDataReader ar)		// ar - account row, just to keep short as possible
		{
			ClientID			=			(int)ar["ClientID"];
			AccType				=	(AccountType)ar["AccType"];
			AccID				=			(int)ar["AccID"];
			AccountNumber		=		 (string)ar["AccountNumber"];
			Balance				=		(decimal)ar["Balance"];
			Interest			=		 (double)ar["Interest"];
			Compounding			=		   (bool)ar["Compounding"];
			Opened				=	   (DateTime)ar["Opened"];
			Duration			=			(int)ar["Duration"];
			StopRecalculate		=		   (bool)ar["StopRecalculate"];
			if (ar["Closed"]  != DBNull.Value) Closed = (DateTime)ar["Closed"]; else Closed = null;
			Topupable			=		   (bool)ar["Topupable"];
			WithdrawalAllowed	=		   (bool)ar["WithdrawalAllowed"];
			RecalcPeriod   = (RecalcPeriod)(byte)ar["RecalcPeriod"];
			NumberOfTopUpsInDay =			(int)ar["NumberOfTopUpsInDay"];
			IsBlocked			=		   (bool)ar["IsBlocked"];
			InterestAccumulationAccNum = (string)ar["InterestAccumulationAccNum"];
			AccumulatedInterest		  = (decimal)ar["AccumulatedInterest"];
		}
	}
}
