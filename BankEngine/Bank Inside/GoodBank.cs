using Enumerables;
using ClientClasses;
using Interfaces_Data;
using System.Collections.Generic;
using Transaction_Class;

namespace BankInside
{
	public partial class GoodBank : IGoodBank
	{
		public GoodBank()
		{
			clients  = new List<Client>();
			accounts = new List<Account>();
			log		 = new List<Transaction>();
		}
	}
}
