namespace FSH.WebApi.Infrastructure.Common.Settings;

public class CDCISettings {

    public string QoveryAPIUrl { get; set; }
    public string QoveryAPIToken { get; set; }

    public string ThisProjectName { get; set; }
    public string FrontendContainerName { get; set; }
    public string ThisContainerName { get; set; }
    public string DatabaseName { get; set; }

    public QoveryContainerResult FrontendContainer { get; set; }
    public QoveryContainerResult ThisContainer { get; set; }
    public QoveryEnvironmentResult ThisEnvironment { get; set; }
    public QoveryDatabaseContainerResult Database { get; set; }
}
