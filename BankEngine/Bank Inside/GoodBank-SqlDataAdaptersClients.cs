using System.Data;
using System.Data.SqlClient;
using Interfaces_Actions;

namespace BankInside
{
	public partial class GoodBank : IClientsActions 
	{
		private DataSet			ds;
		private SqlDataAdapter	daClients, daClientsMain, daVIPclients, daSIMclients, daORGclients;
		private SqlConnection	gbConn;

		public void PopulateTables()
		{
			ds = new DataSet();
			gbConn = SetGoodBankConnection();

			SetupClientsSqlDataAdapter();
			SetupClientsMainSqlDataAdapter();
			SetupVIPclientsSqlDataAdapter();
			SetupSIMclientsSqlDataAdapter();
			SetupORGclientsSqlDataAdapter();

			SetupAccountsParentSqlDataAdapter();
			SetupDepositsSqlDataAdapter();
			SetupCreditsSqlDataAdapter();

			SetupTransactionsSqlDataAdapter();
		}

		private void SetupClientsSqlDataAdapter()
		{
			daClients = new SqlDataAdapter();

			string sqlCommand = @"
SELECT	 [ID] 
		,[ClientType] 
		,[ClientTypeTag] 
		,[FirstName]
		,[MiddleName]
		,[LastName]
		,[MainName]
		,[DirectorName]
		,[CreationDate]
		,[PassportOrTIN]
		,[Telephone]
		,[Email]
		,[Address]
		,[NumberOfSavingAccounts]
		,[NumberOfDeposits]
		,[NumberOfCredits]
		,[NumberOfClosedAccounts]
FROM	(SELECT
			 [ClientsMain].[ID] AS [ID]
			,0 AS [ClientType]		-- VIP
			,N'ВИП' AS [ClientTypeTag] 
			,[FirstName]
			,[MiddleName]
			,[LastName]
			,[LastName] + ' ' + [FirstName]	+ ' ' + [MiddleName] AS [MainName]
			,'' AS [DirectorName]
			,[BirthDate] AS [CreationDate]
			,[PassportNumber] AS [PassportOrTIN]
			,[Telephone]
			,[Email]
			,[Address]
			,[NumberOfSavingAccounts]
			,[NumberOfDeposits]
			,[NumberOfCredits]
			,[NumberOfClosedAccounts]
		FROM	[VIPclients], [ClientsMain]
		WHERE	[ClientsMain].[ID] = [VIPclients].[id] 
		) AS vip
UNION SELECT
		[ClientsMain].[ID] AS [ID]
		,1 AS [ClientType]			-- Simple
		,N'Физик' AS [ClientTypeTag]
		,[FirstName]
		,[MiddleName]
		,[LastName]
		,[LastName] + ' ' + [FirstName] + ' ' + [MiddleName] AS [MainName]
		,'' AS [DirectorName]
		,[BirthDate] AS [CreationDate]
		,[PassportNumber] AS [PassportOrTIN]
		,[Telephone]
		,[Email]
		,[Address]
		,[NumberOfSavingAccounts]
		,[NumberOfDeposits]
		,[NumberOfCredits]
		,[NumberOfClosedAccounts]
FROM	[SIMclients], [ClientsMain]
WHERE	[ClientsMain].[ID] = [SIMclients].[id]
UNION SELECT
		 [ClientsMain].[ID]	  AS [ID]
		,2 AS [ClientType]			-- Organization
		,N'Юрик'			  AS [ClientTypeTag]
		,[DirectorFirstName]  AS [FirstName]
		,[DirectorMiddleName] AS [MiddleName]
		,[DirectorLastName]   AS [LastName]
		,[OrgName]			  AS [MainName]
		,[DirectorLastName] + ' ' + [DirectorFirstName] + ' ' + [DirectorMiddleName]
		 AS [DirectorName]
		,[RegistrationDate]   AS [CreationDate]
		,[TIN]				  AS [PassportOrTIN]
		,[Telephone]
		,[Email]
		,[Address]
		,[NumberOfSavingAccounts]
		,[NumberOfDeposits]
		,[NumberOfCredits]
		,[NumberOfClosedAccounts]
FROM	[ORGclients], [ClientsMain]
WHERE	[ClientsMain].[ID] = [ORGclients].[id]
ORDER BY [ClientType] ASC
";
			daClients.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daClients.Fill(ds, "Clients");
		}

		private void SetupClientsMainSqlDataAdapter()
		{
			daClientsMain = new SqlDataAdapter();

			string sqlCommand = @"SELECT * FROM [dbo].[ClientsMain];";
			daClientsMain.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daClientsMain.Fill(ds, "ClientsMain");

			DataColumn[] pk = new DataColumn[1];
			pk[0] = ds.Tables["ClientsMain"].Columns["ID"];
			ds.Tables["ClientsMain"].PrimaryKey = pk;

			sqlCommand = @"
			INSERT INTO [dbo].[ClientsMain] ([Telephone], [Email], [Address])
					VALUES (@telephone, @email, @address);
			SET @id=@@IDENTITY
			;";
			daClientsMain.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			var id = 
			daClientsMain.InsertCommand.Parameters.Add("@id",		 SqlDbType.Int,	 4,	  "ID");
			id.Direction = ParameterDirection.Output;

			daClientsMain.InsertCommand.Parameters.Add("@telephone", SqlDbType.NVarChar, 30,  "Telephone");
			daClientsMain.InsertCommand.Parameters.Add("@email",	 SqlDbType.NVarChar, 128, "Email");
			daClientsMain.InsertCommand.Parameters.Add("@address",	 SqlDbType.NVarChar, 256, "Address");

			sqlCommand = @"
			UPDATE [dbo].[ClientsMain] 
			SET [Telephone]=@telephone, [Email]=@email, [Address]=@address
			WHERE [ID]=@id
			;";
			daClientsMain.UpdateCommand = new SqlCommand(sqlCommand, gbConn);
			daClientsMain.UpdateCommand.Parameters.Add("@id",		 SqlDbType.Int,		 4,   "ID");
			daClientsMain.UpdateCommand.Parameters.Add("@telephone", SqlDbType.NVarChar, 30,  "Telephone");
			daClientsMain.UpdateCommand.Parameters.Add("@email",	 SqlDbType.NVarChar, 128, "Email");
			daClientsMain.UpdateCommand.Parameters.Add("@address",	 SqlDbType.NVarChar, 256, "Address");
		}

		private void SetupVIPclientsSqlDataAdapter()
		{
			daVIPclients = new SqlDataAdapter();

			string sqlCommand = @"SELECT * FROM [dbo].[VIPclients];";
			daVIPclients.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daVIPclients.Fill(ds, "VIPclients");

			DataColumn[] pk = new DataColumn[1];
			pk[0] = ds.Tables["VIPclients"].Columns["id"];
			ds.Tables["VIPclients"].PrimaryKey = pk;

			sqlCommand = @"
			INSERT INTO [dbo].[VIPclients] ([id], 
											[FirstName], 
											[MiddleName], 
											[LastName],
											[PassportNumber],
											[BirthDate])
					VALUES (@id, @fname, @mname, @lname, @pnum, @bd)
			;";
			daVIPclients.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			daVIPclients.InsertCommand.Parameters.Add("@id",	SqlDbType.Int,		4, "id");
			daVIPclients.InsertCommand.Parameters.Add("@fname",	SqlDbType.NVarChar, 50, "FirstName");
			daVIPclients.InsertCommand.Parameters.Add("@mname",	SqlDbType.NVarChar, 50, "MiddleName");
			daVIPclients.InsertCommand.Parameters.Add("@lname",	SqlDbType.NVarChar, 50, "LastName");
			daVIPclients.InsertCommand.Parameters.Add("@pnum",	SqlDbType.NVarChar, 11, "PassportNumber");
			daVIPclients.InsertCommand.Parameters.Add("@bd",	SqlDbType.Date,		3,  "BirthDate");

			sqlCommand = @"
			UPDATE	[dbo].[VIPclients] 
			SET		[FirstName]		=@fname, 
					[MiddleName]	=@mname, 
					[LastName]		=@lname,
					[PassportNumber]=@pnum,
					[BirthDate]		=@bd
			WHERE	[id]=@id
			;";
			daVIPclients.UpdateCommand = new SqlCommand(sqlCommand, gbConn);
			daVIPclients.UpdateCommand.Parameters.Add("@id",	SqlDbType.Int,		4,	"id");
			daVIPclients.UpdateCommand.Parameters.Add("@fname", SqlDbType.NVarChar, 50, "FirstName");
			daVIPclients.UpdateCommand.Parameters.Add("@mname", SqlDbType.NVarChar, 50, "MiddleName");
			daVIPclients.UpdateCommand.Parameters.Add("@lname", SqlDbType.NVarChar, 50, "LastName");
			daVIPclients.UpdateCommand.Parameters.Add("@pnum",	SqlDbType.NVarChar, 11, "PassportNumber");
			daVIPclients.UpdateCommand.Parameters.Add("@bd",	SqlDbType.Date,		3,	"BirthDate");
		}

		private void SetupSIMclientsSqlDataAdapter()
		{
			daSIMclients = new SqlDataAdapter();

			string sqlCommand = @"SELECT * FROM [dbo].[SIMclients];";
			daSIMclients.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daSIMclients.Fill(ds, "SIMclients");

			DataColumn[] pk = new DataColumn[1];
			pk[0] = ds.Tables["SIMclients"].Columns["id"];
			ds.Tables["SIMclients"].PrimaryKey = pk;

			sqlCommand = @"
			INSERT INTO [dbo].[SIMclients] ([id], 
											[FirstName], 
											[MiddleName], 
											[LastName],
											[PassportNumber],
											[BirthDate])
					VALUES (@id, @fname, @mname, @lname, @pnum, @bd)
			;";
			daSIMclients.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			daSIMclients.InsertCommand.Parameters.Add("@id",	SqlDbType.Int,		4,	"id");
			daSIMclients.InsertCommand.Parameters.Add("@fname", SqlDbType.NVarChar, 50, "FirstName");
			daSIMclients.InsertCommand.Parameters.Add("@mname", SqlDbType.NVarChar, 50, "MiddleName");
			daSIMclients.InsertCommand.Parameters.Add("@lname", SqlDbType.NVarChar, 50, "LastName");
			daSIMclients.InsertCommand.Parameters.Add("@pnum",	SqlDbType.NVarChar, 11, "PassportNumber");
			daSIMclients.InsertCommand.Parameters.Add("@bd",	SqlDbType.Date,		3,	"BirthDate");

			sqlCommand = @"
			UPDATE	[dbo].[SIMclients] 
			SET		[FirstName]		=@fname, 
					[MiddleName]	=@mname, 
					[LastName]		=@lname,
					[PassportNumber]=@pnum,
					[BirthDate]		=@bd
			WHERE	[id]=@id
			;";
			daSIMclients.UpdateCommand = new SqlCommand(sqlCommand, gbConn);
			daSIMclients.UpdateCommand.Parameters.Add("@id",	SqlDbType.Int,		4,	"id");
			daSIMclients.UpdateCommand.Parameters.Add("@fname", SqlDbType.NVarChar, 50, "FirstName");
			daSIMclients.UpdateCommand.Parameters.Add("@mname", SqlDbType.NVarChar, 50, "MiddleName");
			daSIMclients.UpdateCommand.Parameters.Add("@lname", SqlDbType.NVarChar, 50, "LastName");
			daSIMclients.UpdateCommand.Parameters.Add("@pnum",	SqlDbType.NVarChar, 11, "PassportNumber");
			daSIMclients.UpdateCommand.Parameters.Add("@bd",	SqlDbType.Date,		3,	"BirthDate");
		}

		private void SetupORGclientsSqlDataAdapter()
		{
			daORGclients = new SqlDataAdapter();

			string sqlCommand = @"SELECT * FROM [dbo].[ORGclients];";
			daORGclients.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			daORGclients.Fill(ds, "ORGclients");

			DataColumn[] pk = new DataColumn[1];
			pk[0] = ds.Tables["ORGclients"].Columns["id"];
			ds.Tables["ORGclients"].PrimaryKey = pk;

			sqlCommand = @"
			INSERT INTO [dbo].[ORGclients] ([id], 
											[OrgName],
											[DirectorFirstName], 
											[DirectorMiddleName], 
											[DirectorLastName],
											[TIN],
											[RegistrationDate])
					VALUES (@id, @orgname, @dfname, @dmname, @dlname, @tin, @rd)
			;";
			daORGclients.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			daORGclients.InsertCommand.Parameters.Add("@id",	  SqlDbType.Int,	  4,   "id");
			daORGclients.InsertCommand.Parameters.Add("@orgname", SqlDbType.NVarChar, 256, "OrgName");
			daORGclients.InsertCommand.Parameters.Add("@dfname",  SqlDbType.NVarChar, 50,  "DirectorFirstName");
			daORGclients.InsertCommand.Parameters.Add("@dmname",  SqlDbType.NVarChar, 50,  "DirectorMiddleName");
			daORGclients.InsertCommand.Parameters.Add("@dlname",  SqlDbType.NVarChar, 50,  "DirectorLastName");
			daORGclients.InsertCommand.Parameters.Add("@tin",	  SqlDbType.NVarChar, 10,  "TIN");
			daORGclients.InsertCommand.Parameters.Add("@rd",	  SqlDbType.Date,	  3,   "RegistrationDate");

			sqlCommand = @"
			UPDATE	[dbo].[ORGclients] 
			SET		[OrgName]			=@orgname,
					[DirectorFirstName]	=@dfname, 
					[DirectorMiddleName]=@dmname, 
					[DirectorLastName]	=@dlname,
					[TIN]				=@tin,
					[RegistrationDate]	=@rd
			WHERE	[id]=@id
			;";
			daORGclients.UpdateCommand = new SqlCommand(sqlCommand, gbConn);
			daORGclients.UpdateCommand.Parameters.Add("@id",	  SqlDbType.Int,	  4,   "ID");
			daORGclients.UpdateCommand.Parameters.Add("@orgname", SqlDbType.NVarChar, 256, "OrgName");
			daORGclients.UpdateCommand.Parameters.Add("@dfname",  SqlDbType.NVarChar, 50,  "DirectorFirstName");
			daORGclients.UpdateCommand.Parameters.Add("@dmname",  SqlDbType.NVarChar, 50,  "DirectorMiddleName");
			daORGclients.UpdateCommand.Parameters.Add("@dlname",  SqlDbType.NVarChar, 50,  "DirectorLastName");
			daORGclients.UpdateCommand.Parameters.Add("@tin",	  SqlDbType.NVarChar, 10,  "TIN");
			daORGclients.UpdateCommand.Parameters.Add("@rd",	  SqlDbType.Date,	  3,   "RegistrationDate");
		}

	}
} 
