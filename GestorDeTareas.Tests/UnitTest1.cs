using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GestorDeTareas.Business.DTOs;
using GestorDeTareas.Business.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GestorDeTareas.Tests;

public class TaskEndpointsTests
{
    [Fact]
    public async Task CrudEndpoints_CreateReadUpdateAndDeleteTask()
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/tasks", ValidRequest());

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);
        Assert.EndsWith($"/api/tasks/{created.Id}", createResponse.Headers.Location!.ToString());
        Assert.Equal("Tarea de prueba", created.Title);

        var getResponse = await client.GetAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await client.PutAsJsonAsync($"/api/tasks/{created.Id}", ValidRequest("Actualizada", "hecha"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Actualizada", updated.Title);
        Assert.Equal("hecha", updated.State);

        var deleteResponse = await client.DeleteAsync($"/api/tasks/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/tasks/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task CreateAndList_RejectInvalidTaskAndListParameters()
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();

        var invalidTask = await client.PostAsJsonAsync("/api/tasks", ValidRequest(state: "bloqueada") with { Title = " " });
        Assert.Equal(HttpStatusCode.BadRequest, invalidTask.StatusCode);
        var validationErrors = await invalidTask.Content.ReadAsStringAsync();
        Assert.Contains("title", validationErrors);
        Assert.Contains("state", validationErrors);

        var invalidQuery = await client.PostAsJsonAsync("/api/tasks/list", new TaskListRequest { Page = 0, PageSize = 101 });
        Assert.Equal(HttpStatusCode.BadRequest, invalidQuery.StatusCode);

        var invalidOffset = await client.PostAsJsonAsync(
            "/api/tasks/list",
            new TaskListRequest { Page = 2147483647, PageSize = 100 });
        Assert.Equal(HttpStatusCode.BadRequest, invalidOffset.StatusCode);
    }

    [Fact]
    public async Task List_FiltersByStateAndResponsibleAndPaginatesInStableOrder()
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/tasks", ValidRequest("Primera", "hecha", "Ana"));
        await client.PostAsJsonAsync("/api/tasks", ValidRequest("Segunda", "hecha", "ana"));
        await client.PostAsJsonAsync("/api/tasks", ValidRequest("Pendiente", "pendiente", "Ana"));

        var defaultPageResponse = await client.PostAsJsonAsync("/api/tasks/list", new TaskListRequest());
        var defaultPage = await defaultPageResponse.Content.ReadFromJsonAsync<TaskPage>();
        Assert.NotNull(defaultPage);
        Assert.Equal(10, defaultPage.PageSize);

        var firstPageResponse = await client.PostAsJsonAsync(
            "/api/tasks/list",
            new TaskListRequest { Estado = "hecha", Responsable = "ANA", Page = 1, PageSize = 1 });
        var firstPage = await firstPageResponse.Content.ReadFromJsonAsync<TaskPage>();
        Assert.NotNull(firstPage);
        Assert.Equal(2, firstPage.TotalCount);
        Assert.Single(firstPage.Items);
        Assert.Equal("Primera", firstPage.Items[0].Title);

        var secondPageResponse = await client.PostAsJsonAsync(
            "/api/tasks/list",
            new TaskListRequest { Estado = "hecha", Responsable = "ANA", Page = 2, PageSize = 1 });
        var secondPage = await secondPageResponse.Content.ReadFromJsonAsync<TaskPage>();
        Assert.NotNull(secondPage);
        Assert.Equal("Segunda", Assert.Single(secondPage.Items).Title);
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await client.GetAsync("/api/tasks")).StatusCode);
    }

    [Fact]
    public async Task GetAndDelete_ReturnNotFoundForUnknownTask()
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/tasks/999")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync("/api/tasks/999")).StatusCode);
    }

    [Fact]
    public async Task UpdateAndList_ReturnExpectedErrorsForInvalidInputs()
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();
        var createResponse = await client.PostAsJsonAsync("/api/tasks", ValidRequest());
        var created = await createResponse.Content.ReadFromJsonAsync<TaskResponse>();
        Assert.NotNull(created);

        var invalidUpdate = await client.PutAsJsonAsync($"/api/tasks/{created.Id}", ValidRequest() with { State = "bloqueada" });
        Assert.Equal(HttpStatusCode.BadRequest, invalidUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync("/api/tasks/999", ValidRequest())).StatusCode);
        var invalidList = await client.PostAsJsonAsync(
            "/api/tasks/list",
            new TaskListRequest { Estado = "desconocido" });
        Assert.Equal(HttpStatusCode.BadRequest, invalidList.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequestForMalformedJson()
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();
        using var content = new StringContent("{json inválido", Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/tasks", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsBadRequestForMalformedOrMissingBody()
    {
        using var factory = new TaskApiFactory();
        using var client = factory.CreateClient();
        using var malformedContent = new StringContent("{json inválido", Encoding.UTF8, "application/json");
        using var emptyContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        var malformedResponse = await client.PostAsync("/api/tasks/list", malformedContent);
        var missingBodyResponse = await client.PostAsync("/api/tasks/list", emptyContent);

        Assert.Equal(HttpStatusCode.BadRequest, malformedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, missingBodyResponse.StatusCode);
    }

    [Fact]
    public async Task Swagger_IsAvailableInDevelopmentAndDocumentsTaskEndpoints()
    {
        using var factory = new TaskApiFactory(environment: "Development");
        using var client = factory.CreateClient();

        var uiResponse = await client.GetAsync("/swagger/index.html");
        var ui = await uiResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, uiResponse.StatusCode);
        Assert.Contains("Swagger UI", ui);

        var documentResponse = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, documentResponse.StatusCode);
        using var document = JsonDocument.Parse(await documentResponse.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");

        Assert.True(paths.GetProperty("/api/tasks").TryGetProperty("post", out _));
        Assert.True(paths.GetProperty("/api/tasks/{id}").TryGetProperty("get", out _));
        Assert.True(paths.GetProperty("/api/tasks/{id}").TryGetProperty("put", out _));
        Assert.True(paths.GetProperty("/api/tasks/{id}").TryGetProperty("delete", out _));

        var listOperation = paths.GetProperty("/api/tasks/list").GetProperty("post");
        var requestSchema = listOperation
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema");
        Assert.Equal("#/components/schemas/TaskListRequest", requestSchema.GetProperty("$ref").GetString());
    }

    [Fact]
    public async Task Swagger_IsUnavailableOutsideDevelopment()
    {
        using var factory = new TaskApiFactory(environment: "Production");
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/swagger")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/swagger/v1/swagger.json")).StatusCode);
    }

    [Fact]
    public async Task Controller_UsesServiceRegisteredThroughDependencyInjection()
    {
        var service = new StubTaskService();
        using var factory = new TaskApiFactory(services =>
        {
            services.RemoveAll<ITaskService>();
            services.AddSingleton<ITaskService>(service);
        });
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tasks/42");
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(task);
        Assert.Equal("Servicio inyectado", task.Title);
        Assert.Equal(42, service.LastRequestedId);

        var listRequest = new TaskListRequest { Estado = "hecha", Responsable = "Vlad", Page = 3, PageSize = 7 };
        var listResponse = await client.PostAsJsonAsync("/api/tasks/list", listRequest);

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.Equal(listRequest, service.LastListRequest);
    }

    private static TaskRequest ValidRequest(string title = "Tarea de prueba", string state = "pendiente", string responsible = "Ana") =>
        new(title, "Descripción", state, responsible, new DateOnly(2026, 12, 1));

    private sealed class StubTaskService : ITaskService
    {
        public int? LastRequestedId { get; private set; }
        public TaskListRequest? LastListRequest { get; private set; }

        public Task<TaskServiceResult<TaskPage>> ListAsync(TaskListRequest request, CancellationToken cancellationToken)
        {
            LastListRequest = request;
            return Task.FromResult(TaskServiceResult<TaskPage>.Success(
                new TaskPage([], request.Page, request.PageSize, 0)));
        }

        public Task<TaskServiceResult<TaskResponse>> GetAsync(int id, CancellationToken cancellationToken)
        {
            LastRequestedId = id;
            return Task.FromResult(TaskServiceResult<TaskResponse>.Success(
                new TaskResponse(id, "Servicio inyectado", "Descripción", "pendiente", "Ana", new DateOnly(2026, 12, 1))));
        }

        public Task<TaskServiceResult<TaskResponse>> CreateAsync(TaskRequest request, CancellationToken cancellationToken) =>
            Task.FromResult(TaskServiceResult<TaskResponse>.Invalid(new Dictionary<string, string[]>()));

        public Task<TaskServiceResult<TaskResponse>> UpdateAsync(
            int id,
            TaskRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(TaskServiceResult<TaskResponse>.Missing());

        public Task<TaskServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(TaskServiceResult<bool>.Missing());
    }
}
