namespace FSH.Core.Dto.BankId;

public class BankIdResponses
{
    public AuthResponse AuthResponse { get; set; }
    public ApiCallResponse ApiCallResponse { get; set; }
    public string ErrorMessage { get; set; }
    public BankIdUser User { get; set; }
    public List<string> NewUserCreatedMessage { get; set; }
    public TokenResponse TokenResponse { get; set; }
    public string UserHostUrl { get; set; }
    public UserDetailsDto UserDetails { get; set; }
    public List<RoleDto> UserRolesAndPermissions { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}

public class ApiCallResponse
{
    public string Success { get; set; }
    public string StatusMessage { get; set; }
    public string QrString { get; set; }
    public string QrImage { get; set; }

    public ApiResponse Response { get; set; }
}

public class ApiResponse
{
    public string OrderRef { get; set; }

    // Sign & Auth
    public string AutoStartToken { get; set; }

    // CollectStatus
    public string Status { get; set; }
    public string HintCode { get; set; }
    public CompletionData CompletionData { get; set; }
}

public class CompletionData
{
    public string Signature { get; set; } // Base64
    public string OcspResponse { get; set; } // Base64
    public BankIdUser User { get; set; }
    public DeviceIP device { get; set; }
    public CertInfo cert { get; set; }
}

public class DeviceIP
{
    public string IpAddress { get; set; }
}

public class CertInfo
{
    public string notBefore { get; set; }
    public string notAfter { get; set; }
}

public class BankIdUser
{
    public string PersonalNumber { get; set; }
    public string GivenName { get; set; }
    public string Surname { get; set; }
}
