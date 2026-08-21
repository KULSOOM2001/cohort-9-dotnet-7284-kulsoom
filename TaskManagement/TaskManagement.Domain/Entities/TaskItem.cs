namespace TaskManagement.Domain.Entities
{
    public enum TaskStatusEnum
    {
        Pending,
        InProgress,
        Completed
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }

    public class TaskItem
    {
        private string _title = string.Empty;
        private User _assignedToUser = null!;

        public int Id { get; set; }

        public required string Title
        {
            get => _title;
            set => _title = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Title cannot be null or empty.", nameof(value))
                : value;
        }

        public string? Description { get; set; }
        public TaskStatusEnum Status { get; set; } = TaskStatusEnum.Pending;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int AssignedToUserId { get; set; }

        public required User AssignedToUser
        {
            get => _assignedToUser;
            set => _assignedToUser = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}