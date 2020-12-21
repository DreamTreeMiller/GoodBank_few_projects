using Enumerables;
using System.Windows;

namespace Window_Name_Tags
{
	/// <summary>
	/// Номер окошка. В зависимости от номера, разное наполнение
	/// </summary>
	public enum WindowID
	{
		DepartmentVIP,
		DepartmentSIM,
		DepartmentORG,
		DepartmentALL,
		AddClientVIP,
		AddClientSIM,
		AddClientORG,
		AddClientALL,
		EditClientVIP,
		EditClientSIM,
		EditClientORG,
		AccountCurrent,
		AccountDeposit,
		AccountCredit,
		SearchResultPersons,
		SearchResultOrganizations,
		SearchResultAccounts
	}

	public class WindowNameTags
	{
		public string SystemWindowTitle;
		public string WindowHeader;
		public string SelectClientTag;
		public string AddClientTag;

		public WindowNameTags(WindowID wid)
		{
			switch (wid)
			{
				case WindowID.DepartmentVIP:
					SystemWindowTitle	= "Очень важные персоны";
					WindowHeader		= "ОЧЕНЬ ВАЖНЫЕ ПЕРСОНЫ";
					SelectClientTag		= "Показать персону";
					AddClientTag		= "Добавить персону";
					break;
				case WindowID.DepartmentSIM:
					SystemWindowTitle	= "Физики";
					WindowHeader		= "ФИЗИКИ";
					SelectClientTag		= "Показать физика";
					AddClientTag		= "Добавить физика";
					break;
				case WindowID.DepartmentORG:
					SystemWindowTitle	= "Юрики";
					WindowHeader		= "ЮРИКИ";
					SelectClientTag		= "Показать юрика";
					AddClientTag		= "Добавить юрика";
					break;
				case WindowID.DepartmentALL:
					SystemWindowTitle	= "Управляющий банком";
					WindowHeader		= "ВСЕ, ВСЕ, ВСЕ";
					SelectClientTag		= "Показать клиента";
					AddClientTag		= "Добавить клиента";
					break;
			}
		}
	}

	/// <summary>
	/// Структура для передачи текста полей списка клиентов в зависимости от окна и типа клиентов
	/// </summary>
	public class ClientsViewNameTags
	{
		public string CreationDateCBTag;        // Дата рождения или дата регистрации
		public string PassportOrTIN_CB_Tag;     // Номер паспорта или ИНН
		public Visibility ShowDirectorCB;       // Показывать checkbox директор
		public Visibility ShowClientTypeColumn; // Показывать колонку тип - физик или юрик
		public string MainNameTag;              // ФИО или Название организации
		public string TotalNameTag;             // Сводка: ВИП клиентов, физиков, юриков

		public ClientsViewNameTags(WindowID wid)
		{
			switch (wid)
			{
				case WindowID.DepartmentVIP:
					CreationDateCBTag	 = "Дата рождения";
					PassportOrTIN_CB_Tag = "Паспорт";
					ShowClientTypeColumn = Visibility.Collapsed;
					ShowDirectorCB		 = Visibility.Collapsed;
					MainNameTag			 = "ФИО";
					TotalNameTag		 = "ВИП клиентов:";
					break;
				case WindowID.DepartmentSIM:
					CreationDateCBTag	 = "Дата рождения";
					PassportOrTIN_CB_Tag = "Паспорт";
					ShowClientTypeColumn = Visibility.Collapsed;
					ShowDirectorCB		 = Visibility.Collapsed;
					MainNameTag			 = "ФИО";
					TotalNameTag		 = "физиков:";
					break;
				case WindowID.DepartmentORG:
					CreationDateCBTag	 = "Дата регистрации";
					PassportOrTIN_CB_Tag = "ИНН";
					ShowClientTypeColumn = Visibility.Collapsed;
					ShowDirectorCB		 = Visibility.Visible;
					MainNameTag			 = "Название";
					TotalNameTag		 = "юриков:";
					break;
				case WindowID.DepartmentALL:
					CreationDateCBTag	 = "Дата рожд./рег.";
					PassportOrTIN_CB_Tag = "Паспорт/ИНН";
					ShowClientTypeColumn = Visibility.Visible;
					ShowDirectorCB		 = Visibility.Visible;
					MainNameTag			 = "ФИО / Название";
					TotalNameTag		 = "клиентов:";
					break;
				case WindowID.SearchResultPersons:
					CreationDateCBTag	 = "Дата рождения";
					PassportOrTIN_CB_Tag = "Паспорт";
					ShowClientTypeColumn = Visibility.Visible;
					ShowDirectorCB		 = Visibility.Collapsed;
					MainNameTag			 = "ФИО";
					TotalNameTag		 = "клиентов:";
					break;
				case WindowID.SearchResultOrganizations:
					CreationDateCBTag	 = "Дата регистрации";
					PassportOrTIN_CB_Tag = "ИНН";
					ShowClientTypeColumn = Visibility.Visible;
					ShowDirectorCB		 = Visibility.Visible;
					MainNameTag			 = "Название";
					TotalNameTag		 = "юриков:";
					break;
			}
		}
	}

	public class AddEditClientNameTags
	{
		public WindowID WID;
		public ClientType ClientType;
		public string SystemWindowTitle;
		public string WindowHeader;
		public Visibility ClientTypeComboBox = Visibility.Collapsed;
		public Visibility OrganizationVisibility = Visibility.Collapsed;
		public Visibility PersonVisibility = Visibility.Visible;

		public AddEditClientNameTags(WindowID wid)
		{
			WID = wid;
			switch(wid)
			{
				case WindowID.AddClientVIP:
					SystemWindowTitle		= "Добавить ВИП";
					WindowHeader			= "ВНЕСТИ ДАННЫЕ ОЧЕНЬ ВАЖНОЙ ПЕРСОНЫ";
					ClientType				= ClientType.VIP;
					break;
				case WindowID.AddClientSIM:
					SystemWindowTitle		= "Добавить физика";
					WindowHeader			= "ВНЕСТИ ДАННЫЕ ФИЗИКА";
					ClientType				= ClientType.Simple;
					break;
				case WindowID.AddClientORG:
					SystemWindowTitle		= "Добавить юрика";
					WindowHeader			= "ВНЕСТИ ДАННЫЕ ЮРИКА";
					OrganizationVisibility	= Visibility.Visible;
					PersonVisibility		= Visibility.Collapsed;
					ClientType				= ClientType.Organization;
					break;
				case WindowID.AddClientALL:
					SystemWindowTitle		= "Добавить клиента";
					WindowHeader			= "ВНЕСТИ ДАННЫЕ КЛИЕНТА";
					OrganizationVisibility	= Visibility.Collapsed;
					PersonVisibility		= Visibility.Visible;
					ClientTypeComboBox		= Visibility.Visible;
					ClientType				= ClientType.Simple;
					break;
				case WindowID.EditClientVIP:
					SystemWindowTitle		= "Изменить данные ВИП";
					WindowHeader			= "ИЗМЕНИТЬ ДАННЫЕ ОЧЕНЬ ВАЖНОЙ ПЕРСОНЫ";
					ClientType				= ClientType.VIP;
					break;
				case WindowID.EditClientSIM:
					SystemWindowTitle		= "Изменить данные физика";
					WindowHeader			= "ИЗМЕНИТЬ ДАННЫЕ ФИЗИКА";
					ClientType				= ClientType.Simple;
					break;
				case WindowID.EditClientORG:
					SystemWindowTitle		= "Изменить данные юрика";
					WindowHeader			= "ИЗМЕНИТЬ ДАННЫЕ ЮРИКА";
					OrganizationVisibility	= Visibility.Visible;
					PersonVisibility		= Visibility.Collapsed;
					ClientType				= ClientType.Organization;
					break;
			}
		}
	}

	/// <summary>
	/// Создает надписи для окна Карточка счета в зависимости от типа счета
	/// </summary>
	public class AccountWindowNameTags
	{
		public string SystemWindowTitle;        // Системный заголовок окна
		public string WindowHeader;				// Заголовок по центру окна
		public Visibility WithdrawCashButtonVisibility	= Visibility.Visible;       
		public Visibility WireButtonVisibility			= Visibility.Visible;
		public Visibility DepositPartVisibility			= Visibility.Collapsed;

		public AccountWindowNameTags(AccountType accType)
		{
			switch (accType)
			{
				case AccountType.Saving:
					SystemWindowTitle = "Карточка счета * Текущий";
					WindowHeader	  = "ДАННЫЕ ТЕКУЩЕГО СЧЕТА";

					break;
				case AccountType.Deposit:
					SystemWindowTitle = "Карточка счета * Вклад";
					WindowHeader	  = "ДАННЫЕ ВКЛАДА";
					DepositPartVisibility = Visibility.Visible;
					break;
				case AccountType.Credit:
					SystemWindowTitle = "Карточка счета * Кредит";
					WindowHeader	  = "ДАННЫЕ КРЕДИТНОГО СЧЕТА";
					WithdrawCashButtonVisibility = Visibility.Collapsed;
					WireButtonVisibility		 = Visibility.Collapsed;
					break;
			}
		}
	}

}
