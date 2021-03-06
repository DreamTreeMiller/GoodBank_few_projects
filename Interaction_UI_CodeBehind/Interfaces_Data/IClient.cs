﻿namespace Interfaces_Data
{
	public interface IClient
	{
		/// <summary>
		/// ID клиента в базе
		/// </summary>
		uint ID { get; }

		string Telephone { get; set; }
		string Email { get; set; }
		string Address { get; set; }

		int NumberOfCurrentAccounts { get; set; }
		int NumberOfDeposits { get; set; }
		int NumberOfCredits { get; set; }
		int NumberOfClosedAccounts { get; set; }
	}
}
