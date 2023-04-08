using FSH.WebApi.Infrastructure.Persistence;

namespace FSH.WebApi.Infrastructure.Common.Settings;

public class CDCISettings {

    public string QoveryAPIUrl { get; set; }
    public string QoveryAPIToken { get; set; }

    public string ThisProjectName { get; set; }
    public string FrontendContainerName { get; set; }
    public string ThisContainerName { get; set; }
    public string DatabaseName { get; set; }

    public QoveryContainer FrontendContainer { get; set; }
    public QoveryContainer ThisContainer { get; set; }
    public QoveryEnvironment ThisEnvironment { get; set; }
    public DatabaseContainer Database { get; set; }
}