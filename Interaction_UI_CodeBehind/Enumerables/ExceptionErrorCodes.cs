namespace Enumerables
{
	public enum ExceptionErrorCodes
	{
		AccountIsBlcoked,
		AccountIsClosed,
		CannotCloseAccountWithMinusBalance,
		TopUpIsNotAllowed,
		WithdrawalIsNotAllowed,
		NotEnoughMoneyOnAccount,
		RecipientCannotReceiveWire
	}

	public static class Error
	{
		/// <summary>
		/// Мужские имена
		/// </summary>
		public static string[] Message =
		{
			"Счет заблокирован",
			"Счет закрыт",
			"Пополнение невозможно",
			"Снятие невозможно",
			"Недостаточно средств на счете",
			"Получатель не может принять деньги на счет"
		};
	}
}
