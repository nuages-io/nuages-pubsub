using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.EntityFramework.MySql;

public static class PubSubMySqlConfigExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IPubSubBuilder AddPubSubMySqlStorage(this IPubSubBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
    { 
        return AddPubSubMySqlStorage<MySqlPubSubDbContext>(builder, optionsAction, serviceLifetime);
    }
    
    // ReSharper disable once MemberCanBePrivate.Global
    public static IPubSubBuilder AddPubSubMySqlStorage<T>(this IPubSubBuilder builder,  
        Action<DbContextOptionsBuilder>? optionsAction = null, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : MySqlPubSubDbContext
    {
        builder.Services.AddScoped<IPubSubStorage, PubSubStorageEntityFramework<T>>();

        builder.Services.AddDbContext<T>(optionsAction, serviceLifetime, serviceLifetime);

        return builder;
    }
}