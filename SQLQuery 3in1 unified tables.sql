USE MyDB;

SELECT MainID, [ClientType], [Name], [Telephone], [Email], [Address]
FROM	(SELECT
			 Clients.id AS MainID,
			 'VIP' AS [ClientType],
			 FirstName + ' ' + LastName AS [Name],
			 [Telephone],
			 [Email],
			 [Address]
		FROM	VIPclients, Clients
		WHERE	Clients.id = VIPclients.ContactData 
		) AS vip
UNION SELECT
		Clients.id AS MainID,
		'Simple' AS [ClientType],
		FirstName + ' ' + LastName AS [Name],
		[Telephone],
		[Email],
		[Address]
FROM	SIMclients, Clients
WHERE	Clients.id = SIMclients.ContactData
UNION SELECT
		Clients.id AS MainID,
		'Organization' AS [ClientType],
		OrgName AS [Name],
		[Telephone],
		[Email],
		[Address]
FROM	ORGclients, Clients
WHERE	Clients.id = ORGclients.ContactData
ORDER BY [ClientType] DESC
