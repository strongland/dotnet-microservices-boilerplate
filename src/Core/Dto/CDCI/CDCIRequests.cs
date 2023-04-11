namespace FSH.Core.Dto.CDCI;

public class CreateBackendRequest
{
    public string TenantName { get; set; }
}

public class DisableBackendRequest
{
    public string EnvironmentName { get; set; }
}

public class DeleteBackendRequest
{
    public string OrderRef { get; set; }
}
