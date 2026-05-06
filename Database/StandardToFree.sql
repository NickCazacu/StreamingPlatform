-- =====================================================================
-- Migrație: planul Standard a fost eliminat. Toți utilizatorii cu
-- SubscriptionType = 'Standard' devin 'Free' (cu aceleași limite ca Standard).
-- =====================================================================

USE StreamZoneDB;
GO

UPDATE dbo.Users
SET SubscriptionType = 'Free',
    SubscriptionExpiresAt = NULL
WHERE SubscriptionType = 'Standard';
GO

-- Verificare
SELECT SubscriptionType, COUNT(*) AS NumarConturi
FROM dbo.Users
GROUP BY SubscriptionType
ORDER BY SubscriptionType;
GO

PRINT '✓ Conturile Standard au fost mutate la Free.';
GO
