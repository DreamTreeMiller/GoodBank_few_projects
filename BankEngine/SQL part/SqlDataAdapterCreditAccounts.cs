using System.Data;
using System.Data.SqlClient;

namespace SQL
{
    public class SqlDataAdapterCreditAccounts
    {
        public	DataTable       dTableCreditAccounts;
        private string          gbConnStr;
		private SqlConnection	gbConn;
        private string          sqlCommand;
        private SqlDataAdapter  daCreditAccounts;

        public SqlDataAdapterCreditAccounts(string gbCS, DataSet ds)
		{
			SetupConnectionString(gbCS);
			SetupCreditAccountsSqlDataAdapter(ds);
			SetupCreditAccountsPrimaryKey();
			SetupCreditAccountsInsertCommand();
			SetupCreditAccountsUpdateCommand();
		}

		private void SetupConnectionString(string gbCS)
		{
			gbConnStr		 = gbCS;
		}

		private void SetupCreditAccountsSqlDataAdapter(DataSet ds)
		{
			gbConn				= new SqlConnection(gbConnStr);
			daCreditAccounts	= new SqlDataAdapter();
			sqlCommand			= @"SELECT * FROM [dbo].[CreditAccounts];";
			daCreditAccounts.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daCreditAccounts.Fill(ds, "CreditAccounts");
			dTableCreditAccounts = ds.Tables["CreditAccounts"];
		}

		private void SetupCreditAccountsPrimaryKey()
		{ 
			DataColumn[] pk = new DataColumn[1];
			pk[0]			= dTableCreditAccounts.Columns["id"];
			dTableCreditAccounts.PrimaryKey = pk;
		}

		private void SetupCreditAccountsInsertCommand()
		{ 
			sqlCommand = @"
			INSERT INTO [dbo].[CreditAccounts] 
				([id]					-- INT NOT NULL PRIMARY KEY, FOREIGN KEY
				,[AccumulatedInterest]	-- MONEY DEFAULT 0	NOT NULL
				)
			VALUES (@id, @accumulatedInterest);
				";
			daCreditAccounts.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			daCreditAccounts.InsertCommand.Parameters.
				Add("@id",					SqlDbType.Int,	 4, "id");
			daCreditAccounts.InsertCommand.Parameters.
				Add("@accumulatedInterest",	SqlDbType.Money, 8, "AccumulatedInterest");
		}

		private void SetupCreditAccountsUpdateCommand()
		{ 
			sqlCommand = @"
			UPDATE	[dbo].[CreditAccounts] 
			SET		[AccumulatedInterest]	= @accumulatedInterest
			WHERE	[id]=@id;
				";
			daCreditAccounts.UpdateCommand = new SqlCommand(sqlCommand, gbConn);
			daCreditAccounts.UpdateCommand.Parameters.
				Add("@id",					SqlDbType.Int,	 4, "id");
			daCreditAccounts.UpdateCommand.Parameters.
				Add("@accumulatedInterest", SqlDbType.Money, 8, "AccumulatedInterest");
		}

		public void Update()
		{
			daCreditAccounts.Update(dTableCreditAccounts);
		}
    }
}
