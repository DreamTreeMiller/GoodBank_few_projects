using System.Data;
using System.Windows.Controls;

namespace Data_Grid_User_Controls
{
	/// <summary>
	/// Interaction logic for TransactionsGrid.xaml
	/// </summary>
	public partial class TransactionsLogUserControl : UserControl
	{
		public void SetTransactionsLogItemsSource(DataView transactions)
		{
			TransactionsLog.ItemsSource = transactions;
		}
		public TransactionsLogUserControl()
		{
			InitializeComponent();
		}
	}
}
