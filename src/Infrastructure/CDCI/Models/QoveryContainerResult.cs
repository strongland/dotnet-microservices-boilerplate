namespace FSH.WebApi.Infrastructure.Common.Settings;

public class QoveryContainerResult {
    public string Id { get; set; }
    public string Name { get; set; }
    public string HostName { get; set; }
    public string Tag { get; set; }
    public QoveryEnvironmentResult Environment { get; set; }
}
