using GestorDeTareas.Business.Services;
using GestorDeTareas.Business.Services.Implemented;
using GestorDeTareas.Data.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var solutionRoot = new DirectoryInfo(AppContext.BaseDirectory);
while (solutionRoot is not null && !File.Exists(Path.Combine(solutionRoot.FullName, "GestorDeTareas.slnx")))
{
    solutionRoot = solutionRoot.Parent;
}

var databaseDirectory = solutionRoot is null
    ? AppContext.BaseDirectory
    : Path.Combine(solutionRoot.FullName, "GestorDeTareas.Data");
Directory.CreateDirectory(databaseDirectory);
var databasePath = Path.Combine(databaseDirectory, "tasks.db");
var connectionString = builder.Configuration.GetConnectionString("Tasks") ?? $"Data Source={databasePath}";
builder.Services.AddDbContext<TasksDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new() { Title = "Gestor de tareas API", Version = "v1" }));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("./v1/swagger.json", "Gestor de tareas API v1"));
}

app.MapControllers();

app.Run();

public partial class Program;
