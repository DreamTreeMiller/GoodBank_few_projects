namespace Interfaces_Data
{
	public interface IClient
	{
		/// <summary>
		/// ID клиента в базе
		/// </summary>
		int ID { get; }

		string Telephone { get; set; }
		string Email { get; set; }
		string Address { get; set; }

		int NumberOfSavingAccounts { get; set; }
		int NumberOfDeposits { get; set; }
		int NumberOfCredits { get; set; }
		int NumberOfClosedAccounts { get; set; }
	}
}
