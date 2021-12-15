namespace Nuages.PubSub.API.Sdk.Tests;

class ReponseData
{
    public string connectionId { get; set; } = "";
}

class Response
{
    public string? type { get; set; }
    public ReponseData? data { get; set; }
}