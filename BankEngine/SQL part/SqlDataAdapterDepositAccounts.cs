using System.Data;
using System.Data.SqlClient;

namespace SQL
{
    public class SqlDataAdapterDepositAccounts
    {
        public	DataTable       dTableDepositAccounts;
        private string          gbConnStr;
		private SqlConnection	gbConn;
        private string          sqlCommand;
        private SqlDataAdapter  daDepositAccounts;

        public SqlDataAdapterDepositAccounts(string gbCS, DataSet ds)
		{
			SetupConnectionString(gbCS);
			SetupDepositAccountsSqlDataAdapter(ds);
			SetupDepositAccountsPrimaryKey();
			SetupDepositAccountsInsertCommand();
			SetupDepositAccountsUpdateCommand();
		}

		private void SetupConnectionString(string gbCS)
		{
			gbConnStr		 = gbCS;
		}

		private void SetupDepositAccountsSqlDataAdapter(DataSet ds)
		{
			gbConn				= new SqlConnection(gbConnStr);
			daDepositAccounts	= new SqlDataAdapter();
			sqlCommand			= @"SELECT * FROM [dbo].[DepositAccounts];";
			daDepositAccounts.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daDepositAccounts.Fill(ds, "DepositAccounts");
			dTableDepositAccounts = ds.Tables["DepositAccounts"];
		}

		private void SetupDepositAccountsPrimaryKey()
		{ 
			DataColumn[] pk = new DataColumn[1];
			pk[0]			= dTableDepositAccounts.Columns["id"];
			dTableDepositAccounts.PrimaryKey = pk;
		}

		private void SetupDepositAccountsInsertCommand()
		{ 
			sqlCommand = @"
			INSERT INTO [dbo].[DepositAccounts] 
				([id]							-- INT
				,[InterestAccumulationAccID]	-- INT				NOT NULL
				,[InterestAccumulationAccNum]	-- NVARCHAR (15)	NOT NULL
				,[AccumulatedInterest]			-- MONEY DEFAULT 0	NOT NULL
				)
			VALUES 
				(@id 
				,@interestAccumulationAccID
				,@interestAccumulationAccNum
				,@accumulatedInterest
				);
				";
			daDepositAccounts.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			daDepositAccounts.InsertCommand.Parameters.
				Add("@id",							SqlDbType.Int,		4,	"id");
			daDepositAccounts.InsertCommand.Parameters.
				Add("@interestAccumulationAccID",	SqlDbType.Int,		4,	"InterestAccumulationAccID");
			daDepositAccounts.InsertCommand.Parameters.
				Add("@interestAccumulationAccNum",	SqlDbType.NVarChar,	15, "InterestAccumulationAccNum");
			daDepositAccounts.InsertCommand.Parameters.
				Add("@accumulatedInterest",			SqlDbType.Money,	8,	"AccumulatedInterest");
		}

		private void SetupDepositAccountsUpdateCommand()
		{ 
			sqlCommand = @"
			UPDATE	[dbo].[DepositAccounts] 
			SET		 [InterestAccumulationAccID]	= @interestAccumulationAccID
					,[InterestAccumulationAccNum]	= @interestAccumulationAccNum
					,[AccumulatedInterest]			= @accumulatedInterest
			WHERE	[id]=@id;
				";
			daDepositAccounts.UpdateCommand = new SqlCommand(sqlCommand, gbConn);
			daDepositAccounts.UpdateCommand.Parameters.
				Add("@id",							SqlDbType.Int,		4,	"id");
			daDepositAccounts.UpdateCommand.Parameters.
				Add("@interestAccumulationAccID",	SqlDbType.Int,		4,	"InterestAccumulationAccID");
			daDepositAccounts.UpdateCommand.Parameters.
				Add("@interestAccumulationAccNum",	SqlDbType.NVarChar,	15, "InterestAccumulationAccNum");
			daDepositAccounts.UpdateCommand.Parameters.
				Add("@accumulatedInterest",			SqlDbType.Money,	8,	"AccumulatedInterest");
		}
		
		public void Update()
		{
			daDepositAccounts.Update(dTableDepositAccounts);
		}
    }
}
