namespace GestorDeTareas.Data.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public TaskState State { get; set; }
        public required string Responsible { get; set; }
        public DateOnly DueDate { get; set; }
    }

    public enum TaskState
    {
        Pending,
        InProgress,
        Done
    }
}