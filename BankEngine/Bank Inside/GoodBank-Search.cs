using DTO;
using Interfaces;
using Interfaces_Data;
using Search_Engine;
using System.Collections.ObjectModel;

namespace BankInside
{
	public partial class GoodBank : ISearch
	{
		public ObservableCollection<IClientDTO> FindClients(Compare predicate)
		{
			ObservableCollection<IClientDTO> clientsList = new ObservableCollection<IClientDTO>();
			foreach (var c in clients)
			{
				bool flag = true;
				flag = predicate(c, ref flag);
				if (flag) clientsList.Add(new ClientDTO(c));
			}
			return clientsList;
		}
	}
}
