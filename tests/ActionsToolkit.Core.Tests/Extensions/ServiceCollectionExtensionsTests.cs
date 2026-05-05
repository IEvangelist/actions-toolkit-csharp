// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace ActionsToolkit.Core.Tests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGitHubActionsCorrectlyRegistersServicesTest()
    {
        // Arrange / Act
        IServiceCollection services = new ServiceCollection();
        _ = services.AddGitHubActionsCore();

        var serviceTypes =
            services.Select(s => s.ServiceType);

        // Assert
        static bool AllTypesRegistered(
            IServiceCollection services,
            params (Type Type, ServiceLifetime Lifetime)[] types)
        {
            var serviceTypes =
                services.Select(s => (s.ServiceType, s.Lifetime));

            return types.All(type =>
            {
                Assert.Contains(type, serviceTypes);
                return true;
            });
        }

        Assert.Equal(4, services.Count);
        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(TestConsole));
        Assert.True(AllTypesRegistered(
            services,
            (typeof(IConsole), ServiceLifetime.Singleton),
            (typeof(ICommandIssuer), ServiceLifetime.Transient),
            (typeof(IFileCommandIssuer), ServiceLifetime.Transient),
            (typeof(ICoreService), ServiceLifetime.Transient)));
    }
}
