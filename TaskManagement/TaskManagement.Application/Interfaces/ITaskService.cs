using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces
{
    public interface ITaskService
    {
        Task<List<TaskResponseDto>> GetTasksAsync(int currentUserId, string currentUserRole);
        Task<TaskResponseDto?> GetTaskByIdAsync(int id, int currentUserId, string currentUserRole);
        Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto dto);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskDto dto, int currentUserId, string currentUserRole);
        Task<bool> DeleteTaskAsync(int id, string currentUserRole);
    }
}
