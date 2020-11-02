using Interfaces_Data;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Data_Grid_User_Controls
{
	/// <summary>
	/// Interaction logic for AccountsList.xaml
	/// </summary>
	public partial class AccountsList : UserControl
	{
		#region Accessors to UserControl properties

		public void SetAccountsDataGridItemsSource(ObservableCollection<IAccountDTO> accountsList)
		{
			AccountsDataGrid.ItemsSource = accountsList;
		}

		public void SetAccountsTotals(int totalAccounts, 
									  double totalCurr, double totalDeposit, double totalCredit)
		{
			AccountsTotalNumberValue.Text	= $"{totalAccounts:N0}";
			CurrentTotalAmount.Text			= $"{totalCurr:N2}";
			DepositsTotalAmount.Text		= $"{totalDeposit:N2}";
			CreditsTotalAmount.Text			= $"{totalCredit:N2}";
		}

		public IAccountDTO GetSelectedItem()
		{
			return AccountsDataGrid.SelectedItem as IAccountDTO;
		}

		#endregion
		public AccountsList()
		{
			InitializeComponent();
			AccountsDataGrid.Items.Clear(); // Почему-то вставляется пустой элемент после инициализации
											// надо удалить, чтобы корректно всё работало
		}

		#region Accounts DataGrid CheckBoxes handlers

		private void CurrentAccountsCB_Click(object sender, RoutedEventArgs e)
		{
			// Если все другие галочки уже сняты, и эта тоже только что была снята
			if (CurrentAccountsCB.IsChecked == false &&
					   DepositsCB.IsChecked == false &&
						CreditsCB.IsChecked == false &&
				 ClosedAccountsCB.IsChecked == false)
			{
				// То устанавливаем галочку обратно и выходим
				CurrentAccountsCB.IsChecked = true;
				return;
			}

			if (CurrentAccountsCB.IsChecked == true)
				CurrAccountColumn.Visibility = Visibility.Visible;
			else
				CurrAccountColumn.Visibility = Visibility.Collapsed;
		}

		private void DepositsCB_Click(object sender, RoutedEventArgs e)
		{
			// Если все другие галочки уже сняты, и эта тоже только что была снята
			if (CurrentAccountsCB.IsChecked == false &&
					   DepositsCB.IsChecked == false &&
						CreditsCB.IsChecked == false &&
				 ClosedAccountsCB.IsChecked == false)
			{
				// То устанавливаем галочку обратно и выходим
				DepositsCB.IsChecked = true;
				return;
			}

			if (DepositsCB.IsChecked == true)
				DepositColumn.Visibility = Visibility.Visible;
			else
				DepositColumn.Visibility = Visibility.Collapsed;
		}

		private void CreditsCB_Click(object sender, RoutedEventArgs e)
		{
			// Если все другие галочки уже сняты, и эта тоже только что была снята
			if (CurrentAccountsCB.IsChecked == false &&
					   DepositsCB.IsChecked == false &&
						CreditsCB.IsChecked == false &&
				 ClosedAccountsCB.IsChecked == false)
			{
				// То устанавливаем галочку обратно и выходим
				CreditsCB.IsChecked = true;
				return;
			}

			if (CreditsCB.IsChecked == true)
				CreditColumn.Visibility = Visibility.Visible;
			else
				CreditColumn.Visibility = Visibility.Collapsed;
		}

		private void ClosedAccountsCB_Click(object sender, RoutedEventArgs e)
		{
			// Если все другие галочки уже сняты, и эта тоже только что была снята
			if (CurrentAccountsCB.IsChecked == false &&
					   DepositsCB.IsChecked == false &&
						CreditsCB.IsChecked == false &&
				 ClosedAccountsCB.IsChecked == false)
			{
				// То устанавливаем галочку обратно и выходим
				ClosedAccountsCB.IsChecked = true;
				return;
			}

			if (ClosedAccountsCB.IsChecked == true)
				ClosedDateColumn.Visibility = Visibility.Visible;
			else
				ClosedDateColumn.Visibility = Visibility.Collapsed;
		}

		#endregion

	}

}
