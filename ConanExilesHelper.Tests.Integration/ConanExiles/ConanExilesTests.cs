﻿using FluentAssertions;
using ConanExilesHelper.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using ConanExilesHelper.Helpers;
using ConanExilesHelper.Games.ConanExiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Runtime.Versioning;

namespace ConanExilesHelper.Tests.Integration.ConanExiles;

[TestFixture]
[Explicit("Run manually")]
public class ConanExilesTests
{
    private IHost _host;

    [SetUp]
    public void SetUp()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, serviceCollection) =>
            {
                var config = hostContext.Configuration!;
                serviceCollection.Configure<ConanExilesSettings>(config.GetSection("conanExilesSettings"));
            });

        _host = host.Build();
    }

    [Test]
    [SupportedOSPlatform("windows")]
    public async Task TestRestartAsync()
    {
        // Arrange
        var options = _host.Services.GetRequiredService<IOptions<ConanExilesSettings>>();

        var restartService = new RestartService(new NullLogger<RestartService>(), options);

        // Act
        var response = await restartService.RestartAsync();

        // Assert
        response.Should().Be(RestartResponse.Success);
    }
}
