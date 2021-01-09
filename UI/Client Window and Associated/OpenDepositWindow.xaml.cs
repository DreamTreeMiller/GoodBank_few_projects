using BankTime;
using DTO;
using Enumerables;
using Interfaces_Data;
using System;
using System.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Client_Window
{
	/// <summary>
	/// Interaction logic for OpenDepositWindow.xaml
	/// </summary>
	public partial class OpenDepositWindow : Window, INotifyPropertyChanged
	{
		#region Поля и свойства вклада

		public double	depositAmount = 0;
		public string	DepositAmount
		{
			get => $"{depositAmount:N2}";
			set
			{
				if (!IsDoubleValid(value, out double tmp))
				{
					SetFocusOnDepositAmountEntryBox();
					return;
				}
				depositAmount = tmp;
			}
		}

		private double minInterest, maxInterest;
		public double   interest;
		public string	Interest	
		{ 
			get => $"{(interest * 100):N2}"; 
			set
			{
				if (!IsInterestValid(value, out double tmp))
				{
					SetFocusOnInterestEntryBox();
					return;
				}
				interest = tmp / 100;
			}
		}
		public DateTime Opened		{ get; }	  = GoodBankTime.Today;

		public int		duration = 12;
		public string	Duration 
		{ 
			get => $"{duration}"; 
			set
			{
				if (!IsDurationValid(value, out int tmp))
				{
					SetFocusOnDurationEntryBox();
					return;
				}
				duration = tmp;
				NotifyPropertyChanged("EndDate");
			}
		}
		public DateTime EndDate 
		{ 
			get => Opened.AddMonths(duration);
		}
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		private void CompoundingCheckBox_Click(object sender, RoutedEventArgs e)
		{
			if ((bool)(sender as CheckBox).IsChecked)
			{
				AccumulationAccountLabel.Visibility = Visibility.Collapsed;
				AccumulationAccount.Visibility = Visibility.Collapsed;
			}
			else
			{
				AccumulationAccountLabel.Visibility = Visibility.Visible;
				AccumulationAccount.Visibility = Visibility.Visible;
			}
		}

		#region Валидация введенных данных

		/// <summary>
		/// Проверяет, является ли введенная строка корректным числом с плав. запятой
		/// </summary>
		/// <param name="input">Введенная строка</param>
		/// <param name="tmp">Преобразованное значение. Если ввод некорректный, то значение неопределено</param>
		/// <returns>true/false. Если true, то в tmp результат преобразования</returns>
		private bool IsDoubleValid(string input, out double tmp)
		{
			if (String.IsNullOrEmpty(input))
			{
				MessageBox.Show("Введите число.");
				tmp = 0;
				return false;
			}
			if (!Double.TryParse(input, out tmp))
			{
				MessageBox.Show("Некорректрый ввод! Введите число.");
				return false;
			}
			if (tmp < 0)
			{
				MessageBox.Show("Число не должно быть отрицательным");
				return false;
			}
			return true;
		}

		private bool IsInterestValid(string input, out double tmp)
		{
			if (String.IsNullOrEmpty(input))
			{
				MessageBox.Show("Введите число.");
				tmp = 0;
				return false;
			}
			if (!Double.TryParse(input, out tmp))
			{
				MessageBox.Show("Некорректрый ввод! Введите число.");
				return false;
			}
			if ( tmp < minInterest|| maxInterest < tmp )
			{
				MessageBox.Show($"Процент должен быть между {minInterest:N2} ~ {maxInterest:N2} %");
				return false;
			}
			return true;
		}

		private bool IsDurationValid(string input, out int tmp)
		{
			if (String.IsNullOrEmpty(input))
			{
				MessageBox.Show("Введите число.");
				tmp = 0;
				return false;
			}
			if (!Int32.TryParse(input, out tmp))
			{
				MessageBox.Show("Некорректрый ввод! Введите число.");
				return false;
			}
			if (tmp < 1)
			{
				MessageBox.Show("Число месяцев должно быть больше 0");
				return false;
			}
			return true;
		}

		#endregion

		#region Set Focus on specific TextBox

		private void SetFocusOnDepositAmountEntryBox()
		{
			Dispatcher.BeginInvoke((ThreadStart)delegate
			{
				DepositAmountEntryBox.Focus();
				DepositAmountEntryBox.SelectionStart = DepositAmountEntryBox.Text.Length;
			});
		}

		private void SetFocusOnInterestEntryBox()
		{
			Dispatcher.BeginInvoke((ThreadStart)delegate
			{
				InterestEntryBox.Focus();
				InterestEntryBox.SelectionStart = InterestEntryBox.Text.Length;
			});
		}

		private void SetFocusOnDurationEntryBox()
		{
			Dispatcher.BeginInvoke((ThreadStart)delegate
			{
				DurationEntryBox.Focus();
				DurationEntryBox.SelectionStart = DurationEntryBox.Text.Length;
			});
		}

		#endregion

		public OpenDepositWindow(DataView accumulationAccounts, ClientType clientType)
		{
			InitializeComponent();
			InitializeWindowLabelsAndData(accumulationAccounts, clientType);
			SetFocusOnDepositAmountEntryBox();
		}

		private void InitializeWindowLabelsAndData(DataView accumulationAccounts, ClientType clientType)
		{
			BankTodayDate.Text = $"Сегодня {GoodBankTime.Today:dd.MM.yyyy} г.";
			AccumulationAccount.ItemsSource = accumulationAccounts;

			switch (clientType)
			{
				case ClientType.VIP:
					InterestLabel.Text = "Процент (11 ~ 20 %)";
					minInterest = 11;
					maxInterest = 20;
					interest	= 0.11;
					break;
				case ClientType.Simple:
					InterestLabel.Text = "Процент (5 ~ 10 %)";
					minInterest = 5;
					maxInterest = 10;
					interest	= 0.05;
					break;
				case ClientType.Organization:
					InterestLabel.Text = "Процент (7 ~ 15 %)";
					minInterest = 7;
					maxInterest = 15;
					interest	= 0.07;
					break;
			}
			DataContext = this;
		}

		private void btnOk_OpenDeposit_Click(object sender, RoutedEventArgs e)
		{
			if (duration == 0)
			{
				MessageBox.Show("Число месяцев должно быть больше 0");
				SetFocusOnDurationEntryBox();
				return;
			}
			DialogResult = true;
		}

	}
}
