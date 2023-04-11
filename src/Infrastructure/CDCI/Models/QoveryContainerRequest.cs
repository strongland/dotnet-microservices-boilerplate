namespace FSH.WebApi.Infrastructure.Common.Settings;

public class QoveryContainerRequest {
    public string Name { get; set; }
    public string Registry_id { get; set; }
    public string Image_name { get; set; }
    public string Tag { get; set; }
    public int Cpu { get; set; }
    public int Memory { get; set; }
    public int Min_running_instances { get; set; }
    public int Max_running_instances { get; set; }
    public List<QoveryContainerPort> Ports { get; set; }
}

public class QoveryContainerPort {
    public string Name { get; set; }
    public int Internal_port { get; set; }
    public int External_port { get; set; }
    public bool Publicly_accessible { get; set; }
    public bool Is_default { get; set; }
    public string Protocol { get; set; }
}
