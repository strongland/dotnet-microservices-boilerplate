namespace FSH.Core.Dto.BankId;

public record TokenResponse(string Token, string RefreshToken, DateTime RefreshTokenExpiryTime);
