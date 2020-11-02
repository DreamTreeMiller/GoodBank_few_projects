using Search_Engine;
using Interfaces_Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UI_Search
{
	/// <summary>
	/// Interaction logic for EnterSearchRequestForIndividualWindow.xaml
	/// </summary>
	public partial class IndividualsSearchRequestWindow : Window, INotifyPropertyChanged
	{
		public Compare CheckAllFields;

		#region First Name

		private string firstName;
		public string FirstName 
		{ 
			get => firstName; 
			set 
			{
				if (String.IsNullOrEmpty(value)) CheckFirstName = null;
				firstName = value;
				SetCheckFirstName(value);
				NotifyPropertyChanged();
			} 
		}

		Compare CheckFirstName;

		private void SetCheckFirstName(string value)
		{
			FirstNameComparator FNC = new FirstNameComparator(value);
			CheckFirstName = FNC.Compare;
		}

		#endregion

		#region Middle Name

		private string middleName;
		public string MiddleName
		{
			get => middleName;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckMiddleName = null;
				middleName = value;
				SetCheckMiddleName(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckMiddleName;

		private void SetCheckMiddleName(string value)
		{
			MiddleNameComparator MNC = new MiddleNameComparator(value);
			CheckMiddleName = MNC.Compare;
		}

		#endregion

		#region Last Name

		private string lastName;
		public string LastName
		{
			get => lastName;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckLastName = null;
				lastName = value;
				SetCheckLastName(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckLastName;

		private void SetCheckLastName(string value)
		{
			LastNameComparator LNC = new LastNameComparator(value);
			CheckLastName = LNC.Compare;
		}

		#endregion

		#region Start Date

		private DateTime? startDate = null;
		public DateTime? StartDate
		{
			get => startDate;
			set
			{
				if (endDate < value) value = endDate;
				startDate = value;
				SetCheckStartDate((DateTime)value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckStartDate = null;

		private void SetCheckStartDate(DateTime value)
		{
			StartDateComparator SDC = new StartDateComparator(value);
			CheckStartDate = SDC.Compare;
		}

		#endregion

		#region End Date

		private DateTime? endDate = null;
		public DateTime? EndDate
		{
			get => endDate;
			set
			{
				if (value < startDate) value = startDate;
				endDate = value;
				SetCheckEndDate((DateTime)value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckEndDate = null;

		private void SetCheckEndDate(DateTime value)
		{
			EndDateComparator EDC = new EndDateComparator(value);
			CheckEndDate = EDC.Compare;
		}

		#endregion

		#region Passport Number

		private string passportNumber;
		public string PassportNumber
		{
			get => passportNumber;
			set
			{
				if (String.IsNullOrEmpty(value)) CheckPassportNumber = null;
				passportNumber = value;
				SetCheckPassportNumber(value);
				NotifyPropertyChanged();
			}
		}

		Compare CheckPassportNumber;

		private void SetCheckPassportNumber(string value)
		{
			PassportNumberComparator PNC = new PassportNumberComparator(value);
			CheckPassportNumber = PNC.Compare;
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

		public IndividualsSearchRequestWindow()
		{
			InitializeComponent();
			DataContext = this;
		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			CheckAllFields  = CheckIfPerson;
			CheckAllFields += CheckFirstName;
			CheckAllFields += CheckMiddleName;
			CheckAllFields += CheckLastName;
			CheckAllFields += CheckStartDate;
			CheckAllFields += CheckEndDate;
			CheckAllFields += CheckPassportNumber;
			CheckAllFields += CheckTelephone;
			CheckAllFields += CheckEmail;
			CheckAllFields += CheckAddress;

			DialogResult = true;
		}

		private bool CheckIfPerson(IClient p, ref bool flag)
		{
			flag = false;
			if (p is IClientOrg) return false;
			flag = true;
			return true;
		}
	}


}
