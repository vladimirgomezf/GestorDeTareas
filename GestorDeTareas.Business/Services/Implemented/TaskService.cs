using GestorDeTareas.Business.DTOs;
using GestorDeTareas.Data.Models;
using GestorDeTareas.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestorDeTareas.Business.Services.Implemented
{
    public class TaskService(TasksDbContext dbContext) : ITaskService
    {
        private const int MaximumPageSize = 100;

        public async Task<TaskServiceResult<TaskPage>> ListAsync(TaskListRequest request,
            CancellationToken cancellationToken)
        {
            var estado = request.Estado;
            var responsable = request.Responsable;
            var page = request.Page;
            var pageSize = request.PageSize;
            var errors = new Dictionary<string, string[]>();

            if (page < 1)
            {
                errors[nameof(page)] = ["La página debe ser mayor que cero."];
            }

            if (pageSize is < 1 or > MaximumPageSize)
            {
                errors[nameof(pageSize)] = [$"El tamaño de página debe estar entre 1 y {MaximumPageSize}."];
            }

            if (page >= 1 && pageSize >= 1 && (long)(page - 1) * pageSize > int.MaxValue)
            {
                errors[nameof(page)] = ["La combinación de página y tamaño supera el límite admitido."];
            }

            TaskState? state = null;
            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (!TryParseState(estado, out var parsedState))
                {
                    errors[nameof(estado)] = ["El estado debe ser 'pendiente', 'en curso' o 'hecha'."];
                }
                else
                {
                    state = parsedState;
                }
            }

            if (responsable is not null && string.IsNullOrWhiteSpace(responsable))
            {
                errors[nameof(responsable)] = ["El responsable no puede estar vacío."];
            }

            if (errors.Count > 0)
            {
                return TaskServiceResult<TaskPage>.Invalid(errors);
            }

            var query = dbContext.Tasks.AsNoTracking();
            if (state.HasValue)
            {
                query = query.Where(task => task.State == state.Value);
            }

            if (!string.IsNullOrWhiteSpace(responsable))
            {
                var normalizedResponsible = responsable.Trim().ToLowerInvariant();
                query = query.Where(task => task.Responsible.ToLower() == normalizedResponsible);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var tasks = await query
                .OrderBy(task => task.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            var items = tasks.Select(ToResponse).ToArray();

            return TaskServiceResult<TaskPage>.Success(new TaskPage(items, page, pageSize, totalCount));
        }

        public async Task<TaskServiceResult<TaskResponse>> GetAsync(int id, CancellationToken cancellationToken)
        {
            var task = await dbContext.Tasks.AsNoTracking().SingleOrDefaultAsync(task => task.Id == id, cancellationToken);
            return task is null
                ? TaskServiceResult<TaskResponse>.Missing()
                : TaskServiceResult<TaskResponse>.Success(ToResponse(task));
        }

        public async Task<TaskServiceResult<TaskResponse>> CreateAsync(TaskRequest request, CancellationToken cancellationToken)
        {
            if (!TryCreateTask(request, out var task, out var errors))
            {
                return TaskServiceResult<TaskResponse>.Invalid(errors);
            }

            dbContext.Tasks.Add(task);
            await dbContext.SaveChangesAsync(cancellationToken);

            return TaskServiceResult<TaskResponse>.Success(ToResponse(task));
        }

        public async Task<TaskServiceResult<TaskResponse>> UpdateAsync(
            int id,
            TaskRequest request,
            CancellationToken cancellationToken)
        {
            var task = await dbContext.Tasks.SingleOrDefaultAsync(task => task.Id == id, cancellationToken);
            if (task is null)
            {
                return TaskServiceResult<TaskResponse>.Missing();
            }

            if (!TryCreateTask(request, out var updatedTask, out var errors))
            {
                return TaskServiceResult<TaskResponse>.Invalid(errors);
            }

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.State = updatedTask.State;
            task.Responsible = updatedTask.Responsible;
            task.DueDate = updatedTask.DueDate;
            await dbContext.SaveChangesAsync(cancellationToken);

            return TaskServiceResult<TaskResponse>.Success(ToResponse(task));
        }

        public async Task<TaskServiceResult<bool>> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var task = await dbContext.Tasks.SingleOrDefaultAsync(task => task.Id == id, cancellationToken);
            if (task is null)
            {
                return TaskServiceResult<bool>.Missing();
            }

            dbContext.Tasks.Remove(task);
            await dbContext.SaveChangesAsync(cancellationToken);
            return TaskServiceResult<bool>.Success(true);
        }

        private static bool TryCreateTask(TaskRequest request, out TaskItem task, out Dictionary<string, string[]> errors)
        {
            errors = new Dictionary<string, string[]>();
            ValidateText(request.Title, "title", 200, errors);
            ValidateText(request.Description, "description", 2000, errors);
            ValidateText(request.Responsible, "responsible", 120, errors);

            if (!TryParseState(request.State, out var state))
            {
                errors["state"] = ["El estado debe ser 'pendiente', 'en curso' o 'hecha'."];
            }

            if (request.DueDate is null)
            {
                errors["dueDate"] = ["La fecha límite es obligatoria y debe tener formato YYYY-MM-DD."];
            }

            if (errors.Count > 0)
            {
                task = null!;
                return false;
            }

            task = new TaskItem
            {
                Title = request.Title!.Trim(),
                Description = request.Description!.Trim(),
                State = state,
                Responsible = request.Responsible!.Trim(),
                DueDate = request.DueDate!.Value
            };
            return true;
        }

        private static void ValidateText(string? value, string field, int maximumLength, IDictionary<string, string[]> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors[field] = ["El campo es obligatorio."];
            }
            else if (value.Trim().Length > maximumLength)
            {
                errors[field] = [$"El campo no puede superar {maximumLength} caracteres."];
            }
        }

        private static bool TryParseState(string? value, out TaskState state)
        {
            state = value?.Trim().ToLowerInvariant() switch
            {
                "pendiente" => TaskState.Pending,
                "en curso" => TaskState.InProgress,
                "hecha" => TaskState.Done,
                _ => (TaskState)(-1)
            };

            return Enum.IsDefined(state);
        }

        private static TaskResponse ToResponse(TaskItem task) => new(
            task.Id,
            task.Title,
            task.Description,
            task.State switch
            {
                TaskState.Pending => "pendiente",
                TaskState.InProgress => "en curso",
                TaskState.Done => "hecha",
                _ => throw new InvalidOperationException("Estado de tarea desconocido.")
            },
            task.Responsible,
            task.DueDate);
    }
}