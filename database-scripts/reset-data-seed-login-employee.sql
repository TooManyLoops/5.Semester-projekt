/*
    Clears application data while keeping the role catalog unchanged.

    The script:
    - Deletes shifts, shift requirements, shift assignments, employees,
      employments, and employee-role links.
    - Does not insert, update, or delete rows in [EmployeeDB].[Employee].[Roles].
    - Creates 29 realistic demo employees.
    - Gives each of the selected non-schedule roles at least two employees.
    - Keeps the login employee id from login.ts and assigns that employee to Kok.
    - Creates assigned shifts for ISO weeks 25, 26, and 27 of 2026.
    - Uses 5-8 employees per week in the schedule, with some employees
      repeated across weeks.
    - Gives the login employee at least two shifts per week.
    - Creates shifts only for Kok, Tjener, and Opvasker.

    Run against the local SQL Server used by docker-compose, for example:
    docker compose exec mssql /opt/mssql-tools18/bin/sqlcmd \
      -S localhost -U sa -P "Password123!" -C \
      -i /path/inside/container/reset-data-seed-login-employee.sql
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @LoginEmployeeId uniqueidentifier = CONVERT(uniqueidentifier, '03328d8a-195c-49e5-8045-5ac71b7d7729');
    DECLARE @LoginEmployeeRoleId uniqueidentifier = CONVERT(uniqueidentifier, '03328d8a-195c-49e5-8045-5ac71b7d7730');
    DECLARE @KokRoleId uniqueidentifier;
    DECLARE @TjenerRoleId uniqueidentifier;
    DECLARE @OpvaskerRoleId uniqueidentifier;
    DECLARE @LukkeansvarligRoleId uniqueidentifier;
    DECLARE @Lukkeansvarligp80RoleId uniqueidentifier;
    DECLARE @SalgsassistentRoleId uniqueidentifier;
    DECLARE @Salgsassistentp80RoleId uniqueidentifier;
    DECLARE @ButiksassistentRoleId uniqueidentifier;
    DECLARE @Butiksassistentp80RoleId uniqueidentifier;


    DECLARE @Employees TABLE (
        [employeeNumber] int IDENTITY(1,1) NOT NULL,
        [employee_Id] uniqueidentifier NOT NULL,
        [employeeRole_Id] uniqueidentifier NOT NULL,
        [firstName] nvarchar(100) NOT NULL,
        [lastName] nvarchar(100) NOT NULL,
        [email] nvarchar(255) NOT NULL,
        [phoneNr] nvarchar(20) NOT NULL,
        [role_Id] uniqueidentifier NOT NULL
    );

    DECLARE @ShiftSeeds TABLE (
        [shiftNumber] int IDENTITY(1,1) NOT NULL,
        [shift_Id] uniqueidentifier NOT NULL,
        [employeeNumber] int NOT NULL,
        [startTime] datetime2 NOT NULL,
        [endTime] datetime2 NOT NULL
    );

    DECLARE @SeededAssignments TABLE (
        [shift_Id] uniqueidentifier NOT NULL,
        [employeeRole_Id] uniqueidentifier NOT NULL,
        [role_Id] uniqueidentifier NOT NULL,
        [startTime] datetime2 NOT NULL,
        [endTime] datetime2 NOT NULL
    );

    SELECT TOP (1) @KokRoleId = [role_Id]
    FROM [EmployeeDB].[Employee].[Roles]
    WHERE [role_name] = N'Kok'
    ORDER BY [role_Id];

    SELECT TOP (1) @TjenerRoleId = [role_Id]
    FROM [EmployeeDB].[Employee].[Roles]
    WHERE [role_name] = N'Tjener'
    ORDER BY [role_Id];

    SELECT TOP (1) @OpvaskerRoleId = [role_Id]
    FROM [EmployeeDB].[Employee].[Roles]
    WHERE [role_name] = N'Opvasker'
    ORDER BY [role_Id];

    SELECT TOP (1) @LukkeansvarligRoleId = [role_Id]
    FROM [EmployeeDB].[Employee].[Roles]
    WHERE [role_name] = N'7001 - Lukkeansvarlig'
    ORDER BY [role_Id];

    SELECT TOP (1) @Salgsassistentp80RoleId = [role_Id]
    FROM [EmployeeDB].[Employee].[Roles]
    WHERE [role_name] = N'7002 - Salgsassistent_p80'
    ORDER BY [role_Id];

    SELECT TOP (1) @SalgsassistentRoleId = [role_Id]
    FROM [EmployeeDB].[Employee].[Roles]
    WHERE [role_name] = N'7002 - Salgsassistent'
    ORDER BY [role_Id];

    SELECT TOP (1) @ButiksassistentRoleId = [role_Id]
    FROM [EmployeeDB].[Employee].[Roles]
    WHERE [role_name] = N'7003 - Butiksassistent u-18 år'
    ORDER BY [role_Id];

    SELECT TOP (1) @Butiksassistentp80RoleId = [role_Id]
    FROM [EmployeeDB].[Employee].[Roles]
    WHERE [role_name] = N'7003 - Butiksassistent u-18 år_p80'
    ORDER BY [role_Id];

    SELECT TOP (1) @Lukkeansvarligp80RoleId = [role_Id]
    FROM [EmployeeDB].[Employee].[Roles]
    WHERE [role_name] = N'7001 - Lukkeansvarlig_p80'
    ORDER BY [role_Id]

    IF @KokRoleId IS NULL OR @TjenerRoleId IS NULL OR @OpvaskerRoleId IS NULL
    BEGIN
        THROW 51000, 'Rollerne Kok, Tjener og Opvasker skal findes i [EmployeeDB].[Employee].[Roles]. Rollelisten bliver ikke aendret af dette script.', 1;
    END;

    IF OBJECT_ID(N'[ShiftDB].[Shift].[ShiftAssignments]', N'U') IS NOT NULL
        DELETE FROM [ShiftDB].[Shift].[ShiftAssignments];

    IF OBJECT_ID(N'[ShiftDB].[Shift].[ShiftRequirements]', N'U') IS NOT NULL
        DELETE FROM [ShiftDB].[Shift].[ShiftRequirements];

    IF OBJECT_ID(N'[ShiftDB].[Shift].[Shifts]', N'U') IS NOT NULL
        DELETE FROM [ShiftDB].[Shift].[Shifts];

    IF OBJECT_ID(N'[EmployeeDB].[Employee].[EmployeeRoles]', N'U') IS NOT NULL
        DELETE FROM [EmployeeDB].[Employee].[EmployeeRoles];

    IF OBJECT_ID(N'[EmployeeDB].[Employee].[Employments]', N'U') IS NOT NULL
        DELETE FROM [EmployeeDB].[Employee].[Employments];

    IF OBJECT_ID(N'[EmployeeDB].[Employee].[Employees]', N'U') IS NOT NULL
        DELETE FROM [EmployeeDB].[Employee].[Employees];

    INSERT INTO @Employees (
        [employee_Id],
        [employeeRole_Id],
        [firstName],
        [lastName],
        [email],
        [phoneNr],
        [role_Id]
    )
    VALUES
        (@LoginEmployeeId, @LoginEmployeeRoleId, N'Stefan', N'Kokholm', N'skokholm@gmail.com', N'22554411', @KokRoleId),
        (NEWID(), NEWID(), N'Maja', N'Lund', N'mlund@hotmail.com', N'28741152', @KokRoleId),
        (NEWID(), NEWID(), N'Nikolaj', N'Berg', N'nberg@yahoo.com', N'31458967', @KokRoleId),
        (NEWID(), NEWID(), N'Frederik', N'Holm', N'fholm@gmail.com', N'42617385', @KokRoleId),
        (NEWID(), NEWID(), N'Camilla', N'Iversen', N'civersen@hotmail.com', N'53628419', @KokRoleId),
        (NEWID(), NEWID(), N'Emma', N'Nielsen', N'enielsen@gmail.com', N'68135792', @TjenerRoleId),
        (NEWID(), NEWID(), N'Lucas', N'Hansen', N'lhansen@hotmail.com', N'79246813', @TjenerRoleId),
        (NEWID(), NEWID(), N'Sofie', N'Larsen', N'slarsen@yahoo.com', N'81357924', @TjenerRoleId),
        (NEWID(), NEWID(), N'Victor', N'Andersen', N'vandersen@gmail.com', N'92468135', @TjenerRoleId),
        (NEWID(), NEWID(), N'Ida', N'Pedersen', N'ipedersen@hotmail.com', N'13579246', @TjenerRoleId),
        (NEWID(), NEWID(), N'Noah', N'Eriksen', N'neriksen@gmail.com', N'92468136', @OpvaskerRoleId),
        (NEWID(), NEWID(), N'Alma', N'Bak', N'abak@hotmail.com', N'13579247', @OpvaskerRoleId),
        (NEWID(), NEWID(), N'William', N'Lind', N'wlind@yahoo.com', N'24681359', @OpvaskerRoleId),
        (NEWID(), NEWID(), N'Clara', N'Nygaard', N'cnygaard@gmail.com', N'35792460', @OpvaskerRoleId),
        (NEWID(), NEWID(), N'Anders', N'Jensen', N'ajensen@hotmail.com', N'46813579', @LukkeansvarligRoleId),
        (NEWID(), NEWID(), N'Maria', N'Hansen', N'mhansen@gmail.com', N'57924681', @LukkeansvarligRoleId),
        (NEWID(), NEWID(), N'Jonas', N'Pedersen', N'jpedersen@yahoo.com', N'68135794', @Lukkeansvarligp80RoleId),
        (NEWID(), NEWID(), N'Sofia', N'Christensen', N'schristensen@gmail.com', N'79246815', @Lukkeansvarligp80RoleId),
        (NEWID(), NEWID(), N'Mikkel', N'Larsen', N'mlarsen@hotmail.com', N'81357926', @SalgsassistentRoleId),
        (NEWID(), NEWID(), N'Laura', N'Rasmussen', N'lrasmussen@yahoo.com', N'92468137', @SalgsassistentRoleId),
        (NEWID(), NEWID(), N'Emil', N'Andersen', N'eandersen@gmail.com', N'13579248', @Salgsassistentp80RoleId),
        (NEWID(), NEWID(), N'Nina', N'Madsen', N'nmadsen@hotmail.com', N'24681357', @Salgsassistentp80RoleId),
        (NEWID(), NEWID(), N'Kasper', N'Nielsen', N'knielsen@yahoo.com', N'35792468', @ButiksassistentRoleId),
        (NEWID(), NEWID(), N'Sarah', N'Sorensen', N'ssorensen@gmail.com', N'46813570', @ButiksassistentRoleId),
        (NEWID(), NEWID(), N'Oscar', N'Poulsen', N'opoulsen@hotmail.com', N'57924682', @Butiksassistentp80RoleId),
        (NEWID(), NEWID(), N'Caroline', N'Madsen', N'cmadsen@gmail.com', N'68135793', @Butiksassistentp80RoleId),
        (NEWID(), NEWID(), N'Tobias', N'Christiansen', N'tchristiansen@hotmail.com', N'79246814', @Butiksassistentp80RoleId),
        (NEWID(), NEWID(), N'Amalie', N'Rasmussen', N'arasmussen@yahoo.com', N'81357925', @LukkeansvarligRoleId),
        (NEWID(), NEWID(), N'Mikkel', N'Ostergaard', N'mostergaard@hotmail.com', N'92468138', @OpvaskerRoleId);

    INSERT INTO [EmployeeDB].[Employee].[Employees] (
        [employee_Id],
        [firstName],
        [lastName],
        [email],
        [phoneNr],
        [employeeStatus],
        [hiredAt]
    )
    SELECT
        [employee_Id],
        [firstName],
        [lastName],
        [email],
        [phoneNr],
        1,
        CONVERT(date, SYSUTCDATETIME())
    FROM @Employees;

    INSERT INTO [EmployeeDB].[Employee].[EmployeeRoles] (
        [employeeRole_Id],
        [employee_Id],
        [role_Id],
        [isPrimary]
    )
   
   SELECT
        [employeeRole_Id],
        [employee_Id],
        [role_Id],
        1
    FROM @Employees;

    INSERT INTO @ShiftSeeds (
        [shift_Id],
        [employeeNumber],
        [startTime],
        [endTime]
    )
    VALUES
        (NEWID(), 1, CONVERT(datetime2, '2026-06-15T08:00:00'), CONVERT(datetime2, '2026-06-15T16:00:00')),
        (NEWID(), 6, CONVERT(datetime2, '2026-06-15T11:00:00'), CONVERT(datetime2, '2026-06-15T19:00:00')),
        (NEWID(), 11, CONVERT(datetime2, '2026-06-16T12:00:00'), CONVERT(datetime2, '2026-06-16T20:00:00')),
        (NEWID(), 2, CONVERT(datetime2, '2026-06-17T08:00:00'), CONVERT(datetime2, '2026-06-17T16:00:00')),
        (NEWID(), 1, CONVERT(datetime2, '2026-06-18T08:00:00'), CONVERT(datetime2, '2026-06-18T16:00:00')),
        (NEWID(), 7, CONVERT(datetime2, '2026-06-19T11:00:00'), CONVERT(datetime2, '2026-06-19T19:00:00')),
        (NEWID(), 12, CONVERT(datetime2, '2026-06-20T12:00:00'), CONVERT(datetime2, '2026-06-20T20:00:00')),
        (NEWID(), 8, CONVERT(datetime2, '2026-06-21T11:00:00'), CONVERT(datetime2, '2026-06-21T19:00:00')),
        (NEWID(), 1, CONVERT(datetime2, '2026-06-22T08:00:00'), CONVERT(datetime2, '2026-06-22T16:00:00')),
        (NEWID(), 9, CONVERT(datetime2, '2026-06-22T11:00:00'), CONVERT(datetime2, '2026-06-22T19:00:00')),
        (NEWID(), 13, CONVERT(datetime2, '2026-06-23T12:00:00'), CONVERT(datetime2, '2026-06-23T20:00:00')),
        (NEWID(), 3, CONVERT(datetime2, '2026-06-24T08:00:00'), CONVERT(datetime2, '2026-06-24T16:00:00')),
        (NEWID(), 1, CONVERT(datetime2, '2026-06-25T08:00:00'), CONVERT(datetime2, '2026-06-25T16:00:00')),
        (NEWID(), 10, CONVERT(datetime2, '2026-06-26T11:00:00'), CONVERT(datetime2, '2026-06-26T19:00:00')),
        (NEWID(), 14, CONVERT(datetime2, '2026-06-27T12:00:00'), CONVERT(datetime2, '2026-06-27T20:00:00')),
        (NEWID(), 6, CONVERT(datetime2, '2026-06-28T11:00:00'), CONVERT(datetime2, '2026-06-28T19:00:00')),
        (NEWID(), 1, CONVERT(datetime2, '2026-06-29T08:00:00'), CONVERT(datetime2, '2026-06-29T16:00:00')),
        (NEWID(), 7, CONVERT(datetime2, '2026-06-29T11:00:00'), CONVERT(datetime2, '2026-06-29T19:00:00')),
        (NEWID(), 11, CONVERT(datetime2, '2026-06-30T12:00:00'), CONVERT(datetime2, '2026-06-30T20:00:00')),
        (NEWID(), 4, CONVERT(datetime2, '2026-07-01T08:00:00'), CONVERT(datetime2, '2026-07-01T16:00:00')),
        (NEWID(), 1, CONVERT(datetime2, '2026-07-02T08:00:00'), CONVERT(datetime2, '2026-07-02T16:00:00')),
        (NEWID(), 8, CONVERT(datetime2, '2026-07-03T11:00:00'), CONVERT(datetime2, '2026-07-03T19:00:00')),
        (NEWID(), 12, CONVERT(datetime2, '2026-07-04T12:00:00'), CONVERT(datetime2, '2026-07-04T20:00:00')),
        (NEWID(), 2, CONVERT(datetime2, '2026-07-05T08:00:00'), CONVERT(datetime2, '2026-07-05T16:00:00'));

    INSERT INTO @SeededAssignments (
        [shift_Id],
        [employeeRole_Id],
        [role_Id],
        [startTime],
        [endTime]
    )
    SELECT
        ss.[shift_Id],
        e.[employeeRole_Id],
        e.[role_Id],
        ss.[startTime],
        ss.[endTime]
    FROM @ShiftSeeds ss
    INNER JOIN @Employees e ON e.[employeeNumber] = ss.[employeeNumber];

    INSERT INTO [ShiftDB].[Shift].[Shifts] (
        [Shift_Id],
        [startTime],
        [endTime]
    )
    SELECT
        [shift_Id],
        [startTime],
        [endTime]
    FROM @SeededAssignments;

    INSERT INTO [ShiftDB].[Shift].[ShiftRequirements] (
        [ShiftRequirement_Id],
        [ShiftId],
        [Role_Id],
        [Amount]
    )
    SELECT
        NEWID(),
        [shift_Id],
        [role_Id],
        1
    FROM @SeededAssignments;

    INSERT INTO [ShiftDB].[Shift].[ShiftAssignments] (
        [ShiftAssignment_Id],
        [shift_Id],
        [employeeRole_Id],
        [Status],
        [Assigned_At]
    )
    SELECT
        NEWID(),
        [shift_Id],
        [employeeRole_Id],
        1,
        SYSUTCDATETIME()
    FROM @SeededAssignments;

    COMMIT TRANSACTION;

    SELECT
        e.[firstName],
        e.[lastName],
        e.[email],
        e.[phoneNr],
        r.[role_name]
    FROM [EmployeeDB].[Employee].[Employees] e
    INNER JOIN [EmployeeDB].[Employee].[EmployeeRoles] er ON er.[employee_Id] = e.[employee_Id]
    INNER JOIN [EmployeeDB].[Employee].[Roles] r ON r.[role_Id] = er.[role_Id]
    ORDER BY
        r.[role_name],
        e.[lastName],
        e.[firstName];

    SELECT
        r.[role_Id],
        r.[role_name],
        COUNT(er.[employeeRole_Id]) AS [employee_count]
    FROM [EmployeeDB].[Employee].[Roles] r
    INNER JOIN [EmployeeDB].[Employee].[EmployeeRoles] er ON er.[role_Id] = r.[role_Id]
    GROUP BY r.[role_Id], r.[role_name]
    ORDER BY r.[role_name], r.[role_Id];

    SELECT
        r.[role_name],
        COUNT(*) AS [shift_count]
    FROM [ShiftDB].[Shift].[ShiftRequirements] sr
    INNER JOIN [EmployeeDB].[Employee].[Roles] r ON r.[role_Id] = sr.[Role_Id]
    GROUP BY r.[role_name]
    ORDER BY r.[role_name];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
