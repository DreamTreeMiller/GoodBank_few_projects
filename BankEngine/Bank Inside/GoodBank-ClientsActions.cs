using Enumerables;
using ClientClasses;
using DTO;
using Interfaces_Actions;
using Interfaces_Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;

namespace BankInside
{
	public partial class GoodBank : IClientsActions
	{
		private List<Client> clients;
		/// <summary>
		/// Находит клиента с указанным ID
		/// </summary>
		/// <param name="id">ID клиента</param>
		/// <returns></returns>
		public IClient GetClientByID(int id)
		{
			string sqlCommand = @"
SELECT 
";
			return clients.Find(c => c.ID == id);
		}

		/// <summary>
		/// Adds new client to data base
		/// </summary>
		/// <param name="client">DTO with new client's data</param>
		/// <returns>ID of added client</returns>

		public int AddClient(IClientDTO c)
		{
			DataRow[] newRow = new DataRow[1];
			newRow[0] = ds.Tables["ClientsMain"].NewRow();

			newRow[0]["ID"]			= 0;	// это потому что нельзя null передавать 
											// в поле primary key
											// потом получаем из базы реальное значение
			newRow[0]["Telephone"]	= c.Telephone;
			newRow[0]["Email"]		= c.Email;
			newRow[0]["Address"]	= c.Address;

			ds.Tables["ClientsMain"].Rows.Add(newRow[0]);
			daClientsMain.Update(newRow);

			int id = (int)newRow[0]["ID"];

			DataRow[] newClientTypeRow = new DataRow[1];
			switch (c.ClientType)
			{
				case ClientType.VIP:
					newClientTypeRow[0] = ds.Tables["VIPclients"].NewRow();
					newClientTypeRow[0]["id"]				= id;  // Foreign Key для связи с таблицей Clients
					newClientTypeRow[0]["FirstName"]		= c.FirstName;
					newClientTypeRow[0]["MiddleName"]		= c.MiddleName;
					newClientTypeRow[0]["LastName"]			= c.LastName;
					newClientTypeRow[0]["PassportNumber"]	= c.PassportOrTIN;
					newClientTypeRow[0]["BirthDate"]		= c.CreationDate;

					ds.Tables["VIPclients"].Rows.Add(newClientTypeRow[0]);
					daVIPclients.Update(newClientTypeRow);
					break;

				case ClientType.Simple:
					newClientTypeRow[0] = ds.Tables["SIMclients"].NewRow();
					newClientTypeRow[0]["id"] = id; // c.ID;  // Foreign Key для связи с таблицей Clients
					newClientTypeRow[0]["FirstName"] = c.FirstName;
					newClientTypeRow[0]["MiddleName"] = c.MiddleName;
					newClientTypeRow[0]["LastName"] = c.LastName;
					newClientTypeRow[0]["PassportNumber"] = c.PassportOrTIN;
					newClientTypeRow[0]["BirthDate"] = c.CreationDate;
					ds.Tables["SIMclients"].Rows.Add(newClientTypeRow[0]);
					daSIMclients.Update(newClientTypeRow);
					break;

				case ClientType.Organization:
					newClientTypeRow[0] = ds.Tables["ORGclients"].NewRow();

					newClientTypeRow[0]["id"]					= id; // Foreign Key для связи с таблицей Clients
					newClientTypeRow[0]["OrgName"]				= c.MainName;
					newClientTypeRow[0]["DirectorFirstName"]	= c.FirstName;
					newClientTypeRow[0]["DirectorMiddleName"]	= c.MiddleName;
					newClientTypeRow[0]["DirectorLastName"]		= c.LastName;
					newClientTypeRow[0]["TIN"]					= c.PassportOrTIN;
					newClientTypeRow[0]["RegistrationDate"]		= c.CreationDate;

					ds.Tables["ORGclients"].Rows.Add(newClientTypeRow[0]);
					daORGclients.Update(newClientTypeRow);
					break;
			}
			// Обновляем таблицу для показа
			ds.Tables["Clients"].Clear();
			daClients.Fill(ds, "Clients");
			return id;
		}

		public DataView GetClientsTable(ClientType ct)
		{
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
		public void UpdateClient(DataRowView clientRowInTable, IClientDTO updatedClient)
		{
			// Обновляем базу данных - две таблицы:
			// Родительскую таблицу Clients и одну из трёх VIP, SIM or ORGclients
			// в зависимости от типа клиента
			DataRow[] newRow = new DataRow[1];
			newRow[0] = ds.Tables["ClientsMain"].Rows.Find(updatedClient.ID);

			newRow[0]["Telephone"]	= updatedClient.Telephone;
			newRow[0]["Email"]		= updatedClient.Email;
			newRow[0]["Address"]	= updatedClient.Address;

			daClientsMain.Update(newRow);

			DataRow[] newClientTypeRow = new DataRow[1];
			switch (updatedClient.ClientType)
			{
				case ClientType.VIP:
					newClientTypeRow[0] = ds.Tables["VIPclients"].Rows.Find(updatedClient.ID);

					newClientTypeRow[0]["FirstName"]	  = updatedClient.FirstName;
					newClientTypeRow[0]["MiddleName"]	  = updatedClient.MiddleName;
					newClientTypeRow[0]["LastName"]		  = updatedClient.LastName;
					newClientTypeRow[0]["PassportNumber"] = updatedClient.PassportOrTIN;
					newClientTypeRow[0]["BirthDate"]	  = updatedClient.CreationDate;

					daVIPclients.Update(newClientTypeRow);
					break;

				case ClientType.Simple:
					newClientTypeRow[0] = ds.Tables["SIMclients"].Rows.Find(updatedClient.ID);

					newClientTypeRow[0]["FirstName"]	  = updatedClient.FirstName;
					newClientTypeRow[0]["MiddleName"]	  = updatedClient.MiddleName;
					newClientTypeRow[0]["LastName"]		  = updatedClient.LastName;
					newClientTypeRow[0]["PassportNumber"] = updatedClient.PassportOrTIN;
					newClientTypeRow[0]["BirthDate"]	  = updatedClient.CreationDate;

					daSIMclients.Update(newClientTypeRow);
					break;

				case ClientType.Organization:
					newClientTypeRow[0] = ds.Tables["ORGclients"].Rows.Find(updatedClient.ID);

					newClientTypeRow[0]["OrgName"]			  = updatedClient.MainName;
					newClientTypeRow[0]["DirectorFirstName"]  = updatedClient.FirstName;
					newClientTypeRow[0]["DirectorMiddleName"] = updatedClient.MiddleName;
					newClientTypeRow[0]["DirectorLastName"]	  = updatedClient.LastName;
					newClientTypeRow[0]["TIN"]				  = updatedClient.PassportOrTIN;
					newClientTypeRow[0]["RegistrationDate"]	  = updatedClient.CreationDate;

					daORGclients.Update(newClientTypeRow);
					break;
			}

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
