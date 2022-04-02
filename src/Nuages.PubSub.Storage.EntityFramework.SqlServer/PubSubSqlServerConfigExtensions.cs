using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;
using Nuages.PubSub.Services.Storage;

namespace Nuages.PubSub.Storage.EntityFramework.SqlServer;

public static class PubSubSqlServerConfigExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IPubSubBuilder AddPubSubSqlServerStorage(this IPubSubBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
    { 
        return AddPubSubSqlServerStorage<SqlServerPubSubDbContext>(builder, optionsAction, serviceLifetime);
    }
    
    // ReSharper disable once MemberCanBePrivate.Global
    public static IPubSubBuilder AddPubSubSqlServerStorage<T>(this IPubSubBuilder builder,  
        Action<DbContextOptionsBuilder>? optionsAction = null, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : SqlServerPubSubDbContext
    {
        builder.Services.AddScoped<IPubSubStorage, PubSubStorageEntityFramework<T>>();

        builder.Services.AddDbContext<T>(optionsAction, serviceLifetime, serviceLifetime);

        
        return builder;
    }
}