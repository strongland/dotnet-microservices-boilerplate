namespace FSH.Core.Dto.CDCI;

public class CreateBackendRequest
{
    public string EndUserIp { get; set; }
}

public class DisableBackendRequest
{
    public string EndUserIp { get; set; }
}

public class DeleteBackendRequest
{
    public string OrderRef { get; set; }
}
