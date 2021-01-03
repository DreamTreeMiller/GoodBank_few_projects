using System.Data;
using System.Data.SqlClient;

namespace Interfaces_Actions
{
	public interface ISqlDA
	{
		DataSet ds { get; set; }
		SqlDataAdapter daClients { get; set; }
	}
}
