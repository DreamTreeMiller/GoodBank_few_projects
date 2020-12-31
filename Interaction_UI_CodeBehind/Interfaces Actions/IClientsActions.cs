using Interfaces_Data;
using System.Collections.ObjectModel;

namespace Interfaces_Actions
{
	public interface IClientsActions
	{
		IClient GetClientByID(int id);
		ObservableCollection<IClientDTO> GetClientsList<TClient>();
		IClientDTO AddClient(IClientDTO client);
		void UpdateClient(IClientDTO updatedClient);
	}
}
