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

		public IClientDTO AddClient(IClientDTO client)
		{
			Client newClient = null;
			switch (client.ClientType)
			{
				case ClientType.VIP:
					newClient = new ClientVIP(client);
					break;
				case ClientType.Simple:
					newClient = new СlientSIM(client);
					break;
				case ClientType.Organization:
					newClient = new СlientORG(client);
					break;
			}
			IClientDTO nc = new ClientDTO(newClient);
			InsertClient(nc);
			return nc;
		}

	public void InsertClient(IClientDTO c)
	{
		DataRow[] newRow = new DataRow[1];
		newRow[0] = ds.Tables["Clients"].NewRow();

		newRow[0]["Telephone"]	= c.Telephone;
		newRow[0]["Email"]		= c.Email;
		newRow[0]["Address"]	= c.Address;

		ds.Tables["Clients"].Rows.Add(newRow[0]);
		daClients.Update(newRow);

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

				ds.Tables["VIPclients"].Rows.Add(newClientTypeRow);
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
				ds.Tables["SIMclients"].Rows.Add(newClientTypeRow);
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
				ds.Tables["ORGclients"].Rows.Add(newClientTypeRow);
				daORGclients.Update(newClientTypeRow);
				break;
		}
	}

	public ObservableCollection<IClientDTO> GetClientsList<TClient>()
		{
			ObservableCollection<IClientDTO> clientsList = new ObservableCollection<IClientDTO>();
			foreach (var c in clients)
				if (c is TClient) clientsList.Add(new ClientDTO(c));
			return clientsList;
		}

		public void UpdateClient(IClientDTO updatedClient)
		{
			int ci = clients.FindIndex(c => c.ID == updatedClient.ID);
			clients[ci].UpdateMyself(updatedClient);
		}
	}
}
