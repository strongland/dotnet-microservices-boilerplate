namespace FSH.Core.Dto.BankId;

public class BankIdAuth : BankIdCredentials
{
    public string EndUserIp { get; set; }
}

public class BankIdSign : BankIdCredentials
{
    public string EndUserIp { get; set; }
    public string UserVisibleData { get; set; }
    public string UserNonVisibleData { get; set; }
}

public class BankIdCollect : BankIdCredentials
{
    public string OrderRef { get; set; }
}

public class BankIdCredentials
{
    public string ApiUser { get; set; }
    public string Password { get; set; }
    public string CompanyApiGuid { get; set; }
}
