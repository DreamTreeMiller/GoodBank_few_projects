using System;
using System.Data;
using Interfaces_Data;
using BankInside;

namespace ClientClasses
{
	public class СlientORG : Client, IClientOrg
	{
		#region Название организации, ФИО директора, ИНН, дата регистрации

		public string	OrgName				{ get; set; }
		public string	DirectorFirstName	{ get; set; }
		public string	DirectorMiddleName	{ get; set; } = "";
		public string	DirectorLastName	{ get; set; }
		
		/// <summary>
		/// ИНН - Taxpayer Individual Number
		/// </summary>
		public string	TIN	{ get; set; }

		/// <summary>
		/// Дата регистрации организации
		/// </summary>
		public DateTime	RegistrationDate	{ get; set; }

		#endregion

		#region Конструктор

		public СlientORG(IClientDTO newClient)
			: base(newClient.Telephone, newClient.Email, newClient.Address)
		{
			OrgName				= newClient.MainName;
			this.TIN			= newClient.PassportOrTIN;
			RegistrationDate	= (DateTime)newClient.CreationDate;
			DirectorFirstName	= newClient.FirstName;
			DirectorMiddleName	= newClient.MiddleName;
			DirectorLastName	= newClient.LastName;

		}

		#endregion
		public override void UpdateMyself(IClientDTO updatedClient)
		{
			OrgName				= updatedClient.MainName;
			DirectorFirstName	= updatedClient.FirstName;
			DirectorMiddleName	= updatedClient.MiddleName;
			DirectorLastName	= updatedClient.LastName;
			TIN					= updatedClient.PassportOrTIN;
			RegistrationDate	= (DateTime)updatedClient.CreationDate;
			Telephone			= updatedClient.Telephone;
			Email				= updatedClient.Email;
			Address				= updatedClient.Address;
		}
	}
}
