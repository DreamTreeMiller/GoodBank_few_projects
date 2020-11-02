using Interfaces_Data;
using Search_Engine;
using System.Collections.ObjectModel;

namespace Interfaces_Actions
{
	public interface ISearch
	{
		public ObservableCollection<IClientDTO> FindClients(Compare predicate);
	}
}
