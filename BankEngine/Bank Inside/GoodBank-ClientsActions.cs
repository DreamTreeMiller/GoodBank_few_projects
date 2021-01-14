using Enumerables;
using DTO;
using Interfaces_Actions;
using Interfaces_Data;
using System.Data;
using System.Data.SqlClient;

namespace BankInside
{
	public partial class GoodBank : IClientsActions
	{
		/// <summary>
		/// Находит клиента с указанным ID
		/// </summary>
		/// <param name="id">ID клиента</param>
		/// <returns></returns>
		public DataRow GetClientByID(int id)
		{
			DataRow clientRow = ds.Tables["ClientsView"].Rows.Find(id);
			return clientRow;
		}

		/// <summary>
		/// Adds new client to data base
		/// </summary>
		/// <param name="client">DTO with new client's data</param>
		/// <returns>ID of added client</returns>

		public int AddClient(IClientDTO c)
		{
			string sqlCommandAddClient = $@"
DECLARE @newClientId INT;
EXEC @newClientId=[dbo].[SP_AddClient] 
	 {(byte)c.ClientType}		
	,N'{c.MainName}'		-- org name or empty
	,N'{c.FirstName}'
	,N'{c.MiddleName}'
	,N'{c.LastName}'
	,'{c.PassportOrTIN}'
	,'{c.CreationDate:yyyy-MM-dd}'
	,'{c.Telephone}'
	,'{c.Email}'
	,N'{c.Address}';
SELECT @newClientId;
";
			int newClientID = 0;

			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				sqlCommand = new SqlCommand(sqlCommandAddClient, gbConn);
				newClientID = (int)sqlCommand.ExecuteScalar();
			}

			return newClientID;
		}

		/// <summary>
		/// Удаляет и заново создаёт таблицу [dbo].[ClietnsView], 
		/// в которой собраны данные из всех трёх таблиц клиентов
		/// </summary>
		public void RefreshClientsViewTable()
		{
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string sqlExpression = @"
IF EXISTS (SELECT [name],[type] FROM sys.objects WHERE [name]='ClientsView' AND [type]='U')	
	DROP TABLE [dbo].[ClientsView];
SELECT
	 [ClientsMain].[ID]	AS [ID]
	,0					AS [ClientType]		-- VIP
	,N'ВИП'				AS [ClientTypeTag] 
	,[FirstName]
	,[MiddleName]
	,[LastName]
	,[LastName] + ' ' + [FirstName]	+ ' ' + [MiddleName] 
						AS [MainName]
	,''					AS [DirectorName]
	,[BirthDate]		AS [CreationDate]
	,[PassportNumber]	AS [PassportOrTIN]
	,[Telephone]
	,[Email]
	,[Address]
	,[NumberOfSavingAccounts]
	,[NumberOfDeposits]
	,[NumberOfCredits]
	,[NumberOfClosedAccounts]
INTO [dbo].[ClientsView]
FROM	[VIPclients], [ClientsMain]
WHERE	[ClientsMain].[ID] = [VIPclients].[id] 
UNION SELECT
		 [ClientsMain].[ID]	AS [ID]
		,1					AS [ClientType]			-- Simple
		,N'Физик'			AS [ClientTypeTag]
		,[FirstName]
		,[MiddleName]
		,[LastName]
		,[LastName] + ' ' + [FirstName] + ' ' + [MiddleName] 
							AS [MainName]
		,''					AS [DirectorName]
		,[BirthDate]		AS [CreationDate]
		,[PassportNumber]	AS [PassportOrTIN]
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
		,2					  AS [ClientType]			-- Organization
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
WHERE	[ClientsMain].[ID] = [ORGclients].[id]";
				sqlCommand = new SqlCommand(sqlExpression, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}

		public DataView GetClientsTable(ClientType ct)
		{
			// Обновляем таблицу для показа
			RefreshClientsViewTable();
			ds.Tables["ClientsView"].Clear();
			daClientsView.Fill(ds, "ClientsView");

			string rowfilter = (ct == ClientType.All) ? "" : "ClientType = " + (int)ct;
			DataView clientsViewTable = 
				new DataView(ds.Tables["ClientsView"],	// Table to show
							 rowfilter,					// Row filter (select type)
							 "ID ASC",					// Sort ascending by 'ID' field
							 DataViewRowState.CurrentRows);
			return clientsViewTable;
		}

		/// <summary>
		/// Обновляет данные о клиенте в базе данных
		/// </summary>
		/// <param name="clientRowInTable">Строка со старыми данными о клиенете в таблице показа</param>
		/// <param name="updatedClient">Обновлённые данные о клиенте</param>
		public void UpdateClientPersonalData(IClientDTO updatedClient)
		{

			// Обновляем данные в самой базе, НЕ в списке на экране!!!
			string sqlCommandAddClient = $@"
EXEC [dbo].[SP_UpdateClientPersonalData] 
	 {updatedClient.ID}
	,{(byte)updatedClient.ClientType}
	,N'{updatedClient.MainName}'		-- org name or empty
	,N'{updatedClient.FirstName}'
	,N'{updatedClient.MiddleName}'
	,N'{updatedClient.LastName}'
	,'{updatedClient.PassportOrTIN}'
	,'{updatedClient.CreationDate:yyyy-MM-dd}'
	,'{updatedClient.Telephone}'
	,'{updatedClient.Email}'
	,N'{updatedClient.Address}';
";
			using (gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				sqlCommand = new SqlCommand(sqlCommandAddClient, gbConn);
				sqlCommand.ExecuteNonQuery();
			}
		}
	}
}
