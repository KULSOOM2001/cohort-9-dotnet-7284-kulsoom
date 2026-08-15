using System.ComponentModel.DataAnnotations;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.DTOs
{
    public class CreateTaskDto
    {
        [Required, StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public int AssignedToUserId { get; set; }
    }

    public class UpdateTaskDto
    {
        [Required, StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public TaskStatusEnum Status { get; set; }
        public TaskPriority Priority { get; set; }
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public int AssignedToUserId { get; set; }
    }

    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; } = string.Empty;
    }
}