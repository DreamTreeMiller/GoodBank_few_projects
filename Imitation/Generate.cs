using Interfaces_Data;
using Binding_UI_CodeBehind;
using System;
using DTO;
using Enumerables;
using BankTime;
using System.Threading.Tasks;

namespace Imitation
{
	public static class Generate
	{
		static BankActions BA;
		static Random r = new Random();
		static int orgCount = 1;

		public static void Bank(BankActions ba, int vip, int sim, int org)
		{
			BA = ba;
			GenerateVIPclientsAndAccounts(vip);
			GenerateSIMclientsAndAccounts(sim);
			GenerateORGclientsAndAccounts(org);
		}

		public static void BankAsync(BankActions ba, int vip, int sim, int org)
		{
			BA = ba;
			Parallel.For(0, vip-1, GenerateVIPclientsAndAccountsAsync);
			Parallel.For(0, sim-1, GenerateSIMclientsAndAccountsAsync);
			Parallel.For(0, org-1, GenerateORGclientsAndAccountsAsync);
		}

		#region Создание клиентов

		private static void GenerateVIPclientsAndAccountsAsync(int num)
		{
			string FN, MN, LN;

			// Half will be men, half women
			if ((num & 1) == 0)
			{   // Male names
				FN = GenMFN(); MN = GenMMN(); LN = GenMLN();
			}
			else
			{   // Female names
				FN = GenFFN(); MN = GenFMN(); LN = GenFLN();
			}

			// Генерируем контейнер для передачи данных в бекэнд
			IClientDTO client =
				new ClientDTO(ClientType.VIP, FN, MN, LN,
								GenBirthDate(), GenPassportNum(), GenTel(), GenEmail(),
								"Тропики, Лазурный берег, Жемчужный дворец, комната 8");
			// Присваивание client необходимо, т.к. AddClient генерирует уникальный ID
			client = BA.Clients.AddClient(client);
			GenerateAccountsForClient(client);
		}

		private static void GenerateSIMclientsAndAccountsAsync(int num)
		{
			string FN, MN, LN;

			// Half will be men, half women
			if ((num & 1) == 0)
			{   // Male names
				FN = GenMFN(); MN = GenMMN(); LN = GenMLN();
			}
			else
			{   // Female names
				FN = GenFFN(); MN = GenFMN(); LN = GenFLN();
			}

			// Генерируем контейнер для передачи данных в бекэнд
			IClientDTO client =
				new ClientDTO(ClientType.Simple, FN, MN, LN,
								GenBirthDate(), GenPassportNum(), GenTel(), GenEmail(),
								"Мой адрес не дом и не улица. Там жыл Вася.");
			// Присваивание client необходимо, т.к. AddClient генерирует уникальный ID
			client = BA.Clients.AddClient(client);
			GenerateAccountsForClient(client);
		}

		private static void GenerateORGclientsAndAccountsAsync(int num)
		{
			string DFN, DMN, DLN;
			int regcode;

			// Half will be men, half women
			if ((num & 1) == 0)
			{   // Male names
				DFN = GenMFN(); DMN = GenMMN(); DLN = GenMLN();
			}
			else
			{   // Female names
				DFN = GenFFN(); DMN = GenFMN(); DLN = GenFLN();
			}

			// Генерируем контейнер для передачи данных в бекэнд
			IClientDTO client =
				new ClientDTO(ClientType.Organization, GenOrgName(), DFN, DMN, DLN,
								GenRegDate(), GenTIN(out regcode),
								GenTel(), GenEmail(), GenOrgAddress(regcode));
			// Присваивание client необходимо, т.к. AddClient генерирует уникальный ID
			client = BA.Clients.AddClient(client);
			GenerateAccountsForClient(client);
		}

		#endregion


		#region Создание клиентов

		private static void GenerateVIPclientsAndAccounts(int num)
		{
			string FN, MN, LN;
			for (int i = 0; i < num; i++)
			{
				// Half will be men, half women
				if ((i & 1) == 0)
				{   // Male names
					FN = GenMFN(); MN = GenMMN(); LN = GenMLN();
				}
				else
				{   // Female names
					FN = GenFFN(); MN = GenFMN(); LN = GenFLN();
				}

				// Генерируем контейнер для передачи данных в бекэнд
				IClientDTO client =
					new ClientDTO(	ClientType.VIP, FN, MN, LN,
									GenBirthDate(), GenPassportNum(), GenTel(), GenEmail(),
									"Тропики, Лазурный берег, Жемчужный дворец, комната 8");
				// Присваивание client необходимо, т.к. AddClient генерирует уникальный ID
				client = BA.Clients.AddClient(client);
				GenerateAccountsForClient(client);
			}
		}

		private static void GenerateSIMclientsAndAccounts(int num)
		{
			string FN, MN, LN;
			for (int i = 0; i < num; i++)
			{
				// Half will be men, half women
				if ((i & 1) == 0)
				{   // Male names
					FN = GenMFN(); MN = GenMMN(); LN = GenMLN();
				}
				else
				{   // Female names
					FN = GenFFN(); MN = GenFMN(); LN  = GenFLN();
				}

				// Генерируем контейнер для передачи данных в бекэнд
				IClientDTO client = 
					new ClientDTO(	ClientType.Simple, FN, MN, LN,
									GenBirthDate(), GenPassportNum(), GenTel(), GenEmail(),
									"Мой адрес не дом и не улица. Там жыл Вася.");
				// Присваивание client необходимо, т.к. AddClient генерирует уникальный ID
				client = BA.Clients.AddClient(client);
				GenerateAccountsForClient(client);
			}
		}

		private static void GenerateORGclientsAndAccounts(int num)
		{
			string DFN, DMN, DLN;
			int regcode;
			for (int i = 0; i < num; i++)
			{
				// Half will be men, half women
				if ((i & 1) == 0)
				{   // Male names
					DFN = GenMFN(); DMN = GenMMN(); DLN = GenMLN();
				}
				else
				{   // Female names
					DFN = GenFFN(); DMN = GenFMN(); DLN = GenFLN();
				}

				// Генерируем контейнер для передачи данных в бекэнд
				IClientDTO client =
					new ClientDTO(	ClientType.Organization, GenOrgName(), DFN, DMN, DLN,
									GenRegDate(), GenTIN(out regcode), 
									GenTel(), GenEmail(), GenOrgAddress(regcode));
				// Присваивание client необходимо, т.к. AddClient генерирует уникальный ID
				client = BA.Clients.AddClient(client);
				GenerateAccountsForClient(client);
			}
		}

		#endregion

		#region Генерация счетов

		private static void GenerateAccountsForClient(IClientDTO client)
		{
			GenerateCurrentAccounts(client, r.Next(0, 6));
			GenerateDeposits(client, r.Next(0, 6));
			GenerateCredits(client, r.Next(0, 6));
		}

		private static void GenerateCurrentAccounts(IClientDTO c, int num) 
		{
			for (int i = 0; i < num; i++)
				BA.Accounts.GenerateAccount(
					 new AccountDTO(c.ClientType, c.ID, AccountType.Current,
									r.Next(0,100) * 1000, 					// сумма на текущем счеты
									0,										// процент по вкладу
					false, 0, "не используется",
					GenAccOpeningDate(c), true, true, RecalcPeriod.NoRecalc, 0, 0));
		}

		private static void GenerateDeposits(IClientDTO c, int num) 
		{
			for (int i = 0; i < num; i++)
			{
				DateTime openingDate = GenAccOpeningDate(c);
				int		 monthsElapsed;
				int		 duration	 = GenDepositCreditDuration(openingDate, out monthsElapsed);

				double interest = 0;
				switch(c.ClientType)
				{
					case ClientType.VIP:
						interest = ((double)r.Next(11, 21)) / 100;
						break;
					case ClientType.Simple:
						interest = ((double)r.Next(5, 11)) / 100;
						break;
					case ClientType.Organization:
						interest = ((double)r.Next(7, 16)) / 100;
						break;
				}

				BA.Accounts.GenerateAccount(
					 new AccountDTO(c.ClientType, c.ID, AccountType.Deposit,
									r.Next(100, 300) * 10000,		// сумма на счету. У ВИП > 1 mln
									interest,	// процент
									TrueFalse(),					// капитализация
									0,                              // У нас ещё нет счета для перечисления ...
									"внутренний счет",
									openingDate,
									TrueFalse(),					// Можем поплнять или нет
									TrueFalse(),					// Можем частично снимать или нет
									GenDepositRecalc(),				// Периодичность пересчета
									duration,						// Количество месяцев вклада
									monthsElapsed));                // Сколько месяцев уже прошло до настоящего момента 
			}
		}

		private static void GenerateCredits(IClientDTO c, int num)
		{
			for (int i = 0; i < num; i++)
			{
				DateTime openingDate = GenAccOpeningDate(c);
				int monthsElapsed;
				int duration = GenDepositCreditDuration(openingDate, out monthsElapsed);
				int amount = duration * 10_000 * r.Next(1,4);

				double interest = 0;
				switch (c.ClientType)
				{
					case ClientType.VIP:
						interest = ((double)r.Next(7, 13)) / 100;
						break;
					case ClientType.Simple:
						interest = ((double)r.Next(12, 21)) / 100;
						break;
					case ClientType.Organization:
						interest = ((double)r.Next(15, 26)) / 100;
						break;
				}

				BA.Accounts.GenerateAccount(
					 new AccountDTO(c.ClientType, c.ID, AccountType.Credit,
									-amount,				// долг
									interest,				// процент
									true,					// капитализация
									0,                      // на собственный
									"не используется",
									openingDate,
									true,					// Можем поплнять или нет
									false,					// Можем частично снимать или нет
									RecalcPeriod.Monthly,	// Периодичность пересчета
									duration,				// Количество месяцев вклада
									monthsElapsed));		// Сколько месяцев уже прошло до настоящего момента
			}
		}

		private static bool TrueFalse()
		{
			return r.Next(0, 2) == 0 ? false : true;
		}

		/// <summary>
		/// Сгенерировать дату открытия счета. 
		/// Она должна быть после рождения клиента и даты создания банка
		/// </summary>
		/// <returns></returns>
		private static DateTime GenAccOpeningDate(IClientDTO c)
		{
			if ((DateTime)c.CreationDate < GoodBankTime.BankFoundationDay)
				return GenDate(GoodBankTime.BankFoundationDay, GoodBankTime.Today);
			return GenDate((DateTime)c.CreationDate, GoodBankTime.Today);
		}

		private static int GenDepositCreditDuration(DateTime sd, out int monthsElapsed)
		{
			monthsElapsed = (int)(GoodBankTime.Today.Subtract(sd).TotalDays / 365.25 * 12);
			return monthsElapsed + r.Next(1, 61);
		}

		#endregion

		#region Генерация полей имени клиента

		private static string GenMFN()
		{
			return Names.MFN[r.Next(0, Names.MFN.Length)];
		}

		private static string GenMMN()
		{
			return Names.MMN[r.Next(0, Names.MMN.Length)];
		}

		private static string GenMLN()
		{
			return Names.MLN[r.Next(0, Names.MLN.Length)];
		}

		private static string GenFFN()
		{
			return Names.FFN[r.Next(0, Names.FFN.Length)];
		}

		private static string GenFMN()
		{
			return Names.FMN[r.Next(0, Names.FMN.Length)];
		}

		private static string GenFLN()
		{
			return Names.FLN[r.Next(0, Names.FLN.Length)];
		}

		private static string GenOrgName()
		{
			return $"Организация {orgCount++}";
		}

		private static string GenPassportNum()
		{
			return $"{r.Next(1, 10_000):0000} {r.Next(1,1_000_000):000000}"; 
		}

		private static string GenTIN(out int region)
		{
			region = r.Next(1, 86);
			return $"{region:00}{r.Next(1,100):00}{r.Next(1,1_000_000):000000}";
		}
		private static string GenTel()
		{
			return "+7 9" + $"{r.Next(0, 100):00} "                        // area code
					+ $"{r.Next(1, 1000):000}-" + $"{r.Next(0, 10000):0000}";	// telephone
		}

		private static DateTime GenBirthDate()
		{
			DateTime startDate =
				new DateTime(GoodBankTime.Today.Year - 100,
							 GoodBankTime.Today.Month,
							 GoodBankTime.Today.Day);
			DateTime endDate =
				new DateTime(GoodBankTime.Today.Year - 19,
							 GoodBankTime.Today.Month,
							 GoodBankTime.Today.Day);
			return GenDate(startDate, endDate);
		}

		private static DateTime GenRegDate()
		{
			DateTime startDate = new DateTime(1990, 1, 1);
			DateTime endDate   = GoodBankTime.Today;
			return GenDate(startDate, endDate);
		}


		private static DateTime GenDate(DateTime sd, DateTime ed)
		{
			if (sd >= ed) return ed;
			DateTime ndate;
			int ny = r.Next(sd.Year, ed.Year + 1);
			int nm = r.Next(1, 13);
			int nd = 1;
			if (nm == 2) nd = r.Next(1, 29);
			else if (nm == 4 || nm == 6 || nm == 9 || nm == 11)
				nd = r.Next(1, 31);
			else nd = r.Next(1, 32);
			ndate = new DateTime(ny, nm, nd);
			if (ndate < sd) ndate = sd;
			if (ndate > ed) ndate = ed;
			return ndate;
		}

		private static string GenEmail()
		{
			return "email" + Name() + "@" + Domain[r.Next(0, Domain.Length)] + TopDomain[r.Next(0, TopDomain.Length)];
		}

		public static string[] Domain = { "gmail", "yahoo", "outlook", "mail", "mymail", "yourmail", "onemail", "bmail", "cmail", "paper"};
		public static string[] TopDomain = { ".com", ".org", ".edu", ".gov", ".ru", ".net", ".xyz" };

		public static string Name()
		{
			return Guid.NewGuid().ToString().Substring(0, 5);
		}

		private static string GenOrgAddress(int region)
		{
			return $"Город_{region}, ул. Улица_{r.Next(0, 100)}, {r.Next(0, 100)} офс. {r.Next(0, 1000)}";
		}

		private static RecalcPeriod GenDepositRecalc()
		{
			int toss = r.Next(1, 8);
			if (1 <= toss && toss <= 4) return RecalcPeriod.Monthly;
			if (5 <= toss && toss <= 6) return RecalcPeriod.Annually;
			return RecalcPeriod.AtTheEnd;
		}

		#endregion
	}
}
