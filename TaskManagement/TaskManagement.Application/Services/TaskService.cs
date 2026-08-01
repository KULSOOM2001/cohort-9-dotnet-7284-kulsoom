using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        public async Task<List<TaskResponseDto>> GetTasksAsync(int currentUserId, string currentUserRole)
        {
            List<TaskItem> tasks = currentUserRole == "Admin"
                ? await _taskRepository.GetAllAsync()
                : await _taskRepository.GetByUserIdAsync(currentUserId);

            return tasks.Select(MapToDto).ToList();
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int id, int currentUserId, string currentUserRole)
        {
            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                return null;

            if (currentUserRole != "Admin" && task.AssignedToUserId != currentUserId)
                return null;

            return MapToDto(task);
        }

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto, int currentUserId, string currentUserRole)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Task title is required");

            var assignedToId = currentUserRole == "Admin" ? dto.AssignedToUserId : currentUserId;

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Category = dto.Category,
                DueDate = dto.DueDate,
                AssignedToUserId = assignedToId,
                Status = TaskStatusEnum.Pending
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            var createdTask = await _taskRepository.GetByIdAsync(task.Id);
            if (createdTask == null)
                throw new InvalidOperationException("Task was created but could not be retrieved");

            return MapToDto(createdTask);
        }

        public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto, int currentUserId, string currentUserRole)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                return false;

            if (currentUserRole != "Admin" && task.AssignedToUserId != currentUserId)
                return false;

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = dto.Status;
            task.Priority = dto.Priority;
            task.Category = dto.Category;
            task.DueDate = dto.DueDate;

            // Only Admin can reassign a task to someone else
            if (currentUserRole == "Admin")
                task.AssignedToUserId = dto.AssignedToUserId;

            _taskRepository.Update(task);
            await _taskRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTaskAsync(int id, string currentUserRole)
        {
            if (currentUserRole != "Admin")
                return false;

            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                return false;

            _taskRepository.Delete(task);
            await _taskRepository.SaveChangesAsync();

            return true;
        }

        private static TaskResponseDto MapToDto(TaskItem task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                Priority = task.Priority.ToString(),
                Category = task.Category,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                AssignedToUserId = task.AssignedToUserId,
                AssignedToUserName = task.AssignedToUser?.FullName ?? ""
            };
        }
    }
}