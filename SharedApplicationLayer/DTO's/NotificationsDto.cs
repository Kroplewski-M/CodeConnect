using ApplicationLayer.DTO_s.User;
using DomainLayer.Constants;

namespace ApplicationLayer.DTO_s;

public record NotificationsDto(Guid Id, UserBasicDto FromUser, Consts.NotificationTypes NotificationType, DateTime CreatedAt, string TargetId, string? ParentId);

public record GetNotificationsDto(bool Flag, List<NotificationsDto> Notifications);