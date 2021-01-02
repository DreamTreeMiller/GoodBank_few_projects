using System;
using System.Data;
using Interfaces_Data;
using BankInside;

namespace ClientClasses
{
	public class СlientSIM : Client, IClientSimple
	{
		#region ФИО, № паспорта, дата рождения

		public string	FirstName		{ get; set; }
		public string	MiddleName		{ get; set; } = "";
		public string	LastName		{ get; set; }
		public string	PassportNumber  { get; set; }
		public DateTime BirthDate		{ get; set; }

		#endregion

		#region Конструктор

		public СlientSIM(IClientDTO newClient)
			: base(newClient.Telephone, newClient.Email, newClient.Address)
		{
			FirstName		= newClient.FirstName;
			MiddleName		= newClient.MiddleName;
			LastName		= newClient.LastName;
			PassportNumber	= newClient.PassportOrTIN;
			BirthDate		= (DateTime)newClient.CreationDate;
		}

		#endregion
		public override void UpdateMyself(IClientDTO updatedClient)
		{
			FirstName		= updatedClient.FirstName;
			MiddleName		= updatedClient.MiddleName;
			LastName		= updatedClient.LastName;
			PassportNumber	= updatedClient.PassportOrTIN;
			BirthDate		= (DateTime)updatedClient.CreationDate;
			Telephone		= updatedClient.Telephone;
			Email			= updatedClient.Email;
			Address			= updatedClient.Address;

			// Запись ВИП клиента в базу
			DataRow[] newClientRow = new DataRow[1];
			newClientRow[0] = GoodBank.ds.Tables["SIMclients"].NewRow();
			newClientRow[0]["id"]				= ID;  // Foreign Key для связи с таблицей Clients
			newClientRow[0]["FirstName"]		= FirstName;
			newClientRow[0]["MiddleName"]		= MiddleName;
			newClientRow[0]["LastName"]			= LastName;
			newClientRow[0]["PassportNumber"]	= PassportNumber;
			newClientRow[0]["BirthDate"]		= BirthDate;

			GoodBank.ds.Tables["SIMclients"].Rows.Add(newClientRow);
			GoodBank.daSIMclients.Update(newClientRow);
		}
	}
}
