using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;

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

        builder.Services.AddDbContext<T>(optionsAction);

        return builder;
    }
}