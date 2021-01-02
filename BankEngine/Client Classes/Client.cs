using Interfaces_Data;
using System.Data;
using BankInside;

namespace ClientClasses
{
	public abstract class Client : IClient
	{

		#region Статическая часть для генерации уникального ID

		/// <summary>
		/// Текущий ID счета
		/// </summary>
		private static int staticID;

		/// <summary>
		/// Статический конструктор. Обнуляет счетчик ID
		/// </summary>
		static Client()
		{
			staticID = 0;
		}

		/// <summary>
		/// Герерирует следующий ID
		/// </summary>
		/// <returns>New unique ID</returns>
		private static int NextID()
		{
			staticID++;
			return staticID;
		}

		#endregion

		#region Свойства одинаковые для всех клиентов

		/// <summary>
		/// ID клиента в базе
		/// </summary>
		public int		ID						{ get; }

		public string	Telephone				{ get; set; }
		public string	Email					{ get; set; }
		public string	Address					{ get; set; }

		public int		NumberOfSavingAccounts	{ get; set; }
		public int		NumberOfDeposits		{ get; set; }
		public int		NumberOfCredits			{ get; set; }
		public int		NumberOfClosedAccounts	{ get; set; }

		#endregion

		#region Конструктор нового клиента

		/// <summary>
		/// Базовый конструктор для любого клиента. Обнуляет количество всех счетов
		/// </summary>
		/// <param name="tel">Телефон</param>
		/// <param name="email">Электронная почта</param>
		/// <param name="address">Адрес</param>
		public Client(string tel, string email, string address)
		{
			ID						= NextID();
			Telephone				= tel;
			Email					= email;
			Address					= address;
			NumberOfSavingAccounts	= 0;
			NumberOfDeposits		= 0;
			NumberOfCredits			= 0;
			NumberOfClosedAccounts	= 0;

			// Creating DB entry
			DataRow[] newRow = new DataRow[1];
			newRow[0] = GoodBank.ds.Tables["Clients"].NewRow();

			newRow[0]["Telephone"]	= tel;
			newRow[0]["Email"]		= email;
			newRow[0]["Address"]	= address;

			GoodBank.ds.Tables["Clients"].Rows.Add(newRow[0]);
			GoodBank.daClients.Update(newRow);

			ID = (int)newRow[0]["ID"];

		}

		#endregion

		public abstract void UpdateMyself(IClientDTO updatedClient);
	}
}
