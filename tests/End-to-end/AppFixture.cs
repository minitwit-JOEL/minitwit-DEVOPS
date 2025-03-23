using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;

namespace End_to_end;

public class AppFixture : IAsyncLifetime
{
    // This network will contain the API and DB containers
    private readonly INetwork _network;

    private readonly IFutureDockerImage _ApiImage;

    private readonly IContainer _ApiContainer;
    private readonly IContainer _DbContainer;

    // Clients are used by tests to input data and assert results
    private HttpClient? _Client;

    public HttpClient Client => _Client ?? throw new InvalidOperationException("Client was not initialized");


    public class AppFixture()
    {
        public AppFixture()
        {
            _network = new NetworkBuilder()
                .Build();

            _ApiImage = new ImageFromDockerfileBuilder()
                .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), "../../src/minitwit.Api")
                .WithDockerfile("Dockerfile")
                .Build();

            _DbContainer = new PostgreSqlBuilder()
                .WithImage("postgres:latest")
                .WithNetwork(_network)
                .Build();

            _ApiContianer = new ContianerBuilder()
                .WithImage(_ApiImage)
                .WithNetwork(_network)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilContainerIsHealthy())
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _network.CreateAsync();
            await _DbContainer.StartAsync();
            await _ApiImage.CreateAsync();
            await _ApiImage.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _ApiContainer.DisposeAsync();
            await _DbContainer.DisposeAsync();
            await _network.DisposeAsync();
        }
    }
}