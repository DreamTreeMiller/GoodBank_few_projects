using System.Data;
using System.Data.SqlClient;

namespace BankInside
{
	public partial class GoodBank
	{
		private DataSet			ds;
		private SqlDataAdapter	daClientsView;
		private SqlConnection	gbConn;
		private SqlCommand		sqlCommand;

		public void SetupSqlDataAdaptersAndStoredProceduresForTables()
		{
			ds = new DataSet();
			gbConn = SetGoodBankConnection();

			SetupClientsViewSqlDataAdapter();
			SetupSP_AddClient();
			SetupSP_UpdateClientPersonalData();
			SetupSP_UpdateNumberOfAccounts();

			SetupAccountsViewSqlDataAdapter();
			SetupClientAccountsViewSqlDataAdapter();
			SetupSP_AddAccount();
			SetupSP_GetAccountDTObyID();
		}

		/// <summary>
		/// Создаёт в Data Set таблицу ClientsView со всеми клиентами 
		/// </summary>
		private void SetupClientsViewSqlDataAdapter()
		{
			daClientsView				= new SqlDataAdapter();
			string sqlExpression		= @"SELECT * FROM [dbo].[ClientsView]";
			daClientsView.SelectCommand = new SqlCommand(sqlExpression, gbConn);
			daClientsView.Fill(ds, "ClientsView");

			// Set up primary key in order to perform Rows.Find(id)
			DataColumn[] pk = new DataColumn[1];
			pk[0] = ds.Tables["ClientsView"].Columns["ID"];
			ds.Tables["ClientsView"].PrimaryKey = pk;

		}

		private void SetupSP_AddClient()
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string sqlExpression = @"
IF EXISTS (SELECT [name],[type] FROM sys.objects WHERE [name]='SP_AddClient' AND [type]='P')
	DROP PROC [dbo].[SP_AddClient];
";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();

				sqlExpression = @"
CREATE PROC [dbo].[SP_AddClient]
	 @clientType	TINYINT
	,@orgName		NVARCHAR (256)	
	,@firstName		NVARCHAR (50)	
	,@middleName	NVARCHAR (50)	
	,@lastName		NVARCHAR (50)	
	,@passportOrTIN	NVARCHAR (11)	
	,@creationDate	DATE			
	,@telephone		NVARCHAR (30)
	,@email			NVARCHAR (128)
	,@address		NVARCHAR (256)
AS
BEGIN
	DECLARE @clientID INT;
	INSERT INTO [dbo].[ClientsMain]
		([Telephone], [Email], [Address])
	VALUES (@telephone, @email, @address);
	SET @clientID=@@IDENTITY;
	IF @clientType=0	-- VIP
		INSERT INTO [dbo].[VIPclients] 
			([id], [FirstName], [MiddleName], [LastName], [PassportNumber], [BirthDate])
		VALUES (@clientID, @firstName, @middleName, @lastName, @passportOrTIN, @creationDate);
	ELSE 
	IF @clientType=1	-- Simple
		INSERT INTO [dbo].[SIMclients] 
			([id], [FirstName], [MiddleName], [LastName], [PassportNumber], [BirthDate])
		VALUES (@clientID, @firstName, @middleName, @lastName, @passportOrTIN, @creationDate);
	ELSE 
	IF @clientType=2	-- Org
		INSERT INTO [dbo].[ORGclients] 
			([id]
			,[OrgName]
			,[DirectorFirstName]
			,[DirectorMiddleName]
			,[DirectorLastName]
			,[TIN]
			,[RegistrationDate])
		VALUES (@clientID, @orgName, @firstName, @middleName, @lastName, @passportOrTIN, @creationDate);
	RETURN @clientID;
END;			
";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Обновляет основные таблицы данных о клиенте:
		/// ClientsMain, VIP, SIM, ORGclients.
		/// НЕ обновляет таблицу ClientsView для показа на экране
		/// </summary>
		private void SetupSP_UpdateClientPersonalData()
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string sqlExpression = @"
IF EXISTS (SELECT [name],[type] FROM sys.objects WHERE [name]='SP_UpdateClientPersonalData' AND [type]='P')
	DROP PROC [dbo].[SP_UpdateClientPersonalData];
";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();

				sqlExpression = @"
CREATE PROC [dbo].[SP_UpdateClientPersonalData]
	 @clientID		INT
	,@clientType	TINYINT
	,@orgName		NVARCHAR (256)	
	,@firstName		NVARCHAR (50)	
	,@middleName	NVARCHAR (50)	
	,@lastName		NVARCHAR (50)	
	,@passportOrTIN	NVARCHAR (10)	
	,@creationDate	DATE			
	,@telephone		NVARCHAR (30)
	,@email			NVARCHAR (128)
	,@address		NVARCHAR (256)
AS
BEGIN
	UPDATE [dbo].[ClientsMain] 
	SET [Telephone]=@telephone, [Email]=@email, [Address]=@address
	WHERE [ID]=@clientID;
	IF @clientType=0	-- VIP
		UPDATE	[dbo].[VIPclients] 
		SET		[FirstName]		=@firstName, 
				[MiddleName]	=@middleName, 
				[LastName]		=@lastName,
				[PassportNumber]=@passportOrTIN,	
				[BirthDate]		=@creationDate	
		WHERE	[id]=@clientID
	ELSE 
	IF @clientType=1	-- Simple
		UPDATE	[dbo].[SIMclients] 
		SET		[FirstName]		=@firstName, 
				[MiddleName]	=@middleName, 
				[LastName]		=@lastName,
				[PassportNumber]=@passportOrTIN,	
				[BirthDate]		=@creationDate	
		WHERE	[id]=@clientID
	ELSE 
	IF @clientType=2	-- Org
		UPDATE	[dbo].[ORGclients] 
		SET		[OrgName]			=@orgname,
				[DirectorFirstName]	=@firstName,  
				[DirectorMiddleName]=@middleName,  
				[DirectorLastName]	=@lastName,
				[TIN]				=@passportOrTIN,
				[RegistrationDate]	=@creationDate	
		WHERE	[id]=@clientID
END;";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}

		private void SetupSP_UpdateNumberOfAccounts()
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string sqlExpression = @"
IF EXISTS (SELECT [name], [type] FROM sys.objects WHERE [name]='SP_UpdateNumberOfAccounts' AND [type]='P')
	DROP PROC [dbo].[SP_UpdateNumberOfAccounts];
";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();

				sqlExpression = @"
CREATE PROCEDURE [dbo].[SP_UpdateNumberOfAccounts]
	 @clientID			INT
	,@savingsUpdate		INT
	,@depositsUpdate	INT
	,@creditsUpdate		INT
	,@closedUpdate		INT
AS
DECLARE	@numOfSavingAcc	INT;
DECLARE	@numOfDeposits	INT;
DECLARE	@numOfCredits	INT;
DECLARE	@numOfClosedAcc	INT;

SELECT   @numOfSavingAcc = [NumberOfSavingAccounts]
		,@numOfDeposits	 = [NumberOfDeposits]
		,@numOfCredits	 = [NumberOfCredits]
		,@numOfClosedAcc = [NumberOfClosedAccounts]
FROM	[dbo].[ClientsMain]
WHERE	[ID]=@clientID;

UPDATE  [dbo].[ClientsMain]
SET		 [NumberOfSavingAccounts] = @numOfSavingAcc	+ @savingsUpdate
		,[NumberOfDeposits]		  =	@numOfDeposits	+ @depositsUpdate
		,[NumberOfCredits]		  =	@numOfCredits	+ @creditsUpdate
		,[NumberOfClosedAccounts] =	@numOfClosedAcc	+ @closedUpdate
WHERE	[ID] = @clientID;
";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}
	}
} 
