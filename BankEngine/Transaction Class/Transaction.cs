using Enumerables;
using Interfaces_Data;
using System;

namespace Transaction_Class
{
	public class Transaction : ITransaction
	{
		#region Статический генератор уникального ID

		/// <summary>
		/// Текущий ID счета
		/// </summary>
		private static uint staticID;

		/// <summary>
		/// Статический конструктор. Обнуляет счетчик ID
		/// </summary>
		static Transaction()
		{
			staticID = 0;
		}

		/// <summary>
		/// Герерирует следующий ID
		/// </summary>
		/// <returns>New unique ID</returns>
		private static uint NextID()
		{
			staticID++;
			return staticID;
		}

		#endregion

		/// <summary>
		/// Уникальный ID транзакции
		/// </summary>
		public uint				TransactionID		{ get; }

		/// <summary>
		/// Счет, над которым совершено действие
		/// </summary>
		public uint TransactionAccountID { get; }

		/// <summary>
		/// Дата и время транзакции
		/// </summary>
		public DateTime			TransactionDateTime	{ get; }

		/// <summary>
		/// Счёт, над которым совершили транзакцию
		/// </summary>
		public string			SourceAccount		{ get; }

		/// <summary>
		/// Счёт, куда или откуда переводят деньги. Null - если операция с наличкой
		/// </summary>
		public string			DestinationAccount	{ get; }

		/// <summary>
		/// Тип операции - вклад или снятие налички, перевод с/на счёт
		/// </summary>
		public OperationType	OperationType		{ get; }

		/// <summary>
		/// Сумма операции. Плюс - вклад, минус - снятие
		/// </summary>
		public double			Amount				{ get; }

		/// <summary>
		/// Комментарий
		/// </summary>
		public string			Comment				{ get; }

		/// <summary>
		/// Конструктор для создания новой транзакции
		/// </summary>
		/// <param name="dt">Дата и время транзакции</param>
		/// <param name="sourceAcc">Счёт, над которым совершили транзакцию</param>
		/// <param name="destinationAcc">Счёт, куда или откуда переводят деньги</param>
		/// <param name="opType">Тип операции - вклад или снятие налички, перевод с/на счёт</param>
		/// <param name="amount">Сумма операции. Плюс - вклад, минус - снятие</param>
		/// <param name="interest">Процент в операции. 0 - текщий счет</param>
		/// <param name="comment">Комментарий</param>
		public Transaction(uint senderAccID,
							DateTime dt,
							string sourceAcc,
							string destinationAcc,
							OperationType opType,
							double amount,
							string comment)
		{
			TransactionID		 = NextID();
			TransactionAccountID = senderAccID;
			TransactionDateTime	 = dt;
			SourceAccount		 = sourceAcc;
			DestinationAccount	 = destinationAcc;
			OperationType		 = opType;
			Amount				 = amount;
			Comment				 = comment;
		}

	}
}
