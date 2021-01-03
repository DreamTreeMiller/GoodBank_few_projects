using System.Data;
using System.Data.SqlClient;
using Interfaces_Actions;
using Interfaces_Data;
using Enumerables;
using System.Collections.Generic;
using System.Text;
using ClientClasses;
using System.Collections.ObjectModel;
using DTO;

namespace BankInside
{
	public partial class GoodBank : IClientsActions, ISqlDA 
	{
		public DataSet			ds			{ get; set; }
		public SqlDataAdapter	daClients	{ get; set; }
		public SqlDataAdapter	da,
			daClientsMain, daVIPclients, daSIMclients, daORGclients,
			daAccounts, daDeposits, daCredits, // no da for Saving accounts
			daTransactions;
		internal static SqlConnection	gbConn;

		public void PopulateTables()
		{
			ds = new DataSet();
			da = new SqlDataAdapter();

			string sqlCommand;
			gbConn = SetGoodBankConnection();

			sqlCommand = @"
SELECT	 [ID] 
		,[ClientType] 
		,[ClientTypeTag] 
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
			 [ClientsMain].[ID] AS [ID],
			 0 AS [ClientType],		-- VIP
			 N'ВИП' AS [ClientTypeTag], 
			 [LastName] + ' ' + [FirstName] + ' ' + [MiddleName] AS [MainName],
			 '' AS [DirectorName],
			 [BirthDate] AS [CreationDate],
			 [PassportNumber] AS [PassportOrTIN],
			 [Telephone],
			 [Email],
			 [Address]
			,[NumberOfSavingAccounts]
			,[NumberOfDeposits]
			,[NumberOfCredits]
			,[NumberOfClosedAccounts]
		FROM	[VIPclients], [ClientsMain]
		WHERE	[ClientsMain].[ID] = [VIPclients].[id] 
		) AS vip
UNION SELECT
		[ClientsMain].[ID] AS [ID],
		1 AS [ClientType],			-- Simple
		N'Физик' AS [ClientTypeTag], 
		[LastName] + ' ' + [FirstName] + ' ' + [MiddleName] AS [MainName],
		'' AS [DirectorName],
		[BirthDate] AS [CreationDate],
		[PassportNumber] AS [PassportOrTIN],
		[Telephone],
		[Email],
		[Address]
		,[NumberOfSavingAccounts]
		,[NumberOfDeposits]
		,[NumberOfCredits]
		,[NumberOfClosedAccounts]
FROM	[SIMclients], [ClientsMain]
WHERE	[ClientsMain].[ID] = [SIMclients].[id]
UNION SELECT
		[ClientsMain].[ID] AS [ID],
		2 AS [ClientType],			-- Organization
		N'Юрик' AS [ClientTypeTag], 
		[OrgName] AS [MainName],
		[DirectorLastName] + ' ' + [DirectorFirstName] + ' ' + [DirectorMiddleName] AS [DirectorName],
		[RegistrationDate] AS [CreationDate],
		[TIN] AS [PassportOrTIN],
		[Telephone],
		[Email],
		[Address]
		,[NumberOfSavingAccounts]
		,[NumberOfDeposits]
		,[NumberOfCredits]
		,[NumberOfClosedAccounts]
FROM	[ORGclients], [ClientsMain]
WHERE	[ClientsMain].[ID] = [ORGclients].[id]
ORDER BY [ClientType] ASC
";
			da.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			da.Fill(ds, "Clients");
			SetupClientsSqlDataAdapter();

			sqlCommand = @"SELECT * FROM [dbo].[ClientsMain];";
			da.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			da.Fill(ds, "ClientsMain");
			SetupClientsMainSqlDataAdapter();

			sqlCommand = @"SELECT * FROM [dbo].[VIPclients];";
			da.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			da.Fill(ds, "VIPclients");
			SetupVIPclientsSqlDataAdapter();

			sqlCommand = @"SELECT * FROM [dbo].[SIMclients];";
			da.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			da.Fill(ds, "SIMclients");
			SetupSIMclientsSqlDataAdapter();

			sqlCommand = @"SELECT * FROM [dbo].[ORGclients];";
			da.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			da.Fill(ds, "ORGclients");
			SetupORGclientsSqlDataAdapter();

			sqlCommand = @"SELECT * FROM [dbo].[Accounts];";
			da.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			da.Fill(ds, "Accounts");

			sqlCommand = @"SELECT * FROM [dbo].[DepositAccounts];";
			da.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			da.Fill(ds, "DepositAccounts");

			sqlCommand = @"SELECT * FROM [dbo].[CreditAccounts];";
			da.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			da.Fill(ds, "CreditAccounts");

			sqlCommand = @"SELECT * FROM [dbo].[Transactions];";
			da.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			da.Fill(ds, "Transactions");
		}

		private void SetupClientsSqlDataAdapter()
		{

		}
		private void SetupClientsMainSqlDataAdapter()
		{
			string sqlCommand;
			daClientsMain = new SqlDataAdapter();

			sqlCommand = @"
			INSERT INTO [dbo].[ClientsMain] ([Telephone], [Email], [Address])
					VALUES (@telephone, @email, @address);
			SET @id=@@IDENTITY
			;";
			daClientsMain.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			var id = daClientsMain.InsertCommand.Parameters.Add("@id",		 SqlDbType.Int,	 4,	  "ID");
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
			string sqlCommand;
			daVIPclients = new SqlDataAdapter();

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
			string sqlCommand;
			daSIMclients = new SqlDataAdapter();

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
			string sqlCommand;
			daORGclients = new SqlDataAdapter();

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
