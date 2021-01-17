using Interfaces_Data;
using Enumerables;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System;

namespace Data_Grid_User_Controls
{
	/// <summary>
	/// Interaction logic for AccountsList.xaml
	/// </summary>
	public partial class AccountsDataGridUC : UserControl
	{
		public int		AccountsTotalNumber	{ get; set; }
		public decimal	SavingTotalAmount	{ get; set; }
		public decimal	DepositsTotalAmount { get; set; }
		public decimal	CreditsTotalAmount	{ get; set; }

		private DataView accountsListSource;

		#region Accessors to UserControl properties

		public void SetAccountsDataGridItemsSource(DataView accountsList, ClientType ct)
		{
			accountsListSource = accountsList;
			AccountsDataGrid.ItemsSource = accountsListSource;
			if (ct == ClientType.All)
				ClientTypeColumn.Visibility = Visibility.Visible;
		}

		public void SetAccountsTotals(int totalAccounts, 
									  decimal totalSaving, decimal totalDeposit, decimal totalCredit)
		{
			AccountsTotalNumber	= totalAccounts;
			SavingTotalAmount	= totalSaving;
			DepositsTotalAmount	= totalDeposit;
			CreditsTotalAmount	= totalCredit;
		}

		public DataRowView GetSelectedItem()
		{
			return AccountsDataGrid.SelectedItem as DataRowView;
		}

		#endregion

		public AccountsDataGridUC()
		{
			InitializeComponent();
			AccountsDataGrid.Items.Clear(); // Почему-то вставляется пустой элемент после инициализации
											// надо удалить, чтобы корректно всё работало
			DataContext = this;
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

		public void AddNewAccountToDataGrid(IAccountDTO addedAcc)
		{
			DataRowView newAccRow = accountsListSource.AddNew();
			UpdateAccountRowInDataGrid(newAccRow, addedAcc);
			++AccountsTotalNumber;
		}

		public void UpdateAccountRowInDataGrid(DataRowView newAccRow, IAccountDTO updatedAcc)
		{
			newAccRow["AccID"]			= updatedAcc.AccID;
			newAccRow["ClientID"]		= updatedAcc.ClientID;
			newAccRow["ClientType"]		= updatedAcc.ClientType;
			newAccRow["ClientTypeTag"]	= updatedAcc.ClientTypeTag;
			newAccRow["ClientName"]		= updatedAcc.ClientName;
			newAccRow["AccountNumber"]	= updatedAcc.AccountNumber;
			newAccRow["AccType"]		= updatedAcc.AccType;
			newAccRow["CurrentAmount"]	= updatedAcc.CurrentAmount;
			newAccRow["DepositAmount"]	= updatedAcc.DepositAmount;
			newAccRow["DebtAmount"]		= updatedAcc.DebtAmount;
			newAccRow["Interest"]		= updatedAcc.Interest;
			newAccRow["Opened"]			= updatedAcc.Opened;
			if (updatedAcc.Closed == null)
			{ newAccRow["Closed"] = DBNull.Value; }
			else
			{ newAccRow["Closed"] = updatedAcc.Closed; }
			newAccRow["Topupable"]		= updatedAcc.Topupable;

			SavingTotalAmount   += updatedAcc.CurrentAmount;
			DepositsTotalAmount += updatedAcc.DepositAmount;
			CreditsTotalAmount  += updatedAcc.DebtAmount;

		}
	}

}
