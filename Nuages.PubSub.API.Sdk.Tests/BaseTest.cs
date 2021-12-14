using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace Nuages.PubSub.API.Sdk.Tests;

public class BaseTest
{
    protected readonly ITestOutputHelper _testOutputHelper;
    protected string _url;
    protected string _apiKey;
    protected string _userId;
    protected string _hub;
    protected string _audience;

    protected BaseTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory)?.FullName)
            .AddJsonFile("appsettings.local.json", true)
            .Build();

        _url = configuration.GetSection("Url").Value;
        _apiKey = configuration.GetSection("ApiKey").Value;
        _userId = configuration.GetSection("UserId").Value;
        _hub = configuration.GetSection("Hub").Value;
        _audience = configuration.GetSection("Audience").Value;
    }
}