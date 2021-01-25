using Enumerables;
using System;

namespace Interfaces_Data
{
	public interface IAccountDTO 
	{
		ClientType	ClientType		{ get; set; }
		string		ClientTypeTag	{ get; set; }
		string		ClientName		{ get; set; }
		int			ClientID		{ get; set; }
		AccountType	AccType			{ get; set; }
		int			AccID			{ get; set; }
		string		AccountNumber	{ get; set; }
		decimal		Balance			{ get; set; }
		decimal		CurrentAmount	{ get; }
		decimal		DepositAmount	{ get; }
		decimal		DebtAmount		{ get; }
		double		Interest		{ get; set; }

		/// <summary>
		/// С капитализацией или без
		/// </summary>
		bool		Compounding		{ get; set; }

		/// <summary>
		/// ID счета, куда перечислять проценты.
		/// При капитализации, совпадает с ИД счета депозита
		/// </summary>
		int			InterestAccumulationAccID		{ get; set; }

		string		InterestAccumulationAccNum		{ get; set; }

		decimal		AccumulatedInterest				{ get; set; }

		DateTime	Opened				{ get; set; }

		/// <summary>
		/// Количество месяцев, на который открыт вклад, выдан кредит.
		/// 0 - бессрочно
		/// </summary>
		int			Duration			{ get; set; }

		/// <summary>
		/// Количество месяцев, прошедших с открытия вклада
		/// </summary>
		int			MonthsElapsed		{ get; set; }

		/// <summary>
		/// Дата окончания вклада/кредита. 
		/// null - бессрочно
		/// </summary>
		DateTime?	EndDate				{ get; }

		/// <summary>
		/// Закончился ли период вклада/кредита, чтобы больше не пересчитывать проценты
		/// </summary>
		bool		StopRecalculate		{ get; set; }
		DateTime?	Closed				{ get; set; }

		/// <summary>
		/// Пополняемый счет или нет
		/// </summary>
		bool		Topupable			{ get; set; }

		/// <summary>
		/// С правом частичного снятия или нет
		/// </summary>
		bool		WithdrawalAllowed	{ get; set; }

		/// <summary>
		/// Период пересчета процентов - ежедневно, ежемесячно, ежегодно, один раз в конце
		/// </summary>
		RecalcPeriod RecalcPeriod		{ get; set; }

		int			NumberOfTopUpsInDay { get; set; }
		bool		IsBlocked			{ get; set; }

	}
}
