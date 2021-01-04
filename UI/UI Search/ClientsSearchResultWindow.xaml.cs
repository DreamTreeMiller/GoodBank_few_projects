using Binding_UI_CodeBehind;
using Interfaces_Data;
using Window_Name_Tags;
using Client_Window;
using Data_Grid_User_Controls;
using System.Collections.ObjectModel;
using System.Windows;

namespace UI_Search
{
	/// <summary>
	/// Interaction logic for PersonsSearchResultWindow.xaml
	/// </summary>
	public partial class ClientsSearchResultWindow : Window
	{
		private BankActions BA;
		private ClientsList clientsListUserControl;

		public void SetMainTitle(string text)
		{
			MainTitle.Text = text;
		}

		public ClientsSearchResultWindow(
			BankActions ba, 
			ObservableCollection<IClientDTO> searchResult, 
			WindowID searchType)
		{
			InitializeComponent();
			InitializeBankActionsAndClientsListUserControl(ba, searchResult, searchType);
		}

		private void InitializeBankActionsAndClientsListUserControl(
			BankActions ba, 
			ObservableCollection<IClientDTO> searchResult,
			WindowID searchType)
		{
			BA = ba;
			ClientsViewNameTags tags = new ClientsViewNameTags(searchType);
			clientsListUserControl = new ClientsList(tags, BA);
			//clientsListUserControl.SetClientsDataGridItemsSource(searchResult);
			clientsListUserControl.SetClientsTotal(searchResult.Count);
			ClientsList.Content = clientsListUserControl;
		}
		private void btn_SelectClient_Click(object sender, RoutedEventArgs e)
		{
			IClientDTO client = clientsListUserControl.GetSelectedItem();
			if (client == null)
			{
				MessageBox.Show("Выберите клиента для показа");
				return;
			}
			ClientWindow clientWindow = new ClientWindow(BA, client);
			clientWindow.ShowDialog();
		}
	}
}
