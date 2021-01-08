using Enumerables;
using ClientClasses;
using DTO;
using Interfaces_Actions;
using Interfaces_Data;
using System.Collections.Generic;
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
		public IClientDTO GetClientByID(int id)
		{
			string sqlCommand = @"
SELECT 
";
			return new ClientDTO();
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

		public DataView GetClientsTable(ClientType ct)
		{
			// Обновляем таблицу для показа
			ds.Tables["Clients"].Clear();
			daClients.Fill(ds, "Clients");

			string rowfilter = (ct == ClientType.All) ? "" : "ClientType = " + (int)ct;
			DataView clientsTable = 
				new DataView(ds.Tables["Clients"],		// Table to show
							 rowfilter,					// Row filter (select type)
							 "MainName ASC",			// Sort ascending by 'MainName' field
							 DataViewRowState.CurrentRows);
			return clientsTable;
		}

		/// <summary>
		/// Обновляет данные о клиенте в базе данных и в таблице для показа
		/// </summary>
		/// <param name="clientRowInTable">Строка со старыми данными о клиенете в таблице показа</param>
		/// <param name="updatedClient">Обновлённые данные о клиенте</param>
		public void UpdateClientPersonalData(DataRowView clientRowInTable, IClientDTO updatedClient)
		{
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

			#region Update Client's Personal Data via Data Set -- disabled

			// Обновляем базу данных - две таблицы:
			// Родительскую таблицу Clients и одну из трёх VIP, SIM or ORGclients
			// в зависимости от типа клиента
			//DataRow[] newRow = new DataRow[1];
			//newRow[0] = ds.Tables["ClientsMain"].Rows.Find(updatedClient.ID);

			//newRow[0]["Telephone"]	= updatedClient.Telephone;
			//newRow[0]["Email"]		= updatedClient.Email;
			//newRow[0]["Address"]	= updatedClient.Address;

			//daClientsMain.Update(newRow);

			//DataRow[] newClientTypeRow = new DataRow[1];
			//switch (updatedClient.ClientType)
			//{
			//	case ClientType.VIP:
			//		newClientTypeRow[0] = ds.Tables["VIPclients"].Rows.Find(updatedClient.ID);

			//		newClientTypeRow[0]["FirstName"]	  = updatedClient.FirstName;
			//		newClientTypeRow[0]["MiddleName"]	  = updatedClient.MiddleName;
			//		newClientTypeRow[0]["LastName"]		  = updatedClient.LastName;
			//		newClientTypeRow[0]["PassportNumber"] = updatedClient.PassportOrTIN;
			//		newClientTypeRow[0]["BirthDate"]	  = updatedClient.CreationDate;

			//		daVIPclients.Update(newClientTypeRow);
			//		break;

			//	case ClientType.Simple:
			//		newClientTypeRow[0] = ds.Tables["SIMclients"].Rows.Find(updatedClient.ID);

			//		newClientTypeRow[0]["FirstName"]	  = updatedClient.FirstName;
			//		newClientTypeRow[0]["MiddleName"]	  = updatedClient.MiddleName;
			//		newClientTypeRow[0]["LastName"]		  = updatedClient.LastName;
			//		newClientTypeRow[0]["PassportNumber"] = updatedClient.PassportOrTIN;
			//		newClientTypeRow[0]["BirthDate"]	  = updatedClient.CreationDate;

			//		daSIMclients.Update(newClientTypeRow);
			//		break;

			//	case ClientType.Organization:
			//		newClientTypeRow[0] = ds.Tables["ORGclients"].Rows.Find(updatedClient.ID);

			//		newClientTypeRow[0]["OrgName"]			  = updatedClient.MainName;
			//		newClientTypeRow[0]["DirectorFirstName"]  = updatedClient.FirstName;
			//		newClientTypeRow[0]["DirectorMiddleName"] = updatedClient.MiddleName;
			//		newClientTypeRow[0]["DirectorLastName"]	  = updatedClient.LastName;
			//		newClientTypeRow[0]["TIN"]				  = updatedClient.PassportOrTIN;
			//		newClientTypeRow[0]["RegistrationDate"]	  = updatedClient.CreationDate;

			//		daORGclients.Update(newClientTypeRow);
			//		break;
			//}

			#endregion

			// Обновляем таблицу показа
			// Это работает, но не подгружает данные из базы данных
			// Поэтому, если база используется одновременно несколькими пользователями,
			// и кто-то другой изменит базу в другом месте, 
			// эти изменения в таблицу показа не попадут.
			// Но заморачиваться с отслеживанием такого поведения сил уже нет.
			// Хочу поскорее сдать это задание.
			clientRowInTable["ClientType"]				= updatedClient.ClientType;
			clientRowInTable["FirstName"]				= updatedClient.FirstName;
			clientRowInTable["MiddleName"]				= updatedClient.MiddleName;
			clientRowInTable["LastName"]				= updatedClient.LastName;
			clientRowInTable["MainName"]				= updatedClient.MainName;
			clientRowInTable["DirectorName"]			= updatedClient.DirectorName;
			clientRowInTable["CreationDate"]			= updatedClient.CreationDate;
			clientRowInTable["PassportOrTIN"]			= updatedClient.PassportOrTIN;
			clientRowInTable["Telephone"]				= updatedClient.Telephone;
			clientRowInTable["Email"]					= updatedClient.Email;
			clientRowInTable["Address"]					= updatedClient.Address;
			clientRowInTable["NumberOfSavingAccounts"]	= updatedClient.NumberOfSavingAccounts;
			clientRowInTable["NumberOfDeposits"]		= updatedClient.NumberOfDeposits;
			clientRowInTable["NumberOfCredits"]			= updatedClient.NumberOfCredits;
			clientRowInTable["NumberOfClosedAccounts"]	= updatedClient.NumberOfClosedAccounts;
		}
	}
}
