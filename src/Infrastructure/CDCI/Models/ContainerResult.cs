namespace FSH.WebApi.Infrastructure.Common.Settings;

public class QoveryContainer {
    public string Id { get; set; }
    public string Name { get; set; }
    public string HostName { get; set; }
    public QoveryEnvironment Environment { get; set; }
}
