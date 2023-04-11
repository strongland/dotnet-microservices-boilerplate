namespace FSH.Core.Dto.CDCI;

public class CreateBackendResponse {
    public bool EnvironmentExists { get; set; }
    public bool FoundProductionTag { get; set; }
    public string BackendEnvironmentId { get; set; }
}

public class DisableBackendResponse
{
    public string EnvironmentName { get; set; }
    public string State { get; set; }
    public string Message { get; set; }
}

public class DeleteBackendResponse
{
    public string OrderRef { get; set; }
}
