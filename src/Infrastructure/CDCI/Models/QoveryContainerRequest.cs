namespace FSH.WebApi.Infrastructure.Common.Settings;

public class QoveryContainerRequest {
    public string name { get; set; }
    public string registry_id { get; set; }
    public string image_name { get; set; }
    public string image_tag { get; set; }
    public string tag { get; set; }
    public int cpu { get; set; }
    public int memory { get; set; }
    public int min_running_instances { get; set; }
    public int max_running_instances { get; set; }
    public List<QoveryContainerPort> ports { get; set; }
}

public class QoveryContainerPort {
    public string name { get; set; }
    public int internal_port { get; set; }
    public int external_port { get; set; }
    public bool publicly_accessible { get; set; }
    public bool is_default { get; set; }
    public string protocol { get; set; }
}
