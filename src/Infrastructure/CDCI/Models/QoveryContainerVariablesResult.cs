namespace FSH.WebApi.Infrastructure.Common.Settings;

public class QoveryContainerVariablesResult { 
    public List<QoveryContainerVariable> results { get; set; }
}

public class QoveryContainerVariable {
    public string id { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public string key { get; set; }
    public string value { get; set; }
    public string scope { get; set; }
    public string overridden_variable { get; set; }
    public QoveryAliasedVariable aliased_variable { get; set; }
    public string service_id { get; set; }
    public string service_name { get; set; }
    public string service_type { get; set; }
}

public class EnvironmentVariableAlias {
    public string Key { get; set; }
}

public class QoveryAliasedVariable {
    public string Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
    public string Scope { get; set; }
}