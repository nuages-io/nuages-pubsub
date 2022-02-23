using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nuages.PubSub.Services;

namespace Nuages.PubSub.Storage.EntityFramework.SqlServer;

public static class PubSubSqlServerConfigExtensions
{
    public static IPubSubBuilder AddPubSubSqlServerStorage(this IPubSubBuilder builder,
        Action<DbContextOptionsBuilder>? optionsAction = null)
    { 
        return AddPubSubSqlServerStorage<SqlServerPubSubDbContext>(builder, optionsAction);
    }
    
    public static IPubSubBuilder AddPubSubSqlServerStorage<T>(this IPubSubBuilder builder,  
        Action<DbContextOptionsBuilder>? optionsAction = null) where T : SqlServerPubSubDbContext
    {
        builder.Services.AddScoped<IPubSubStorage, PubSubStorageEntityFramework<T>>();

        builder.Services.AddDbContext<T>(optionsAction);

        return builder;
    }
}