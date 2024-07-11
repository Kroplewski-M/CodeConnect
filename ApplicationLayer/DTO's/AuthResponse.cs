namespace ApplicationLayer.DTO_s;

public record AuthResponse(bool Flag,string? Token, string? RefreshToken, string Message);