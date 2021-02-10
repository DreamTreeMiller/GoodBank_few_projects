using System.Data;
using System.Data.SqlClient;

namespace Log
{
	public class LogAction
	{
		private string			gbConnectionString;
		private DataTable		dTableTransactions;
		private SqlDataAdapter	daTransactions;
		public LogAction (string gbCS, DataSet ds) 
		{
			gbConnectionString = gbCS;
			daTransactions = new SqlDataAdapter();
			SetupTransactionsSqlDataAdapter(ds);
			SetupSP_WriteLog();
		}

		private void SetupTransactionsSqlDataAdapter(DataSet ds)
		{
			SqlConnection gbConn = new SqlConnection(gbConnectionString);

			string sqlCommand = @"SELECT * FROM [dbo].[Transactions];";
			daTransactions.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daTransactions.Fill(ds, "Transactions");
			dTableTransactions = ds.Tables["Transactions"];

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
				Add("@transactionDateTime",	 SqlDbType.SmallDateTime, 4,	"TransactionDateTime");
			daTransactions.InsertCommand.Parameters.
				Add("@sourceAccount",		 SqlDbType.NVarChar,	  20,	"SourceAccount");
			daTransactions.InsertCommand.Parameters.
				Add("@destinationAccount",	 SqlDbType.NVarChar,	  15,	"DestinationAccount");
			daTransactions.InsertCommand.Parameters.
				Add("@operationType",		 SqlDbType.Int,			  1,	"OperationType");
			daTransactions.InsertCommand.Parameters.
				Add("@amount",				 SqlDbType.Money,		  4,	"Amount");
			daTransactions.InsertCommand.Parameters.
				Add("@Comment",				 SqlDbType.NVarChar,	  256,	"Comment");
		}

		public void Clear()
		{
			dTableTransactions.Clear();
		}

		public void Update()
		{
			daTransactions.Update(dTableTransactions);
		}

		private void SetupSP_WriteLog()
		{
			using (SqlConnection gbConn = new SqlConnection(gbConnectionString))
			{
				gbConn.Open();
				string sqlExpression = @"
IF EXISTS (SELECT [name],[type] FROM sys.objects WHERE [name]='SP_WriteLog' AND [type]='P')
	DROP PROC [dbo].[SP_WriteLog];
				";
				SqlCommand sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();

				sqlExpression = @"
CREATE PROC [dbo].[SP_WriteLog]
	 @ransactionAccountID	INT				-- 				NOT NULL
	,@transactionDateTime	SMALLDATETIME	-- 				NOT NULL
	,@sourceAccount			NVARCHAR (15)	-- DEFAULT ''	NOT NULL
	,@destinationAccount	NVARCHAR (15)	-- DEFAULT ''	NOT NULL
	,@operationType			INT				-- 				NOT NULL
	,@amount				MONEY			-- DEFAULT 0	NOT NULL
	,@comment				NVARCHAR (256)	-- DEFAULT ''	NOT NULL
AS
	INSERT INTO [dbo].[Transactions]
		([TransactionAccountID]	
		,[TransactionDateTime]	
		,[SourceAccount]			
		,[DestinationAccount]	
		,[OperationType]			
		,[Amount]				
		,[Comment]				
		)
	VALUES 
		(@ransactionAccountID	
		,@transactionDateTime	
		,@sourceAccount			
		,@destinationAccount	
		,@operationType			
		,@amount				
		,@comment				
		);
				";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}

		public void WriteLog(TransactionDTO tr)
		{
			string sqlExpression = $@"
EXEC SP_WriteLog
			 {tr.TransactionAccountID}
			,'{tr.TransactionDateTime:yyyy-MM-dd HH:mm:ss}'
			,N'{tr.SourceAccount}'
			,N'{tr.DestinationAccount}'
			,{(int)tr.OperationType}
			,{tr.Amount}
			,N'{tr.Comment}'
			";

			using (SqlConnection gbConn = new SqlConnection(gbConnectionString))
			{
				gbConn.Open();
				SqlCommand sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Формирует список всех транзакций указанного счета
		/// </summary>
		/// <param name="account"></param>
		/// <returns></returns>
		public DataView GetAccountTransactionsLog(int accID)
		{
			dTableTransactions.Clear();
			daTransactions.Fill(dTableTransactions);

			string rowfilter = "TransactionAccountID = " + accID;
			DataView TransactionsLogView =
				new DataView(dTableTransactions,   // Table to show
							 rowfilter,
							 "TransactionID ASC",                     // Sort ascending by 'ID' field
							 DataViewRowState.CurrentRows);
			return TransactionsLogView;
		}
	}
}
