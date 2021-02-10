using Interfaces_Actions;
using BankInside;

namespace Binding_UI_CodeBehind
{
	public class BankActions
	{
		public IClientsActions	Clients;
		public IAccountsActions	Accounts;
		public ILogActions		Log;

		private GoodBank		bank = new GoodBank();

		public BankActions()
		{
			Clients  = bank;
			Accounts = bank;
			Log		 = bank;
		}
	}
}
