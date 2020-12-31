using Enumerables;
using Interfaces_Data;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DTO
{
	/// <summary>
	/// Data Transfer Object для передачи данных при работе с клиентом 
	/// При показе или вводе данных о клиенте
	/// Ручной ввод осуществляется только в свойства с { get; set; }
	/// </summary>
	public class ClientDTO : IClientDTO, INotifyPropertyChanged
	{
		#region Свойства

		public int			ID						{ get; }
		public ClientType	ClientType				{ get; set; }
		public string		ClientTypeTag			
		{
			get
			{
				string tmp = "";
				switch(ClientType)
				{
					case ClientType.VIP:
						tmp = "ВИП";
						break;
					case ClientType.Simple:
						tmp = "Физик";
						break;
					case ClientType.Organization:
						tmp = "Юрик";
						break;
				}
				return tmp;
			}
		}
		public string		FirstName
		{ 
			get => _firstName; 
			set
			{
				_firstName = value;
				NotifyPropertyChanged();
				NotifyMainNameOrDirName();
			}
		}

		public string		MiddleName
		{
			get => _middleName;
			set
			{
				_middleName = value;
				NotifyPropertyChanged();
				NotifyMainNameOrDirName();
			}
		}

		public string		LastName
		{
			get => _lastName;
			set
			{
				_lastName = value;
				NotifyPropertyChanged();
				NotifyMainNameOrDirName();
			}
		}

		/// <summary>
		/// Содержит либо полноые ФИО, либо название организации
		/// в зависимости от типа клиента
		/// </summary>
		public string		MainName				
		{ 
			get
			{
				if (ClientType == ClientType.Organization) return _orgName;
				// Это надо для показа клиента
				return	LastName + " " + FirstName +
						(String.IsNullOrEmpty(MiddleName) ? "" : " ") +
						MiddleName;

			}
			set { _orgName = value; }
		}

		/// <summary>
		/// Поле для показа в списке
		/// </summary>
		public string		DirectorName			
		{ 
			get
			{
				if (ClientType != ClientType.Organization) return "";
				return	_lastName + " " + _firstName +
						(String.IsNullOrEmpty(_middleName) ? "" : " ") +
						_middleName;
			}
		}

		public DateTime?	CreationDate
		{ 
			get => _creationDate; 
			set
			{
				_creationDate = value;
				NotifyPropertyChanged();
			}
		}

		public string		PassportOrTIN
		{ 
			get => _passportOrTIN; 
			set
			{
				_passportOrTIN = value;
				NotifyPropertyChanged();
			}
		}

		public string		Telephone
		{ 
			get => _telephone; 
			set
			{
				_telephone = value;
				NotifyPropertyChanged();
			}
		}

		public string		Email
		{ 
			get => _email; 
			set
			{
				_email = value;
				NotifyPropertyChanged();
			}
		}
		public string		Address
		{ 
			get => _address; 
			set
			{
				_address = value;
				NotifyPropertyChanged();
			}
		}

		public int			NumberOfSavingAccounts { get; } = 0;
		public int			NumberOfDeposits		{ get; } = 0;
		public int			NumberOfCredits			{ get; } = 0;
		public int			NumberOfClosedAccounts	{ get; } = 0;

		#endregion

		#region Поля

		private string		_firstName;
		private string		_middleName;
		private string		_lastName;
		private string		_orgName;
		private DateTime?	_creationDate = null;
		private string		_passportOrTIN;
		private string		_telephone;
		private string		_email;
		private string		_address;

		#endregion

		#region Конструкторы

		public ClientDTO() {}

		/// <summary>
		/// Конструктор для генерации ВИП или физика. 
		/// Подразумевает, что входные данные верны
		/// </summary>
		public ClientDTO(ClientType ct, string fn, string mn, string ln,
						 DateTime bd, string pNum,
						 string tel, string email, string address)
		{
			ClientType		= ct;
			_firstName		= fn;
			_middleName		= mn;
			_lastName		= ln;
			_creationDate	= bd;
			_passportOrTIN	= pNum;
			_telephone		= tel;
			_email			= email;
			_address		= address;
		}

		/// <summary>
		/// Конструктор для генерации Организации. 
		/// Подразумевает, что входные данные верны
		/// </summary>
		public ClientDTO(ClientType ct, string orgN, string dfn, string dmn, string dln,
						 DateTime rd, string tin,
						 string tel, string email, string address)
		{
			ClientType		= ct;
			_orgName		= orgN;
			_firstName		= dfn;
			_middleName		= dmn;
			_lastName		= dln;
			_creationDate	= rd;
			_passportOrTIN	= tin;
			_telephone		= tel;
			_email			= email;
			_address		= address;
		}

		/// <summary>
		/// Конструктор для выборки из базы и показа в списке
		/// Подразумевается, что все данные введены корректно
		/// </summary>
		/// <param name="c">Клиент из базы</param>
		public ClientDTO(IClient c)
		{
			ID						= c.ID;
			_telephone				= c.Telephone;
			_email					= c.Email;
			_address				= c.Address;
			NumberOfSavingAccounts = c.NumberOfSavingAccounts;
			NumberOfDeposits		= c.NumberOfDeposits;
			NumberOfCredits			= c.NumberOfCredits;
			NumberOfClosedAccounts  = c.NumberOfClosedAccounts;

			if (c is IClientVIP)
			{
				ClientType		= ClientType.VIP;
				_firstName		= (c as IClientVIP).FirstName;
				_middleName		= (c as IClientVIP).MiddleName;
				_lastName		= (c as IClientVIP).LastName;
				_passportOrTIN	= (c as IClientVIP).PassportNumber;
				_creationDate	= (c as IClientVIP).BirthDate;
				return;
			}

			if (c is IClientSimple)
			{
				ClientType		= ClientType.Simple;
				_firstName		= (c as IClientSimple).FirstName;
				_middleName		= (c as IClientSimple).MiddleName;
				_lastName		= (c as IClientSimple).LastName;
				_passportOrTIN	= (c as IClientSimple).PassportNumber;
				_creationDate	= (c as IClientSimple).BirthDate;
			}

			if (c is IClientOrg)
			{
				ClientType		= ClientType.Organization;
				_orgName		= (c as IClientOrg).OrgName;
				_firstName		= (c as IClientOrg).DirectorFirstName;
				_middleName		= (c as IClientOrg).DirectorMiddleName;
				_lastName		= (c as IClientOrg).DirectorLastName;
				_passportOrTIN	= (c as IClientOrg).TIN;
				_creationDate	= (c as IClientOrg).RegistrationDate;
			}
		}

		/// <summary>
		/// Конструктор для клонирования объекта
		/// </summary>
		/// <param name="c"></param>
		public ClientDTO(ClientDTO c)
		{
			ID						= c.ID;
			ClientType				= c.ClientType;
			_firstName				= c._firstName;
			_middleName				= c._middleName;
			_lastName				= c._lastName;
			_orgName				= c._orgName;
			_creationDate			= c._creationDate;
			_passportOrTIN			= c._passportOrTIN;
			_telephone				= c._telephone;
			_email					= c._email;
			_address				= c._address;
			NumberOfSavingAccounts	= c.NumberOfSavingAccounts;
			NumberOfDeposits		= c.NumberOfDeposits;
			NumberOfCredits			= c.NumberOfCredits;
			NumberOfClosedAccounts	= c.NumberOfClosedAccounts;
		}
		#endregion

		#region Обработчики изменения свойств

		public event PropertyChangedEventHandler PropertyChanged;

		// This method is called by the Set accessor of each property.  
		// The CallerMemberName attribute that is applied to the optional propertyName  
		// parameter causes the property name of the caller to be substituted as an argument.  
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void NotifyMainNameOrDirName()
		{
			if (ClientType == ClientType.Organization)
				NotifyPropertyChanged("DirectorName");
			else
				NotifyPropertyChanged("MainName");
		}

		#endregion

		public ClientDTO Clone()
		{
			return new ClientDTO(this);
		}

		/// <summary>
		/// Метод для обновления экземпляра, чтобы сработал вызов NotifyPropertyChanged
		/// и обновился вывод в нужных местах.
		/// </summary>
		/// <param name="c"></param>
		public void UpdateMyself(ClientDTO c)
		{
			if (ClientType == ClientType.Organization)
				MainName	= c.MainName;
			FirstName		= c.FirstName;
			MiddleName		= c.MiddleName;
			LastName		= c.LastName;
			CreationDate	= c.CreationDate;
			PassportOrTIN	= c.PassportOrTIN;
			Telephone		= c.Telephone;
			Email			= c.Email;
			Address			= c.Address;
		}
	}
}
