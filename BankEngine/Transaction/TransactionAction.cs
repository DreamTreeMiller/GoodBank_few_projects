using System.Data;
using System.Data.SqlClient;

namespace Transaction
{
	public class TransactionAction
	{
		private string			gbConnectionString;
		private DataSet			ds;
		private SqlConnection	gbConn;
		private SqlDataAdapter	daTransactions;
		public TransactionAction (string gbCS, DataSet ds) 
		{
			gbConnectionString = gbCS;
			this.ds = ds;
			daTransactions = new SqlDataAdapter();
		}

		private void SetupTransactionsSqlDataAdapter()
		{
			gbConn = new SqlConnection(gbConnectionString);

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
				Add("@transactionAccountID", SqlDbType.Int, 4, "TransactionAccountID");
			daTransactions.InsertCommand.Parameters.
				Add("@transactionDateTime", SqlDbType.SmallDateTime, 4, "TransactionDateTime");
			daTransactions.InsertCommand.Parameters.
				Add("@sourceAccount", SqlDbType.NVarChar, 15, "SourceAccount");
			daTransactions.InsertCommand.Parameters.
				Add("@destinationAccount", SqlDbType.NVarChar, 15, "DestinationAccount");
			daTransactions.InsertCommand.Parameters.
				Add("@operationType", SqlDbType.Int, 1, "OperationType");
			daTransactions.InsertCommand.Parameters.
				Add("@amount", SqlDbType.Money, 4, "Amount");
			daTransactions.InsertCommand.Parameters.
				Add("@Comment", SqlDbType.NVarChar, 256, "Comment");
		}

		public void WriteLog(TransactionDTO tr)
		{
			return; // for now
			string sqlExpression = $@"
EXEC SP_WriteLog
			 {tr.TransactionAccountID}
			,{tr.TransactionDateTime}
			,{tr.SourceAccount}
			,{tr.DestinationAccount}
			,{tr.OperationType}
			,{tr.Amount}
			,{tr.Comment}
			";

			using (SqlConnection gbConn = new SqlConnection(gbConnectionString))
			{
				gbConn.Open();
				SqlCommand sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}
	}
}
