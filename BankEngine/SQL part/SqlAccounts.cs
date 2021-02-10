using System;
using System.Data;
using Log;
using Enumerables;
using Interfaces_Data;

namespace SQL
{
	public class SqlAccounts
	{
		private DataSet ds;
		private SqlDataAdapterAccountsParent  AccountsParent;
		private SqlDataAdapterSavingAccounts  SavingAccounts;
		private SqlDataAdapterDepositAccounts DepositAccounts;
		private SqlDataAdapterCreditAccounts  CreditAccounts;
		private LogAction			  TrAct;

		private DataTable accountsDT;
		private DataTable savingDT;
		private DataTable depositDT;
		private DataTable creditDT;

		private DateTime gbToday;
		private Func<int, IAccountDTO> GetAccountByID;

		/// <summary>
		/// Получение перевода на счет денег со счета-источника. 
		/// Увеличивает поле Balance в ДТО поле получатея. Базу не меняет!
		/// Записывает транзакцию
		/// </summary>
		/// <param name="senderAccNumber">Номер счета-источника</param>
		/// <param name="recipientAcc">ДТО счета получателя</param>
		/// <param name="wireAmount">сумма перевода</param>
		private Func<string, IAccountDTO, decimal, IAccountDTO> ReceiveFromAccount;

		/// <summary>
		/// Загружает всю базу в память, т.е. создаёт:
		/// таблицы счетов, заполняет таблицы данными из базы.
		/// Создаёт Data Adapters для каждой таблицы.
		/// </summary>
		/// <param name="gbCS">Строка подключения базы</param>
		/// <param name="transactionAction">Табилца транзакций и класс работы с ними</param>
		public SqlAccounts(string gbCS, LogAction transactionAction)
		{
			// создаём временное хранилище базы
			ds = new DataSet();

			// Создаём таблицы счетов, заполняем таблицы данными из базы
			// Создаём Data Adapters для каждой таблицы
			AccountsParent  = new SqlDataAdapterAccountsParent (gbCS, ds);
			SavingAccounts  = new SqlDataAdapterSavingAccounts (gbCS, ds);
			DepositAccounts = new SqlDataAdapterDepositAccounts(gbCS, ds);
			CreditAccounts  = new SqlDataAdapterCreditAccounts (gbCS, ds);
			TrAct			= transactionAction;

			accountsDT = AccountsParent.dTableAccountsParent;
			savingDT   = SavingAccounts.dTableSavingAccounts;
			depositDT  = DepositAccounts.dTableDepositAccounts;
			creditDT   = CreditAccounts.dTableCreditAccounts;
		}

		/// <summary>
		/// Освобождает память, занятую базой
		/// </summary>
		public void Dispose()
		{
			ds.Dispose();
		}

		public void RecalculateInterest(
			DateTime gbtoday, 
			Func<int, IAccountDTO> getAccountByID,
			Func<string, IAccountDTO, decimal, IAccountDTO> receiveFromAccount)
		{
			gbToday = gbtoday;
			GetAccountByID	   = getAccountByID;
			ReceiveFromAccount = receiveFromAccount;
			DataRow accParentRow;

			// Пересчет процентов для кредитов
			foreach (DataRow creditRow in creditDT.Rows)
			{
				accParentRow = accountsDT.Rows.Find(creditRow["id"]);
				RecalculateCredits(accParentRow, creditRow);
			}

			foreach (DataRow depositRow in depositDT.Rows)
			{
				accParentRow = accountsDT.Rows.Find(depositRow["id"]);

				// Если счёт заблокирован, берём следующий
				if ((bool)accParentRow["IsBlocked"]) continue;

				// Если счёт закрыт, берём следующий
				if (accParentRow["Closed"] != DBNull.Value) continue;

				// Разрешаем вносить средства на счёт
				accParentRow["NumberOfTopUpsInDay"] = 0;

				if ((bool)accParentRow["StopRecalculate"]) continue;

				decimal currInterest = RecalculateDeposits(accParentRow, depositRow);

				// Если процент по вкладу надо переводить на другой счёт
				int destAccID = (int)depositRow["InterestAccumulationAccID"];
				if (destAccID != 0)
				{
					IAccountDTO destAcc = GetAccountByID(destAccID);
					// Проверить, можно ли ещё переводить средства на счёт назначения
					// Может быть по каким-то причинам это сейчас стало невозможно
					if (destAcc.Topupable)
					{
						WireInterestToAccount(
							(int)accParentRow["AccID"], 
							(string)accParentRow["AccountNumber"],
							destAcc, currInterest);
					}
					else
					{
						// Перевести деньги на другой счёт уже нельзя
						// Добавим их на текущий баланс, НЕ на внутренний счёт!!!
						accParentRow["Balance"] = (decimal)accParentRow["Balance"] + currInterest;
					}
				}
			}

			// Сохранение обновлённых таблиц в базу
			AccountsParent. Update();
			DepositAccounts.Update();
			CreditAccounts. Update();
		}

		/// <summary>
		/// Пересчитвыает проценты по кредиту
		/// Все транзакции фиксируются в журнале
		/// </summary>
		/// <param name="parent">строка главной таблицы счетов</param>
		/// <param name="credit">соответствующая строка зависимой таблицы кредитных счетов</param>
		private void RecalculateCredits(DataRow parent, DataRow credit)
		{
			if ((bool)parent["IsBlocked"]) return;

			if (parent["Closed"] != DBNull.Value) return;

			parent["NumberOfTopUpsInDay"] = 0;

			// Пересчёт не нужен. 
			// Клиент должен пополнить счет до 0 и закрыть
			if ((int)parent["Duration"] == (int)parent["MonthsElapsed"]) return;

			int monthsElapsed = (int)parent["MonthsElapsed"];
			monthsElapsed++;
			parent["MonthsElapsed"] = monthsElapsed;

			decimal calculatedInterest	=
				(decimal)parent["Balance"] * (decimal)(double)parent["Interest"] / 12;
			credit["AccumulatedInterest"] = 
				(decimal)credit["AccumulatedInterest"] + calculatedInterest;
			parent["Balance"] = (decimal)parent["Balance"] + calculatedInterest;

			string destAccNum = (string)parent["AccountNumber"];
			TransactionDTO interestAccrualTransaction = new TransactionDTO(
				(int)parent["AccID"],
				gbToday,
				"",												// source account number
				destAccNum,										// destination account number
				OperationType.InterestAccrual,
				calculatedInterest,
				"Начисление процентов на счет " + destAccNum
				+ $" на сумму {calculatedInterest:N2} руб."
				);
			TrAct.WriteLog(interestAccrualTransaction);
		}

		/// <summary>
		/// Пересчитывает проценты по вкладу. Если надо, пересылает проценты на другой счёт. 
		/// Все транзакции фиксируются в журнале
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="deposit"></param>
		private decimal RecalculateDeposits(DataRow parent, DataRow deposit)
		{
			int monthsElapsed = (int)parent["MonthsElapsed"];
			monthsElapsed++;
			parent["MonthsElapsed"] = monthsElapsed;

			decimal calculatedInterest = 0;
			switch ((RecalcPeriod)(byte)parent["RecalcPeriod"])
			{
				// Счет, у которого начисление происходит всего один раз в конце периода
				case RecalcPeriod.AtTheEnd:
					if ((DateTime)parent["EndDate"] <= gbToday)
					{
						calculatedInterest = 
							(decimal)parent["Balance"] * (decimal)(double)parent["Interest"];
						deposit["AccumulatedInterest"] = calculatedInterest;

						// Срок истёк. Пополнять больше нельзя. 
						// Владельцу счета надо снять деньги и закрыть счет
						parent["Topupable"]			= false;
						parent["WithdrawalAllowed"] = true;
						parent["StopRecalculate"]		= true;

						UpdateLog(calculatedInterest, parent, deposit);

					}
					break;

				// Счет, у которого начисление происходит раз в год
				case RecalcPeriod.Annually:
					// Срок год или больше, и прошёл ровно год 
					if ((int)parent["MonthsElapsed"] % 12 == 0)
					{
						calculatedInterest =		(decimal)parent["Balance"] * 
											 (decimal)(double)parent["Interest"];
						deposit["AccumulatedInterest"] =
							(decimal)deposit["AccumulatedInterest"] + calculatedInterest;

						UpdateLog(calculatedInterest, parent, deposit);
					}

					if ((DateTime)parent["EndDate"] <= gbToday)
					{
						// Срок истёк. Пополнять больше нельзя. 
						// Владельцу счета надо снять деньги и закрыть счет
						parent["Topupable"]			= false;
						parent["WithdrawalAllowed"] = true;
						parent["StopRecalculate"] = true;

						// Если прошло меньше года после последнего начисления процента
						// то надо доначислить проценты 
						int MonthsRemained = (int)parent["MonthsElapsed"] % 12;
						if (MonthsRemained != 0)
						{
							calculatedInterest = (decimal)parent["Balance"] *
												 (decimal)(double)parent["Interest"] *
												 MonthsRemained / 12;
							deposit["AccumulatedInterest"] =
								(decimal)deposit["AccumulatedInterest"] + calculatedInterest;

							UpdateLog(calculatedInterest, parent, deposit);
						}
					}
					break;
				// Счет, у которого начисление происходит раз в месяц
				case RecalcPeriod.Monthly:
					calculatedInterest = (decimal)parent["Balance"] *
										 (decimal)(double)parent["Interest"] / 12;
					deposit["AccumulatedInterest"] =
						(decimal)deposit["AccumulatedInterest"] + calculatedInterest;

					if ((DateTime)parent["EndDate"] <= gbToday)
					{
						// Срок истёк. Пополнять больше нельзя. 
						// Владельцу счета надо снять деньги и закрыть счет
						parent["Topupable"]			= false;
						parent["WithdrawalAllowed"] = true;
						parent["StopRecalculate"]		= true;
					}

					UpdateLog(calculatedInterest, parent, deposit);
					break;
			}

			if ((bool)parent["Compounding"]) 
				parent["Balance"] = (decimal)parent["Balance"] + calculatedInterest;
			return calculatedInterest;
		}

		/// <summary>
		/// Вызывается методом ежемесячного обновления счетов
		/// </summary>
		/// <param name="sourceAcc"></param>
		/// <param name="recipientAcc"></param>
		/// <param name="accumulatedInterest"></param>
		private void WireInterestToAccount(int senderAccID, string senderAccNum, IAccountDTO recipientAcc, decimal accumulatedInterest)
		{
			SendInterestToAccount(senderAccID, recipientAcc, accumulatedInterest);
			// Обновить значение баланса счёта-получателя
			// Записать журнал транзакций
			recipientAcc = ReceiveFromAccount(senderAccNum, recipientAcc, accumulatedInterest);

			// Обновить баланс счета-получателя в таблицах 
			DataRow accParentRow = accountsDT.Rows.Find(recipientAcc.AccID);
			accParentRow["Balance"] = recipientAcc.Balance;

		}

		/// <summary>
		/// Переводит на другой счет накопленный процент.
		/// Это нельзя делать обычным переводом, т.к. деньги не списываются с основного счета
		/// </summary>
		/// <param name="destAcc"></param>
		/// <param name="accumulatedInterest"></param>
		public void SendInterestToAccount(int senderAccID, IAccountDTO recipientAcc, decimal accumulatedInterest)
		{
			TransactionDTO withdrawCashTransaction = new TransactionDTO(
				senderAccID,
				gbToday,
				"накопленный процент",
				recipientAcc.AccountNumber,
				OperationType.SendWireToAccount,
				accumulatedInterest,
				"Перевод накопленных процентов"
				+ " на счет " + recipientAcc.AccountNumber
				+ $" в размере {accumulatedInterest:N2} руб."
				);
			TrAct.WriteLog(withdrawCashTransaction);
		}

		private void UpdateLog(decimal calculatedInterest, DataRow parent, DataRow deposit)
		{
			string comment;

			if ((int)deposit["InterestAccumulationAccID"] == 0)
			{
				comment = "Начисление процентов на внутренний счет"
							+ $" на сумму {calculatedInterest:N2} руб.";
			}
			else
			{
				comment = $"Начисление процентов на сумму {calculatedInterest:N2} руб.";
			}


			TransactionDTO interestAccrualTransaction = new TransactionDTO(
				(int)parent["AccID"],
				gbToday,
				"",
				(string)deposit["InterestAccumulationAccNum"],
				OperationType.InterestAccrual,
				calculatedInterest,
				comment
				);

			TrAct.WriteLog(interestAccrualTransaction);
		}
	}
}
