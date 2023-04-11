namespace FSH.WebApi.Infrastructure.Common.Settings;

public class QoveryContainersResult {
    public List<QoveryContainerResult> Results { get; set; }
}

public class QoveryContainerResult {
    public string Id { get; set; }
    public string Name { get; set; }
    public string HostName { get; set; }
    public string Tag { get; set; }
    public QoveryEnvironmentResult Environment { get; set; }
}

public class QoveryContainerStopResult {
    public string Id { get; set; }
    public string Name { get; set; }
    public string State { get; set; }
    public string Message { get; set; }
}
