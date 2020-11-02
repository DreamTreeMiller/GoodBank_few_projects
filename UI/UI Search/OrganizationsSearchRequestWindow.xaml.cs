using Interfaces_Data;
using Search_Engine;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace UI_Search
{
	/// <summary>
	/// Interaction logic for OrganizationsSearchRequestWindow1.xaml
	/// </summary>
	public partial class OrganizationsSearchRequestWindow : Window, INotifyPropertyChanged
	{
		public Compare CheckAllFields;

		#region Organization Name

		private string orgName;
		public string OrgName
		{
			get => orgName;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckOrgName = null;
				orgName = value;
				SetCheckOrgName(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckOrgName;

		private void SetCheckOrgName(string value)
		{
			OrgNameComparator ONC = new OrgNameComparator(value);
			CheckOrgName = ONC.Compare;
		}

		#endregion

		#region Director First Name

		private string directorFirstName;
		public string DirectorFirstName
		{
			get => directorFirstName;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckDirectorFirstName = null;
				directorFirstName = value;
				SetCheckDirectorFirstName(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckDirectorFirstName;

		private void SetCheckDirectorFirstName(string value)
		{
			DirectorFirstNameComparator DFNC = new DirectorFirstNameComparator(value);
			CheckDirectorFirstName = DFNC.Compare;
		}

		#endregion

		#region Director Middle Name

		private string directorMiddleName;
		public string DirectorMiddleName
		{
			get => directorMiddleName;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckDirectorMiddleName = null;
				directorMiddleName = value;
				SetCheckDirectorMiddleName(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckDirectorMiddleName;

		private void SetCheckDirectorMiddleName(string value)
		{
			DirectorMiddleNameComparator DMNC = new DirectorMiddleNameComparator(value);
			CheckDirectorMiddleName = DMNC.Compare;
		}

		#endregion

		#region Director Last Name

		private string directorLastName;
		public string DirectorLastName
		{
			get => directorLastName;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckDirectorLastName = null;
				directorLastName = value;
				SetCheckDirectorLastName(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckDirectorLastName;

		private void SetCheckDirectorLastName(string value)
		{
			DirectorLastNameComparator DLNC = new DirectorLastNameComparator(value);
			CheckDirectorLastName = DLNC.Compare;
		}

		#endregion

		#region Registration Start Date

		private DateTime? registrationStartDate = null;
		public DateTime? RegistrationStartDate
		{
			get => registrationStartDate;
			set
			{
				if (registrationEndDate < value) value = registrationEndDate;
				registrationStartDate = value;
				SetCheckRegistrationStartDate((DateTime)value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckRegistrationStartDate = null;

		private void SetCheckRegistrationStartDate(DateTime value)
		{
			RegistrationStartDateComparator SDC = new RegistrationStartDateComparator(value);
			CheckRegistrationStartDate = SDC.Compare;
		}

		#endregion

		#region Registration End Date

		private DateTime? registrationEndDate = null;
		public DateTime? RegistrationEndDate
		{
			get => registrationEndDate;
			set
			{
				if (value < registrationStartDate) value = registrationStartDate;
				registrationEndDate = value;
				SetCheckRegistrationEndDate((DateTime)value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckRegistrationEndDate = null;

		private void SetCheckRegistrationEndDate(DateTime value)
		{
			RegistrationEndDateComparator EDC = new RegistrationEndDateComparator(value);
			CheckRegistrationEndDate = EDC.Compare;
		}

		#endregion

		#region TIN

		private string _TIN;
		public string TIN
		{
			get => _TIN;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckTIN = null;
				_TIN = value;
				SetCheckTIN(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckTIN;

		private void SetCheckTIN(string value)
		{
			TINComparator PNC = new TINComparator(value);
			CheckTIN = PNC.Compare;
		}

		#endregion

		#region Telephone

		private string telephone;
		public string Telephone
		{
			get => telephone;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckTelephone = null;
				telephone = value;
				SetCheckTelephone(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckTelephone;

		private void SetCheckTelephone(string value)
		{
			TelephoneComparator TC = new TelephoneComparator(value);
			CheckTelephone = TC.Compare;
		}

		#endregion

		#region Email

		private string email;
		public string Email
		{
			get => email;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckEmail = null;
				email = value;
				SetCheckEmail(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckEmail;

		private void SetCheckEmail(string value)
		{
			EmailComparator EC = new EmailComparator(value);
			CheckEmail = EC.Compare;
		}

		#endregion

		#region Address

		private string address;
		public string Address
		{
			get => address;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckAddress = null;
				address = value;
				SetCheckAddress(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckAddress;

		private void SetCheckAddress(string value)
		{
			AddressComparator AC = new AddressComparator(value);
			CheckAddress = AC.Compare;
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public OrganizationsSearchRequestWindow()
		{
			InitializeComponent();
			DataContext = this;
		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			CheckAllFields  = CheckIfOrganization;
			CheckAllFields += CheckOrgName;
			CheckAllFields += CheckDirectorFirstName;
			CheckAllFields += CheckDirectorMiddleName;
			CheckAllFields += CheckDirectorLastName;
			CheckAllFields += CheckRegistrationStartDate;
			CheckAllFields += CheckRegistrationEndDate;
			CheckAllFields += CheckTIN;
			CheckAllFields += CheckTelephone;
			CheckAllFields += CheckEmail;
			CheckAllFields += CheckAddress;

			DialogResult = true;
		}

		private bool CheckIfOrganization(IClient p, ref bool flag)
		{
			if (p is IClientOrg) return true;
			flag = false;
			return false;
		}
	}
}
