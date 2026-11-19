using GestorDeTareas.Data.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GestorDeTareas.Tests;

public sealed class TaskApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly Action<IServiceCollection>? _configureServices;
    private readonly string _environment;

    public TaskApiFactory(Action<IServiceCollection>? configureServices = null, string environment = "Testing")
    {
        _configureServices = configureServices;
        _environment = environment;
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TasksDbContext>();
            services.RemoveAll<DbContextOptions<TasksDbContext>>();
            services.AddDbContext<TasksDbContext>(options => options.UseSqlite(_connection));
            _configureServices?.Invoke(services);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}