using System;
using System.Windows;
using System.Data;
using Account_Windows;
using DTO;
using Data_Grid_User_Controls;
using Binding_UI_CodeBehind;
using Interfaces_Data;
using BankTime;
using Enumerables;
using Window_Name_Tags;

namespace Client_Window
{
	/// <summary>
	/// Interaction logic for ClientWindow.xaml
	/// </summary>
	public partial class ClientWindow : Window
	{
		private BankActions		BA;
		private AccountsList	accountsListView;
		private WindowID		wid	= WindowID.EditClientVIP;
		private IClientDTO		client = new ClientDTO();
		private DataRowView		clientRowInTable;

		// Событие, возникающее тогда, когда изменились данные клиента
		// Вне класса ClientWindow необходимо подписать на это событие обработчик
		// который будет обновлять 
		public delegate void UpdateClientRowHandler(DataRowView clientRowInTable, IClientDTO clientDTO); 
		public event UpdateClientRowHandler ClientDataChanged;

		public bool accountsNeedUpdate = false;

		public ClientWindow(BankActions ba, DataRowView clientrowintable)
		{
			InitializeComponent();
			InitializeAccountsView(ba, clientrowintable);
			ShowAccounts();
		}

		private void InitializeAccountsView(BankActions ba, DataRowView clientrowintable)
		{
			BankTodayDate.Text = $"Сегодня {GoodBankTime.Today:dd.MM.yyyy} г.";
			BA = ba;
			OrganizationInfo.Visibility = Visibility.Collapsed;
			PersonalInfo.Visibility		= Visibility.Visible;
			this.clientRowInTable		= clientrowintable;
			this.client					= new ClientDTO(clientrowintable);
			switch (this.client.ClientType)
			{
				case ClientType.VIP:
					Title						= "ВИП";
					MainTitle.Text				= "ОЧЕНЬ ВАЖНАЯ ПЕРСОНА";
					break;
				case ClientType.Simple:
					Title						= "Физик";
					MainTitle.Text				= "ФИЗИК";
					wid							= WindowID.EditClientSIM;
					break;
				case ClientType.Organization:
					Title						= "Юрик";
					MainTitle.Text				= "ЮРИК";
					OrganizationInfo.Visibility = Visibility.Visible;
					PersonalInfo.Visibility		= Visibility.Collapsed;
					wid							= WindowID.EditClientORG;
					break;
			}

			ClientInfo.DataContext	= client;
			accountsListView		= new AccountsList();
			AccountsList.Content	= accountsListView;

		}

		private void ShowAccounts()
		{
			var accList = BA.Accounts.GetClientAccounts(client.ID);
			var accountsList = accList.accountsViewTable;
			accountsListView.SetAccountsDataGridItemsSource(accountsList, client.ClientType);
			accountsListView.SetAccountsTotals(accList.accountsViewTable.Count,
				accList.totalSaving, accList.totalDeposit, accList.totalCredit);
		}

		/// <summary>
		/// Редактирует данные клиента, не относящиеся к счетам
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ClientWindow_EditClient_Click(object sender, RoutedEventArgs e)
		{
			var tags				= new AddEditClientNameTags(wid);
			var editClientWindow	= new AddEditClientWindow(tags, client);
			var result = editClientWindow.ShowDialog();
			if (result != true) return;

			// Обновляем данные клиента в окне клиента
			(client as ClientDTO).UpdateMyself(editClientWindow.newOrUpdatedClient as ClientDTO);

			// Обновляем данные клиента в базе.
			BA.Clients.UpdateClientPersonalData(client);

			// Обновляем данные клиента в списке клиентов на экране в окне департамента
			ClientDataChanged?.Invoke(clientRowInTable, client);
		}

		private void ClientWindow_AccountDetails_Click(object sender, RoutedEventArgs e)
		{
			DataRowView account = accountsListView.GetSelectedItem();
			if (account == null)
			{
				MessageBox.Show("Выберите счет для показа");
				return;
			}

			AccountWindow accountWindow = new AccountWindow(BA, account);
			accountWindow.ShowDialog();
			if (accountWindow.accountsNeedUpdate)
			{
				ShowAccounts();
				accountsNeedUpdate = true;
			}
		}

		private void OpenSavingAccountButton_Click(object sender, RoutedEventArgs e)
		{
			OpenSavingAccountWindow ocawin = new OpenSavingAccountWindow();
			var result = ocawin.ShowDialog();
			if (result != true) return;
			IAccountDTO newAcc = new AccountDTO(client.ClientType, client.ID, AccountType.Saving,
				ocawin.startAmount, 0, false, 0, "не используется", ocawin.Opened, true, true, RecalcPeriod.NoRecalc, 0, 0);

			// Увеличиваем количество текущих счетов клиента на 1
			client.NumberOfSavingAccounts++;

			// Добавляем созданный счет в базу.
			// Обновляем данные о количестве счетов клиента в списке клиентов на экране.
			// Добавляем новый счёт в список в окошке клиента
			// Добавляем новый счёт в общий список счетов в окне департамента
			AddAccountUpdateClientDataInDBandView(newAcc, client);
		}

		private void OpenDepositButton_Click(object sender, RoutedEventArgs e)
		{
			// Получаем список текущих счетов клиента, 
			// на которых можно накапливать проценты со вклада,
			// если выбран режим - без капитализации
			// Этот список будет выпадающим списком в окошке ввода данных вклада
			var accumulationAccounts = BA.Accounts.GetClientAccountsToAccumulateInterest(client.ID);

			// Клиент может накапливать проценты на отдельном безымянном счете, 
			// привязанном ко вкладу. Я назвал его "внутренний счет"
			// создаем заглушку для этого счета и добавляем ее в список счетов для накопления процентов
			DataRowView internalAccount		 = accumulationAccounts.AddNew();
			internalAccount["AccID"]		 = 0;
			internalAccount["AccountNumber"] = "внутренний счет";

			OpenDepositWindow odwin = new OpenDepositWindow(accumulationAccounts, client.ClientType);
			var result = odwin.ShowDialog();
			if (result != true) return;

			// Получаем номер счета в базе счетов, на котором будут накапливаться проценты
			// Если был выбран "внутренний счет", то 
			// его ID == 0, AccountNumber == "внутренний счет"

			int		selectedAccIndx		= odwin.AccumulationAccount.SelectedIndex;
			DataRowView	selectedAccount = odwin.AccumulationAccount.Items[selectedAccIndx] as DataRowView;
			int		  AccumulationAccID	=	 (int)selectedAccount["AccID"];
			string	InterestAccumAccNum = (string)selectedAccount["AccountNumber"];

			// Упаковываем информацию для создания счета
			IAccountDTO newAcc = 
				 new AccountDTO(client.ClientType, client.ID, AccountType.Deposit,
								odwin.depositAmount, 
								odwin.interest, 
								(bool)odwin.CompoundingCheckBox.IsChecked,
								AccumulationAccID,
								InterestAccumAccNum,
								odwin.Opened, 
								(bool)odwin.TopUpCheckBox.IsChecked, 
								(bool)odwin.WithdrawalAllowedCheckBox.IsChecked, 
								(RecalcPeriod)odwin.Recalculation.SelectedIndex, 
								odwin.duration,
								0);

			// Увеличиваем количество депозитов клиента на 1
			client.NumberOfDeposits++;

			// Добавляем созданный счет в базу.
			// Обновляем данные о количестве счетов клиента в списке клиентов на экране.
			// Добавляем новый счёт в список в окошке клиента
			// Добавляем новый счёт в общий список счетов в окне департамента
			AddAccountUpdateClientDataInDBandView(newAcc, client);
		}

		private void OpenCreditButton_Click(object sender, RoutedEventArgs e)
		{
			// Получаем список текущих счетов клиента, 
			// на один из которых нужно перечислить выданный кредит
			// Этот список будет выпадающим списком в окошке ввода данных вклада
			var creditRecipientAccounts = BA.Accounts.GetClientSavingAccounts(client.ID);

			// Клиент может получить кредит наличными
			// создаем и добавляем этот элемент списка в список счетов для накопления процентов
			DataRowView cash		= creditRecipientAccounts.AddNew();
			cash["AccID"]			= 0;
			cash["AccountNumber"]	= "получить наличными";

			OpenCreditWindow ocrwin = new OpenCreditWindow(creditRecipientAccounts, client.ClientType);
			var result = ocrwin.ShowDialog();
			if (result != true) return;

			// Получаем номер счета в базе счетов, на котором будет перечислена выданная сумма
			// Если было выбрано "получить наличными", то его ID == 0
			int		selectedAccountIndx	 = ocrwin.CreditRecipientAccount.SelectedIndex;
			DataRowView selectedAccount	 = ocrwin.CreditRecipientAccount.Items[selectedAccountIndx] as DataRowView;
			int		CreditRecipientAccID =	 (int)selectedAccount["AccID"];
			string CreditRecipientAccNum = (string)selectedAccount["AccountNumber"];


			// Упаковываем информацию для создания счета
			IAccountDTO newAcc =
				 new AccountDTO(client.ClientType, client.ID, AccountType.Credit,
								-ocrwin.creditAmount,	// Записываем сумму со знаком минус!
								ocrwin.interest,
								true,					// Это счет с капитализацией
								CreditRecipientAccID,	//    ID счета, на который перечислить выданный кредит
								CreditRecipientAccNum,  // Номер счета, на который перечислить выданный кредит
								ocrwin.Opened,
								true,					// Пополняемый счет
								false,					// Понятие досрочного снятия неприменимо к кредиту
								RecalcPeriod.Monthly,	// Начисление процентов ежемесячно
								ocrwin.duration,
								0);

			// Увеличиваем количество кредитов клиента на 1
			client.NumberOfCredits++;

			// Добавляем созданный счет в базу.
			// Обновляем данные о количестве счетов клиента в списке клиентов на экране.
			// Добавляем новый счёт в список в окошке клиента
			// Добавляем новый счёт в общий список счетов в окне департамента
			AddAccountUpdateClientDataInDBandView(newAcc, client);

			if (CreditRecipientAccID == 0)
			{
				MessageBox.Show($"Получите в кассе {ocrwin.creditAmount:N2} рублей.");
			}
			// Переводим выданный кредит на указанный текущий счет
			// Если было указано "выдать наличными", то ничего не произойдёт
			else
			{
				BA.Accounts.TopUpCash(CreditRecipientAccID, ocrwin.creditAmount);
				MessageBox.Show($"Сумма {ocrwin.creditAmount:N2} рублей переведена на\n"
					+ $"счет №: {CreditRecipientAccNum}" 
					);
			}
		}

		private void AddAccountUpdateClientDataInDBandView(IAccountDTO newAcc, IClientDTO updatedClient)
		{
			// Добавляем счет в базу в бэкенд
			// Также обновляется количество счетов у клиента в базе
			BA.Accounts.AddAccount(newAcc);

			//Обновляем количество счетов у клиента в списке на экране in ClientsView Table
			ClientDataChanged?.Invoke(clientRowInTable, updatedClient);

			accountsNeedUpdate = true;
			ShowAccounts();
		}
	}
}
