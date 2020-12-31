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
		//newRow["ID"]		= c.ID;				// Уникальный ID клиента
		newRow[0]["Telephone"] = c.Telephone;
		newRow[0]["Email"] = c.Email;
		newRow[0]["Address"] = c.Address;
		ds.Tables["Clients"].Rows.Add(newRow[0]);
		daClients.Update(newRow);

		int id = (int)newRow[0]["ID"];

		DataRow newClientTypeRow;
		switch (c.ClientType)
		{
			case ClientType.VIP:
				newClientTypeRow = ds.Tables["VIPclients"].NewRow();
				newClientTypeRow["id"] = id; // c.ID;  // Foreign Key для связи с таблицей Clients
				newClientTypeRow["FirstName"] = c.FirstName;
				newClientTypeRow["MiddleName"] = c.MiddleName;
				newClientTypeRow["LastName"] = c.LastName;
				newClientTypeRow["PassportNumber"] = c.PassportOrTIN;
				newClientTypeRow["BirthDate"] = c.CreationDate;
				ds.Tables["VIPclients"].Rows.Add(newClientTypeRow);
				break;

			case ClientType.Simple:
				newClientTypeRow = ds.Tables["SIMclients"].NewRow();
				newClientTypeRow["id"] = id; // c.ID;  // Foreign Key для связи с таблицей Clients
				newClientTypeRow["FirstName"] = c.FirstName;
				newClientTypeRow["MiddleName"] = c.MiddleName;
				newClientTypeRow["LastName"] = c.LastName;
				newClientTypeRow["PassportNumber"] = c.PassportOrTIN;
				newClientTypeRow["BirthDate"] = c.CreationDate;
				ds.Tables["SIMclients"].Rows.Add(newClientTypeRow);
				break;

			case ClientType.Organization:
				newClientTypeRow = ds.Tables["ORGclients"].NewRow();
				newClientTypeRow["id"] = id; // c.ID;  // Foreign Key для связи с таблицей Clients
				newClientTypeRow["OrgName"] = c.MainName;
				newClientTypeRow["DirectorFirstName"] = c.FirstName;
				newClientTypeRow["DirectorMiddleName"] = c.MiddleName;
				newClientTypeRow["DirectorLastName"] = c.LastName;
				newClientTypeRow["TIN"] = c.PassportOrTIN;
				newClientTypeRow["RegistrationDate"] = c.CreationDate;
				ds.Tables["ORGclients"].Rows.Add(newClientTypeRow);
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
