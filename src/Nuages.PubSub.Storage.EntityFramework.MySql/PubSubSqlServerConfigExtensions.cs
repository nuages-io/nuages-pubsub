using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.EntityFramework.MySql;

public static class PubSubMySqlConfigExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IPubSubBuilder AddPubSubMySqlStorage(this IPubSubBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null)
    { 
        return AddPubSubMySqlStorage<MySqlPubSubDbContext>(builder, optionsAction);
    }
    
    // ReSharper disable once MemberCanBePrivate.Global
    public static IPubSubBuilder AddPubSubMySqlStorage<T>(this IPubSubBuilder builder,  
        Action<DbContextOptionsBuilder>? optionsAction = null) where T : MySqlPubSubDbContext
    {
        builder.Services.AddScoped<IPubSubStorage, PubSubStorageEntityFramework<T>>();

        builder.Services.AddDbContext<T>(optionsAction, ServiceLifetime.Singleton, ServiceLifetime.Singleton);

        return builder;
    }
}