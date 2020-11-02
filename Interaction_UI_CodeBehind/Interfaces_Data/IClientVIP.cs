﻿using System;

namespace Interfaces_Data
{
	public interface IClientVIP : IClient
	{
		string FirstName		{ get; set; }
		string MiddleName		{ get; set; }
		string LastName			{ get; set; }
		string PassportNumber	{ get; set; }
		DateTime BirthDate		{ get; set; }
	}
}
