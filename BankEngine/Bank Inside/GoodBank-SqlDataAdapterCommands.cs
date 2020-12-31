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
	public partial class GoodBank : IClientsActions
	{

		public SqlDataAdapter	da,
								daClients, daVIPclients, daSIMclients, daORGclients,
								daAccounts, daDeposits, daCredits, // no da for Saving accounts
								daTransactions;
		public  DataSet			ds;
		private SqlConnection	gbConn;

		public void PopulateTables()
		{
			ds = new DataSet();
			da = new SqlDataAdapter();

			string sqlCommand;
			gbConn = SetGoodBankConnection();

			sqlCommand = @"SELECT * FROM [dbo].[Clients];";
			da.SelectCommand = new SqlCommand(sqlCommand, gbConn);
			da.Fill(ds, "Clients");
			SetupClientsSqlDataAdapter();

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
			string sqlCommand;
			daClients = new SqlDataAdapter();

			sqlCommand = @"
			INSERT INTO [dbo].[Clients] ([Telephone], [Email], [Address])
					VALUES (@telephone, @email, @address);
			SET @id=@@IDENTITY
			;";
			daClients.InsertCommand = new SqlCommand(sqlCommand, gbConn);
			var id = daClients.InsertCommand.Parameters.Add("@id",		 SqlDbType.Int,	 4,	  "ID");
			id.Direction = ParameterDirection.Output;

			daClients.InsertCommand.Parameters.Add("@telephone", SqlDbType.NVarChar, 30,  "Telephone");
			daClients.InsertCommand.Parameters.Add("@email",	 SqlDbType.NVarChar, 128, "Email");
			daClients.InsertCommand.Parameters.Add("@address",	 SqlDbType.NVarChar, 256, "Address");

			sqlCommand = @"
			UPDATE [dbo].[Clients] 
			SET [Telephone]=@telephone, [Email]=@email, [Address]=@address
			WHERE [ID]=@id
			;";
			daClients.UpdateCommand = new SqlCommand(sqlCommand, gbConn);
			daClients.UpdateCommand.Parameters.Add("@id",		 SqlDbType.Int,		 4,   "ID");
			daClients.UpdateCommand.Parameters.Add("@telephone", SqlDbType.NVarChar, 30,  "Telephone");
			daClients.UpdateCommand.Parameters.Add("@email",	 SqlDbType.NVarChar, 128, "Email");
			daClients.UpdateCommand.Parameters.Add("@address",	 SqlDbType.NVarChar, 256, "Address");
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
