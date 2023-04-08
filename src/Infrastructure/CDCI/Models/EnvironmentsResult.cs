﻿namespace FSH.WebApi.Infrastructure.Common.Settings;

public class EnvironmentsResult {
    public List<QoveryEnvironment> Results { get; set; }
}

public class CloudProvider {
    public string Provider { get; set; }
    public string Cluster { get; set; }
}

public class Project {
    public string Id { get; set; }
}

public class QoveryEnvironment {
    public string Id { get; set; }
    public DateTime Created_at { get; set; }
    public DateTime Updated_at { get; set; }
    public string Name { get; set; }
    public Project Project { get; set; }
    public CloudProvider Cloud_provider { get; set; }
    public string Mode { get; set; }
    public string Cluster_id { get; set; }
    public string Cluster_name { get; set; }
}