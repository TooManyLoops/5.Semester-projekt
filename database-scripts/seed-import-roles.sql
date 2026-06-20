/*
    Seeds the role catalog needed by the Excel shift import test file.

    Safe to run multiple times:
    - Existing roles are matched by role_name.
    - Existing descriptions are updated.
    - Missing roles are inserted with stable IDs.

    Example:
    docker compose exec mssql /opt/mssql-tools18/bin/sqlcmd \
      -S localhost -U sa -P "Password123!" -C -d Timegrip \
      -i /path/inside/container/seed-import-roles.sql
*/

SET NOCOUNT ON;

MERGE [Employee].[Roles] AS target
USING (
    VALUES
        (
            CONVERT(uniqueidentifier, '11111111-7001-0001-0000-000000000001'),
            N'7001 - Lukkeansvarlig',
            N'Import role used by the Excel shift import test file.'
        ),
        (
            CONVERT(uniqueidentifier, '11111111-7001-0080-0000-000000000001'),
            N'7001 - Lukkeansvarlig_p80',
            N'P80 import role used by the Excel shift import test file.'
        ),
        (
            CONVERT(uniqueidentifier, '11111111-7002-0001-0000-000000000001'),
            N'7002 - Salgsassistent',
            N'Import role used by the Excel shift import test file.'
        ),
        (
            CONVERT(uniqueidentifier, '11111111-7002-0080-0000-000000000001'),
            N'7002 - Salgsassistent_p80',
            N'P80 import role used by the Excel shift import test file.'
        ),
        (
            CONVERT(uniqueidentifier, '11111111-7003-0001-0000-000000000001'),
            N'7003 - Butiksassistent u-18 år',
            N'Import role used by the Excel shift import test file.'
        ),
        (
            CONVERT(uniqueidentifier, '11111111-7003-0080-0000-000000000001'),
            N'7003 - Butiksassistent u-18 år_p80',
            N'P80 import role used by the Excel shift import test file.'
        )
) AS source ([role_Id], [role_name], [description])
ON target.[role_name] = source.[role_name]
WHEN MATCHED THEN
    UPDATE SET target.[description] = source.[description]
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([role_Id], [role_name], [description])
    VALUES (source.[role_Id], source.[role_name], source.[description]);

SELECT
    [role_Id],
    [role_name],
    [description]
FROM [Employee].[Roles]
WHERE [role_name] IN (
    N'7001 - Lukkeansvarlig',
    N'7001 - Lukkeansvarlig_p80',
    N'7002 - Salgsassistent',
    N'7002 - Salgsassistent_p80',
    N'7003 - Butiksassistent u-18 år',
    N'7003 - Butiksassistent u-18 år_p80'
)
ORDER BY [role_name];
