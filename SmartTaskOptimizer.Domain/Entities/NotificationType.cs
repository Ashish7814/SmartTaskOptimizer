namespace SmartTaskOptimizer.Domain.Entities;

public enum NotificationType
{
    TaskCreated = 1,
    TaskUpdated = 2,
    TaskStatusChanged = 3,
    TaskAssigned = 4,
    CommentAdded = 5,
    MemberAdded = 6,
    ProjectUpdated = 7,
    Mentioned = 8
}
