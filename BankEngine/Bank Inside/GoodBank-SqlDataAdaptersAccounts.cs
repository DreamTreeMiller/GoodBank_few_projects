using System.Data;
using System.Data.SqlClient;
using Interfaces_Actions;

namespace BankInside
{
	public partial class GoodBank
	{
		private SqlDataAdapter	daAccountsView, daClientAccountsView;

		private void SetupSP_GetAccountsViewTotals()
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string sqlExpression = @"
IF EXISTS (SELECT [name],[type] FROM sys.objects WHERE [name]='SP_GetAccountsViewTotals' AND [type]='P')
	DROP PROC [dbo].[SP_GetAccountsViewTotals];
				";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();

				sqlExpression = @"
CREATE PROC [dbo].[SP_GetAccountsViewTotals]
	 @clientType	TINYINT
	,@totalSaving	MONEY OUTPUT
	,@totalDeposit	MONEY OUTPUT
	,@totalCredit	MONEY OUTPUT
AS
IF @clientType = 3
	SELECT 
		 @totalSaving  = SUM(CurrentAmount)
		,@totalDeposit = SUM(DepositAmount)
		,@totalCredit  = SUM(DebtAmount) 
	FROM [dbo].[AccountsView]
ELSE
	SELECT 
		 @totalSaving  = SUM(CurrentAmount)
		,@totalDeposit = SUM(DepositAmount)
		,@totalCredit  = SUM(DebtAmount) 
	FROM [dbo].[AccountsView]
	WHERE [ClientType]=@clientType;
				";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}

		private void SetupAccountsViewSqlDataAdapter()
		{
			gbConn = SetGoodBankConnection();
			daAccountsView = new SqlDataAdapter();
			string sqlCommand = @"SELECT * FROM [dbo].[AccountsView]";
			daAccountsView.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daAccountsView.Fill(ds, "AccountsView");
		}

		private void SetupClientAccountsViewSqlDataAdapter()
		{
			gbConn = SetGoodBankConnection();
			daClientAccountsView = new SqlDataAdapter();
			string sqlCommand = @"SELECT * FROM [dbo].[ClientAccountsView]";
			daClientAccountsView.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daClientAccountsView.Fill(ds, "ClientAccountsView");
		}

		private void SetupSP_AddAccount()
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string sqlExpression = @"
IF EXISTS (SELECT [name],[type] FROM sys.objects WHERE [name]='SP_AddAccount' AND [type]='P')
	DROP PROC [dbo].[SP_AddAccount];
";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();

				sqlExpression = @"
CREATE PROC [dbo].[SP_AddAccount]
	 @accType 						TINYINT
	,@clientID						INT
	,@balance						MONEY
	,@interest						FLOAT
	,@compounding					BIT
	,@opened						DATE
	,@duration						INT
	,@monthsElapsed					INT
	,@endDate						DATE
	,@closed						DATE
	,@topupable						BIT
	,@withdrawalAllowed				BIT
	,@recalcPeriod					TINYINT
	,@interestAccumulationAccID		INT
	,@interestAccumulationAccNum	NVARCHAR (15)
AS
BEGIN
	DECLARE @accountID		INT;
	DECLARE @accountNumber	NVARCHAR (15);

	INSERT INTO [dbo].[AccountsParent]
		([ClientID]
		,[Balance]
		,[Interest]
		,[Compounding]
		,[Opened]
		,[Duration]
		,[MonthsElapsed]
		,[EndDate]
		,[Closed]
		,[Topupable]
		,[WithdrawalAllowed]
		,[RecalcPeriod]
		)
	VALUES 
		(@clientID
		,@balance
		,@interest
		,@compounding
		,@opened
		,@duration
		,@monthsElapsed
		,@endDate
		,@closed
		,@topupable
		,@withdrawalAllowed
		,@recalcPeriod
		);
	SET @accountID=@@IDENTITY;

	IF @accType=0			--Saving
		SET @accountNumber = 'SAV' + RIGHT(REPLICATE('0',12) + CAST(@accountID AS NVARCHAR),12)
	ELSE IF @accType=1		--Deposit
		SET @accountNumber = 'DEP' + RIGHT(REPLICATE('0',12) + CAST(@accountID AS NVARCHAR),12)
	ELSE IF @accType=2		--Credit
		SET @accountNumber = 'CRE' + RIGHT(REPLICATE('0',12) + CAST(@accountID AS NVARCHAR),12);
	UPDATE [dbo].[AccountsParent]
	SET [AccountNumber] = @accountNumber
	WHERE [AccID]=@accountID;

	IF @accType=0	-- Saving
		INSERT INTO [dbo].[SavingAccounts] ([id])
		VALUES (@accountID);
	ELSE
	IF @accType=1	-- Deposit
		INSERT INTO [dbo].[DepositAccounts] 
			([id]
			,[InterestAccumulationAccID]
			,[InterestAccumulationAccNum]
			)
		VALUES 
			(@accountID
			,@interestAccumulationAccID	
			,@interestAccumulationAccNum
			);
	ELSE 
	IF @accType=2	-- Credit
		INSERT INTO [dbo].[CreditAccounts] ([id])
		VALUES (@accountID);
	SELECT @accountID AS [NewAccID], @accountNumber AS [NewAccNumber];
END;			
";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}

		private void SetupSP_GetAccountDTObyID()
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string sqlExpression = @"
IF EXISTS (SELECT [name],[type] FROM sys.objects WHERE [name]='SP_GetAccountDTObyID' AND [type]='P')
	DROP PROC [dbo].[SP_GetAccountDTObyID];
				";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();

				sqlExpression = @"
CREATE PROC [SP_GetAccountDTObyID]
	@accID	INT
AS
SELECT
	 0 AS [ClientType]		
	,[ClientID]			
	,'' AS [ClientName]		
	,[AccType]			
	,[AccID]			
	,[AccountNumber]	
	,[Balance]			
	,[Interest]			
	,[Compounding]		
	,[InterestAccumulationAccID]		-- only deposit field
	,[InterestAccumulationAccNum]		-- only deposit field
	,[AccumulatedInterest]				-- deposit and credit field
	,[Opened]			
	,[Duration]			
	,[MonthsElapsed]
	,[StopRecalculate]
	,[Closed]			
	,[Topupable]			
	,[WithdrawalAllowed]
	,[RecalcPeriod]		
	,[NumberOfTopUpsInDay]
	,[IsBlocked]		
FROM 
(SELECT	 [AccID]
		,[ClientID]
		,[AccountNumber]
		,0  AS [AccType]		-- Saving account
		,[Balance]
		,[Interest]
		,[Compounding]
		,0  AS [InterestAccumulationAccID]		-- only deposit field
		,'' AS [InterestAccumulationAccNum]		-- only deposit field
		,0  AS [AccumulatedInterest]			-- only deposit field
		,[Opened]
		,[Duration]			
		,[MonthsElapsed]	
		,[StopRecalculate]
		,[Closed]
		,[Topupable]			
		,[WithdrawalAllowed]
		,[RecalcPeriod]		
		,[NumberOfTopUpsInDay]
		,[IsBlocked]		
	 FROM	[dbo].[SavingAccounts], [dbo].[AccountsParent]
	 WHERE [SavingAccounts].[id] = [AccountsParent].[AccID] 
	 UNION	SELECT   [AccID]
				,[ClientID]
				,[AccountNumber]
				,1 AS [AccType]		-- Deposit account
				,[Balance]
				,[Interest]
				,[Compounding]
				,[InterestAccumulationAccID]		-- only deposit field
				,[InterestAccumulationAccNum]		-- only deposit field
				,[AccumulatedInterest]				-- deposit and credit field
				,[Opened]
				,[Duration]			
				,[MonthsElapsed]	
				,[StopRecalculate]
				,[Closed]
				,[Topupable]			
				,[WithdrawalAllowed]
				,[RecalcPeriod]		
				,[NumberOfTopUpsInDay]
				,[IsBlocked]		
			FROM	[dbo].[DepositAccounts], [dbo].[AccountsParent]
			WHERE [DepositAccounts].[id] = [AccountsParent].[AccID] 
	 UNION	SELECT   [AccID]
				,[ClientID]
				,[AccountNumber]
				,2 AS [AccType]		-- Credit account
				,[Balance]
				,[Interest]
				,[Compounding]
				,0  AS [InterestAccumulationAccID]		-- only deposit field
				,'' AS [InterestAccumulationAccNum]		-- only deposit field
				,[AccumulatedInterest]					-- deposit and credit field
				,[Opened]
				,[Duration]			
				,[MonthsElapsed]	
				,[StopRecalculate]
				,[Closed]
				,[Topupable]			
				,[WithdrawalAllowed]
				,[RecalcPeriod]		
				,[NumberOfTopUpsInDay]
				,[IsBlocked]		
			FROM	[dbo].[CreditAccounts], [dbo].[AccountsParent]
			WHERE [CreditAccounts].[id] = [AccountsParent].[AccID] ) AS Accounts
WHERE	[Accounts].[AccID]  = @accID 
			";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}

		private void SetupSP_UpdateAccount()
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string sqlExpression = @"
IF EXISTS (SELECT [name],[type] FROM sys.objects WHERE [name]='SP_UpdateAccount' AND [type]='P')
	DROP PROC [dbo].[SP_UpdateAccount];
				";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();

				sqlExpression = @"
CREATE PROC [dbo].[SP_UpdateAccount]
-- parameters to select account
	 @accountID						INT			-- need to identify account
	,@accType 						TINYINT		-- need to identify type
-- parameters to update in account
	,@balance						MONEY
	,@accumulatedInterest			MONEY
	,@monthsElapsed					INT
	,@stopRecalculate				BIT
	,@closed						DATE
	,@topupable						BIT
	,@withdrawalAllowed				BIT
	,@numberOfTopUpsInDay			INT
	,@isBlocked						BIT
AS
BEGIN
	UPDATE	 [dbo].[AccountsParent]
	SET		 [Balance]				= @balance
			,[MonthsElapsed]		= @monthsElapsed
			,[StopRecalculate]		= @stopRecalculate
			,[Closed]				= @closed
			,[Topupable]			= @topupable
			,[WithdrawalAllowed]	= @withdrawalAllowed
			,[NumberOfTopUpsInDay]	= @numberOfTopUpsInDay
			,[IsBlocked]			= @isBlocked
	WHERE	 [AccID] =  @accountID;

	IF @accType=1	-- Deposit
		UPDATE	[dbo].[DepositAccounts] 
		SET		[AccumulatedInterest] = @accumulatedInterest
		WHERE	[id] = @accountID
	ELSE 
	IF @accType=2	-- Credit
		UPDATE	[dbo].[CreditAccounts]
		SET		[AccumulatedInterest] = @accumulatedInterest
		WHERE	[id] = @accountID
END;			
				";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}
	}
}
