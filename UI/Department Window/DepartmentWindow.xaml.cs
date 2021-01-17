using BankTime;
using Binding_UI_CodeBehind;
using Enumerables;
using Interfaces_Data;
using Account_Windows;
using Client_Window;
using Data_Grid_User_Controls;
using Window_Name_Tags;
using System.Windows;
using System.Data;

namespace Department_Window
{
	/// <summary>
	/// Interaction logic for VIPclientsWindow.xaml
	/// </summary>
	public partial class DepartmentWindow : Window
	{
		private BankActions			BA;
		private WindowID			wid;

		private WindowNameTags		deptwinnametags;
		private ClientsDataGridUC	clientsTableInDeptWindowUC;
		private ClientsViewNameTags	clntag;
		private WindowID			addClientWID;
		private DataView			clientsListToShow;

		private ClientType			ClientTypeForAccountsList;
		private AccountsDataGridUC	accountsTableInDeptWindowUC;
		private DataView			accountsListToShow;

		public DepartmentWindow(WindowID wid, BankActions ba)
		{
			InitializeComponent();
			InitializeView(wid, ba);
			InitializeClientsAndWindowTypes();
			ShowAccounts();
		}

		#region Инициализация обработчиков кнопок, вида, списков

		private void InitializeView(WindowID wid, BankActions ba)
		{
			BankTodayDate.Text = $"Сегодня {GoodBankTime.Today:dd.MM.yyyy}";

			// Прикручиваем банк с обработчиками всех действий над счетами
			BA = ba;
			this.wid = wid;

			// Вставляем нужные надписи в окошко департаментов
						 deptwinnametags = new WindowNameTags(wid);
								   Title = deptwinnametags.SystemWindowTitle;
						  MainTitle.Text = deptwinnametags.WindowHeader;
			WinMenu_SelectClient.Content = deptwinnametags.SelectClientTag;
			   WinMenu_AddClient.Content = deptwinnametags.AddClientTag;

			// Создаем вложенное окно(область) для показа списка клиентов. 
			// Вставляем нужные надписи
			clntag							= new ClientsViewNameTags(wid);
			clientsTableInDeptWindowUC		= new ClientsDataGridUC(clntag, BA);
			// Привязываем DataGrid UserControl к окну Department
			ClientsTableViewArea.Content	= clientsTableInDeptWindowUC;

			// Создаем вложенное окно(область) для показа списка счетов. Вставляем нужные надписи
			accountsTableInDeptWindowUC		= new AccountsDataGridUC();
			// Привязываем DataGrid UserControl к окну Department
			AccountsTableViewArea.Content	= accountsTableInDeptWindowUC;
		}

		/// <summary>
		/// Получает список клиентов для показа в окне департамента.
		/// Связывает этот список с вложенным окном Clients Data Grid User Control 
		/// </summary>
		private void InitializeClientsAndWindowTypes()
		{
			switch (wid)
			{
				case WindowID.DepartmentVIP:
					clientsListToShow = BA.Clients.GetClientsTable(ClientType.VIP);
					ClientTypeForAccountsList	= ClientType.VIP;
					addClientWID				= WindowID.AddClientVIP;
					break;
				case WindowID.DepartmentSIM:
					clientsListToShow = BA.Clients.GetClientsTable(ClientType.Simple);
					ClientTypeForAccountsList	= ClientType.Simple;
					addClientWID				= WindowID.AddClientSIM;
					break;
				case WindowID.DepartmentORG:
					clientsListToShow = BA.Clients.GetClientsTable(ClientType.Organization);
					ClientTypeForAccountsList	= ClientType.Organization;
					addClientWID				= WindowID.AddClientORG;
					break;
				case WindowID.DepartmentALL:
					clientsListToShow = BA.Clients.GetClientsTable(ClientType.All);
					ClientTypeForAccountsList	= ClientType.All;
					addClientWID				= WindowID.AddClientALL;
					break;
			}
			clientsTableInDeptWindowUC.SetClientsDataGridItemsSource(clientsListToShow);
			clientsTableInDeptWindowUC.SetClientsTotal(clientsListToShow.Count);
		}

		private void ShowAccounts()
		{
			// Получаем список счетов для показа в области счетов в окне департамента
			// и сумму денег по каждой категории счетов
			var accListAndTotals	= BA.Accounts.GetAccountsList(ClientTypeForAccountsList);
			accountsListToShow		= accListAndTotals.accountsViewTable; // DataView
			// Привязывает список счетов к обласи показа DataGrid UserControl
			accountsTableInDeptWindowUC.
				SetAccountsDataGridItemsSource(accountsListToShow, ClientTypeForAccountsList);
			
			accountsTableInDeptWindowUC.
				SetAccountsTotals(accListAndTotals.accountsViewTable.Count, 
								  accListAndTotals.totalSaving, 
								  accListAndTotals.totalDeposit, 
								  accListAndTotals.totalCredit);
		}

		#endregion

		private void WinMenu_SelectClient_Click(object sender, RoutedEventArgs e)
		{
			DataRowView client = clientsTableInDeptWindowUC.GetSelectedItem();
			if (client == null)
			{
				MessageBox.Show("Выберите клиента для показа");
				return;
			}

			// Создаём окно работы с клиентом
			ClientWindow clientWindow = new ClientWindow(BA, client);

			// Подписываемся на событие, возникающее, когда изменились данные клиента:
			// Подцепляем к этому событию обработчик - метод обновления данных клиента в списке на экране.
			// Этот метод содержится в UserControl ClientsList
			clientWindow.ClientDataChanged	+= clientsTableInDeptWindowUC.UpdateClientRowInView;
			clientWindow.NewAccountAdded	+= AddNewAccountToDataGrid;
			clientWindow.AccountDataChanged += UpdateAccountInDataGrid;
			clientWindow.ShowDialog();

			if (clientWindow.accountsNeedUpdate) ShowAccounts();
		}

		private void WinMenu_AddClient_Click(object sender, RoutedEventArgs e)
		{
			AddEditClientNameTags nameTags  = new AddEditClientNameTags(addClientWID);
			IClientDTO newClient = null;
			AddEditClientWindow addСlientWin = new AddEditClientWindow(nameTags, newClient);
			bool? result = addСlientWin.ShowDialog();
			
			if (result != true) return;
			// Добавляем нового клиента в базу в бэкэнде
			newClient = addСlientWin.newOrUpdatedClient;
			newClient.ID = BA.Clients.AddClient(newClient);

			// Добавляем нового клиента в список на экране
			AddNewClientToDataGrid(newClient);
		}

		/// <summary>
		/// Добавляет только что созданного клиента к списку на экране
		/// Это делается вместо получения заново всего списка клиентов одного типа
		/// </summary>
		/// <param name="addedClient"></param>
		private void AddNewClientToDataGrid(IClientDTO addedClient)
		{
			DataRowView newClient = clientsListToShow.AddNew();
			clientsTableInDeptWindowUC.UpdateClientRowInView(newClient, addedClient);
			clientsTableInDeptWindowUC.SetClientsTotal(clientsListToShow.Count);
		}

		private void WinMenu_SelectAccount_Click(object sender, RoutedEventArgs e)
		{
			DataRowView account = accountsTableInDeptWindowUC.GetSelectedItem();
			if (account == null)
			{
				MessageBox.Show("Выберите счет для показа");
				return;
			}
			AccountWindow accountWindow = new AccountWindow(BA, (int)account["AccID"]);
			accountWindow.ShowDialog();

			if (accountWindow.accountsNeedUpdate) ShowAccounts();
		}

		/// <summary>
		/// Добавляет только что созданный счёт к списку на экране
		/// Это делается вместо получения заново всего списка счетов одного типа
		/// </summary>
		/// <param name="addedClient"></param>
		private void AddNewAccountToDataGrid(IAccountDTO addedAccount)
		{
			accountsTableInDeptWindowUC.AddNewAccountToDataGrid(addedAccount);
		}

		private void UpdateAccountInDataGrid(DataRowView accRowInTable, IAccountDTO updatedAcc)
		{
			accountsTableInDeptWindowUC.UpdateAccountRowInDataGrid(accRowInTable, updatedAcc);
		}
	}


}
