using System;

namespace BankTime
{
	public class GoodBankTime
	{
		public static DateTime BankFoundationDay = new DateTime(1992, 1, 1);
		public static DateTime Today = DateTime.Now;
		public static DateTime GetBanksTodayWithCurrentTime()
		{
			return new DateTime(Today.Year, Today.Month, Today.Day,
				DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
		}
	}
}
