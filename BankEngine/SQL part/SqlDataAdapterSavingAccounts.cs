using System.Data;
using System.Data.SqlClient;

namespace SQL
{
    public class SqlDataAdapterSavingAccounts
    {
        public	DataTable       dTableSavingAccounts;
        private string          gbConnStr;
		private SqlConnection	gbConn;
        private string          sqlCommand;
        private SqlDataAdapter  daSavingAccounts;

        public SqlDataAdapterSavingAccounts(string gbCS, DataSet ds)
		{
			SetupConnectionString(gbCS);
			SetupSavingAccountsSqlDataAdapter(ds);
			SetupSavingAccountsPrimaryKey();
			SetupSavingAccountsInsertCommand();
		}

		private void SetupConnectionString(string gbCS)
		{
			gbConnStr		 = gbCS;
		}

		private void SetupSavingAccountsSqlDataAdapter(DataSet ds)
		{
			gbConn				= new SqlConnection(gbConnStr);
			daSavingAccounts	= new SqlDataAdapter();
			sqlCommand			= @"SELECT * FROM [dbo].[SavingAccounts];";
			daSavingAccounts.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daSavingAccounts.Fill(ds, "SavingAccounts");
			dTableSavingAccounts = ds.Tables["SavingAccounts"];
		}

		private void SetupSavingAccountsPrimaryKey()
		{ 
			DataColumn[] pk = new DataColumn[1];
			pk[0]			= dTableSavingAccounts.Columns["id"];
			dTableSavingAccounts.PrimaryKey = pk;
		}

		private void SetupSavingAccountsInsertCommand()
		{ 
			sqlCommand = @"INSERT INTO [dbo].[SavingAccounts] ([id]) VALUES (@id);";
			daSavingAccounts.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			daSavingAccounts.InsertCommand.Parameters.Add("@id", SqlDbType.Int, 4, "id");
		}
    }
}
