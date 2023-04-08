using System.ComponentModel.DataAnnotations;

namespace FSH.WebApi.Infrastructure.Common.Settings;

public class DatabaseContainer : IValidatableObject
{
    public string StorageProvider { get; set; }
    public bool UseHardCodedDBSettings { get; set; }
    public string Name { get; set; }
    public string? HostName { get; set; }
    public string? Port { get; set; }
    public string? DatabaseName { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ConnectionString { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Runtime.CDCI.Database.StorageProvider))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseContainer)}.{nameof(Runtime.CDCI.Database.StorageProvider)} is not configured",
                new[] { nameof(Runtime.CDCI.Database.StorageProvider) });
        }

        if (string.IsNullOrEmpty(Runtime.CDCI.Database.ConnectionString))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseContainer)}.{nameof(Runtime.CDCI.Database.ConnectionString)} is not configured",
                new[] { nameof(Runtime.CDCI.Database.ConnectionString) });
        }
    }
}