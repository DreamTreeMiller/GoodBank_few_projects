﻿using System.Data.SqlClient;
using Enumerables;
using ClientClasses;
using DTO;
using Interfaces_Actions;
using Interfaces_Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BankDataBaseEngine
{
	public partial class GoodBankDB : IClientsActions
	{
		private List<Client> clients;

		/// <summary>
		/// Находит клиента с указанным ID
		/// </summary>
		/// <param name="id">ID клиента</param>
		/// <returns></returns>
		public IClient GetClientByID(uint id)
		{
			using (SqlConnection gbConn = SetGoodBankConnection())
			{

			}
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

			clients.Add(newClient);

			return new ClientDTO(newClient);
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
