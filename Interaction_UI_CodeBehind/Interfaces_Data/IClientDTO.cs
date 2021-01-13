using Enumerables;
using System;

namespace Interfaces_Data
{
	public interface IClientDTO
	{
		int			ID						{ get; }
		ClientType	ClientType				{ get; set; }
		string		FirstName				{ get; set; }
		string		MiddleName				{ get; set; }
		string		LastName				{ get; set; }
		string		MainName				{ get; set; }
		string		DirectorName			{ get; }
		string		PassportOrTIN			{ get; set; }
		DateTime?	CreationDate			{ get; set; }
		string		Telephone				{ get; set; }
		string		Email					{ get; set; }
		string		Address					{ get; set; }
		int			NumberOfSavingAccounts	{ get; set; }
		int			NumberOfDeposits		{ get; set; }
		int			NumberOfCredits			{ get; set; }
		int			NumberOfClosedAccounts  { get; set; }
	}
}
