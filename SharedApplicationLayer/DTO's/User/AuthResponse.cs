namespace ApplicationLayer.DTO_s.User;

public record AuthResponse(bool Flag,string? Token,string? RefreshToken, string Message);