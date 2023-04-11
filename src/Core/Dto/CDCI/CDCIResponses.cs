namespace FSH.Core.Dto.CDCI;

public class CreateBackendResponse
{
    public bool EnvironmentExists { get; set; }
}

public class DisableBackendResponse
{
    public string EndUserIp { get; set; }
    public string UserVisibleData { get; set; }
    public string UserNonVisibleData { get; set; }
}

public class DeleteBackendResponse
{
    public string OrderRef { get; set; }
}
