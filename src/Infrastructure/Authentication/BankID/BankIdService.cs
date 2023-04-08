using FSH.Core.Common;
using FSH.Core.Dto.BankId;
using FSH.Core.Dto.CDCI;
using FSH.WebApi.Infrastructure.Common.Settings;
using FSH.WebApi.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace FSH.Infrastructure.Auth;

public class BankIdService : IBankIdService {

    private readonly ILogger Logger;

    private readonly IUserService UserService;

    public BankIdService(IConfiguration config, ILogger logger, IUserService userService) {
        Logger = logger;
        UserService = userService;
        Runtime.SecuritySettings = config.GetSection("SecuritySettings").Get<SecuritySettings>();
    }

    public async Task<BankIdResponses> Auth(BankIdAuth auth, CancellationToken cancellationToken) {
        auth.ApiUser = Runtime.SecuritySettings.BankId.ApiUser;
        auth.Password = Runtime.SecuritySettings.BankId.Password;
        auth.CompanyApiGuid = Runtime.SecuritySettings.BankId.CompanyApiGuid;
        var result = await new RestClient(Runtime.SecuritySettings.BankId.ApiUrl).POST("auth", JsonConvert.SerializeObject(auth));
        return JsonConvert.DeserializeObject<BankIdResponses>(result.Content);
    }

    public async Task<BankIdResponses> Sign(BankIdSign sign, CancellationToken cancellationToken) {
        sign.ApiUser = Runtime.SecuritySettings.BankId.ApiUser;
        sign.Password = Runtime.SecuritySettings.BankId.Password;
        sign.CompanyApiGuid = Runtime.SecuritySettings.BankId.CompanyApiGuid;
        var result = await new RestClient(Runtime.SecuritySettings.BankId.ApiUrl).POST("sign", JsonConvert.SerializeObject(sign));
        return JsonConvert.DeserializeObject<BankIdResponses>(result.Content);
    }

    public async Task<BankIdResponses> CollectQR(BankIdCollect collectQr, CancellationToken cancellationToken) {
        collectQr.ApiUser = Runtime.SecuritySettings.BankId.ApiUser;
        collectQr.Password = Runtime.SecuritySettings.BankId.Password;
        collectQr.CompanyApiGuid = Runtime.SecuritySettings.BankId.CompanyApiGuid;
        var result = await new RestClient(Runtime.SecuritySettings.BankId.ApiUrl).POST("collectqr", JsonConvert.SerializeObject(collectQr));
        return JsonConvert.DeserializeObject<BankIdResponses>(result.Content);
    }

    public async Task<BankIdResponses> CollectStatus(BankIdCollect collectStatus, string userIpAddress, string userRequestOrigin, CancellationToken cancellationToken) {
        collectStatus.ApiUser = Runtime.SecuritySettings.BankId.ApiUser;
        collectStatus.Password = Runtime.SecuritySettings.BankId.Password;
        collectStatus.CompanyApiGuid = Runtime.SecuritySettings.BankId.CompanyApiGuid;

        var resultJson = await new RestClient(Runtime.SecuritySettings.BankId.ApiUrl).POST("collectstatus", JsonConvert.SerializeObject(collectStatus));
        var result = JsonConvert.DeserializeObject<BankIdResponses>(resultJson.Content);

        if (result != null && result.ApiCallResponse.Response != null && result.ApiCallResponse.Response.CompletionData != null) {
            if (result.ApiCallResponse.Response.CompletionData.User != null) {
                result.User = result.ApiCallResponse.Response.CompletionData.User;
                if (result.User.PersonalNumber != null) {

                    var user = new ApplicationUser {
                        Email = $"{result.User.GivenName.ToLowerInvariant()}@ihavenopermissions.com",
                        UserName = $"{ReplaceSwedishCharacters(result.User.GivenName.ToLower())}{ReplaceSwedishCharacters(result.User.Surname.ToLower())}{result.User.PersonalNumber.Substring(0, 4)}",
                        FirstName = result.User.GivenName.ToLowerInvariant(),
                        LastName = result.User.Surname.ToLowerInvariant(),
                        //PersonalNumber = Argon2.Hash(result.User.PersonalNumber),
                        PersonalNumber = result.User.PersonalNumber,
                        PhoneNumber = String.Empty,
                        IsActive = true,
                        EmailConfirmed = true
                    };

                    try {
                        var resolvedHostUrl = await UserService.ResolveUsersBackendHostUrl(user.PersonalNumber);
                        //result.TokenResponse = await UserService.GenerateAccessTokenInUserBackend(user.PersonalNumber, cancellationToken);
                        var userDetailResponse = await UserService.GetFromPersonalNumberAsync(user.PersonalNumber, cancellationToken);

                        result.TokenResponse = userDetailResponse.TokenResponse;
                        //result.TokenResponse = await TokenService.GetTokenAsync(
                        //    new TokenRequest(
                        //        result.UserDetails.Email,
                        //        result.User.PersonalNumber,
                        //        userIpAddress,
                        //        userRequestOrigin
                        //    ), userIpAddress, cancellationToken);

                        result.User.PersonalNumber = "REDACTED";
                        result.ApiCallResponse.Response.CompletionData.User.PersonalNumber = "REDACTED";

                        result.UserRolesAndPermissions = userDetailResponse.UserRolesAndPermissions;
                        //var userRoles = await UserService.GetRolesAsync(result.UserDetails.Id.ToString(), cancellationToken);
                        //result.UserRolesAndPermissions = new List<RoleDto>();
                        //foreach (var role in userRoles) {
                        //    if (role.Enabled) {
                        //        result.UserRolesAndPermissions.Add(await RoleService.GetByIdWithPermissionsAsync(role.RoleId, cancellationToken));
                        //    }
                        //}

                        Console.WriteLine(JsonConvert.SerializeObject(result));
                    }
                    catch (Exception ex) {
                        Console.WriteLine("NOT REGISTERED");
                        Console.WriteLine(JsonConvert.SerializeObject(result.User, Formatting.Indented));
                        result.User.GivenName = null;
                        result.User.Surname = null;
                        result.User.PersonalNumber = "NOT_REGISTERED";
                    }
                }
            }
            result.ApiCallResponse.Response.CompletionData.Signature = null;
        }
        return result;
    }

    private string ReplaceSwedishCharacters(string originString) =>  originString.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
}
