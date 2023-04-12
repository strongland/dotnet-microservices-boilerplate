namespace FSH.Core.Dto.CDCI;

public class CreateBackendRequest
{
    public string EnvironmentName { get; set; }
}

public class ToggleStateBackendRequest
{
    public string EnvironmentName { get; set; }
    public bool Enabled { get; set; }
}
