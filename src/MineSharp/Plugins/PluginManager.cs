using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MineSharp.Core;
using MineSharp.Sdk;

namespace MineSharp.Plugins;

public class PluginManager
{
    private const string PluginsDirectory = "./plugins";

    private Plugin[] LoadedPlugins { get; set; } = [];

    private readonly MinecraftServer _server;
    private readonly ILogger<PluginManager> _logger;

    public PluginManager(MinecraftServer server)
    {
        _server = server;
        _logger = server.GetLogger<PluginManager>();
    }

    public void ReloadPlugins()
    {
        _logger.LogInformation("Loading plugins...");

        if (!Directory.Exists(PluginsDirectory))
            Directory.CreateDirectory(PluginsDirectory);

        var pluginDirectory = new DirectoryInfo(PluginsDirectory);

        var assemblyFiles = pluginDirectory.GetFiles("*.dll", SearchOption.TopDirectoryOnly);

        var loadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())!;

        var pluginTypes = assemblyFiles
            .Select(assemblyFile => loadContext.LoadFromAssemblyPath(assemblyFile.FullName))
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(Plugin).IsAssignableFrom(type) && !type.IsAbstract)
            .ToHashSet();

        var services = new ServiceCollection()
            .AddSingleton<IServer>(_server);

        var serviceProvider = services.BuildServiceProvider();

        LoadedPlugins = pluginTypes
            .Select(pluginType => (Plugin) ActivatorUtilities.CreateInstance(serviceProvider, pluginType))
            .ToArray();

        _logger.LogInformation($"Loaded {LoadedPlugins.Length} plugin(s)");
    }

    public async Task CallOnTickAsync(TimeSpan elapsed)
    {
        foreach (var plugin in LoadedPlugins)
        {
            await plugin.OnTick(elapsed);
        }
    }
}