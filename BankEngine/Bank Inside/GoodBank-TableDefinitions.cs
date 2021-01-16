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
);
		"},
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
);
		"},
		 {"ClientsView", @"
CREATE TABLE [dbo].[ClientsView] (
	 [ID]						INT	IDENTITY (1, 1)			NOT NULL	PRIMARY KEY
	 FOREIGN KEY ([ID]) REFERENCES [dbo].[ClientsMain]([ID]) ON DELETE CASCADE,
	,[ClientType]				TINYINT						NOT NULL
	,[ClientTypeTag]			NVARCHAR (5)	DEFAULT ''	NOT NULL
	,[FirstName]				NVARCHAR (50)	DEFAULT ''	NOT NULL
	,[MiddleName]				NVARCHAR (50)	DEFAULT ''	NOT NULL
	,[LastName]					NVARCHAR (50)	DEFAULT ''	NOT NULL
	,[MainName]					NVARCHAR (256)	DEFAULT ''	NOT NULL
	,[DirectorName]				NVARCHAR (152)	DEFAULT ''	NOT NULL
	,[CreationDate]				DATE						NOT NULL
	,[PassportOrTIN]			NVARCHAR (11)				NOT NULL
	,[Telephone]				NVARCHAR (30)	DEFAULT ''	NOT NULL
	,[Email]					NVARCHAR (128)	DEFAULT ''	NOT NULL
	,[Address]					NVARCHAR (256)	DEFAULT ''	NOT NULL
	,[NumberOfSavingAccounts]	INT				DEFAULT 0	NOT NULL
	,[NumberOfDeposits]			INT				DEFAULT 0	NOT NULL
	,[NumberOfCredits]			INT				DEFAULT 0	NOT NULL
	,[NumberOfClosedAccounts]	INT				DEFAULT 0	NOT NULL
);
		"},
		 {"AccountsParent", @"
CREATE TABLE [dbo].[AccountsParent] (
	[AccID]				INT			IDENTITY (1, 1) NOT NULL PRIMARY KEY,	-- уникальный ид счета
	[ClientID]			INT				NOT NULL,	-- ID клиента
	FOREIGN KEY ([ClientID]) REFERENCES [dbo].[ClientsMain]([ID]) ON DELETE CASCADE,
	[AccountNumber]		NVARCHAR (15)	DEFAULT '' NOT NULL,		
	[Balance]			MONEY DEFAULT 0	NOT NULL,			
	[Interest]			FLOAT			NOT NULL,
	[Compounding]		BIT				NOT NULL,	-- с капитализацией или без 
	[Opened]			DATE			NOT NULL,	-- дата открытия счета 
	[Duration]			INT				NOT NULL,	-- Количество месяцев, на который открыт вклад, выдан кредит.  
	[MonthsElapsed]		INT				NOT NULL,	-- Количество месяцев, прошедших с открытия вклада 
	[EndDate]			DATE,						-- Дата окончания вклада/кредита. null - бессрочно 
	[Closed]			DATE,						-- Дата закрытия счета. null - счет открыт 
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
	);
		"},
		 {"SavingAccounts", @"
CREATE TABLE [dbo].[SavingAccounts] (
	[id]							INT							NOT NULL PRIMARY KEY,
	FOREIGN KEY ([id]) REFERENCES [dbo].[AccountsParent]([AccID]) ON DELETE CASCADE,
	);
		"},
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
		 {"AccountsView", @"
CREATE TABLE [dbo].[AccountsView] (
	 [AccID]			INT	IDENTITY (1, 1)			NOT NULL	PRIMARY KEY
	,[ClientID]			INT							NOT NULL
	,FOREIGN KEY ([ClientID]) REFERENCES [dbo].[ClientsMain]([ID]) ON DELETE CASCADE
	,[ClientType]		TINYINT						NOT NULL
	,[ClientTypeTag]	NVARCHAR (5)	DEFAULT ''	NOT NULL
	,[ClientName]		NVARCHAR (256)	DEFAULT ''	NOT NULL
	,[AccountNumber]	NVARCHAR (15)	DEFAULT ''	NOT NULL
	,[AccType]			INT							NOT NULL
--	,[Balance]			MONEY			DEFAULT 0	NOT NULL
	,[CurrentAmount]	MONEY			DEFAULT 0	NOT NULL
	,[DepositAmount]	MONEY			DEFAULT 0	NOT NULL
	,[DebtAmount]		MONEY			DEFAULT 0	NOT NULL
	,[Interest]			FLOAT			DEFAULT 0	NOT NULL
--	,[Compounding]		BIT				DEFAULT 0	NOT NULL
	,[Opened]			DATE						NOT NULL
--	,[Duration]			INT				DEFAULT 0	NOT NULL
--	,[MonthsElapsed]	INT				DEFAULT 0	NOT NULL
--	,[EndDate]			DATE
	,[Closed]			DATE
	,[Topupable]		BIT							NOT NULL
--	,[WithdrawalAllowed] BIT						NOT NULL
--	,[RecalcPeriod]		TINYINT			DEFAULT 0	NOT NULL
--	,[IsBlocked]		BIT				DEFAULT 0	NOT NULL	-- not blocked
);
		"},
		 {"ClientAccountsView", @"
CREATE TABLE [dbo].[ClientAccountsView] (
	 [AccID]			INT	IDENTITY (1, 1)			NOT NULL	PRIMARY KEY
	,[ClientID]			INT							NOT NULL
	,FOREIGN KEY ([ClientID]) REFERENCES [dbo].[ClientsMain]([ID]) ON DELETE CASCADE
	,[ClientType]		TINYINT						NOT NULL
	,[ClientTypeTag]	NVARCHAR (5)	DEFAULT ''	NOT NULL
	,[ClientName]		NVARCHAR (256)	DEFAULT ''	NOT NULL
	,[AccountNumber]	NVARCHAR (15)	DEFAULT ''	NOT NULL
	,[AccType]			INT							NOT NULL
	,[CurrentAmount]	MONEY			DEFAULT 0	NOT NULL
	,[DepositAmount]	MONEY			DEFAULT 0	NOT NULL
	,[DebtAmount]		MONEY			DEFAULT 0	NOT NULL
	,[Interest]			FLOAT			DEFAULT 0	NOT NULL
	,[Opened]			DATE						NOT NULL
	,[Closed]			DATE
	,[Topuble]			BIT							NOT NULL
);
		"},
		 {"Transactions", @"
CREATE TABLE [dbo].[Transactions] (
	[TransactionID]			INT			IDENTITY(1,1)	NOT NULL PRIMARY KEY,
	[TransactionAccountID]	INT							NOT NULL,
	FOREIGN KEY ([TransactionAccountID]) REFERENCES [dbo].[AccountsParent]([AccID]) ON DELETE CASCADE,
	[TransactionDateTime]	SMALLDATETIME				NOT NULL,
	[SourceAccount]			NVARCHAR (15)	DEFAULT ''	NOT NULL,
	[DestinationAccount]	NVARCHAR (15)	DEFAULT ''	NOT NULL,
	[OperationType]			INT							NOT NULL,
	[Amount]				MONEY			DEFAULT 0	NOT NULL,
	[Comment]				NVARCHAR (256)	DEFAULT ''	NOT NULL
);"
		}};
	}
}
