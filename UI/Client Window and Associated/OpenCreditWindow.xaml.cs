using BankTime;
using Enumerables;
using Interfaces_Data;
using System;
using System.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace Client_Window
{
	/// <summary>
	/// Interaction logic for OpenCreditWindow.xaml
	/// </summary>
	public partial class OpenCreditWindow : Window, INotifyPropertyChanged
	{
		#region Поля и свойства счета

		public double		creditAmount = 0;
		public	string		CreditAmount
		{
			get => $"{creditAmount:N2}";
			set
			{
				if (!IsDoubleValid(value, out double tmp))
				{
					SetFocusOnCreditAmountEntryBox();
					return;
				}
				creditAmount = tmp;
			}
		}

		private double minInterest, maxInterest;
		public	double		interest = 0.05;
		public	string		Interest
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
		public	DateTime	Opened { get; } = GoodBankTime.Today;

		public	int			duration = 12;
		public	string		Duration
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

		public	DateTime	EndDate
		{
			get => Opened.AddMonths(duration);
		}
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Валидаторы ввода данных

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
			if (tmp < minInterest || maxInterest < tmp)
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

		#region	Установить фокус в окошке

		private void SetFocusOnCreditAmountEntryBox()
		{
			Dispatcher.BeginInvoke((ThreadStart)delegate
			{
				CreditAmountEntryBox.Focus();
				CreditAmountEntryBox.SelectionStart = CreditAmountEntryBox.Text.Length;
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

		public OpenCreditWindow(DataView creditRecipientAccounts, ClientType clientType)
		{
			InitializeComponent();
			InitializeWindowLabelsAndData(creditRecipientAccounts, clientType);

			SetFocusOnCreditAmountEntryBox();
		}

		private void InitializeWindowLabelsAndData(DataView creditRecipientAccounts, ClientType clientType)
		{
			BankTodayDate.Text = $"Сегодня {GoodBankTime.Today:dd.MM.yyyy} г.";
			CreditRecipientAccount.ItemsSource = creditRecipientAccounts;

			switch (clientType)
			{
				case ClientType.VIP:
					InterestLabel.Text = "Процент (7 ~ 12 %)";
					minInterest = 7;
					maxInterest = 12;
					interest	= 0.12;
					break;
				case ClientType.Simple:
					InterestLabel.Text = "Процент (12 ~ 20 %)";
					minInterest = 12;
					maxInterest = 20;
					interest	= 0.20;
					break;
				case ClientType.Organization:
					InterestLabel.Text = "Процент (15 ~ 25 %)";
					minInterest = 15;
					maxInterest = 25;
					interest	= 0.25;
					break;
			}
			DataContext = this;
		}

		private void btnOk_OpenCredit_Click(object sender, RoutedEventArgs e)
		{
			if (creditAmount == 0)
			{
				MessageBox.Show("Сумма кредита должна быть больше нуля");
				SetFocusOnCreditAmountEntryBox();
				return;
			}

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
