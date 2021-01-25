using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Enumerables;
using Interfaces_Actions;
using Interfaces_Data;
using DTO;
using SQL;
using BankTime;
using Transaction;

namespace BankInside
{
	public partial class GoodBank : IAccountsActions
	{
		public IAccountDTO GetAccountByID(int id)
		{
			AccountDTO accountDTO;
			string sqlSP_GetAccountDTObyID = $"EXEC SP_GetAccountDTObyID {id};";

			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				sqlCommand = new SqlCommand(sqlSP_GetAccountDTObyID, gbConn);
				SqlDataReader accountRow = sqlCommand.ExecuteReader();
				accountRow.Read();
				accountDTO = new AccountDTO(accountRow);
			}

			int clientID = accountDTO.ClientID;
			DataRow clientRow = ds.Tables["ClientsView"].Rows.Find(clientID);

			accountDTO.ClientType = (ClientType)(int)clientRow["ClientType"];
			accountDTO.ClientName =			 (string)clientRow["MainName"];
			return accountDTO;
		}

		/// <summary>
		/// Добавляет счет в базу
		/// </summary>
		/// <param name="acc">Данные счета</param>
		/// <returns>Возвращает созданный счет с уникальным ID счета</returns>
		public IAccountDTO AddAccount(IAccountDTO acc)
		{
			string accEndDate = acc.EndDate == null ? "NULL" : $"'{acc.EndDate:yyyy-MM-yy}'";
			string accClosed = acc.Closed == null ? "NULL" : $"'{acc.Closed:yyyy-MM-yy}'";
			string sqlCommandAddAccount = $@"
EXEC SP_AddAccount
	 {(int)acc.AccType}						-- @accType
	,{acc.ClientID}							-- [ClientID]			INT				NOT NULL,	-- ID клиента
	,{acc.Balance}							-- [Balance]			MONEY DEFAULT 0	NOT NULL,			
	,{acc.Interest}							-- [Interest]			FLOAT			NOT NULL,
	,{(acc.Compounding ? 1 : 0)}			-- [Compounding]		BIT				NOT NULL,	-- с капитали
	,'{acc.Opened:yyyy-MM-dd}'				-- [Opened]				DATE			NOT NULL,	-- дата откры
	,{acc.Duration}							-- [Duration]			INT				NOT NULL,	-- Количество
	,{acc.MonthsElapsed}					-- [MonthsElapsed]		INT				NOT NULL,	-- Количество
	,{accEndDate}							-- [EndDate]			DATE,						-- Дата оконч
	,{accClosed}							-- [Closed]				DATE,						-- Дата закры
	,{(acc.Topupable ? 1 : 0)}				-- [Topupable]			BIT				NOT NULL,	-- Пополняемы
	,{(acc.WithdrawalAllowed ? 1 : 0)}		-- [WithdrawalAllowed]	BIT				NOT NULL,	-- С правом ч
	,{(int)acc.RecalcPeriod}				-- [RecalcPeriod]		TINYINT			NOT NULL,	-- Период пер
	,{acc.InterestAccumulationAccID}		-- interest accum acc ID
	,N'{acc.InterestAccumulationAccNum}';	-- interest accum acc Num
			";

			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				sqlCommand = new SqlCommand(sqlCommandAddAccount, gbConn);
				SqlDataReader newAccIDandNumber = sqlCommand.ExecuteReader();
				if (newAccIDandNumber.Read())
				{
					acc.AccID = (int)newAccIDandNumber["NewAccID"];
					acc.AccountNumber = (string)newAccIDandNumber["NewAccNumber"];
				}
				else
					throw new AccountOperationException(ExceptionErrorCodes.CannotObtainAccountID);
			}

			string comment = "";
			switch (acc.AccType)
			{
				case AccountType.Saving:
					UpdateNumberOfAccounts(acc.ClientID, 1, 0, 0, 0);
					comment =	  "Текущий счет " + acc.AccountNumber
								+ $" с начальной суммой {acc.Balance:N2} руб."
								+ " открыт.";
					break;
				case AccountType.Deposit:
					UpdateNumberOfAccounts(acc.ClientID, 0, 1, 0, 0);
					comment =	  "Вклад " + acc.AccountNumber
								+ $" с начальной суммой {acc.Balance:N2} руб."
								+ " открыт.";
					break;
				case AccountType.Credit:
					UpdateNumberOfAccounts(acc.ClientID, 0, 0, 1, 0);
					comment =    "Кредитный счет " + acc.AccountNumber
								+ $" с начальной суммой {acc.Balance:N2} руб."
								+ " открыт.";
					break;
			}
			TransactionDTO DepositFromAccountTransaction = new TransactionDTO(
				acc.AccID,
				acc.Opened,
				"",
				acc.AccountNumber,
				OperationType.OpenAccount,
				acc.Balance,
				comment
				);
			TransactionAction.WriteLog(DepositFromAccountTransaction);
			return acc;
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
		/// Обновляет поля счета в базе
		/// </summary>
		/// <param name="acc">Данные счета с обновлёнными полями</param>
		public void UpdateAccountInDB(IAccountDTO acc)
		{
			string accClosed = acc.Closed == null ? "NULL" : $"'{acc.Closed:yyyy-MM-yy}'";
			string sqlCommandAddAccount = $@"
EXEC SP_UpdateAccount
-- parameters to select account
	 {acc.AccID}						-- INT			-- need to identify account
	,{(int)acc.AccType} 				-- TINYINT		-- need to identify type
-- parameters to update in account
	,{acc.Balance}						-- [Balance]			 MONEY DEFAULT 0	NOT NULL
	,{acc.AccumulatedInterest}			-- [AccumulatedInterest] MONEY DEFAULT 0	NOT NULL
	,{acc.MonthsElapsed}				-- [MonthsElapsed]		INT				NOT NULL
	,{(acc.StopRecalculate? 1 :0)}		-- [StopRecalculate]	BIT				NOT NULL
	,{accClosed}						-- [Closed]				DATE
	,{(acc.Topupable ? 1 : 0)}			-- [Topupable]			BIT				NOT NULL
	,{(acc.WithdrawalAllowed ? 1 : 0)}	-- [WithdrawalAllowed]	BIT				NOT NULL
	,{acc.NumberOfTopUpsInDay}			-- [NumberOfTopUpsInDay] INT DEFAULT 0	NOT NULL
	,{(acc.IsBlocked ? 1 : 0)};			-- interest accum acc Num
			";
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				sqlCommand = new SqlCommand(sqlCommandAddAccount, gbConn);
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
	,[ClientID]
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
	,[Topupable]
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
			,[Topupable]
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
				,[Topupable]
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
				,[Topupable]
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

		/// <summary>
		/// Удаляет и заново создаёт таблицу [dbo].[AccountsView], 
		/// в которой собраны счета всех трёх типов клиентов
		/// </summary>
		private void RefreshClientAccountsViewTable(int clientID)
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string sqlExpression = $@"
IF EXISTS (SELECT [name],[type] FROM sys.objects WHERE [name]='ClientAccountsView' AND [type]='U')	
	DROP TABLE [dbo].[ClientAccountsView];
SELECT 
	 [AccID]	
	,[ClientID]
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
	,[Topupable]
INTO [dbo].[ClientAccountsView]
FROM 
	-- first we take [ClientName] from [dbo].[AccountsView]
	(SELECT 
		 [ClientType]	
		,[ClientTypeTag]
		,[MainName] AS [ClientName]	
	 FROM [dbo].[ClientsView] 
	 WHERE [ID]={clientID}
	)	AS Client,

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
			,[Topupable]
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
				,[Topupable]
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
				,[Topupable]
			FROM	[dbo].[CreditAccounts], [dbo].[AccountsParent]
			WHERE [CreditAccounts].[id] = [AccountsParent].[AccID] 
	) AS Accounts

	-- then unify them
WHERE Accounts.[ClientID] = {clientID}
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
				totalSaving = (decimal)totals["TotalSaving"];
				totalDeposit = (decimal)totals["TotalDeposit"];
				totalCredit = (decimal)totals["TotalCredit"];
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
			DataView accountsView =
				new DataView(ds.Tables["AccountsView"],     // Table to show
							 rowfilter,                     // Row filter (select type)
							 "AccID ASC",                   // Sort ascending by 'ID' field
							 DataViewRowState.CurrentRows);

			DataTable accountsViewTable = accountsView.Table;
			object tmp; // need to check 
			tmp = accountsViewTable.Compute("SUM([CurrentAmount])", rowfilter);
			if (tmp != System.DBNull.Value) totalSaving = (decimal)tmp;

			tmp = accountsViewTable.Compute("SUM([DepositAmount])", rowfilter);
			if (tmp != System.DBNull.Value) totalDeposit = (decimal)tmp;

			tmp = accountsViewTable.Compute("SUM([DebtAmount])", rowfilter);
			if (tmp != System.DBNull.Value) totalCredit = (decimal)tmp;

			return (accountsView, totalSaving, totalDeposit, totalCredit);
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
			// Обновляем таблицу для показа
			RefreshClientAccountsViewTable(clientID);
			ds.Tables["ClientAccountsView"].Clear();
			daClientAccountsView.Fill(ds, "ClientAccountsView");

			decimal totalSaving = 0, totalDeposit = 0, totalCredit = 0;
			string rowfilter = string.Empty;
			DataView clientAccountsView =
				new DataView(ds.Tables["ClientAccountsView"],   // Table to show
							 rowfilter,
							 "AccType ASC",                     // Sort ascending by 'ID' field
							 DataViewRowState.CurrentRows);

			DataTable clientAccountsViewTable = clientAccountsView.Table;
			object tmp; // need to check 
			tmp = clientAccountsViewTable.Compute("SUM([CurrentAmount])", rowfilter);
			if (tmp != System.DBNull.Value) totalSaving = (decimal)tmp;

			tmp = clientAccountsViewTable.Compute("SUM([DepositAmount])", rowfilter);
			if (tmp != System.DBNull.Value) totalDeposit = (decimal)tmp;

			tmp = clientAccountsViewTable.Compute("SUM([DebtAmount])", rowfilter);
			if (tmp != System.DBNull.Value) totalCredit = (decimal)tmp;

			return (clientAccountsView, totalSaving, totalDeposit, totalCredit);
		}

		/// <summary>
		/// Получаем список текущих(Saving) пополняемых (Topupable) счетов клиента, 
		/// на один из которых нужно перечислить выданный кредит
		/// </summary>
		/// <param name="clientID"></param>
		/// <returns></returns>
		public DataView GetClientSavingAccounts(int clientID)
		{
			string rowfilter = "ClientID = " + clientID
								+ " AND AccType = 0"            // Saving account
								+ " AND Topupable <> 0";        // Topupable == true
			DataView clientAccountsViewTable =
				new DataView(ds.Tables["ClientAccountsView"],     // Table to show
							 rowfilter,                     // Row filter (select type)
							 "AccID ASC",                   // Sort ascending by 'ID' field
							 DataViewRowState.CurrentRows);

			return clientAccountsViewTable;
		}

		/// <summary>
		/// Получаем список счетов клиента, 
		/// на которых можно накапливать проценты со вклада,
		/// если выбран режим - без капитализации
		/// Этот список пополняемых (Topupable) счетов клиента. 
		/// Проценты могут накапливаться и на другом депозите
		/// </summary>
		/// <param name="clientID"></param>
		/// <returns></returns>
		public DataView GetClientAccountsToAccumulateInterest(int clientID)
		{
			string rowfilter =    "ClientID = " + clientID
								+ " AND Topupable <> 0";		// Topupable == true
			DataView clientAccountsViewTable =
				new DataView(ds.Tables["ClientAccountsView"],	// Table to show
							 rowfilter,							// Row filter (select type)
							 "AccType ASC",						// Sort ascending by 'AccType' field
							 DataViewRowState.CurrentRows);
			return clientAccountsViewTable;
		}


		public DataView GetTopupableAccountsToWireFrom(int sourceAccID)
		{
			string rowfilter =	  "AccID <> " + sourceAccID
								+ " AND Topupable <> 0";		// Topupable == true
			DataView clientAccountsViewTable =
				new DataView(ds.Tables["AccountsView"],			// Table to show
							 rowfilter,							// Row filter (select type)
							 "AccType ASC",						// Sort ascending by 'AccType' field
							 DataViewRowState.CurrentRows);
			return clientAccountsViewTable;
		}

		public IAccountDTO TopUpCash(int accID, decimal cashAmount)
		{
			IAccountDTO acc = GetAccountByID(accID);
			if (acc.IsBlocked)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsBlcoked);

			if (acc.Closed != null)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsClosed);

			if (!acc.Topupable)
				throw new AccountOperationException(ExceptionErrorCodes.TopUpIsNotAllowed);

			IAccountDTO updatedAcc = AccountAction.TopUpCash(acc, cashAmount);
			UpdateAccountInDB(updatedAcc);
			return updatedAcc;
		}

		public IAccountDTO WithdrawCash(int accID, decimal cashAmount)
		{
			IAccountDTO acc = GetAccountByID(accID);
			if (acc.IsBlocked)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsBlcoked);

			if (acc.Closed != null)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsClosed);

			if (acc.Balance < cashAmount)
				throw new AccountOperationException(ExceptionErrorCodes.NotEnoughMoneyOnAccount);

			if (!acc.WithdrawalAllowed)
				throw new AccountOperationException(ExceptionErrorCodes.WithdrawalIsNotAllowed);

			IAccountDTO updatedAcc = AccountAction.WithdrawCash(acc, cashAmount);
			UpdateAccountInDB(updatedAcc);
			return updatedAcc;
		}

		/// <summary>
		/// Перевод средств со счета на счет
		/// </summary>
		/// <param name="senderAccID"></param>
		/// <param name="recipientAccID"></param>
		/// <param name="wireAmount"></param>
		public (IAccountDTO senderAcc, IAccountDTO recipientAcc) Wire(int senderAccID, int recipientAccID, decimal wireAmount)
		{
			IAccountDTO senderAcc	 = GetAccountByID(senderAccID);
			IAccountDTO recipientAcc = GetAccountByID(recipientAccID);

			if (senderAcc.IsBlocked)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsBlcoked);

			if (senderAcc.Closed != null)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsClosed);

			if (senderAcc.WithdrawalAllowed)
			{
				if (senderAcc.Balance >= wireAmount)
				{
					if (recipientAcc.Topupable)
					{
						senderAcc = AccountAction.SendToAccount(senderAcc, recipientAcc.AccountNumber, wireAmount);
						UpdateAccountInDB(senderAcc);

						recipientAcc = AccountAction.ReceiveFromAccount(senderAcc.AccountNumber, recipientAcc, wireAmount);
						UpdateAccountInDB(recipientAcc);
					}
					else
						throw new AccountOperationException(ExceptionErrorCodes.RecipientCannotReceiveWire);
				}
				else
					throw new AccountOperationException(ExceptionErrorCodes.NotEnoughMoneyOnAccount);
			}
			else
				throw new AccountOperationException(ExceptionErrorCodes.WithdrawalIsNotAllowed);
			return (senderAcc, recipientAcc);
		}

		/// <summary>
		/// Закрыть можно только нулевой счет
		/// Проверку на наличие денег на счете осуществляет вызывающий метод
		/// </summary>
		/// <param name="accID"></param>
		/// <returns></returns>
		public IAccountDTO CloseAccount(int accID, out decimal accumulatedAmount)
		{
			IAccountDTO acc = GetAccountByID(accID);
			if (acc.Closed != null)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsClosed);

			if (acc.IsBlocked)
				throw new AccountOperationException(ExceptionErrorCodes.AccountIsBlcoked);

			if (acc.Balance < 0)
				throw new AccountOperationException(ExceptionErrorCodes.CannotCloseAccountWithMinusBalance);

			acc = AccountAction.CloseAccount(acc, out accumulatedAmount);
			UpdateAccountInDB(acc);

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

			SqlAccounts accounts = new SqlAccounts(GoodBankCS, TransactionAction);
			GoodBankTime.Today	 = GoodBankTime.Today.AddMonths(1);
			accounts.RecalculateInterest(GoodBankTime.Today, GetAccountByID, AccountAction.ReceiveFromAccount);
			accounts.Dispose();
		}
	}
}
