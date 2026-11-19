using GestorDeTareas.Business.DTOs;

namespace GestorDeTareas.Business.Services;

public interface ITaskService
{
    Task<TaskServiceResult<TaskPage>> ListAsync(TaskListRequest request,
        CancellationToken cancellationToken);

    Task<TaskServiceResult<TaskResponse>> GetAsync(int id, CancellationToken cancellationToken);
    Task<TaskServiceResult<TaskResponse>> CreateAsync(TaskRequest request, CancellationToken cancellationToken);

    Task<TaskServiceResult<TaskResponse>> UpdateAsync(
        int id,
        TaskRequest request,
        CancellationToken cancellationToken);

    Task<TaskServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken);
}