﻿using Enumerables;
using Interfaces_Actions;
using Interfaces_Data;
using DTO;
using BankTime;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
			string accEndDate = acc.EndDate == null ? "NULL" : $"'{acc.EndDate:yyyy-MM-yy}'";
			string accClosed  = acc.Closed  == null ? "NULL" : $"'{acc.Closed:yyyy-MM-yy}'";
			string sqlCommandAddAccount = $@"
EXEC SP_AddAccount
	 {(byte)acc.AccType}					-- @accType
	,{acc.ClientID}							-- [ClientID]			INT				NOT NULL,	-- ID клиента
	,{acc.Balance}							-- [Balance]			MONEY DEFAULT 0	NOT NULL,			
	,{acc.Interest}							-- [Interest]			DECIMAL (4,2)	NOT NULL,
	,{(acc.Compounding?1:0)}				-- [Compounding]		BIT				NOT NULL,	-- с капитали
	,'{acc.Opened:yyyy-MM-dd}'				-- [Opened]				DATE			NOT NULL,	-- дата откры
	,{acc.Duration}							-- [Duration]			INT				NOT NULL,	-- Количество
	,{acc.MonthsElapsed}					-- [MonthsElapsed]		INT				NOT NULL,	-- Количество
	,{accEndDate}							-- [EndDate]			DATE,						-- Дата оконч
	,{accClosed}							-- [Closed]				DATE,						-- Дата закры
	,{(acc.Topupable?1:0)}					-- [Topupable]			BIT				NOT NULL,	-- Пополняемы
	,{(acc.WithdrawalAllowed?1:0)}			-- [WithdrawalAllowed]	BIT				NOT NULL,	-- С правом ч
	,{(byte)acc.RecalcPeriod}				-- [RecalcPeriod]		TINYINT			NOT NULL,	-- Период пер
	,{acc.InterestAccumulationAccID}		-- interest accum acc ID
	,N'{acc.InterestAccumulationAccNum}';	-- interest accum acc Num
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

		/// <summary>
		/// Updates number of accounts in client's record. 
		/// In reality usage is. each call updates either one number by +1,
		/// or one number by -1 and closed account by +1.
		/// </summary>
		/// <param name="clientID"></param>
		/// <param name="savingsUpdate">Number of accounts to add/delete</param>
		/// <param name="depositsUpdate"></param>
		/// <param name="creditsUpdate"></param>
		/// <param name="closedUpdate"></param>
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
		/// Удаляет и заново создаёт таблицу [dbo].[AccountsView], 
		/// в которой собраны счета всех трёх типов клиентов
		/// </summary>
		private void RefreshAccountsViewTable()
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string sqlExpression = @"
IF EXISTS (SELECT [name],[type] FROM sys.objects WHERE [name]='AccountsView' AND [type]='U')	
	DROP TABLE [dbo].[AccountsView];
SELECT 
	 [AccID]		
	,[ClientType]	
	,[ClientTypeTag]
	,[ClientName]	
	,[AccountNumber]
	,[AccType]		
	,[CurrentAmount]
	,[DepositAmount]
	,[DebtAmount]	
	,[Interest]		
	,[Opened]		
	,[Closed]		
INTO [dbo].[AccountsView]
FROM 
	-- first we make a list of all clients
	(SELECT  [id]
		,0		AS [ClientType] -- VIP
		,N'ВИП' AS [ClientTypeTag] 
		,[LastName] + ' ' + [FirstName] + ' ' + [MiddleName] AS [ClientName]
	 FROM	[dbo].[VIPclients]
	 UNION	SELECT    [id] 
				,1		  AS [ClientType] -- Simple
				,N'Физик' AS [ClientTypeTag] 
				,[LastName] + ' ' + [FirstName] + ' ' + [MiddleName] AS [ClientName]
			FROM	[dbo].[SIMclients]
	 UNION	SELECT	 [id]
				,2		 AS [ClientType]	-- Organization
				,N'Юрик' AS [ClientTypeTag] 
				,[OrgName] AS [ClientName]
			FROM	[dbo].[ORGclients]
	)	AS Clients,

	-- then list of all accounts
	(SELECT	 [AccID]
			,[ClientID]
			,[AccountNumber]
			,0 AS [AccType]		-- Saving account
			,[Balance]	AS [CurrentAmount]
			,0			AS [DepositAmount]
			,0			AS [DebtAmount]
			,[Interest]
			,[Opened]
			,[Closed]
	 FROM	[dbo].[SavingAccounts], [dbo].[AccountsParent]
	 WHERE [SavingAccounts].[id] = [AccountsParent].[AccID] 
	 UNION	SELECT   [AccID]
				,[ClientID]
				,[AccountNumber]
				,1 AS [AccType]		-- Deposit account
				,0			AS [CurrentAmount]
				,[Balance]	AS [DepositAmount]
				,0			AS [DebtAmount]
				,[Interest]
				,[Opened]
				,[Closed]
			FROM	[dbo].[DepositAccounts], [dbo].[AccountsParent]
			WHERE [DepositAccounts].[id] = [AccountsParent].[AccID] 
	 UNION	SELECT   [AccID]
				,[ClientID]
				,[AccountNumber]
				,2 AS [AccType]		-- Credit account
				,0			AS [CurrentAmount]
				,0			AS [DepositAmount]
				,[Balance]	AS [DebtAmount]
				,[Interest]
				,[Opened]
				,[Closed]
			FROM	[dbo].[CreditAccounts], [dbo].[AccountsParent]
			WHERE [CreditAccounts].[id] = [AccountsParent].[AccID] 
	) AS Accounts

	-- then unify them
WHERE Clients.[id] = Accounts.[ClientID]
				";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}

		private void GetAccountsViewTotals(ClientType ct, out decimal totalSaving, out decimal totalDeposit, out decimal totalCredit)
		{
			string sqlSP_GetAccountsViewtotals = $@"
DECLARE @ts MONEY;
DECLARE @td MONEY;
DECLARE @tc MONEY;

EXEC SP_GetAccountsViewTotals {(byte)ct}, @ts OUT, @td OUT, @tc OUT;
SELECT @ts [TotalSaving], @td [TotalDeposit], @tc [TotalCredit];
			";
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				sqlCommand = new SqlCommand(sqlSP_GetAccountsViewtotals, gbConn);
				SqlDataReader totals = sqlCommand.ExecuteReader();
				totals.Read();
				totalSaving  = (decimal)totals["TotalSaving"];
				totalDeposit = (decimal)totals["TotalDeposit"];
				totalCredit  = (decimal)totals["TotalCredit"];
			}
		}
		/// <summary>
		/// Формирует список счетов данного типа клиентов.
		/// </summary>
		/// <param name="clientType">Тип клиента</param>
		/// <returns>
		/// возвращает коллекцию счетов и общую сумму каждой группы счетов - текущие, вклады, кредиты
		/// </returns>
		public (DataView accountsViewTable, decimal totalSaving, decimal totalDeposit, decimal totalCredit)
			GetAccountsList(ClientType ct)
		{
			decimal totalSaving = 0, totalDeposit = 0, totalCredit = 0;

			// Обновляем таблицу для показа
			RefreshAccountsViewTable();
			ds.Tables["AccountsView"].Clear();
			daAccountsView.Fill(ds, "AccountsView");

			string rowfilter = (ct == ClientType.All) ? "" : "ClientType = " + (int)ct;
			DataView accountsViewTable =
				new DataView(ds.Tables["AccountsView"],		// Table to show
							 rowfilter,						// Row filter (select type)
							 "AccID ASC",					// Sort ascending by 'ID' field
							 DataViewRowState.CurrentRows);
			GetAccountsViewTotals(ct, out totalSaving, out totalDeposit, out totalCredit);

			return (accountsViewTable, totalSaving, totalDeposit, totalCredit);
		}

		/// <summary>
		/// Формирует список счетов заданного клиентов.
		/// </summary>
		/// <param name="clientID">ID клиента</param>
		/// <returns>
		/// возвращает коллекцию счетов и общую сумму каждой группы счетов - текущие, вклады, кредиты
		/// </returns>
		public (DataView accountsViewTable, decimal totalSaving, decimal totalDeposit, decimal totalCredit)
			GetClientAccounts(int clientID)
		{
			DataView accList = new DataView();
			var client = GetClientByID(clientID);
			decimal totalCurr = 0, totalDeposit = 0, totalCredit = 0;
			IAccount acc;

			for (int i = 0; i < accounts.Count; i++)
				if (accounts[i].ClientID == clientID)
				{
					acc = accounts[i];
					switch (acc.AccType)
					{
						case AccountType.Saving:
							totalCurr    = totalCurr    + (decimal)acc.Balance;
							break;
						case AccountType.Deposit:
							totalDeposit = totalDeposit + (decimal)acc.Balance;
							break;
						case AccountType.Credit:
							totalCredit  = totalCredit  + (decimal)acc.Balance;
							break;
					}
					//accList.Add(new AccountDTO(client, acc));
				}
			return (accList, totalCurr, totalDeposit, totalCredit);
		}

		public DataView GetClientAccounts(int clientID, AccountType accType)
		{
			DataView accList = new DataView();
			var client = GetClientByID(clientID);

			for (int i = 0; i < accounts.Count; i++)
			{
				var acc = accounts[i];
				//if (acc.ClientID == clientID && acc.AccType == accType)
					//accList.Add(new AccountDTO(client, acc));
			}
			return accList;

		}

		public DataView GetClientAccountsToAccumulateInterest(int clientID)
		{
			DataView accList = new DataView();
			var client = GetClientByID(clientID);

			for (int i = 0; i < accounts.Count; i++)
			{
				var acc = accounts[i];
				//if (acc.ClientID == clientID &&
				//	acc.AccType == AccountType.Saving &&
				//	acc.Topupable)
				//	accList.Add(new AccountDTO(client, acc));
			}
			return accList;
		}

		public DataView GetTopupableAccountsToWireFrom(int sourceAccID)
		{
			DataView accList = new DataView();
			for (int i = 0; i < accounts.Count; i++)
				if (accounts[i].Topupable && accounts[i].AccID != sourceAccID)
				{
					//accList.Add(accounts[i]);
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
