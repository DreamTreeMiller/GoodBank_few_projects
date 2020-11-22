using System;
using System.Threading;
using System.Windows;

namespace Generate_Clients_and_Accounts
{
	/// <summary>
	/// Interaction logic for GenerateWindow.xaml
	/// </summary>
	public partial class GenerateWindow : Window
	{
		#region Поля и свойства для ввода данных в окне

		public int vipClients;
		public string VIPclients 
		{ 
			get => $"{vipClients}";
			set 
			{
				if (!IsInputValid(value, out int tmp))
				{
					SetFocusOnVIPclientsEntryBox();
					return;
				}	
				vipClients = tmp; 
			}
		}
		public int simClients;
		public string SIMclients
		{
			get => $"{simClients}";
			set
			{
				if (!IsInputValid(value, out int tmp))
				{
					SetFocusOnSIMclientsEntryBox();
					return;
				}
				simClients = tmp;
			}
		}
		public int orgClients;
		public string ORGclients
		{
			get => $"{orgClients}";
			set
			{
				if (!IsInputValid(value, out int tmp))
				{
					SetFocusOnORGclientsEntryBox();
					return;
				}
				orgClients = tmp;
			}
		}

		#endregion

		#region Validation of fields

		private bool IsInputValid(string input, out int tmp)
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
			if (tmp < 0)
			{
				MessageBox.Show("Число не должно быть отрицательным");
				return false;
			}
			return true;
		}

		#endregion

		public GenerateWindow()
		{
			InitializeComponent();
			vipClients = 10;
			simClients = 10;
			orgClients = 10;
			DataContext = this;
		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void SetFocusOnVIPclientsEntryBox()
		{
			Dispatcher.BeginInvoke((ThreadStart)delegate
			{
				VIPclientsEntryBox.Focus();
				VIPclientsEntryBox.SelectionStart = VIPclientsEntryBox.Text.Length;
			});
		}

		private void SetFocusOnSIMclientsEntryBox()
		{
			Dispatcher.BeginInvoke((ThreadStart)delegate
			{
				SIMclientsEntryBox.Focus();
				SIMclientsEntryBox.SelectionStart = SIMclientsEntryBox.Text.Length;
			});
		}

		private void SetFocusOnORGclientsEntryBox()
		{
			Dispatcher.BeginInvoke((ThreadStart)delegate
			{
				ORGclientsEntryBox.Focus();
				ORGclientsEntryBox.SelectionStart = ORGclientsEntryBox.Text.Length;
			});
		}

	}
}
