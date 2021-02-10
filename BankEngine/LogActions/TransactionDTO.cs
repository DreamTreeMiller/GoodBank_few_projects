using Enumerables;
using Interfaces_Data;
using System;

namespace Log
{
	public class TransactionDTO
	{
		#region Статический генератор уникального ID

		/// <summary>
		/// Текущий ID счета
		/// </summary>
		private static int staticID;

		/// <summary>
		/// Статический конструктор. Обнуляет счетчик ID
		/// </summary>
		static TransactionDTO()
		{
			staticID = 0;
		}

		/// <summary>
		/// Герерирует следующий ID
		/// </summary>
		/// <returns>New unique ID</returns>
		private static int NextID()
		{
			staticID++;
			return staticID;
		}

		#endregion

		/// <summary>
		/// Уникальный ID транзакции
		/// </summary>
		public int				TransactionID		{ get; }

		/// <summary>
		/// Счет, над которым совершено действие
		/// </summary>
		public int				TransactionAccountID { get; }

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
		public decimal			 Amount				{ get; }

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
		public TransactionDTO( int				senderAccID,
							DateTime		dt,
							string			sourceAcc,
							string			destinationAcc,
							OperationType	opType,
							decimal			amount,
							string			comment)
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
