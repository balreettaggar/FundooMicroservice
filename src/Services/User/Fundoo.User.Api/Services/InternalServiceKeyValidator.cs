using System.Security.Cryptography;
using System.Text;

namespace Fundoo.User.Api.Services;

public sealed class InternalServiceKeyValidator
{
    private readonly IConfiguration _configuration;

    public InternalServiceKeyValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsValid(string? suppliedKey)
    {
        var configuredKey = _configuration["InternalApi:NotesServiceKey"];

        if (string.IsNullOrWhiteSpace(configuredKey) ||
            configuredKey.Length < 32 ||
            string.IsNullOrWhiteSpace(suppliedKey))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(configuredKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);

        return expectedBytes.Length == suppliedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(
                   expectedBytes,
                   suppliedBytes);
    }
}