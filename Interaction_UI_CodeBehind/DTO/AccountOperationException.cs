using Enumerables;
using System;

namespace DTO
{
	public class AccountOperationException : Exception
	{
		public ExceptionErrorCodes ErrorCode { get; set; }
		public AccountOperationException(ExceptionErrorCodes errCode)
			: base(Error.Message[(int)errCode])
		{
			ErrorCode = errCode;
		}
	}
}
