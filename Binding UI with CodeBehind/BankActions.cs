using Interfaces_Actions;
using BankInside;

namespace Binding_UI_CodeBehind
{
	public class BankActions
	{
		public IClientsActions	Clients;
		public IAccountsActions	Accounts;
		public ITransactions	Log;
		public ISearch			Search;
		public ISqlDA			SqlDA;

		private GoodBank		bank = new GoodBank();
		//private GoodBankDB bank = new GoodBankDB();

		public BankActions()
		{
			Clients  = bank as IClientsActions;
			Accounts = bank as IAccountsActions;
			Log		 = bank as ITransactions;
			Search	 = bank as ISearch;
			SqlDA	 = bank as ISqlDA;
		}
	}
}
