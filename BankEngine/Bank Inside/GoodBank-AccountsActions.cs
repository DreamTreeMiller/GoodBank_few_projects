﻿using Enumerables;
using Interfaces_Actions;
using Interfaces_Data;
using DTO;
using BankTime;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace BankInside
{
	public partial class GoodBank : IAccountsActions
	{
		private List<Account> accounts;

		public IAccount GetAccountByID(int id)
		{
			return accounts.Find(a => a.AccID == id);
		}

		/// <summary>
		/// Добавляет счет в базу
		/// </summary>
		/// <param name="acc">Данные счета</param>
		/// <returns>Возвращает созданный счет с уникальным ID счета</returns>
		public void AddAccount(IAccountDTO acc)
		{
			string sqlCommandAddAccount = $@"
EXEC SP_AddAccount
	 {(byte)acc.AccType}					-- @accType
	,{acc.ClientID}							-- [ClientID]			INT				NOT NULL,	-- ID клиента
	,{acc.Balance}							-- [Balance]			MONEY DEFAULT 0	NOT NULL,			
	,{acc.Interest}							-- [Interest]			DECIMAL (4,2)	NOT NULL,
	,{acc.Compounding}						-- [Compounding]		BIT				NOT NULL,	-- с капитали
	,{acc.Opened:yyyy-MM-dd}				-- [Opened]				DATE			NOT NULL,	-- дата откры
	,{acc.Duration}							-- [Duration]			INT				NOT NULL,	-- Количество
	,{acc.MonthsElapsed}					-- [MonthsElapsed]		INT				NOT NULL,	-- Количество
	,{acc.EndDate:yyyy-MM-yy}				-- [EndDate]			DATE,						-- Дата оконч
	,{acc.Closed:yyyy-MM-yy}				-- [Closed]				DATE,						-- Дата закры
	,{acc.Topupable}						-- [Topupable]			BIT				NOT NULL,	-- Пополняемы
	,{acc.WithdrawalAllowed}				-- [WithdrawalAllowed]	BIT				NOT NULL,	-- С правом ч
	,{(byte)acc.RecalcPeriod}				-- [RecalcPeriod]		TINYINT			NOT NULL,	-- Период пер
	,{acc.IsBlocked}						-- [IsBlocked]			BIT DEFAULT 0	NOT NUll
	,{acc.InterestAccumulationAccID}		-- interest accum acc ID
	,'{acc.InterestAccumulationAccNum}';	-- interest accum acc Num
";
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				sqlCommand = new SqlCommand(sqlCommandAddAccount, gbConn);
				sqlCommand.ExecuteNonQuery();

				switch (acc.AccType)
				{
					case AccountType.Saving:
						UpdateNumberOfAccounts(acc.ClientID, 1, 0, 0, 0);
						break;
					case AccountType.Deposit:
						UpdateNumberOfAccounts(acc.ClientID, 0, 1, 0, 0);
						break;
					case AccountType.Credit:
						UpdateNumberOfAccounts(acc.ClientID, 0, 0, 1, 0);
						break;
				}
			}
		}

		private void UpdateNumberOfAccounts
			(int clientID, 
			 int savingsUpdate, 
			 int depositsUpdate, 
			 int creditsUpdate,
			 int closedUpdate)
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string slqCommandUpdateNumOfAccounts = $@"
EXEC [dbo].[SP_UpdateNumberOfAccounts]
	 {clientID}
	,{savingsUpdate}
	,{depositsUpdate}
	,{creditsUpdate}
	,{closedUpdate}
";
				sqlCommand = new SqlCommand(slqCommandUpdateNumOfAccounts, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Формирует список счетов данного типа клиентов.
		/// </summary>
		/// <param name="clientType">Тип клиента</param>
		/// <returns>
		/// возвращает коллекцию счетов и общую сумму каждой группы счетов - текущие, вклады, кредиты
		/// </returns>
		public (ObservableCollection<IAccountDTO> accList, double totalCurr, double totalDeposit, double totalCredit)
			GetAccountsList(ClientType clientType)
		{
			ObservableCollection<IAccountDTO> accList = new ObservableCollection<IAccountDTO>();
			double totalCurr = 0, totalDeposit = 0, totalCredit = 0;

			return (accList, 0,0,0); // Temporal solution

			IAccount acc;
			if (clientType == ClientType.All)
			{
				for (int i = 0; i < accounts.Count; i++)
				{
					acc = accounts[i];
					switch(acc.AccType)
					{
						case AccountType.Saving:
							totalCurr += acc.Balance;
							break;
						case AccountType.Deposit:
							totalDeposit += acc.Balance;
							if (!acc.Compounding && (acc as IAccountDeposit).InterestAccumulationAccID == 0)
								totalDeposit += (acc as IAccountDeposit).AccumulatedInterest;
							break;
						case AccountType.Credit:
							totalCredit += acc.Balance;
							break;
					}
					accList.Add(new AccountDTO(GetClientByID(accounts[i].ClientID), acc));
				}
				return (accList, totalCurr, totalDeposit, totalCredit);
			}

			for (int i = 0; i < accounts.Count; i++)
				if (accounts[i].ClientType == clientType)
				{
					acc = accounts[i];
					switch (acc.AccType)
					{
						case AccountType.Saving:
							totalCurr += acc.Balance;
							break;
						case AccountType.Deposit:
							totalDeposit += acc.Balance;
							break;
						case AccountType.Credit:
							totalCredit += acc.Balance;
							break;
					}
					accList.Add(new AccountDTO(GetClientByID(accounts[i].ClientID), acc));
				}
			return (accList, totalCurr, totalDeposit, totalCredit);
		}

		/// <summary>
		/// Формирует список счетов заданного клиентов.
		/// </summary>
		/// <param name="clientID">ID клиента</param>
		/// <returns>
		/// возвращает коллекцию счетов и общую сумму каждой группы счетов - текущие, вклады, кредиты
		/// </returns>
		public (ObservableCollection<IAccountDTO> accList, double totalCurr, double totalDeposit, double totalCredit)
			GetClientAccounts(int clientID)
		{
			ObservableCollection<IAccountDTO> accList = new ObservableCollection<IAccountDTO>();
			var client = GetClientByID(clientID);
			double totalCurr = 0, totalDeposit = 0, totalCredit = 0;
			IAccount acc;

			for (int i = 0; i < accounts.Count; i++)
				if (accounts[i].ClientID == clientID)
				{
					acc = accounts[i];
					switch (acc.AccType)
					{
						case AccountType.Saving:
							totalCurr += acc.Balance;
							break;
						case AccountType.Deposit:
							totalDeposit += acc.Balance;
							break;
						case AccountType.Credit:
							totalCredit += acc.Balance;
							break;
					}
					accList.Add(new AccountDTO(client, acc));
				}
			return (accList, totalCurr, totalDeposit, totalCredit);
		}

		public ObservableCollection<IAccountDTO> GetClientAccounts(int clientID, AccountType accType)
		{
			ObservableCollection<IAccountDTO> accList = new ObservableCollection<IAccountDTO>();
			var client = GetClientByID(clientID);

			for (int i = 0; i < accounts.Count; i++)
			{
				var acc = accounts[i];
				if (acc.ClientID == clientID && acc.AccType == accType)
					accList.Add(new AccountDTO(client, acc));
			}
			return accList;

		}

		public ObservableCollection<IAccountDTO> GetClientAccountsToAccumulateInterest(int clientID)
		{
			ObservableCollection<IAccountDTO> accList = new ObservableCollection<IAccountDTO>();
			var client = GetClientByID(clientID);

			for (int i = 0; i < accounts.Count; i++)
			{
				var acc = accounts[i];
				if (acc.ClientID == clientID &&
					acc.AccType == AccountType.Saving &&
					acc.Topupable)
					accList.Add(new AccountDTO(client, acc));
			}
			return accList;
		}

		public ObservableCollection<IAccount> GetTopupableAccountsToWireFrom(int sourceAccID)
		{
			ObservableCollection<IAccount> accList = new ObservableCollection<IAccount>();
			for (int i = 0; i < accounts.Count; i++)
				if (accounts[i].Topupable && accounts[i].AccID != sourceAccID)
				{
					accList.Add(accounts[i]);
				}
			return accList;
		}

		public IAccount TopUpCash(int accID, double cashAmount)
		{
			IAccount acc = GetAccountByID(accID);
			if (acc.IsBlocked)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsBlcoked);

			if (acc.Closed != null)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsClosed);

			if (!acc.Topupable)
				throw new AccountOperationException(ExceptionErrorCodes.TopUpIsNotAllowed);

			acc.TopUpCash(cashAmount);
			return acc;
		}

		public IAccount WithdrawCash(int accID, double cashAmount)
		{
			IAccount acc = GetAccountByID(accID);
			if (acc.IsBlocked)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsBlcoked);

			if (acc.Closed != null)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsClosed);

			if (acc.Balance < cashAmount)
				throw new AccountOperationException(ExceptionErrorCodes.NotEnoughMoneyOnAccount);

			if (!acc.WithdrawalAllowed)
				throw new AccountOperationException(ExceptionErrorCodes.WithdrawalIsNotAllowed);

			acc.WithdrawCash(cashAmount);
			return acc;
		}

		/// <summary>
		/// Перевод средств со счета на счет
		/// </summary>
		/// <param name="sourceAccID"></param>
		/// <param name="destAccID"></param>
		/// <param name="wireAmount"></param>
		public void Wire(int sourceAccID, int destAccID, double wireAmount)
		{
			var sourceAcc = GetAccountByID(sourceAccID);
			if (sourceAcc.IsBlocked)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsBlcoked);

			if (sourceAcc.Closed != null)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsClosed);

			if (sourceAcc.WithdrawalAllowed)
			{
				if (sourceAcc.Balance >= wireAmount)
				{
					var destAcc = GetAccountByID(destAccID);
					if (destAcc.Topupable)
					{
						sourceAcc.SendToAccount(destAcc, wireAmount);
						destAcc.ReceiveFromAccount(sourceAcc, wireAmount);
					}
					else
						throw new AccountOperationException(ExceptionErrorCodes.RecipientCannotReceiveWire);
				}
				else
					throw new AccountOperationException(ExceptionErrorCodes.NotEnoughMoneyOnAccount);
			}
			else
				throw new AccountOperationException(ExceptionErrorCodes.WithdrawalIsNotAllowed);
		}

		/// <summary>
		/// Закрыть можно только нулевой счет
		/// Проверку на наличие денег на счете осуществляет вызывающий метод
		/// </summary>
		/// <param name="accID"></param>
		/// <returns></returns>
		public IAccount CloseAccount(int accID, out double accumulatedAmount)
		{
			IAccount acc = GetAccountByID(accID);
			if (acc.Closed != null)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsClosed);

			if (acc.IsBlocked)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsBlcoked);

			if (acc.Balance < 0)
				throw new AccountOperationException(ExceptionErrorCodes.CannotCloseAccountWithMinusBalance);

			accumulatedAmount = acc.CloseAccount();

			switch (acc.AccType)
			{
				case AccountType.Saving:
					UpdateNumberOfAccounts(acc.ClientID, -1, 0, 0, 1);
					break;
				case AccountType.Deposit:
					UpdateNumberOfAccounts(acc.ClientID, 0, -1, 0, 1);
					break;
				case AccountType.Credit:
					UpdateNumberOfAccounts(acc.ClientID, 0, 0, -1, 1);
					break;
			}

			return acc;
		}
	
		public void AddOneMonth()
		{
			GoodBankTime.Today = GoodBankTime.Today.AddMonths(1);

			for (int i = 0; i< accounts.Count; i++)
			{
				Account acc = accounts[i];
				double currInterest = acc.RecalculateInterest();
				if (acc is AccountDeposit)
				{
					int destAccID = (acc as AccountDeposit).InterestAccumulationAccID;
					if (!(acc as AccountDeposit).Compounding && 
						destAccID    != 0 && 
						currInterest != 0)
					{
						IAccount destAcc = GetAccountByID(destAccID);
						if (destAcc.Topupable)
						{
							WireInterestToAccount(acc as IAccountDeposit, destAcc, currInterest);
						}
						else
						{
							(acc as AccountDeposit).
								AddInterestToSourceAccountWhenDestinationIsNotAvailable(currInterest);
						}
					}
				}
			}
		}

		/// <summary>
		/// Вызывается методом ежемесячного обновления счетов
		/// </summary>
		/// <param name="sourceAcc"></param>
		/// <param name="destAcc"></param>
		/// <param name="accumulatedInterest"></param>
		private void WireInterestToAccount(IAccountDeposit sourceAcc, IAccount destAcc, double accumulatedInterest)
		{
			sourceAcc.SendInterestToAccount(destAcc, accumulatedInterest);
			destAcc.ReceiveFromAccount(sourceAcc, accumulatedInterest);
		}

	}
}
