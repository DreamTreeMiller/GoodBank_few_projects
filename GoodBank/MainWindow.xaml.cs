using Interfaces_Data;
using Binding_UI_CodeBehind;
using Department_Window;
using System.Windows;
using BankTime;
using Window_Name_Tags;
using System.Collections.ObjectModel;
using Imitation;
using Generate_Clients_and_Accounts;
using UI_Search;
using System.Threading;

namespace GoodBankNS
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private	BankActions	BA;
		public	string		BankFoundationDay = $"Основан {GoodBankTime.BankFoundationDay:D}";

		public MainWindow()
		{
			InitializeComponent();
			InitializeBank();
			InitializeWelcomeScreenMessages();
			
		}

		private void InitializeBank()
		{
			BA = new BankActions();
		}

		private void InitializeWelcomeScreenMessages()
		{
			BankFoundationDayMessage.Text = BankFoundationDay;
			BankTodayDate.Text = $"Сегодня {GoodBankTime.Today:dd MMMM yyyy} г.";
		}

		#region Client type select menu handler

		private void VipClientsDeptButton_Click(object sender, RoutedEventArgs e)
		{
			DepartmentWindow vipClientsWin = new DepartmentWindow(WindowID.DepartmentVIP, BA);
			vipClientsWin.ShowDialog();
		}

		private void SimpleClientsDeptButton_Click(object sender, RoutedEventArgs e)
		{
			DepartmentWindow simpleClientsWin = new DepartmentWindow(WindowID.DepartmentSIM, BA);
			simpleClientsWin.ShowDialog();
		}

		private void OrgClientsDeptButton_Click(object sender, RoutedEventArgs e)
		{
			DepartmentWindow orgClientsWin = new DepartmentWindow(WindowID.DepartmentORG, BA);
			orgClientsWin.ShowDialog();
		}

		#endregion

		#region Bank manager button handler

		private void BankManagerButton_Click(object sender, RoutedEventArgs e)
		{
			DepartmentWindow allClientsWin = new DepartmentWindow(WindowID.DepartmentALL, BA);
			allClientsWin.ShowDialog();
		}
		#endregion

		#region Time machine button handler

		private void TimeMachineButton_Click(object sender, RoutedEventArgs e)
		{
			BA.Accounts.AddOneMonth();
			BankTodayDate.Text = $"Сегодня {GoodBankTime.Today:dd MMMM yyyy} г.";
			MessageBox.Show("Время в мире, где существует банк, ушло на месяц вперёд.\n"
						  + "Пересчитаны проценты на всех счетах.");

		}
		#endregion

		#region Search menu handler

		private void SearchPeopleButton_Click(object sender, RoutedEventArgs e)
		{
			IndividualsSearchRequestWindow esriw = new IndividualsSearchRequestWindow();
			var result = esriw.ShowDialog();
			if (result != true) return;

			ObservableCollection<IClientDTO> searchResult = BA.Search.FindClients(esriw.CheckAllFields);
			ShowPersonsSearchResult(searchResult);
		}

		private void ShowPersonsSearchResult(ObservableCollection<IClientDTO> searchResult)
		{
			ClientsSearchResultWindow csrw = 
				new ClientsSearchResultWindow(BA, searchResult, WindowID.SearchResultPersons);
			csrw.SetMainTitle("РЕЗУЛЬТАТ ПОИСКА ВИП И ФИЗИКОВ");
			csrw.ShowDialog();
		}

		private void SearchOrgButton_Click(object sender, RoutedEventArgs e)
		{
			OrganizationsSearchRequestWindow osriw = new OrganizationsSearchRequestWindow();
			var result = osriw.ShowDialog();
			if (result != true) return;

			ObservableCollection<IClientDTO> searchResult = BA.Search.FindClients(osriw.CheckAllFields);
			ShowOrganizationsSearchResult(searchResult);
		}

		private void ShowOrganizationsSearchResult(ObservableCollection<IClientDTO> searchResult)
		{
			ClientsSearchResultWindow csrw = 
				new ClientsSearchResultWindow(BA, searchResult, WindowID.SearchResultOrganizations);
			csrw.SetMainTitle("РЕЗУЛЬТАТ ПОИСКА ЮРИКОВ");
			csrw.ShowDialog();
		}

		private void SearchAccountsButton_Click(object sender, RoutedEventArgs e)
		{

		}

		#endregion

		#region Data Source button handler

		private void DataSource_Click(object sender, RoutedEventArgs e)
		{
			var sdsw = new SelectDataSourceWindow();
			if (sdsw.ShowDialog() != true) return;

		}

		private void GenerateData_Click(object sender, RoutedEventArgs e)
		{
			var gw = new GenerateWindow();
			var result = gw.ShowDialog();
			if (result != true) return;

			Generate.Bank(BA, gw.vipClients, gw.simClients, gw.orgClients);
			MessageBox.Show("Клиенты и счета созданы!");
		}

		#endregion
	}

	class Bag
	{
		public int Vip { get; set; } = 10;
		public int Sim { get; set; } = 10;
		public int Org { get; set; } = 10;
		public Bag (int v, int s, int o) { Vip = v; Sim = s; Org = o; }
	}
}
