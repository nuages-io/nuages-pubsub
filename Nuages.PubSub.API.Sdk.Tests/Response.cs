// ReSharper disable InconsistentNaming
namespace Nuages.PubSub.API.Sdk.Tests;


// ReSharper disable once ClassNeverInstantiated.Global
internal class ReponseData
{
    public string connectionId { get; set; } = "";
}

internal class Response
{
    public string? type { get; set; }
    
    public ReponseData? data { get; set; }
}