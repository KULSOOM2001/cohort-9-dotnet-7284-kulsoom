using System;
using System.Collections.Generic;
using System.Linq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;

        public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            ArgumentNullException.ThrowIfNull(taskRepository);
            ArgumentNullException.ThrowIfNull(userRepository);

            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public async Task<List<TaskResponseDto>> GetTasksAsync(
            int currentUserId,
            string currentUserRole)
        {
            if (string.IsNullOrWhiteSpace(currentUserRole))
                throw new ArgumentException("User role is required", nameof(currentUserRole));

            List<TaskItem> tasks = currentUserRole == "Admin"
                ? await _taskRepository.GetAllAsync()
                : await _taskRepository.GetByUserIdAsync(currentUserId);

            return tasks.Select(MapToDto).ToList();
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(
            int id,
            int currentUserId,
            string currentUserRole)
        {
            if (string.IsNullOrWhiteSpace(currentUserRole))
                throw new ArgumentException("User role is required", nameof(currentUserRole));

            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                return null;

            if (currentUserRole != "Admin" &&
                task.AssignedToUserId != currentUserId)
                return null;

            return MapToDto(task);
        }

        public async Task<TaskResponseDto> CreateTaskAsync(
            CreateTaskDto dto,
            int currentUserId,
            string currentUserRole)
        {
            if (string.IsNullOrWhiteSpace(currentUserRole))
                throw new ArgumentException("User role is required", nameof(currentUserRole));

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Task title is required");

            if (!Enum.IsDefined(typeof(TaskPriority), dto.Priority))
                throw new ArgumentException("Invalid priority value");

            if (!Enum.IsDefined(typeof(TaskStatusEnum), dto.Status))
                throw new ArgumentException("Invalid status value");

            var assignedToId = currentUserRole == "Admin"
                ? dto.AssignedToUserId
                : currentUserId;

            var assignedUser = await _userRepository.GetByIdAsync(assignedToId);

            if (assignedUser == null)
                throw new ArgumentException("Assigned user does not exist");

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Priority = dto.Priority,
                Category = dto.Category,
                DueDate = dto.DueDate,
                AssignedToUserId = assignedToId,
                Status = dto.Status
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            var createdTask = await _taskRepository.GetByIdAsync(task.Id);

            if (createdTask == null)
                throw new InvalidOperationException(
                    "Task was created but could not be retrieved");

            return MapToDto(createdTask);
        }

        public async Task<bool> UpdateTaskAsync(
            int id,
            UpdateTaskDto dto,
            int currentUserId,
            string currentUserRole)
        {
            if (string.IsNullOrWhiteSpace(currentUserRole))
                throw new ArgumentException("User role is required", nameof(currentUserRole));

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var task = await _taskRepository.GetByIdAsync(id);

            if (task == null)
                return false;

            if (currentUserRole != "Admin" &&
                task.AssignedToUserId != currentUserId)
                return false;

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Task title is required");

            if (!Enum.IsDefined(typeof(TaskStatusEnum), dto.Status))
                throw new ArgumentException("Invalid status value");

            if (!Enum.IsDefined(typeof(TaskPriority), dto.Priority))
                throw new ArgumentException("Invalid priority value");

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = dto.Status;
            task.Priority = dto.Priority;
            task.Category = dto.Category;
            task.DueDate = dto.DueDate;

            if (currentUserRole == "Admin")
            {
                var assignedUser =
                    await _userRepository.GetByIdAsync(dto.AssignedToUserId);

                if (assignedUser == null)
                    throw new ArgumentException("Assigned user does not exist");

                task.AssignedToUserId = dto.AssignedToUserId;
            }

            _taskRepository.Update(task);
            await _taskRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteTaskAsync(
            int id,
            string currentUserRole)
        {
            if (string.IsNullOrWhiteSpace(currentUserRole))
                throw new ArgumentException("User role is required", nameof(currentUserRole));

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