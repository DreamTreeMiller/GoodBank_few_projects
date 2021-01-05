using BankTime;
using DTO;
using Interfaces_Data;
using Enumerables;
using Window_Name_Tags;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading;

namespace Client_Window
{
	public class ClientTypeTuple
	{
		public string clientTypeStr  { get; }
		public ClientType clientType { get; }
		public ClientTypeTuple(string s, ClientType t)
		{
			clientTypeStr	= s;
			clientType		= t;
		}
	}
	public partial class AddEditClientWindow : Window
	{
		public IClientDTO newOrUpdatedClient = null;

		public readonly List<ClientTypeTuple> clientTypesDDlist = 
			new List<ClientTypeTuple>()
			{
				new ClientTypeTuple("ВИП клиент", ClientType.VIP),
				new ClientTypeTuple("Физик",	  ClientType.Simple),
				new ClientTypeTuple("Юрик",		  ClientType.Organization)
			};
		public AddEditClientWindow(AddEditClientNameTags nameTags, IClientDTO client)
		{
			InitializeComponent();
			InitializeTextFields(nameTags, client);
		}

		private void InitializeTextFields(AddEditClientNameTags nameTags, IClientDTO client)
		{
			Title		= nameTags.SystemWindowTitle;
			Header.Text = nameTags.WindowHeader;

			// Выбираем, что отображать
			switch(nameTags.WID)
			{
				case WindowID.AddClientALL:
					Height = MinHeight = MaxHeight	= 450;
					SelectClientTypeLine.Visibility = Visibility.Visible;
					SelectTypeEntryBox.ItemsSource = clientTypesDDlist;
					break;
				case WindowID.AddClientVIP:
				case WindowID.AddClientSIM:
				case WindowID.EditClientVIP:
				case WindowID.EditClientSIM:
					PersonsNameGrid.Visibility		= Visibility.Visible;
					OrganizationNameGrid.Visibility = Visibility.Collapsed;
					Height = MinHeight = MaxHeight	= 410;
					break;
				case WindowID.AddClientORG:
				case WindowID.EditClientORG:
					PersonsNameGrid.Visibility		= Visibility.Collapsed;
					OrganizationNameGrid.Visibility = Visibility.Visible;
					Height = MinHeight = MaxHeight	= 470;
					break;
			}

			// Если окошко вызвали для создания нового клиента
			// а это происходит тогда, когда клиент на входе равен null
			// То в болванку ДТО надо поместить тип создаваемого клиента
			if (client == null)
			{
				newOrUpdatedClient = new ClientDTO();
				newOrUpdatedClient.ClientType = nameTags.ClientType;
			}
			else
			{
				newOrUpdatedClient = (client as ClientDTO).Clone();
			}
			DataContext	= this.newOrUpdatedClient;
		}
		private void btnOk_AddClient_Click(object sender, RoutedEventArgs e)
		{
			if (newOrUpdatedClient.ClientType == ClientType.Organization)
			{
				if (!IsOrgNameEntered())			return;
				if (!IsTINEntered())				return;
				if (!IsRegistrationDateEntered())	return;
			}
			else
			{
				if (!IsFirstNamesEntered())			return;
				if (!IsLastNamesEntered())			return;
				if (!IsPassportNumEntered())		return;
				if (!IsBirthDateEntered())			return;
			}
			DialogResult = true;
		}

		#region Проверка заполненности полей

		private bool IsOrgNameEntered()
		{
			if (String.IsNullOrEmpty(newOrUpdatedClient.MainName))
			{
				MessageBox.Show("Введите название организации");
				return false;
			}
			return true;
		}

		private bool IsTINEntered()
		{
			if (String.IsNullOrEmpty(newOrUpdatedClient.PassportOrTIN))
			{
				MessageBox.Show("Введите ИНН");
				// код возвращения фокуса в только что покинутое поле
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					TINEntryBox.Focus();
					TINEntryBox.SelectionStart = TINEntryBox.Text.Length;
				});
				return false;
			}
			return true;
		}

		private bool IsRegistrationDateEntered()
		{
			if (newOrUpdatedClient.CreationDate == null)
			{
				MessageBox.Show("Введите дату регистрации организации");
				// код возвращения фокуса в только что покинутое поле
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					RegistrationDateEntryBox.Focus();
				});
				return false;
			}
			return true;
		}

		private bool IsFirstNamesEntered()
		{
			if (String.IsNullOrEmpty(newOrUpdatedClient.FirstName))
			{
				MessageBox.Show("Имя клиента не должно быть пустым!\n" +
								"     Введите имя клиента.");
				// код возвращения фокуса в только что покинутое поле
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					FirstNameEntryBox.Focus();
					FirstNameEntryBox.SelectionStart = FirstNameEntryBox.Text.Length;
				});
				return false;
			}
			return true;
		}
		
		private bool IsLastNamesEntered()
		{
				if (String.IsNullOrEmpty(newOrUpdatedClient.LastName))
			{
				MessageBox.Show("Фамилия клиента не должна быть пустой!\n" +
								"     Введите фамилию клиента.");
				// код возвращения фокуса в только что покинутое поле
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					LastNameEntryBox.Focus();
					LastNameEntryBox.SelectionStart = LastNameEntryBox.Text.Length;
				});
				return false;
			}
			return true;
		}
	
		private bool IsPassportNumEntered()
		{
			if (String.IsNullOrEmpty(newOrUpdatedClient.PassportOrTIN))
			{
				MessageBox.Show("Введите номер паспорта");
				// код возвращения фокуса в поле
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					PassportNumEntryBox.Focus();
					PassportNumEntryBox.SelectionStart = PassportNumEntryBox.Text.Length;
				});
				return false;
			}
			return true;
		}

		private bool IsBirthDateEntered()
		{
			if (newOrUpdatedClient.CreationDate == null)
			{
				MessageBox.Show("Введите дату рождения клиента");
				// код возвращения фокуса в поле
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					BirthDateEntryBox.Focus();
				});
				return false;
			}
			return true;
		}

		#endregion

		#region Валидаторы свойств для окошка ввода данных

		private bool IsValidRegistrationDate(DateTime date)
		{
				if (date > GoodBankTime.Today)
				{
					MessageBox.Show("Дата не может превосходить сегодняшний день");
					return false;
				}
				return true;
		}

		private bool IsValidBirthDate(DateTime date)
		{

			if ((GoodBankTime.Today - date).TotalDays / 365.25 < 18)
			{
				MessageBox.Show("Только лица, достигшие 18 лет, могут быть клиентами банка.");
				return false;
			}
			if ((GoodBankTime.Today - date).TotalDays / 365.25 > 118)
			{
				MessageBox.Show("Сейчас на земле нет людей, которым больше 118 лет.");
				return false;
			}
			return true;
		}

		/// <summary>
		/// Проверка валидности ИНН
		/// </summary>
		/// <param name="tin">ИНН</param>
		/// <returns></returns>
		private bool ValidTIN(string tin)
		{
			int part;
			string errorMessage = "		Неверный формат ИНН\n" +
									"\n" +
									"	ррННхххххС\n" +
									"	рр - код региона России от 1 до 85\n" +
									"	НН - номер от 1 до 99 налоговой в регионе\n" +
									"	ххххх - код (от 1) организации\n" +
									"	С - контрольная цифра";
			tin = tin.Trim();
			if (tin.Length == 10)
				if (Int32.TryParse(tin.Substring(0, 2), out part))
					if (0 < part && part <= 85)
						if (Int32.TryParse(tin.Substring(2, 2), out part))
							if (0 < part)
								if (Int32.TryParse(tin.Substring(4, 6), out part))
									if (0 < part)
										return true;
			MessageBox.Show(errorMessage);
			return false;
		}

		/// <summary>
		/// Проверка валидности номера паспорта
		/// </summary>
		/// <param name="pNum">Номер паспорта в формате СССС ХХХХХХ</param>
		/// <returns></returns>
		private bool ValidPassportNum(ref string pNum)
		{
			int series, number;
			string errorMessage =   "          Неверный формат номера паспорта!\n" +
									"           Используйте формат CCCC ХХХХХХ\n" +
									"    CCCC   - 4 цифры серии, первая не может быть 0\n" +
									"    ХХХХХХ - 6 цифр номера, первая не может быть 0\n" +
									"Количество пробелов до, между и после групп цифр может быть любым";
			pNum = pNum.Replace(" ", "");
			if (pNum.Length == 10)
				if (Int32.TryParse(pNum.Substring(0, 4), out series))
					if (0 < series)
						if (Int32.TryParse(pNum.Substring(4), out number))
							if (0 < number && number < 1_000_000)
							{
								pNum = $"{series:0000} {number:000000}";
								return true;
							}
			MessageBox.Show(errorMessage);
			return false;
		}

		#endregion


		private void BirthDateEntryBox_SelectedDateChanged(object sender, RoutedEventArgs e)
		{
			if (newOrUpdatedClient.ClientType == ClientType.Organization) return;
			var date = BirthDateEntryBox.SelectedDate;
			if (date == null) return;
			if (!IsValidBirthDate((DateTime)date))
			{
				BirthDateEntryBox.SelectedDate = null;
				// код возвращения фокуса в поле
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					BirthDateEntryBox.Focus();
				});
			}
		}

		private void PassportNumEntryBox_LostFocus(object sender, RoutedEventArgs e)
		{
			var passnum = PassportNumEntryBox.Text;
			if (String.IsNullOrEmpty(passnum)) return;
			if (ValidPassportNum(ref passnum))
			{
				PassportNumEntryBox.Text = passnum;
			}
			else
			{
				// код возвращения фокуса в поле
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					PassportNumEntryBox.Focus();
					PassportNumEntryBox.SelectionStart = PassportNumEntryBox.Text.Length;
				});
			}
		}

		private void RegistrationDateEntryBox_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if (newOrUpdatedClient.ClientType != ClientType.Organization) return;
			var date = RegistrationDateEntryBox.SelectedDate;
			if (date == null) return;
			if (!IsValidRegistrationDate((DateTime)date))
			{
				RegistrationDateEntryBox.SelectedDate = null;
				// код возвращения фокуса в поле
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					RegistrationDateEntryBox.Focus();
				});
			}
		}

		private void TINEntryBox_LostFocus(object sender, RoutedEventArgs e)
		{
			var tin = TINEntryBox.Text;
			if (String.IsNullOrEmpty(tin)) return;
			if (!ValidTIN(tin))
				// код возвращения фокуса в поле
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					TINEntryBox.Focus();
					TINEntryBox.SelectionStart = TINEntryBox.Text.Length;
				});
		}

		private void SelectTypeEntryBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (newOrUpdatedClient == null) return;
			(newOrUpdatedClient as ClientDTO).UpdateMyself(new ClientDTO());
			switch ((SelectTypeEntryBox.SelectedItem as ClientTypeTuple).clientType)
			{
				case ClientType.VIP:
					newOrUpdatedClient.ClientType			= ClientType.VIP;
					PersonsNameGrid.Visibility		= Visibility.Visible;
					OrganizationNameGrid.Visibility = Visibility.Collapsed;
					Height = MinHeight = MaxHeight = 450;
					break;
				case ClientType.Simple:
					newOrUpdatedClient.ClientType			= ClientType.Simple;
					PersonsNameGrid.Visibility      = Visibility.Visible;
					OrganizationNameGrid.Visibility = Visibility.Collapsed;
					Height = MinHeight = MaxHeight = 450;
					break;
				case ClientType.Organization:
					newOrUpdatedClient.ClientType			= ClientType.Organization;
					PersonsNameGrid.Visibility		= Visibility.Collapsed;
					OrganizationNameGrid.Visibility = Visibility.Visible;
					Height = MinHeight = MaxHeight = 510;
					break;
			}
		}
	}
}
