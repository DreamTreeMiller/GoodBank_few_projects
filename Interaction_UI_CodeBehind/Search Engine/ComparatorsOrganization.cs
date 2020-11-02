using Interfaces_Data;
using System;

namespace Search_Engine
{
	public class OrgNameComparator
	{
		string objectToFind;

		public OrgNameComparator(string value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			flag = (sourceP as IClientOrg).OrgName.Contains(objectToFind);
			return flag;
		}
	}


	public class DirectorFirstNameComparator
	{
		string objectToFind;

		public DirectorFirstNameComparator(string value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			flag = (sourceP as IClientOrg).DirectorFirstName.Contains(objectToFind);
			return flag;
		}
	}

	public class DirectorMiddleNameComparator
	{
		string objectToFind;

		public DirectorMiddleNameComparator(string value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			flag = (sourceP as IClientOrg).DirectorMiddleName.Contains(objectToFind);
			return flag;
		}
	}

	public class DirectorLastNameComparator
	{
		string objectToFind;

		public DirectorLastNameComparator(string value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			flag = (sourceP as IClientOrg).DirectorLastName.Contains(objectToFind);
			return flag;
		}
	}

	public class RegistrationStartDateComparator
	{
		DateTime objectToFind;

		public RegistrationStartDateComparator(DateTime value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			flag = objectToFind <= (sourceP as IClientOrg).RegistrationDate;
			return flag;
		}
	}

	public class RegistrationEndDateComparator
	{
		DateTime objectToFind;

		public RegistrationEndDateComparator(DateTime value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			flag = (sourceP as IClientOrg).RegistrationDate <= objectToFind;
			return flag;
		}
	}

	public class TINComparator
	{
		string objectToFind;

		public TINComparator(string value) { objectToFind = value; }

		public bool Compare(IClient sourceP, ref bool flag)
		{
			if (!flag) return false;
			flag = (sourceP as IClientOrg).TIN.Contains(objectToFind);
			return flag;
		}
	}

}
