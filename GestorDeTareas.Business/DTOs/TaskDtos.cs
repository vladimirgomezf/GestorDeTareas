namespace GestorDeTareas.Business.DTOs
{
    public sealed record TaskRequest(string? Title, string? Description, string? State, string? Responsible, DateOnly? DueDate);

    public sealed record TaskResponse(int Id, string Title, string Description, string State, string Responsible, DateOnly DueDate);

    public sealed record TaskPage(IReadOnlyList<TaskResponse> Items, int Page, int PageSize, int TotalCount);

    public sealed record TaskListRequest
    {
        public string? Estado { get; init; }
        public string? Responsable { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}