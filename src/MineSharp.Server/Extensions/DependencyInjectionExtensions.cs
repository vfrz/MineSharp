using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using MineSharp.Network;

namespace MineSharp.Extensions;

public static class DependencyInjectionExtensions
{
    public static void RegisterClientPacketHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClientPacketHandler<>)))
            .Where(type => type is {IsAbstract: false, IsInterface: false});

        foreach (var handlerType in handlerTypes)
        {
            var interfaceTypes = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClientPacketHandler<>));

            foreach (var interfaceType in interfaceTypes)
            {
                services.AddSingleton(interfaceType, handlerType);
            }
        }
    }
}