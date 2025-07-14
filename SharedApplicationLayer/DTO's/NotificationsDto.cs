using ApplicationLayer.DTO_s.User;
using DomainLayer.Constants;

namespace ApplicationLayer.DTO_s;

public record NotificationsDto(UserBasicDto fromUser, Consts.NotificationTypes notificationType, DateTime createdAt, string targetId);

public record GetNotificationsDto(bool Flag, List<NotificationsDto> notifications);