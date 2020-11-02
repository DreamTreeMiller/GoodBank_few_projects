using Interfaces_Data;
using System;

namespace Search_Engine
{
	public class FirstNameComparator
	{
		string objectToFind;

		public FirstNameComparator(string value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			if (sourceP is IClientVIP)
				flag = (sourceP as IClientVIP).FirstName.Contains(objectToFind);
			if (sourceP is IClientSimple)
				flag = (sourceP as IClientSimple).FirstName.Contains(objectToFind);
			return flag;
		}
	}

	public class MiddleNameComparator
	{
		string objectToFind;

		public MiddleNameComparator(string value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			if (sourceP is IClientVIP)
				flag = (sourceP as IClientVIP).MiddleName.Contains(objectToFind);
			if (sourceP is IClientSimple)
				flag = (sourceP as IClientSimple).MiddleName.Contains(objectToFind);
			return flag;
		}
	}

	public class LastNameComparator
	{
		string objectToFind;

		public LastNameComparator(string value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			if (sourceP is IClientVIP)
				flag = (sourceP as IClientVIP).LastName.Contains(objectToFind);
			if (sourceP is IClientSimple)
				flag = (sourceP as IClientSimple).LastName.Contains(objectToFind);
			return flag;
		}
	}

	public class StartDateComparator
	{
		DateTime objectToFind;

		public StartDateComparator(DateTime value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			flag = objectToFind <= (sourceP as IClientSimple).BirthDate;
			return flag;
		}
	}

	public class EndDateComparator
	{
		DateTime objectToFind;

		public EndDateComparator(DateTime value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			if (sourceP is IClientVIP)
				flag = (sourceP as IClientVIP).BirthDate <= objectToFind;
			if (sourceP is IClientSimple)
				flag = (sourceP as IClientSimple).BirthDate <= objectToFind;
			return flag;
		}
	}

	public class PassportNumberComparator
	{
		string objectToFind;

		public PassportNumberComparator(string value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			if (sourceP is IClientVIP)
				flag = (sourceP as IClientVIP).PassportNumber.Contains(objectToFind);
			if (sourceP is IClientSimple)
				flag = (sourceP as IClientSimple).PassportNumber.Contains(objectToFind);
			return flag;
		}
	}
}
