using System;
using static System.Console;
using System.Data.SqlClient;
using System.Configuration;
using Interfaces_Data;
using System.Collections.Generic;
using System.Diagnostics;
using Transaction;
using SQL;

namespace BankInside
{
	public partial class GoodBank
	{
		private string				masterCS	= default;
		private string				GoodBankCS	= default;
		private string				gbdbName	= default;
		private UnifiedAccount		AccountAction;
		private TransactionAction	TransactionAction;

		/// <summary>
		/// Инициализирует рабочую базу данных. 
		/// 1. Сначала проверяет конфиг файл на наличие базы
		/// Если конфиг файл содержит строку GoodBank, то берёт из неё имя базы
		/// Если конфиг файл не содержит строки для GoodBank, то именем базы становится GoodBank
		/// В конфиг файл записывается строка GoodBank
		/// 
		/// 2. Проверка наличия базы
		/// Если нет - создаёт эту базу
		/// 
		/// 3. Создаёт SqlConnection для этой базы
		/// </summary>
		public GoodBank()
		{
			masterCS = GetMasterConnectionString();
			GoodBankCS = GetGoodBankConnectionString();
			gbdbName = ExtractDBname(GoodBankCS);

			if (!DoesDBExist(gbdbName)) CreateDB(gbdbName);
			// Checks if the db has all tables
			// If some table is missing creates it
			CheckThenCreateTables();
			SetupSqlDataAdaptersAndStoredProceduresForTables();

			AccountAction			= new UnifiedAccount();
			TransactionAction		= new TransactionAction(GoodBankCS, ds);
			AccountAction.WriteLog += TransactionAction.WriteLog;
		}

		/// <summary>
		/// Reads app.config and gets "GoodBank" configuration string
		/// If 'GoodBank' property does not exist, then it adds 
		/// </summary>
		/// <returns>
		/// Configuration string associated with "GoodBank",
		/// null otherwise
		/// </returns>
		private string GetGoodBankConnectionString()
		{
			string GoodBankCS = null;
			// here we set the users connection string for the database
			// Get the application configuration file.
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			// Get the connection strings section.
			ConnectionStringsSection csSection = config.ConnectionStrings;
			foreach (ConnectionStringSettings cs in csSection.ConnectionStrings)
				if (cs.Name == "GoodBank")
				{
					GoodBankCS = cs.ConnectionString;
					break;
				}
			if (GoodBankCS == null)
				GoodBankCS = AddGoodBankCStoConfigFile();
			return GoodBankCS;
		}

		/// <summary>
		/// Creates GoodBank entry in configuration file
		/// </summary>
		/// <returns>Configuration string for GoodBank</returns>
		private string AddGoodBankCStoConfigFile()
		{
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			ConnectionStringsSection csSection = config.ConnectionStrings;

			var gbcs = new ConnectionStringSettings()
			{
				Name = "GoodBank",
				ConnectionString = new SqlConnectionStringBuilder()
				{
					DataSource = @"(localdb)\MSSQLLocalDB",
					InitialCatalog = "GoodBank",
					IntegratedSecurity = true,
					Pooling = true
				}.ConnectionString,
				ProviderName = "System.Data.SqlClient"
			};
			csSection.ConnectionStrings.Add(gbcs);

			config.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection(csSection.SectionInformation.Name);

			return gbcs.ConnectionString;
		}

		/// <summary>
		/// Removes mistaken 'GoodBank' connection string from App.config file
		/// and adds 'GoodBank' connection string with 'GoodBank' database name
		/// </summary>
		/// <returns></returns>
		private string CorrectGoodBankCSinConfigFile()
		{
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			ConnectionStringsSection csSection = config.ConnectionStrings;
			ConnectionStringSettings wrongGB = default;
			foreach (ConnectionStringSettings css in csSection.ConnectionStrings)
				if (css.Name == "GoodBank")
				{
					wrongGB = css;
					break;
				}
			csSection.ConnectionStrings.Remove(wrongGB);

			config.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection(csSection.SectionInformation.Name);

			return AddGoodBankCStoConfigFile();
		}

		private string ExtractDBname(string GoodBankCS)
		{
			SqlConnectionStringBuilder strBuilder = default;
			// Check if configuration string is correct
			// if not, save GoodBank config string
			try
			{
				strBuilder = new SqlConnectionStringBuilder(GoodBankCS);
			}
			catch
			{
				GoodBankCS = CorrectGoodBankCSinConfigFile();
				strBuilder = new SqlConnectionStringBuilder(GoodBankCS);
			}
			return strBuilder.InitialCatalog;
		}
		/// <summary>
		/// Builds connection string for (localdb)\MSSQLLocalDB server, 'master' database
		/// </summary>
		/// <returns>Connection string</returns>
		private string GetMasterConnectionString()
		{
			return new SqlConnectionStringBuilder()
			{
				DataSource = @"(localdb)\MSSQLLocalDB",
				InitialCatalog = "master",
				IntegratedSecurity = true,
				Pooling = true
			}.ConnectionString;
		}

		/// <summary>
		/// Создает соединение для первоначальной связи с сервером (localdb)\MSSQLLocalDB,
		/// чтобы в дальнейшем узнать, какие вообще есть на нём базы.
		/// </summary>
		/// <returns>Соединение SqlConnection с базой master сервера</returns>
		private SqlConnection SetMasterConnection()
		{
			return new SqlConnection(masterCS);
		}

		/// <summary>
		/// Создает соединение для связи с базой банка,
		/// </summary>
		/// <returns>Соединение SqlConnection с базой банка</returns>
		private SqlConnection SetGoodBankConnection()
		{
			return new SqlConnection(GoodBankCS);
		}

		/// <summary>
		/// Checks if a database with the specified name already exists in the server
		/// Server is (localdb)\MSSQLLocalDB
		/// </summary>
		/// <param name="dbName">Database name</param>
		/// <returns>true if a database with specified name exists, false otherwise</returns>
		private bool DoesDBExist(string dbName)
		{
			bool result = false;
			using (SqlConnection masterConn = SetMasterConnection())
			{
				masterConn.Open();
				string commandStr = @"SELECT database_id, [name] FROM master.sys.databases WHERE database_id > 4;";
				SqlCommand sqlCommand = new SqlCommand(commandStr, masterConn);
				SqlDataReader dbList;
				try
				{
					dbList = sqlCommand.ExecuteReader();
					while (dbList.Read())
						if ((string)dbList["name"] == dbName)
						{
							result = true;
							break;              // need to close connection first
						}
				}
				catch (Exception ex)
				{
					WriteLine();
					WriteLine("Checking if DB exists. Catch block!");
					WriteLine($"Exception = {ex.Message}");
				}
			}
			return result;
		}

		/// <summary>
		/// Creates on the (localdb)\MSSQLLocalDB server a database with the specified name
		/// Does not check, if such database exists. This has to be done prior to db creation
		/// </summary>
		/// <param name="dbName"></param>
		private void CreateDB(string dbName)
		{
			using (SqlConnection masterConn = SetMasterConnection())
			{
				masterConn.Open();
				string cmdLine = $"CREATE DATABASE {dbName};";
				SqlCommand command = new SqlCommand(cmdLine, masterConn);
				try
				{
					command.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					WriteLine();
					WriteLine($"CREATE DATABASE {dbName}; Catch block!!!");
					WriteLine("Exception = " + ex.Message);
				}
			}
		}

		/// <summary>
		/// Проверяет, существуют ли в базе таблицы Cliet
		/// </summary>
		/// <param name="gbCS"></param>
		/// <returns></returns>
		private void CheckThenCreateTables()
		{
			List<string> tablesList = new List<string>();
			using (SqlConnection gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string cmdText = @$"USE {gbdbName};"
					+ $"SELECT TABLE_NAME FROM [{gbdbName}].INFORMATION_SCHEMA.TABLES"
					;
				SqlCommand cmd = new SqlCommand(cmdText, gbConn);
				SqlDataReader sqlTablesList;
				try
				{
					sqlTablesList = cmd.ExecuteReader();
					while (sqlTablesList.Read())
						tablesList.Add((string)sqlTablesList[0]);
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Exception " + ex.Message);
				}
			}
			// Если база не содержит ни одной таблицы - создать
			if (tablesList.Count == 0)
			{
				foreach (var keyValuePair in SqlTables.definition)
					CreateTable(keyValuePair.Key, keyValuePair.Value);
			}
			else
			// Если уже есть таблицы, то создать недостающие
			{
				foreach (var nd in SqlTables.definition)
					if (!tablesList.Contains(nd.Key)) CreateTable(nd.Key, SqlTables.definition[nd.Key]);
			}
		}

		private void CreateTable(string tableName, string script)
		{
			using (SqlConnection gbConn = SetGoodBankConnection())
			{
				gbConn.Open();
				string cmdLine = script;
				SqlCommand command = new SqlCommand(cmdLine, gbConn);
				try
				{
					command.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					WriteLine();
					WriteLine($"CREATE TABLE [dbo].[{tableName}]; Catch block!!!");
					WriteLine("Exception = " + ex.Message);
				}
			}
		}
	}
}
