using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.DTOs
{
    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public int AssignedToUserId { get; set; }
    }

    public class UpdateTaskDto
    {
        public string Title { get; set; }
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
        public string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AssignedToUserId { get; set; }
        public string AssignedToUserName { get; set; }
    }
}
