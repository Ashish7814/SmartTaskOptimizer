/* Optional one-time bridge for databases created by the original source before EF configurations were applied.
   Review and back up the database before running. */
IF OBJECT_ID(N'dbo.Tasks', N'U') IS NULL AND OBJECT_ID(N'dbo.tblTasks', N'U') IS NOT NULL EXEC sp_rename N'dbo.tblTasks', N'Tasks';
IF OBJECT_ID(N'dbo.Projects', N'U') IS NULL AND OBJECT_ID(N'dbo.tblProjects', N'U') IS NOT NULL EXEC sp_rename N'dbo.tblProjects', N'Projects';
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL AND OBJECT_ID(N'dbo.tblUsers', N'U') IS NOT NULL EXEC sp_rename N'dbo.tblUsers', N'Users';
IF OBJECT_ID(N'dbo.TaskHistories', N'U') IS NULL AND OBJECT_ID(N'dbo.tblTaskHistories', N'U') IS NOT NULL EXEC sp_rename N'dbo.tblTaskHistories', N'TaskHistories';

IF OBJECT_ID(N'dbo.Tasks', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tasks', N'AssigneeId') IS NULL ALTER TABLE dbo.Tasks ADD AssigneeId uniqueidentifier NULL;
IF OBJECT_ID(N'dbo.Tasks', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Tasks', N'RowVersion') IS NULL ALTER TABLE dbo.Tasks ADD RowVersion rowversion;
