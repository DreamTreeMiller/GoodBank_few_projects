using System.Collections.Generic;

namespace BankInside
{
	public partial class GoodBank
	{
		private Dictionary<string, string> tables = new Dictionary<string, string>()
		{{"ClientsMain", @"
CREATE TABLE [dbo].[ClientsMain] (
	[ID]						INT			IDENTITY(1,1)	NOT NULL PRIMARY KEY,
	[Telephone]					NVARCHAR (30)	DEFAULT ''	NOT NULL,
	[Email]						NVARCHAR (128)	DEFAULT ''	NOT NULL,
	[Address]					NVARCHAR (256)	DEFAULT ''	NOT NULL,
	[NumberOfSavingAccounts]	INT				DEFAULT 0	NOT NULL,
	[NumberOfDeposits]			INT				DEFAULT 0	NOT NULL,
	[NumberOfCredits]			INT				DEFAULT 0	NOT NULL,
	[NumberOfClosedAccounts]	INT				DEFAULT 0	NOT NULL
	);
		"},

		 {"VIPclients", @"
CREATE TABLE [dbo].[VIPclients] (
	[id]				INT							NOT NULL PRIMARY KEY,
	FOREIGN KEY ([id]) REFERENCES [dbo].[ClientsMain]([ID]) ON DELETE CASCADE,
	[FirstName]			NVARCHAR (50)	DEFAULT ''	NOT NULL,
	[MiddleName]		NVARCHAR (50)	DEFAULT ''	NOT NULL,
	[LastName]			NVARCHAR (50)	DEFAULT ''	NOT NULL,
	[PassportNumber]	NVARCHAR (11)	DEFAULT ''	NOT NULL,
	[BirthDate]			DATE						NOT NULL
	);
		"},

		 {"SIMclients", @"
CREATE TABLE [dbo].[SIMclients] (
	[id]				INT							NOT NULL PRIMARY KEY,
	FOREIGN KEY ([id]) REFERENCES [dbo].[ClientsMain]([ID]) ON DELETE CASCADE,
	[FirstName]			NVARCHAR (50)	DEFAULT ''	NOT NULL,
	[MiddleName]		NVARCHAR (50)	DEFAULT ''	NOT NULL,
	[LastName]			NVARCHAR (50)	DEFAULT ''	NOT NULL,
	[PassportNumber]	NVARCHAR (11)	DEFAULT ''	NOT NULL,
	[BirthDate]			DATE						NOT NULL
);"
		 },
		 {"ORGclients", @"
CREATE TABLE [dbo].[ORGclients] (
	[id]				INT							NOT NULL PRIMARY KEY,	
	FOREIGN KEY ([id]) REFERENCES [dbo].[ClientsMain]([ID]) ON DELETE CASCADE,
	[OrgName]			NVARCHAR (256)	DEFAULT ''	NOT NULL,
	[DirectorFirstName]	NVARCHAR (50)	DEFAULT ''	NOT NULL,
	[DirectorMiddleName]NVARCHAR (50)	DEFAULT ''	NOT NULL,
	[DirectorLastName]	NVARCHAR (50)	DEFAULT ''	NOT NULL,
	[TIN]				NVARCHAR (10)	DEFAULT ''	NOT NULL,
	[RegistrationDate]	DATE						NOT NULL
);"
		 },

		 {"AccountsParent", @"
CREATE TABLE [dbo].[AccountsParent] (
	[AccID]				INT			IDENTITY (1, 1) NOT NULL PRIMARY KEY,	-- уникальный ид счета
	[AccType]			TINYINT			NOT NULL,	-- Account type
													-- 0 - Saving,
													-- 1 - Deposit,
													-- 2 - Credit,
													-- 3 - Total 
	-- [ClientType]		TINYINT 
	-- I feel ClientType field is not necessay. We can just generate it for output
	-- and if we need, then it's better to use just numbers with comments, like this 0 --VIP, 1 --SIM, 2 --ORG

	[ClientID]			INT				NOT NULL,	-- ID клиента
	[AccountNumber]		NVARCHAR (15)	NOT NULL,		
	[Balance]			MONEY DEFAULT 0	NOT NULL,			
	[Interest]			DECIMAL (4,2)	NOT NULL,
	[Compounding]		BIT				NOT NULL,	-- с капитализацией или без 
	[Opened]			DATE			NOT NULL,	-- дата открытия счета 
	[Duration]			INT				NOT NULL,	-- Количество месяцев, на который открыт вклад, выдан кредит.  
	[MonthsElapsed]		INT				NOT NULL,	-- Количество месяцев, прошедших с открытия вклада 
	[EndDate]			DATE,						-- Дата окончания вклада/кредита. 
													-- null - бессрочно 
	[Closed]			DATE,						-- Дата закрытия счета. Только для закрытых. 
													-- Если счет открыт, то равен null			 
	[Topupable]			BIT				NOT NULL,	-- Пополняемый счет или нет. У закрытого счета - (0) false 
	[WithdrawalAllowed]	BIT				NOT NULL,	-- С правом частичного снятия или нет. У закрытого счета - false 
	[RecalcPeriod]		TINYINT			NOT NULL,	-- Период пересчета процентов - ежемесячно, ежегодно, один раз в конце 
													-- 0 - Monthly,
													-- 1 - Annually,
													-- 2 - AtTheEnd,
													-- 3 - NoRecalc
	-- Поля противодействия отмыванию денег 
	[NumberOfTopUpsInDay]	INT DEFAULT 0	NOT NULL,
	[IsBlocked]				BIT DEFAULT 0	NOT NUll
);"
		 },

		 {"DepositAccounts", @"
CREATE TABLE [dbo].[DepositAccounts] (
	[id]							INT							NOT NULL PRIMARY KEY,
	FOREIGN KEY ([id]) REFERENCES [dbo].[AccountsParent]([AccID]) ON DELETE CASCADE,
	[InterestAccumulationAccID]		INT				DEFAULT 0	NOT NULL,
	[InterestAccumulationAccNum]	NVARCHAR (15)	DEFAULT ''	NOT NULL,
	[AccumulatedInterest]			MONEY			DEFAULT 0	NOT NULL
);"
		 },

		 {"CreditAccounts", @"
CREATE TABLE [dbo].[CreditAccounts] (
	[id]					INT					NOT NULL PRIMARY KEY,
	FOREIGN KEY ([id]) REFERENCES [dbo].[AccountsParent]([AccID]) ON DELETE CASCADE,
	[AccumulatedInterest]	MONEY	DEFAULT 0	NOT NULL
);"
		 },

		 {"Transactions", @"
CREATE TABLE [dbo].[Transactions] (
	[TransactionID]			INT			IDENTITY(1,1)	NOT NULL PRIMARY KEY,
	[TransactionAccountID]	INT							NOT NULL,
	[TransactionDateTime]	SMALLDATETIME				NOT NULL,
	[SourceAccount]			NVARCHAR (15)	DEFAULT ''	NOT NULL,
	[DestinationAccount]	NVARCHAR (15)	DEFAULT ''	NOT NULL,
	[OperationType]			TINYINT						NOT NULL,
	[Amount]				MONEY			DEFAULT 0	NOT NULL,
	[Comment]				NVARCHAR (256)	DEFAULT ''	NOT NULL
);"
		}};
	}
}
