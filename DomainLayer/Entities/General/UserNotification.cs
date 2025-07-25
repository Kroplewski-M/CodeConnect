using DomainLayer.Entities.Auth;

namespace DomainLayer.Entities.General;

public class UserNotification
{
    public Guid Id { get; set; }
    public required string ForUserId { get; set; } 
    public required string FromUserId { get; set; } 
    public int NotificationTypeId { get; set; } 
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required string TargetId { get; set; }
    public ApplicationUser? ForUser { get; set; }
    public ApplicationUser? FromUser { get; set; }
    public NotificationType? NotificationType { get; set; }
    
}