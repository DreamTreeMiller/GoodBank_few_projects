using System.Data;
using Interfaces_Data;
using Enumerables;


namespace Interfaces_Actions
{
	public interface IClientsActions
	{
		IClientDTO GetClientByID(int id);

		void RefreshClientsViewTable();
		DataView GetClientsTable(ClientType ct);

		/// <summary>
		/// Adds new client to data base
		/// </summary>
		/// <param name="client">DTO with new client's data</param>
		/// <returns>ID of added client</returns>
		int AddClient(IClientDTO client);
		void UpdateClientPersonalData(DataRowView clientRowInTable, IClientDTO updatedClient);
	}
}
