﻿using System.Collections.Generic;

namespace BankInside
{
	public partial class GoodBank
	{
		private Dictionary<string, string> tables = new Dictionary<string, string>()
		{{"ClientsMain", @"
CREATE TABLE [dbo].[ClientsMain] (
	[ID]						INT			IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Telephone]					NVARCHAR (20),	-- better 30
	[Email]						NVARCHAR (70),	-- better 128
	[Address]					NVARCHAR (256),
	[NumberOfSavingAccounts]	INT				DEFAULT 0	NOT NULL,
	[NumberOfDeposits]			INT				DEFAULT 0	NOT NULL,		
	[NumberOfCredits]			INT				DEFAULT 0	NOT NULL,			
	[NumberOfClosedAccounts]	INT				DEFAULT 0	NOT NULL
);"
		 },

		 {"VIPclients", @"
CREATE TABLE [dbo].[VIPclients] (
	[id]				INT			NOT NULL PRIMARY KEY,
	[FirstName]			NVARCHAR (50)	NOT NULL,
	[MiddleName]		NVARCHAR (50),
	[LastName]			NVARCHAR (50)	NOT NULL,
	[PassportNumber]	NVARCHAR (11)	NOT NULL,
	[BirthDate]			DATE			NOT NULL
);"
		 },

		 {"SIMclients", @"
CREATE TABLE [dbo].[SIMclients] (
	[id]				INT			NOT NULL PRIMARY KEY,
	[FirstName]			NVARCHAR (50)	NOT NULL,
	[MiddleName]		NVARCHAR (50),
	[LastName]			NVARCHAR (50)	NOT NULL,
	[PassportNumber]	NVARCHAR (11)	NOT NULL,
	[BirthDate]			DATE			NOT NULL
);"
		 },
		 {"ORGclients", @"
CREATE TABLE [dbo].[ORGclients] (
	[id]				INT			NOT NULL PRIMARY KEY,	
	[OrgName]			NVARCHAR (256)	NOT NULL,
	[DirectorFirstName]	NVARCHAR (50),
	[DirectorMiddleName]NVARCHAR (50),
	[DirectorLastName]	NVARCHAR (50),
	[TIN]				NVARCHAR (10)	NOT NULL,
	[RegistrationDate]	DATE			NOT NULL
);"
		 },

		 {"Accounts", @"
CREATE TABLE [dbo].[Accounts] (
	[AccID]				INT			IDENTITY (1, 1) NOT NULL PRIMARY KEY,	-- уникальный ид счета
	[AccType]			TINYINT			NOT NULL,	-- Account type
													-- 0 - Saving,
													-- 1 - Deposit,
													-- 2 - Credit,
													-- 3 - Total 
	-- [ClientType]		TINYINT 
	-- I feel ClientType field is not necessay. We can just generate it for output
	-- and if we need, then it's better to use just numbers with comments, like this 0 --VIP, 1 --SIM, 2 --ORG

	[ClientID]			BIGINT			NOT NULL,	-- ID клиента
	[AccountNumber]		NVARCHAR (15)	NOT NULL,		
	[Balance]			MONEY DEFAULT 0	NOT NULL,			
	[Interest]			DECIMAL (2,2)	NOT NULL,
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
	[id]							INT				NOT NULL PRIMARY KEY,
	[InterestAccumulationAccID]		BIGINT DEFAULT 0,
	[InterestAccumulationAccNum]	NVARCHAR (15),
	[AccumulatedInterest]			MONEY DEFAULT 0
);"
		 },

		 {"CreditAccounts", @"
CREATE TABLE [dbo].[CreditAccounts] (
	[id]					INT			NOT NULL PRIMARY KEY,
	[AccumulatedInterest]	MONEY DEFAULT 0
);"
		 },

		 {"Transactions", @"
CREATE TABLE [dbo].[Transactions] (
	[TransactionID]			INT				IDENTITY(1,1) NOT NULL PRIMARY KEY
	[TransactionAccountID]	INT				NOT NULL,
	[TransactionDateTime]	DATETIME		NOT NULL,
	[SourceAccount]			NVARCHAR (15)	NOT NULL,
	[DestinationAccount]	NVARCHAR (15)	NOT NULL,
	[OperationType]			TINYINT			NOT NULL,
	[Amount]				MONEY			NOT NULL,
	[Comment]				NVARCHAR (256)
);"
		}};
	}
}
