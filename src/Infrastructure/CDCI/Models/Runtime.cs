using FSH.Infrastructure.Auth;

namespace FSH.WebApi.Infrastructure.Common.Settings;

public static class Runtime {

    public static SecuritySettings SecuritySettings { get; set; }
    public static CDCISettings CDCI { get; set; }
}
