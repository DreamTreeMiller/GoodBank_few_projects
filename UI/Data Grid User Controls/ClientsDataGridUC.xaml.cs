﻿using Window_Name_Tags;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Interfaces_Data;
using Binding_UI_CodeBehind;
using Enumerables;

namespace Data_Grid_User_Controls
{
	/// <summary>
	/// Interaction logic for ClientsList.xaml
	/// </summary>
	public partial class ClientsDataGridUC : UserControl
	{
		BankActions BA;
		#region Accessors to UserControl properties

		public void SetClientsDataGridItemsSource(DataView clientsTable)
		{
			ClientsDataGrid.DataContext = clientsTable;
		}

		public void SetClientsTotal(int clientsTotal)
		{
			ClientsTotalNumberValue.Text = $"{clientsTotal:N0}";
		}

		public DataRowView GetSelectedItem()
		{
			return (DataRowView)ClientsDataGrid.SelectedItem;
		}

		#endregion
		public ClientsDataGridUC(ClientsViewNameTags tags, BankActions ba)
		{
			InitializeComponent();
			BA = ba;
			ClientsDataGrid.Items.Clear(); // Почему-то вставляется пустой элемент после инициализации
										   // надо удалить, чтобы корректно всё работало
			InitializeColumnsTags(tags);
		}

				/// <summary>
		/// Обновляем данные клиента в таблице на экране!
		/// </summary>
		/// <param name="clientRowInTable">Указатель на строку клиента в таблице на экране</param>
		/// <param name="updatedClient">Обновлённые данные клиента</param>
		public void UpdateClientRowInView(DataRowView clientRowInTable, IClientDTO updatedClient)
		{
			clientRowInTable["ID"]						= updatedClient.ID;
			clientRowInTable["ClientType"]				= updatedClient.ClientType;
			clientRowInTable["ClientTypeTag"]			= updatedClient.ClientTypeTag;
			clientRowInTable["FirstName"]				= updatedClient.FirstName;
			clientRowInTable["MiddleName"]				= updatedClient.MiddleName;
			clientRowInTable["LastName"]				= updatedClient.LastName;
			clientRowInTable["MainName"]				= updatedClient.MainName;
			clientRowInTable["DirectorName"]			= updatedClient.DirectorName;
			clientRowInTable["CreationDate"]			= updatedClient.CreationDate;
			clientRowInTable["PassportOrTIN"]			= updatedClient.PassportOrTIN;
			clientRowInTable["Telephone"]				= updatedClient.Telephone;
			clientRowInTable["Email"]					= updatedClient.Email;
			clientRowInTable["Address"]					= updatedClient.Address;
			clientRowInTable["NumberOfSavingAccounts"]	= updatedClient.NumberOfSavingAccounts;
			clientRowInTable["NumberOfDeposits"]		= updatedClient.NumberOfDeposits;
			clientRowInTable["NumberOfCredits"]			= updatedClient.NumberOfCredits;
			clientRowInTable["NumberOfClosedAccounts"]	= updatedClient.NumberOfClosedAccounts;
		}


		private void InitializeColumnsTags(ClientsViewNameTags tags)
		{
			// Названия чекбоксов
			CreationDateCheckBox.Content  = tags.CreationDateCBTag;
			PassportOrTINCheckBox.Content = tags.PassportOrTIN_CB_Tag;
			DirectorCheckBox.Visibility   = tags.ShowDirectorCB;

			// Таблица 
			// Показывать колонку типa (физик / юрик) или нет
			ClientTypeColumn.Visibility = tags.ShowClientTypeColumn;

			// ФИО или Название организации
				 MainNameColumn.Header = tags.MainNameTag;    
			 CreationDateColumn.Header = tags.CreationDateCBTag;
			PassportOrTINColumn.Header = tags.PassportOrTIN_CB_Tag;

			// Сводка: ВИП клиентов, физиков, юриков
			ClientsTotalNumberTitle.Text  = tags.TotalNameTag;  
		}

		#region CheckBoxes handlers

		private void CreationDateCheckBox_Click(object sender, RoutedEventArgs e)
		{
			if (CreationDateCheckBox.IsChecked == true)
				CreationDateColumn.Visibility = Visibility.Visible;
			else
				CreationDateColumn.Visibility = Visibility.Collapsed;
		}

		private void DirectorCheckBox_Click(object sender, RoutedEventArgs e)
		{
			if (DirectorCheckBox.IsChecked == true)
				DirectorColumn.Visibility = Visibility.Visible;
			else
				DirectorColumn.Visibility = Visibility.Collapsed;
		}

		private void PassportOrTINCheckBox_Click(object sender, RoutedEventArgs e)
		{
			if (PassportOrTINCheckBox.IsChecked == true)
				PassportOrTINColumn.Visibility = Visibility.Visible;
			else
				PassportOrTINColumn.Visibility = Visibility.Collapsed;
		}

		private void TelCheckBox_Click(object sender, RoutedEventArgs e)
		{
			if (TelCheckBox.IsChecked == true)
				TelephoneColumn.Visibility = Visibility.Visible;
			else
				TelephoneColumn.Visibility = Visibility.Collapsed;
		}

		private void EmailCheckBox_Click(object sender, RoutedEventArgs e)
		{
			if (EmailCheckBox.IsChecked == true)
				EmailColumn.Visibility = Visibility.Visible;
			else
				EmailColumn.Visibility = Visibility.Collapsed;
		}

		private void AddressCheckBox_Click(object sender, RoutedEventArgs e)
		{
			if (AddressCheckBox.IsChecked == true)
				AddressColumn.Visibility = Visibility.Visible;
			else
				AddressColumn.Visibility = Visibility.Collapsed;
		}

		private void NumOfClosedAccountsCheckBox_Click(object sender, RoutedEventArgs e)
		{
			if (NumOfClosedAccountsCheckBox.IsChecked == true)
				NummberOfClosedAccountsColumn.Visibility = Visibility.Visible;
			else
				NummberOfClosedAccountsColumn.Visibility = Visibility.Collapsed;
		}

		#endregion

	}
}