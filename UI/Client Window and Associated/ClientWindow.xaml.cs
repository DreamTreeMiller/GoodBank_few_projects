using Account_Windows;
using DTO;
using Data_Grid_User_Controls;
using System.Windows;
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
		private BankActions			 BA;
		private AccountsList		 accountsListView;
		private WindowID			 wid	= WindowID.EditClientVIP;
		private IClientDTO			 client = new ClientDTO();

		public bool accountsNeedUpdate = false;
		public bool clientsNeedUpdate  = false;

		public ClientWindow(BankActions ba, IClientDTO client)
		{
			InitializeComponent();
			InitializeAccountsView(ba, client);
			ShowAccounts();
		}

		private void InitializeAccountsView(BankActions ba, IClientDTO client)
		{
			BankTodayDate.Text = $"Сегодня {GoodBankTime.Today:dd.MM.yyyy} г.";
			BA = ba;
			OrganizationInfo.Visibility = Visibility.Collapsed;
			PersonalInfo.Visibility		= Visibility.Visible;
			this.client					= client;

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
			var accountsList = accList.accList;
			accountsListView.SetAccountsDataGridItemsSource(accountsList);
			accountsListView.SetAccountsTotals(accList.accList.Count,
				accList.totalCurr, accList.totalDeposit, accList.totalCredit);
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

			// Обновляем визуально редактируемый элемент
			// Обновляем базу клиентов.
			// Эти два действия должны всегда быть вместе!
			(this.client as ClientDTO).UpdateMyself(editClientWindow.tmpClient as ClientDTO);
			BA.Clients.UpdateClient(editClientWindow.tmpClient);
		}

		private void ClientWindow_AccountDetails_Click(object sender, RoutedEventArgs e)
		{
			IAccountDTO account = accountsListView.GetSelectedItem();
			if (account == null)
			{
				MessageBox.Show("Выберите счет для показа");
				return;
			}
			IClient client = BA.Clients.GetClientByID(account.ClientID);
			AccountWindow accountWindow = new AccountWindow(BA, account);
			accountWindow.ShowDialog();
			if (accountWindow.accountsNeedUpdate)
			{
				ShowAccounts();
				accountsNeedUpdate = true;
			}
			if (accountWindow.clientsNeedUpdate) clientsNeedUpdate = true;

		}

		private void OpenSavingAccountButton_Click(object sender, RoutedEventArgs e)
		{
			OpenSavingAccountWindow ocawin = new OpenSavingAccountWindow();
			var result = ocawin.ShowDialog();
			if (result != true) return;
			IAccountDTO newAcc = new AccountDTO(client.ClientType, client.ID, AccountType.Saving,
				ocawin.startAmount, 0, false, 0, "не используется", ocawin.Opened, true, true, RecalcPeriod.NoRecalc, 0, 0);

			// Добавляем счет в базу в бэкенд
			BA.Accounts.AddAccount(newAcc);
			accountsNeedUpdate = true;
			ShowAccounts();
			clientsNeedUpdate = true;
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
			AccountDTO internalAccount		= new AccountDTO();
			internalAccount.AccountNumber	= "внутренний счет";
			accumulationAccounts.Add(internalAccount);

			OpenDepositWindow odwin = new OpenDepositWindow(accumulationAccounts, client.ClientType);
			var result = odwin.ShowDialog();
			if (result != true) return;

			// Получаем номер счета в базе счетов, на котором будут накапливаться проценты
			// Если был выбран "внутренний счет", то 
			// его ID == 0, AccountNumber == "внутренний счет"

			int		AccumAccIndx		= odwin.AccumulationAccount.SelectedIndex;
			uint	AccumulationAccID	= (odwin.AccumulationAccount.Items[AccumAccIndx] as AccountDTO).AccID;
			string	InterestAccumAccNum =
				(odwin.AccumulationAccount.Items[AccumAccIndx] as AccountDTO).AccountNumber;

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
			
			// Добавляем счет в базу в бэкенд
			BA.Accounts.AddAccount(newAcc);

			// Надо будет обновить список счетов и клиентов при выходе из окна клиента
			accountsNeedUpdate = true;

			// Обновляем счета в текущем окне клиента
			ShowAccounts();
			clientsNeedUpdate = true;
		}

		private void OpenCreditButton_Click(object sender, RoutedEventArgs e)
		{
			// Получаем список текущих счетов клиента, 
			// на один из которых нужно перечислить выданный кредит
			// Этот список будет выпадающим списком в окошке ввода данных вклада
			var creditRecipientAccounts = BA.Accounts.GetClientAccounts(client.ID, AccountType.Saving);

			// Клиент может получить кредит наличными
			// создаем и добавляем этот элемент списка в список счетов для накопления процентов
			AccountDTO cash = new AccountDTO();
			cash.AccountNumber = "получить наличными";
			creditRecipientAccounts.Add(cash);

			OpenCreditWindow ocrwin = new OpenCreditWindow(creditRecipientAccounts, client.ClientType);
			var result = ocrwin.ShowDialog();
			if (result != true) return;

			// Получаем номер счета в базе счетов, на котором будет перечислена выданная сумма
			// Если было выбрано "получить наличными", то его ID == 0
			int		CreRecipAccIndx		  =  ocrwin.CreditRecipientAccount.SelectedIndex;
			uint	CreditRecipientAccID  = (ocrwin.CreditRecipientAccount.Items[CreRecipAccIndx] as AccountDTO).AccID;
			string	CreditRecipientAccNum =
				(ocrwin.CreditRecipientAccount.Items[CreRecipAccIndx] as AccountDTO).AccountNumber;

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

			// Добавляем счет в базу в бэкенд
			newAcc = BA.Accounts.AddAccount(newAcc);

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

			// Надо будет обновить список счетов и клиентов при выходе из окна клиента
			accountsNeedUpdate = true;
			clientsNeedUpdate  = true;

			// Обновляем счета в текущем окне клиента
			ShowAccounts();
		}
	}
}
