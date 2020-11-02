using Interfaces_Data;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Data_Grid_User_Controls
{
	/// <summary>
	/// Interaction logic for TransactionsGrid.xaml
	/// </summary>
	public partial class TransactionsLogUserControl : UserControl
	{
		public void SetTransactionsLogItemsSource(ObservableCollection<ITransaction> transactions)
		{
			TransactionsLog.ItemsSource = transactions;
		}
		public TransactionsLogUserControl()
		{
			InitializeComponent();
		}
	}
}
