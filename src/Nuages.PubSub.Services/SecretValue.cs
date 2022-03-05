using Nuages.AWS.Secrets;

namespace Nuages.PubSub.Services;

public class SecretValue : ISecret
{
    public string Value { get; set; } = string.Empty;

    public static bool IsSecret(string? value)
    {
        return !string.IsNullOrEmpty(value) && value.StartsWith("arn:aws:secretsmanager");
    }
}