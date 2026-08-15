/* SmartTaskOptimizer SQL Server schema. Run against the target database before starting the API. */
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        FullName nvarchar(150) NOT NULL,
        Email nvarchar(320) NOT NULL,
        PasswordHash nvarchar(500) NOT NULL,
        Role int NOT NULL,
        IsActive bit NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
        LastLoginAt datetime2 NULL,
        EmailVerifiedAt datetime2 NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL
    );
    CREATE UNIQUE INDEX UX_Users_Email ON dbo.Users(Email);
    CREATE INDEX IX_Users_IsActive ON dbo.Users(IsActive);
END;

IF OBJECT_ID(N'dbo.Projects', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Projects (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Projects PRIMARY KEY,
        Name nvarchar(200) NOT NULL,
        Description nvarchar(2000) NULL,
        OwnerId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CreatedByUserId uniqueidentifier NOT NULL,
        UpdatedByUserId uniqueidentifier NULL,
        IsDeleted bit NOT NULL CONSTRAINT DF_Projects_IsDeleted DEFAULT 0,
        DeletedAt datetime2 NULL,
        DeletedByUserId uniqueidentifier NULL,
        CONSTRAINT FK_Projects_Owner FOREIGN KEY (OwnerId) REFERENCES dbo.Users(Id)
    );
    CREATE INDEX IX_Projects_OwnerId ON dbo.Projects(OwnerId);
    CREATE INDEX IX_Projects_Name ON dbo.Projects(Name);
END;

IF OBJECT_ID(N'dbo.ProjectMembers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProjectMembers (
        ProjectId uniqueidentifier NOT NULL,
        UserId uniqueidentifier NOT NULL,
        Role nvarchar(30) NOT NULL,
        JoinedAt datetime2 NOT NULL,
        CONSTRAINT PK_ProjectMembers PRIMARY KEY(ProjectId, UserId),
        CONSTRAINT FK_ProjectMembers_Project FOREIGN KEY(ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProjectMembers_User FOREIGN KEY(UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
    );
    CREATE INDEX IX_ProjectMembers_UserId ON dbo.ProjectMembers(UserId);
END;

IF OBJECT_ID(N'dbo.Tasks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tasks (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Tasks PRIMARY KEY,
        Title nvarchar(200) NOT NULL,
        Description nvarchar(5000) NULL,
        Priority int NOT NULL,
        Status int NOT NULL,
        EstimatedDurationMinutes int NOT NULL,
        Deadline datetime2 NOT NULL,
        StartedAt datetime2 NULL,
        CompletedAt datetime2 NULL,
        Progress int NOT NULL CONSTRAINT DF_Tasks_Progress DEFAULT 0,
        Category nvarchar(100) NULL,
        CreatedByUserId uniqueidentifier NOT NULL,
        AssigneeId uniqueidentifier NULL,
        ProjectId uniqueidentifier NULL,
        RowVersion rowversion NOT NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        UpdatedByUserId uniqueidentifier NULL,
        IsDeleted bit NOT NULL CONSTRAINT DF_Tasks_IsDeleted DEFAULT 0,
        DeletedAt datetime2 NULL,
        DeletedByUserId uniqueidentifier NULL,
        CONSTRAINT FK_Tasks_Creator FOREIGN KEY(CreatedByUserId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_Tasks_Assignee FOREIGN KEY(AssigneeId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_Tasks_Project FOREIGN KEY(ProjectId) REFERENCES dbo.Projects(Id)
    );
    CREATE INDEX IX_Tasks_Project_Status ON dbo.Tasks(ProjectId, Status);
    CREATE INDEX IX_Tasks_Project_UpdatedAt ON dbo.Tasks(ProjectId, UpdatedAt);
    CREATE INDEX IX_Tasks_Project_Deadline ON dbo.Tasks(ProjectId, Deadline);
    CREATE INDEX IX_Tasks_AssigneeId ON dbo.Tasks(AssigneeId);
    CREATE INDEX IX_Tasks_CreatedByUserId ON dbo.Tasks(CreatedByUserId);
END;

IF OBJECT_ID(N'dbo.TaskHistories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaskHistories (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_TaskHistories PRIMARY KEY,
        TaskId uniqueidentifier NOT NULL,
        OldStatus int NOT NULL,
        NewStatus int NOT NULL,
        OldPriority int NOT NULL,
        NewPriority int NOT NULL,
        ChangedByUserId uniqueidentifier NOT NULL,
        ChangeReason nvarchar(2000) NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT FK_TaskHistories_Task FOREIGN KEY(TaskId) REFERENCES dbo.Tasks(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskHistories_User FOREIGN KEY(ChangedByUserId) REFERENCES dbo.Users(Id)
    );
    CREATE INDEX IX_TaskHistories_Task_CreatedAt ON dbo.TaskHistories(TaskId, CreatedAt);
END;

IF OBJECT_ID(N'dbo.TaskComments', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaskComments (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_TaskComments PRIMARY KEY,
        TaskId uniqueidentifier NOT NULL,
        AuthorId uniqueidentifier NOT NULL,
        Body nvarchar(10000) NOT NULL,
        IsDeleted bit NOT NULL CONSTRAINT DF_TaskComments_IsDeleted DEFAULT 0,
        DeletedAt datetime2 NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT FK_TaskComments_Task FOREIGN KEY(TaskId) REFERENCES dbo.Tasks(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskComments_Author FOREIGN KEY(AuthorId) REFERENCES dbo.Users(Id)
    );
    CREATE INDEX IX_TaskComments_Task_CreatedAt ON dbo.TaskComments(TaskId, CreatedAt);
END;

IF OBJECT_ID(N'dbo.Tags', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tags (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Tags PRIMARY KEY,
        Name nvarchar(50) NOT NULL
    );
    CREATE UNIQUE INDEX UX_Tags_Name ON dbo.Tags(Name);
END;

IF OBJECT_ID(N'dbo.TaskTags', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaskTags (
        TaskId uniqueidentifier NOT NULL,
        TagId uniqueidentifier NOT NULL,
        CONSTRAINT PK_TaskTags PRIMARY KEY(TaskId, TagId),
        CONSTRAINT FK_TaskTags_Task FOREIGN KEY(TaskId) REFERENCES dbo.Tasks(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskTags_Tag FOREIGN KEY(TagId) REFERENCES dbo.Tags(Id) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'dbo.TaskDependencies', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TaskDependencies (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_TaskDependencies PRIMARY KEY,
        TaskId uniqueidentifier NOT NULL,
        DependsOnTaskId uniqueidentifier NOT NULL,
        CreatedAt datetime2 NOT NULL,
        CONSTRAINT FK_TaskDependencies_Task FOREIGN KEY(TaskId) REFERENCES dbo.Tasks(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TaskDependencies_DependsOn FOREIGN KEY(DependsOnTaskId) REFERENCES dbo.Tasks(Id)
    );
    CREATE UNIQUE INDEX UX_TaskDependencies_Task_DependsOn ON dbo.TaskDependencies(TaskId, DependsOnTaskId);
END;

IF OBJECT_ID(N'dbo.Activities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Activities (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Activities PRIMARY KEY,
        ProjectId uniqueidentifier NOT NULL,
        ActorId uniqueidentifier NOT NULL,
        TaskId uniqueidentifier NULL,
        Action nvarchar(100) NOT NULL,
        Field nvarchar(100) NULL,
        OldValue nvarchar(2000) NULL,
        NewValue nvarchar(2000) NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT FK_Activities_Project FOREIGN KEY(ProjectId) REFERENCES dbo.Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Activities_Actor FOREIGN KEY(ActorId) REFERENCES dbo.Users(Id),
        CONSTRAINT FK_Activities_Task FOREIGN KEY(TaskId) REFERENCES dbo.Tasks(Id)
    );
    CREATE INDEX IX_Activities_Project_CreatedAt ON dbo.Activities(ProjectId, CreatedAt);
END;

IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notifications (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
        UserId uniqueidentifier NOT NULL,
        Type int NOT NULL,
        Title nvarchar(200) NOT NULL,
        Message nvarchar(2000) NOT NULL,
        ProjectId uniqueidentifier NULL,
        TaskId uniqueidentifier NULL,
        IsRead bit NOT NULL CONSTRAINT DF_Notifications_IsRead DEFAULT 0,
        ReadAt datetime2 NULL,
        CreatedAt datetime2 NOT NULL,
        UpdatedAt datetime2 NOT NULL,
        CONSTRAINT FK_Notifications_User FOREIGN KEY(UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Notifications_Project FOREIGN KEY(ProjectId) REFERENCES dbo.Projects(Id),
        CONSTRAINT FK_Notifications_Task FOREIGN KEY(TaskId) REFERENCES dbo.Tasks(Id)
    );
    CREATE INDEX IX_Notifications_User_Read_CreatedAt ON dbo.Notifications(UserId, IsRead, CreatedAt);
END;
