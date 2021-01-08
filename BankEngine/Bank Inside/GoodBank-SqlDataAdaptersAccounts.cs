using System.Data;
using System.Data.SqlClient;
using Interfaces_Actions;

namespace BankInside
{
	public partial class GoodBank : IAccountsActions
	{
		private SqlDataAdapter	daAccounts,
								daAccountsParent, daDeposits, daCredits, // no da for Saving accounts
								daTransactions;

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
	,@interest						DECIMAL
	,@compounding					BIT
	,@opened						DATE
	,@duration						INT
	,@monthsElapsed					INT
	,@endDate						DATE
	,@closed						DATE
	,@topupable						BIT
	,@withdrawalAllowed				BIT
	,@recalcPeriod					TINYINT
	,@numberOfTopUpsInDay			INT
	,@isBlocked						BIT
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
		,[NumberOfTopUpsInDay]
		,[IsBlocked]
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
		,@numberOfTopUpsInDay
		,@isBlocked
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
	RETURN;
END;			
";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}


		private void SetupAccountsParentSqlDataAdapter()
		{
			gbConn = SetGoodBankConnection();
			daAccountsParent = new SqlDataAdapter();

			string sqlCommand = @"SELECT * FROM [dbo].[AccountsParent];";
			daAccountsParent.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daAccountsParent.Fill(ds, "AccountsParent");

			DataColumn[] pk = new DataColumn[1];
			pk[0] = ds.Tables["AccountsParent"].Columns["AccID"];
			ds.Tables["AccountsParent"].PrimaryKey = pk;

			sqlCommand = @"
			INSERT INTO [dbo].[AccountsParent] 
				([AccType]					-- TINYINT
				,[ClientID]					-- INT				NOT NULL
				,[AccountNumber]			-- NVARCHAR (15)	NOT NULL
				,[Balance]					-- MONEY DEFAULT 0	NOT NULL
				,[Interest]					-- DECIMAL (4,2)	NOT NULL
				,[Compounding]				-- BIT				NOT NULL
				,[Opened]					-- DATE				NOT NULL
				,[Duration]					-- INT				NOT NULL
				,[MonthsElapsed]			-- INT				NOT NULL
				,[EndDate]					-- DATE		null - бессрочно
				,[Closed]					-- DATE		null - счёт открыт
				,[Topupable]				-- BIT				NOT NULL
				,[WithdrawalAllowed]		-- BIT				NOT NULL
				,[RecalcPeriod]				-- TINYINT			NOT NULL
				,[NumberOfTopUpsInDay]		-- INT DEFAULT 0	NOT NULL
				,[IsBlocked]				-- BIT DEFAULT 0	NOT NUll
				)
			VALUES 
				(@accType 
				,@clientID
				,@accountNumber
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
				,@numberOfTopUpsInDay
				,@isBlocked
				);
			SET @accID=@@IDENTITY
				";
			daAccountsParent.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			var accID =
			daAccountsParent.InsertCommand.Parameters.
				Add("@accID",				SqlDbType.Int,		4,	"AccID");
			accID.Direction = ParameterDirection.Output;

			daAccountsParent.InsertCommand.Parameters.
				Add("@accType",				SqlDbType.TinyInt,	1,	"AccType");
			daAccountsParent.InsertCommand.Parameters.
				Add("@clientID",			SqlDbType.Int,		4,	"ClientID");
			daAccountsParent.InsertCommand.Parameters.
				Add("@accountNumber",		SqlDbType.NVarChar,	15,	"AccountNumber");
			daAccountsParent.InsertCommand.Parameters.
				Add("@balance",				SqlDbType.Money,	8,	"Balance");
			daAccountsParent.InsertCommand.Parameters.
				Add("@interest",			SqlDbType.Decimal,	5,	"Interest");
			daAccountsParent.InsertCommand.Parameters.
				Add("@compounding",			SqlDbType.Bit,		1,	"Compounding");
			daAccountsParent.InsertCommand.Parameters.
				Add("@opened",				SqlDbType.Date,		3,	"Opened");
			daAccountsParent.InsertCommand.Parameters.
				Add("@duration",			SqlDbType.Int,		4,	"Duration");
			daAccountsParent.InsertCommand.Parameters.
				Add("@monthsElapsed",		SqlDbType.Int,		4,	"MonthsElapsed");
			daAccountsParent.InsertCommand.Parameters.
				Add("@endDate",				SqlDbType.Date,		3,	"EndDate");
			daAccountsParent.InsertCommand.Parameters.
				Add("@closed",				SqlDbType.Date,		3,	"Closed");
			daAccountsParent.InsertCommand.Parameters.
				Add("@topupable",			SqlDbType.Bit,		1,	"Topupable");
			daAccountsParent.InsertCommand.Parameters.
				Add("@withdrawalAllowed",	SqlDbType.Bit,		1,	"WithdrawalAllowed");
			daAccountsParent.InsertCommand.Parameters.
				Add("@recalcPeriod",		SqlDbType.TinyInt,	1,	"RecalcPeriod");
			daAccountsParent.InsertCommand.Parameters.
				Add("@numberOfTopUpsInDay", SqlDbType.Int,		4,	"NumberOfTopUpsInDay");
			daAccountsParent.InsertCommand.Parameters.
				Add("@isBlocked",			SqlDbType.Bit,		1,	"IsBlocked");

			sqlCommand = @"
			UPDATE	[dbo].[AccountsParent] 
			SET		 [Balance]				= @balance
					,[MonthsElapsed]		= @monthsElapsed
					,[Closed]				= @closed
					,[Topupable]			= @topupable
					,[WithdrawalAllowed]	= @withdrawalAllowed
					,[NumberOfTopUpsInDay]	= @numberOfTopUpsInDay
					,[IsBlocked]			= @isBlocked
			WHERE	[AccID]=@accID
			;";
			daAccountsParent.UpdateCommand = new SqlCommand(sqlCommand, gbConn);
			daAccountsParent.UpdateCommand.Parameters.
				Add("@accID",				SqlDbType.Int,		1,	"AccID");
			daAccountsParent.UpdateCommand.Parameters.
				Add("@balance",				SqlDbType.Money,	8,	"Balance");
			daAccountsParent.UpdateCommand.Parameters.
				Add("@monthsElapsed",		SqlDbType.Int,		4,	"MonthsElapsed");
			daAccountsParent.UpdateCommand.Parameters.
				Add("@closed",				SqlDbType.Date,		3,	"Closed");
			daAccountsParent.UpdateCommand.Parameters.
				Add("@topupable",			SqlDbType.Bit,		1,	"Topupable");
			daAccountsParent.UpdateCommand.Parameters.
				Add("@withdrawalAllowed",	SqlDbType.Bit,		1,	"WithdrawalAllowed");
			daAccountsParent.UpdateCommand.Parameters.
				Add("@numberOfTopUpsInDay", SqlDbType.Int,		4,	"NumberOfTopUpsInDay");
			daAccountsParent.UpdateCommand.Parameters.
				Add("@isBlocked",			SqlDbType.Bit,		1,	"IsBlocked");
		}

		private void SetupDepositsSqlDataAdapter()
		{
			daDeposits = new SqlDataAdapter();

			string sqlCommand = @"SELECT * FROM [dbo].[DepositAccounts];";
			daDeposits.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daDeposits.Fill(ds, "DepositAccounts");

			DataColumn[] pk = new DataColumn[1];
			pk[0] = ds.Tables["DepositAccounts"].Columns["id"];
			ds.Tables["DepositAccounts"].PrimaryKey = pk;

			sqlCommand = @"
			INSERT INTO [dbo].[DepositAccounts] 
				([id]							-- INT							NOT NULL PRIMARY KEY,
				,[InterestAccumulationAccID]	-- INT				DEFAULT 0	NOT NULL,
				,[InterestAccumulationAccNum]	-- NVARCHAR (15)	DEFAULT ''	NOT NULL,
				,[AccumulatedInterest]			-- MONEY			DEFAULT 0	NON NULL
				)
			VALUES 
				(@id 
				,@interestAccumulationAccID
				,@interestAccumulationAccNum
				,@accumulatedInterest
				);
			";
			daDeposits.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			daDeposits.InsertCommand.Parameters.
				Add("@id",							SqlDbType.Int,		4,	"id");
			daDeposits.InsertCommand.Parameters.
				Add("@interestAccumulationAccID",	SqlDbType.Int,		4,	"InterestAccumulationAccID");
			daDeposits.InsertCommand.Parameters.
				Add("@interestAccumulationAccNum",	SqlDbType.NVarChar, 15, "InterestAccumulationAccNum");
			daDeposits.InsertCommand.Parameters.
				Add("@accumulatedInterest",			SqlDbType.Money,	4,	"AccumulatedInterest");

			sqlCommand = @"
			UPDATE	[dbo].[DepositAccounts] 
			SET		[AccumulatedInterest] = @accumulatedInterest
			WHERE	[id]=@aid
			;";
			daDeposits.UpdateCommand = new SqlCommand(sqlCommand, gbConn);
			daDeposits.UpdateCommand.Parameters.
				Add("@id",					SqlDbType.Int,	 1, "id");
			daDeposits.UpdateCommand.Parameters.
				Add("@accumulatedInterest", SqlDbType.Money, 8, "AccumulatedInterest");
		}

		private void SetupCreditsSqlDataAdapter()
		{
			daCredits = new SqlDataAdapter();

			string sqlCommand = @"SELECT * FROM [dbo].[CreditAccounts];";
			daCredits.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daCredits.Fill(ds, "CreditAccounts");

			DataColumn[] pk = new DataColumn[1];
			pk[0] = ds.Tables["CreditAccounts"].Columns["id"];
			ds.Tables["CreditAccounts"].PrimaryKey = pk;

			sqlCommand = @"
			INSERT INTO [dbo].[DepositAccounts] 
				([id]							-- INT				NOT NULL PRIMARY KEY,
				,[AccumulatedInterest]			-- MONEY DEFAULT 0	NON NULL
				)
			VALUES (@id, @accumulatedInterest);
			";
			daCredits.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			daCredits.InsertCommand.Parameters.
				Add("@id",					SqlDbType.Int,	 4, "id");
			daCredits.InsertCommand.Parameters.
				Add("@accumulatedInterest", SqlDbType.Money, 4, "AccumulatedInterest");

			sqlCommand = @"
			UPDATE	[dbo].[DepositAccounts] 
			SET		[AccumulatedInterest] = @accumulatedInterest
			WHERE	[id]=@aid;
			";
			daCredits.UpdateCommand = new SqlCommand(sqlCommand, gbConn);
			daCredits.UpdateCommand.Parameters.
				Add("@id",					SqlDbType.Int,	 1, "id");
			daCredits.UpdateCommand.Parameters.
				Add("@accumulatedInterest", SqlDbType.Money, 8, "AccumulatedInterest");
		}

		private void SetupTransactionsSqlDataAdapter()
		{
			daTransactions = new SqlDataAdapter();

			string sqlCommand = @"SELECT * FROM [dbo].[Transactions];";
			daTransactions.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daTransactions.Fill(ds, "Transactions");

			sqlCommand = @"
			INSERT INTO [dbo].[Transactions] 
				([TransactionAccountID]	-- INT							NOT NULL,
				,[TransactionDateTime]	-- SMALLDATETIME				NOT NULL,
				,[SourceAccount]		-- NVARCHAR (15)	DEFAULT ''	NOT NULL,
				,[DestinationAccount]	-- NVARCHAR (15)	DEFAULT ''	NOT NULL,
				,[OperationType]		-- TINYINT						NOT NULL,
				,[Amount]				-- MONEY			DEFAULT 0	NOT NULL,
				,[Comment]				-- NVARCHAR (256)	DEFAULT ''	NOT NULL
				)
			VALUES 
				(@transactionAccountID
				,@transactionDateTime
				,@sourceAccount
				,@destinationAccount
				,@operationType
				,@amount
				,@comment
				);
			";
			daTransactions.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			daTransactions.InsertCommand.Parameters.
				Add("@transactionAccountID", SqlDbType.Int,			  4,	"TransactionAccountID");
			daTransactions.InsertCommand.Parameters.
				Add("@transactionDateTime",  SqlDbType.SmallDateTime, 4,	"TransactionDateTime");
			daTransactions.InsertCommand.Parameters.
				Add("@sourceAccount",		 SqlDbType.NVarChar,	  15,	"SourceAccount");
			daTransactions.InsertCommand.Parameters.
				Add("@destinationAccount",	 SqlDbType.NVarChar,	  15,	"DestinationAccount");
			daTransactions.InsertCommand.Parameters.
				Add("@operationType",		 SqlDbType.TinyInt,		  1,	"OperationType");
			daTransactions.InsertCommand.Parameters.
				Add("@amount",				 SqlDbType.Money,		  4,	"Amount");
			daTransactions.InsertCommand.Parameters.
				Add("@Comment",				 SqlDbType.NVarChar,	  256,	"Comment");
		}
	}
}
