using Enumerables;
using System;

namespace Interfaces_Data
{
	public interface IAccount
	{
		/// <summary>
		/// Тип клиента, которому принадлежит счет.
		/// Это избыточное поле, но благодаря ему делается всего один проход по базе 
		/// при показе счетов одного типа клиентов
		/// </summary>
		ClientType		ClientType		{ get; set; }

		/// <summary>
		/// ID владельца счета. 
		/// Это избыточное поле, но так быстрее найти данные владельца 
		/// при показе счетов одного типа клиентов
		/// </summary>
		int			ClientID		{ get; set; }

		/// <summary>
		/// Тип счета текущий, вклад или кредит
		/// </summary>
		AccountType		AccType			{ get; }

		/// <summary>
		/// Уникальный ID счёта - используем для базы
		/// </summary>
		int			AccID				{ get; }

		/// <summary>
		/// Уникальный номер счёта. 
		/// Числовая часть совпадает с ID. 
		/// Есть префикс, указывающий тип счета
		/// </summary>
		string			AccountNumber	{ get; set; }

		/// <summary>
		/// Баланс счёта. Для разных типов разный
		/// Текущий - остаток
		/// Вклад	- сумма вклада
		/// Кредит	- сумма долга
		/// </summary>
		double			 Balance			{ get; set; }

		/// <summary>
		/// Процент. 0 для текущего, прирорст для вклада, минус для долга
		/// </summary>
		double			 Interest		{ get; set; }

		/// <summary>
		/// С капитализацией или без
		/// </summary>
		bool			Compounding		{ get; set; }

		/// <summary>
		/// Дата открытия счета
		/// </summary>
		DateTime		Opened			{ get; set; }

		/// <summary>
		/// Количество месяцев, на который открыт вклад, выдан кредит.
		/// 0 - бессрочно
		/// </summary>
		int				Duration		{ get; set; }

		/// <summary>
		/// Количество месяцев, прошедших с открытия вклада
		/// </summary>
		int				MonthsElapsed	{ get; set; }

		DateTime?		EndDate			{ get; }

		/// <summary>
		/// Дата закрытия счета. Только для закрытых
		/// </summary>
		DateTime?		Closed			{ get; set; }

		/// <summary>
		/// Пополняемый счет или нет
		/// </summary>
		bool			Topupable		{ get; set; }

		/// <summary>
		/// С правом частичного снятия или нет
		/// </summary>
		bool	WithdrawalAllowed		{ get; set; }

		/// <summary>
		/// Период пересчета процентов - ежедневно, ежемесячно, ежегодно, один раз в конце
		/// </summary>
		RecalcPeriod	RecalcPeriod	{ get; set; }

		bool IsBlocked { get; set; }

		/// <summary>
		/// Пополнение счета наличкой
		/// </summary>
		void TopUpCash(double amount);

		/// <summary>
		/// Снятие налички со счета
		/// </summary>
		double WithdrawCash(double amount);

		/// <summary>
		/// Получение перевода на счет денег со счета-источника
		/// </summary>
		/// <param name="wireAmount"></param>
		void ReceiveFromAccount(IAccount sourceAcc, double wireAmount);

		/// <summary>
		/// Перевод средств со счета на счет-получатель
		/// </summary>
		/// <param name="destAcc">Счет-получатель</param>
		/// <param name="wireAmount">Сумма перевода</param>
		void SendToAccount(IAccount destAcc, double wireAmount);

		double CloseAccount();
	}
}
