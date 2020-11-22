using Interfaces_Data;
using Search_Engine;
using System.Collections.ObjectModel;

namespace Interfaces
{
	public interface ISearch
	{
		public ObservableCollection<IClientDTO> FindClients(Compare predicate);
	}
}
