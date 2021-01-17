using BankTime;
using Binding_UI_CodeBehind;
using Data_Grid_User_Controls;
using DTO;
using Enumerables;
using Interfaces_Data;
using System;
using System.Data;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Window_Name_Tags;

namespace Account_Windows
{
	/// <summary>
	/// Interaction logic for AccountWindow.xaml
	/// </summary>
	public partial class AccountWindow : Window, INotifyPropertyChanged
	{
		#region Account Fields in Window

		private int			AccID;
		private AccountType	accountType;
		private string		accountNumber;
		public	string		AccountNumber
		{
			get => accountNumber;
			set {  accountNumber = value; NotifyPropertyChanged(); }
		}

		private	decimal		balance;
		public	decimal		Balance
		{
			get => balance;
			set {  balance = value; NotifyPropertyChanged(); }
		}

		public	double		Interest	{ get; set; }

		public	DateTime	Opened		{ get; set; }

		public	DateTime?	EndDate		{ get; set; }

		private DateTime?	accClosed;
		public	DateTime?	AccClosed 
		{ 
			get => accClosed;
			set {  accClosed = value; NotifyPropertyChanged(); } 
		}

		private bool		topupable;
		public  bool		Topupable 
		{ 
			get => topupable; 
			set {  topupable = value; NotifyPropertyChanged(); }
		}

		private bool		withdrawalAllowed;
		public	bool		WithdrawalAllowed
		{ 
			get => withdrawalAllowed; 
			set {  withdrawalAllowed = value; NotifyPropertyChanged(); }
		}

		public RecalcPeriod RecalcPeriod { get; set; }

		public bool Compounding { get; set; }

		public string InterestAccumulationAccNum { get; set; }

		private decimal accumulatedInterest;
		public  decimal AccumulatedInterest
		{ 
			get => accumulatedInterest; 
			set {  accumulatedInterest = value; NotifyPropertyChanged(); }
		}

		private bool IsBlocked;

		#endregion

		BankActions BA;
		DataRow		clientRow;
		IClientDTO	client;

		public bool accountsNeedUpdate = false;
		public bool clientsNeedUpdate  = false;
		TransactionsLogUserControl transLogUC;

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public AccountWindow(BankActions ba, int accID)
		{
			InitializeComponent();
			InitializeClassScopeVars(ba, accID);
			InitializeAccountFieldLabelsAndVisibility();
			InitializeClientDetails();
			ShowAccountTransactionsLog();
		}

		private void InitializeClassScopeVars(BankActions ba, int accID)
		{
			BankTodayDate.Text = $"Сегодня {GoodBankTime.Today:dd.MM.yyyy} г.";
			BA = ba;
			IAccountDTO acc = BA.Accounts.GetAccountByID(accID);
			clientRow		= BA.Clients.GetClientByID((int)acc.ClientID);
			client			= new ClientDTO(clientRow);

			AccID						= acc.AccID;
			accountType					= acc.AccType;
			AccountNumber				= acc.AccountNumber;
			Balance						= acc.Balance;
			Interest					= acc.Interest;
			Opened						= acc.Opened;
			EndDate						= acc.EndDate;
			AccClosed					= acc.Closed;
			Topupable					= acc.Topupable;
			WithdrawalAllowed			= acc.WithdrawalAllowed;
			RecalcPeriod				= acc.RecalcPeriod;
			Compounding					= acc.Compounding;
			InterestAccumulationAccNum	= acc.InterestAccumulationAccNum;
			AccumulatedInterest			= acc.AccumulatedInterest;
			IsBlocked					= acc.IsBlocked;

			DataContext = this;
		}

		private void InitializeAccountFieldLabelsAndVisibility()
		{
			AccountWindowNameTags tags = new AccountWindowNameTags(accountType);
			Title							= tags.SystemWindowTitle;
			MainTitle.Text					= tags.WindowHeader;
			WithdrawCashButton.Visibility	= tags.WithdrawCashButtonVisibility;
			WireButton.Visibility			= tags.WireButtonVisibility;
			DepositPart.Visibility			= tags.DepositPartVisibility;

			// Без капитализации указываем счет для накопления процентов
			if(Compounding == false)
			{
				InterestAccumulationLine.Visibility = Visibility.Visible;
			}

		}

		private void InitializeClientDetails()
		{
			if (client.ClientType == ClientType.Organization)
			{
				OrganizationInfo.Visibility = Visibility.Visible;
				PersonalInfo.Visibility		= Visibility.Collapsed;
			}
			else
			{
				OrganizationInfo.Visibility = Visibility.Collapsed;
				PersonalInfo.Visibility		= Visibility.Visible;
			}
			ClientInfo.DataContext = client;
		}

		private void ShowAccountTransactionsLog()
		{
			transLogUC = new TransactionsLogUserControl();
			TransactionsGrid.Content = transLogUC;
			UpdateAccountTransactionsLog();
		}

		private void UpdateAccountTransactionsLog()
		{
			DataView accTransLog = BA.Log.GetAccountTransactionsLog(AccID);
			transLogUC.SetTransactionsLogItemsSource(accTransLog);
		}
		private void TopUpButton_Click(object sender, RoutedEventArgs e)
		{
			// Эти проверки мы осуществляем до самой операции
			// Поэтому не можем навесить исключения
			if (AccClosed != null)
			{
				MessageBox.Show($"Счет {AccountNumber} закрыт.");
				return;
			}

			if (IsBlocked)
			{
				MessageBox.Show($"Счет {AccountNumber} заблокирован.");
				return;
			}

			if (!Topupable)
			{
				MessageBox.Show("Пополнение невозможно!");
				return;
			}
			EnterCashAmountWindow cashWin = new EnterCashAmountWindow();
			var result = cashWin.ShowDialog();
			if (result != true) return;

			IAccountDTO updatedAcc = null;
			try
			{
				updatedAcc = BA.Accounts.TopUpCash(AccID, cashWin.amount);
			}
			catch (AccountOperationException ex) 
			when (ex.ErrorCode == ExceptionErrorCodes.AccountIsBlcoked)
			{
				MessageBox.Show($"Счет {AccountNumber} заблокирован.");

				// Т.к. мы вылетели по исключению, то нам не передался обновлённый счёт
				// и т.к. в Account Window мы используем копии полей/свойств счёта
				// то надо обновлять вручную
				Topupable			= false;
				WithdrawalAllowed	= false;
				IsBlocked			= true;
				UpdateAccountTransactionsLog();
				return;
			}

			Balance				= updatedAcc.Balance;
			accountsNeedUpdate	= true;
			UpdateAccountTransactionsLog();
		}

		private void WithdrawCashButton_Click(object sender, RoutedEventArgs e)
		{
			// Эти проверки мы осуществляем до того, 
			// как выводим окошко "введите сумму для снятия"
			// Поэтому не можем навесить исключения
			if (AccClosed != null)
			{
				MessageBox.Show($"Счет {AccountNumber} закрыт.");
				return;
			}

			if (IsBlocked)
			{
				MessageBox.Show($"Счет {AccountNumber} заблокирован.");
				return;
			}

			if (!WithdrawalAllowed)
			{
				MessageBox.Show("Снятие невозможно!");
				return;
			}

			EnterCashAmountWindow cashWin = new EnterCashAmountWindow();
			var result = cashWin.ShowDialog();
			if (result != true) return;

			IAccountDTO updatedAcc;

			try
			{
				updatedAcc = BA.Accounts.WithdrawCash(AccID, cashWin.amount);
			}
			catch(AccountOperationException ex) 
			when (ex.ErrorCode == ExceptionErrorCodes.NotEnoughMoneyOnAccount)
			{
				MessageBox.Show("Недостаточно средств для снятия!");
				return;
			}

			Balance = updatedAcc.Balance;
			accountsNeedUpdate = true;
			UpdateAccountTransactionsLog();
		}

		private void WireButton_Click(object sender, RoutedEventArgs e)
		{
			if (AccClosed != null)
			{
				MessageBox.Show($"Счет {AccountNumber} закрыт.");
				return;
			}

			if (IsBlocked)
			{
				MessageBox.Show($"Счет {AccountNumber} заблокирован.");
				return;
			}

			if (!WithdrawalAllowed)
			{
				MessageBox.Show("C данного счета нельзя снимать средства");
				return;
			}

			var topupableAccountsList = BA.Accounts.GetTopupableAccountsToWireFrom(AccID);
			EnterAmountAndAccountWindow eaawin = new EnterAmountAndAccountWindow(topupableAccountsList);
			var result = eaawin.ShowDialog();
			if (result != true) return;

			decimal wireAmount = eaawin.amount;
			int     destAccID = eaawin.destinationAccID;

			try
			{
				BA.Accounts.Wire(AccID, destAccID, wireAmount);
			}
			catch (AccountOperationException ex)
			when (ex.ErrorCode == ExceptionErrorCodes.NotEnoughMoneyOnAccount)
			{
				MessageBox.Show("Недостаточно средств для перевода");
				return;
			}
			catch(AccountOperationException ex)
			when (ex.ErrorCode == ExceptionErrorCodes.RecipientCannotReceiveWire)
			{
				MessageBox.Show("Получатель не может принимать средства");
				return;
			}

			Balance -= wireAmount;
			MessageBox.Show($"Сумма {wireAmount:N2} руб. успешно переведена");
			accountsNeedUpdate = true;
			UpdateAccountTransactionsLog();
		}

		private void CloseAccountButton_Click(object sender, RoutedEventArgs e)
		{
			decimal accumulatedAmount;
			IAccountDTO closedAcc;
			try
			{
				closedAcc = BA.Accounts.CloseAccount(AccID, out accumulatedAmount);
			}
			catch(AccountOperationException ex)
			when (ex.ErrorCode == ExceptionErrorCodes.AccountIsClosed)
			{
				MessageBox.Show($"Счет {AccountNumber} уже закрыт.");
				return;
			}
			catch(AccountOperationException ex)
			when (ex.ErrorCode == ExceptionErrorCodes.AccountIsBlcoked)
			{
				MessageBox.Show($"Счет {AccountNumber} заблокирован.");
				return;
			}
			catch(AccountOperationException ex)
			when (ex.ErrorCode == ExceptionErrorCodes.CannotCloseAccountWithMinusBalance)
			{
				MessageBox.Show("Невозможно закрыть счет, на котором есть долг");
				return;
			}

			if (accumulatedAmount > 0)
			{
				MessageBox.Show($"Получите ваши денюшки\n в размере {accumulatedAmount:N2} руб.");
			}

			// Обновляем суммы, даты, флажки в окошке
			Balance				= (decimal)closedAcc.Balance;

			if (closedAcc is IAccountDeposit)
				AccumulatedInterest = (decimal)(closedAcc as IAccountDeposit).AccumulatedInterest;

			AccClosed			= closedAcc.Closed;
			Topupable			= closedAcc.Topupable;
			WithdrawalAllowed	= closedAcc.WithdrawalAllowed;

			MessageBox.Show($"Счет {AccountNumber} закрыт.\n"
				+ "Счет остается в системе, но его дальнейшее использование невозможно."
				);

			accountsNeedUpdate = true;
			clientsNeedUpdate  = true;
			UpdateAccountTransactionsLog();
		}
	}
}
