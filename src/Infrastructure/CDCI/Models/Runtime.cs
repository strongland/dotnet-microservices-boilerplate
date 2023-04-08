using FSH.Infrastructure.Authentication.BankID.Models;
using FSH.WebApi.Infrastructure.Persistence;

namespace FSH.WebApi.Infrastructure.Common.Settings;

public static class Runtime {

    public static SecuritySettings SecuritySettings { get; set; }
    public static CDCISettings CDCI { get; set; }
}