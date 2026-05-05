// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.HttpClient.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHttpClientServicesCorrectlyRegistersExpectedClients()
    {
        // Arrange
        var services = new ServiceCollection()
            .AddHttpClientServices();

        // Act
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpCredentialClientFactory>();

        // Assert
        Assert.NotNull(factory);
        Assert.IsType<DefaultHttpKeyedClientFactory>(factory);

        var basicClient = factory.CreateBasicClient("username", "password");
        Assert.NotNull(basicClient);
        Assert.IsType<DefaultHttpClient>(basicClient);
    }
}
