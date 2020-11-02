using System;
using System.Threading;
using System.Windows;

namespace Account_Windows
{
	/// <summary>
	/// Interaction logic for EnterTopUpCashAmountWindow.xaml
	/// </summary>
	public partial class EnterCashAmountWindow : Window
	{
		public double amount = 0;

		/// <summary>
		/// Проверяет, является ли введенная строка корректным числом с плав. запятой
		/// </summary>
		/// <param name="input">Введенная строка</param>
		/// <param name="tmp">Преобразованное значение. Если ввод некорректный, то значение неопределено</param>
		/// <returns>true/false. Если true, то в tmp результат преобразования</returns>
		private bool IsInputValid(string input, out double tmp)
		{
			if (String.IsNullOrEmpty(input))
			{
				MessageBox.Show("Введите число.");
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					AmountEnterBox.Text = "0.00";
					AmountEnterBox.Focus();
					AmountEnterBox.SelectionStart = AmountEnterBox.Text.Length;
				});
				tmp = 0;
				return false;
			}

			if (!Double.TryParse(input, out tmp))
			{
				MessageBox.Show("Некорректрый ввод! Введите число.");
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					AmountEnterBox.Text = "0.00";
					AmountEnterBox.Focus();
					AmountEnterBox.SelectionStart = AmountEnterBox.Text.Length;
				});

				return false;
			}
			if (tmp < 0)
			{
				MessageBox.Show("Число не должно быть отрицательным");
				Dispatcher.BeginInvoke((ThreadStart)delegate
				{
					AmountEnterBox.Focus();
					AmountEnterBox.SelectionStart = AmountEnterBox.Text.Length;
				});

				return false;
			}
			return true;
		}

		public EnterCashAmountWindow()
		{
			InitializeComponent();
			DataContext = this;
			Dispatcher.BeginInvoke((ThreadStart)delegate
			{
				AmountEnterBox.Text = "0.00";
				AmountEnterBox.Focus();
				AmountEnterBox.SelectionStart = AmountEnterBox.Text.Length;
			});

		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			if (IsInputValid(AmountEnterBox.Text, out double tmp))
			{
				amount = tmp;
				DialogResult = true;
			}
		}
	}
}
