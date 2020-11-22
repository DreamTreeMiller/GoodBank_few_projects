using Interfaces_Data;
using System.Collections.ObjectModel;

namespace Interfaces
{
	public interface IClientsActions
	{
		IClient GetClientByID(uint id);
		ObservableCollection<IClientDTO> GetClientsList<TClient>();
		IClientDTO AddClient(IClientDTO client);
		IClient AddGeneratedClient(IClientDTO client);
		void UpdateClient(IClientDTO updatedClient);
	}
}
