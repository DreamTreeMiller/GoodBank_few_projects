using DTO;
using Enumerables;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Data_Grid_User_Controls
{
	[ValueConversion(typeof(Object[]), typeof(Visibility))]
	public class HideRowConverter : IMultiValueConverter
	{
		public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null) 
			{
				// AccType == AccountType.Saving && SavingAccountsCB.IsChecked == True
				if ((value[0] as DataRowView)["Closed"] == DBNull.Value &&
					(AccountType)(value[0] as DataRowView)["AccType"] == AccountType.Saving)
					return (bool)value[1] ? Visibility.Visible : Visibility.Collapsed;

				// AccType == AccountType.Deposit && DepositCB.IsChecked == True
				if ((value[0] as DataRowView)["Closed"] == DBNull.Value &&
					(AccountType)(value[0] as DataRowView)["AccType"] == AccountType.Deposit)
					return (bool)value[2] ? Visibility.Visible : Visibility.Collapsed;

				// AccType == AccountType.Credit && CreditCB.IsChecked == True
				if ((value[0] as DataRowView)["Closed"] == DBNull.Value &&
					(AccountType)(value[0] as DataRowView)["AccType"] == AccountType.Credit)
					return (bool)value[3] ? Visibility.Visible : Visibility.Collapsed;

				if ((value[0] as DataRowView)["Closed"] != DBNull.Value)
					return (bool)value[4] ? Visibility.Visible : Visibility.Collapsed;
			}
			return Visibility.Visible;

		}

		public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
