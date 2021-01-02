﻿using System;
using System.Data;
using Interfaces_Data;
using BankInside;

namespace ClientClasses
{
	public class ClientVIP : Client, IClientVIP
	{
		#region ФИО, № паспорта, дата рождения

		public string	FirstName		{ get; set; }
		public string	MiddleName		{ get; set; }
		public string	LastName		{ get; set; }
		public string	PassportNumber	{ get; set; }
		public DateTime	BirthDate		{ get; set; }

		#endregion

		#region Конструктор

		public ClientVIP(IClientDTO newClient)
			: base(newClient.Telephone, newClient.Email, newClient.Address)
		{
			FirstName		= newClient.FirstName;
			MiddleName		= newClient.MiddleName;
			LastName		= newClient.LastName;
			PassportNumber	= newClient.PassportOrTIN;
			BirthDate		= (DateTime)newClient.CreationDate;

			// Запись ВИП клиента в базу
			DataRow[] newClientRow = new DataRow[1];
			newClientRow[0] = GoodBank.ds.Tables["VIPclients"].NewRow();
			newClientRow[0]["id"]				= ID;  // Foreign Key для связи с таблицей Clients
			newClientRow[0]["FirstName"]		= FirstName;
			newClientRow[0]["MiddleName"]		= MiddleName;
			newClientRow[0]["LastName"]			= LastName;
			newClientRow[0]["PassportNumber"]	= PassportNumber;
			newClientRow[0]["BirthDate"]		= BirthDate;

			GoodBank.ds.Tables["VIPclients"].Rows.Add(newClientRow);
			GoodBank.daVIPclients.Update(newClientRow);
		}

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
		}

		#endregion
	}
}
