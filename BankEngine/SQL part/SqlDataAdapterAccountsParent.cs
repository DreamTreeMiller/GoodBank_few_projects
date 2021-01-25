using System.Data;
using System.Data.SqlClient;

namespace SQL
{
    public class SqlDataAdapterAccountsParent
    {
        public	DataTable       dTableAccountsParent;
        private string          gbConnStr;
		private SqlConnection	gbConn;
        private string          sqlCommand;
        private SqlDataAdapter  daAccountsParent;

        public SqlDataAdapterAccountsParent(string gbCS, DataSet ds)
		{
			SetupConnectionString(gbCS);
			SetupAccountsParentSqlDataAdapter(ds);
			SetupAccountsParentPrimaryKey();
			SetupAccountsParentInsertCommand();
			SetupAccountsParentUpdateCommand();
		}

		private void SetupConnectionString(string gbCS)
		{
			gbConnStr		 = gbCS;
		}

		private void SetupAccountsParentSqlDataAdapter(DataSet ds)
		{
			gbConn				= new SqlConnection(gbConnStr);
			daAccountsParent	= new SqlDataAdapter();
			sqlCommand			= @"SELECT * FROM [dbo].[AccountsParent];";
			daAccountsParent.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daAccountsParent.Fill(ds, "AccountsParent");
			dTableAccountsParent = ds.Tables["AccountsParent"];
		}

		private void SetupAccountsParentPrimaryKey()
		{ 
			DataColumn[] pk = new DataColumn[1];
			pk[0]			= dTableAccountsParent.Columns["AccID"];
			dTableAccountsParent.PrimaryKey = pk;
		}

		private void SetupAccountsParentInsertCommand()
		{ 
			sqlCommand = @"
			INSERT INTO [dbo].[AccountsParent] 
				([AccType]					-- INT
				,[ClientID]					-- INT				NOT NULL
				,[AccountNumber]			-- NVARCHAR (15)	NOT NULL
				,[Balance]					-- MONEY DEFAULT 0	NOT NULL
				,[Interest]					-- FLOAT			NOT NULL
				,[Compounding]				-- BIT				NOT NULL
				,[Opened]					-- DATE				NOT NULL
				,[Duration]					-- INT				NOT NULL
				,[MonthsElapsed]			-- INT				NOT NULL
				,[EndDate]					-- DATE		null - бессрочно
				,[StopRecalculate]			-- BIT				NOT NULL
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
				,@stopRecalculate
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
				Add("@accType",				SqlDbType.Int,		4,	"AccType");
			daAccountsParent.InsertCommand.Parameters.
				Add("@clientID",			SqlDbType.Int,		4,	"ClientID");
			daAccountsParent.InsertCommand.Parameters.
				Add("@accountNumber",		SqlDbType.NVarChar,	15,	"AccountNumber");
			daAccountsParent.InsertCommand.Parameters.
				Add("@balance",				SqlDbType.Money,	8,	"Balance");
			daAccountsParent.InsertCommand.Parameters.
				Add("@interest",			SqlDbType.Float,	4,	"Interest");
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
				Add("@stopRecalculate",		SqlDbType.Bit,		1,  "StopRecalculate");
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
		}

		private void SetupAccountsParentUpdateCommand()
		{ 
			sqlCommand = @"
			UPDATE	[dbo].[AccountsParent] 
			SET		 [Balance]				= @balance
					,[MonthsElapsed]		= @monthsElapsed
					,[StopRecalculate]		= @stopRecalculate
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
				Add("@stopRecalculate",		SqlDbType.Bit,		1,  "StopRecalculate");
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
		
		public void Update()
		{
			daAccountsParent.Update(dTableAccountsParent);
		}
    }
}
