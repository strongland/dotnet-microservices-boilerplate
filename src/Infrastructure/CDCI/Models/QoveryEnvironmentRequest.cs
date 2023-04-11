namespace FSH.WebApi.Infrastructure.Common.Settings;

public class QoveryEnvironmentRequest
{
    public string name { get; set; }
    public string cluster { get; set; } // cluster ID
    public string mode { get; set; }
}
