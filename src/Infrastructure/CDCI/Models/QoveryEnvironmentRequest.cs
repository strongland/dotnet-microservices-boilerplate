namespace FSH.WebApi.Infrastructure.Common.Settings;

public class QoveryEnvironmentRequest
{
    public string Name { get; set; }
    public string Cluster { get; set; } // cluster ID
    public string Mode { get; set; }
}
