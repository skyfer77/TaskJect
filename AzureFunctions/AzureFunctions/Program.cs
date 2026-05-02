using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Data.DbContexts;
using Domain.Database;
using Data.DomainEvent;
using Data;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // AutoMapper
        services.AddAutoMapper(
            typeof(Data.Mapper.DataMappingProfile).Assembly
        );

        services.AddScoped<DomainEventDispatcher>();
        services.AddIdentity<ApplicationUser, IdentityRole>()
      .AddEntityFrameworkStores<ApplicationDbContext>()
      .AddDefaultTokenProviders();

        services.AddScoped<OrganizationUsedStorageFunction>();
        services.AddScoped<TariffEndCheckFunction>();
        services.AddScoped<LockingNewestMembersFunction>();
        services.AddScoped<DeletingOldInfoFunction>();
		    services.AddScoped<RemoveOldNotificationsFunction>();
        services.AddData(context.Configuration);

    })
    .Build();

await host.RunAsync();