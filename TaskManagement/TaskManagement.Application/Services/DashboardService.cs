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
    public class DashboardService : IDashboardService
    {
        private readonly ITaskRepository _taskRepository;

        public DashboardService(ITaskRepository taskRepository)
        {
            ArgumentNullException.ThrowIfNull(taskRepository);
            _taskRepository = taskRepository;
        }

        public async Task<DashboardDto> GetDashboardAsync(int currentUserId, string currentUserRole)
        {
            var tasks = currentUserRole == "Admin"
                ? await _taskRepository.GetAllAsync()
                : await _taskRepository.GetByUserIdAsync(currentUserId);

            return new DashboardDto
            {
                PendingCount = tasks.Count(t => t.Status == TaskStatusEnum.Pending),
                InProgressCount = tasks.Count(t => t.Status == TaskStatusEnum.InProgress),
                CompletedCount = tasks.Count(t => t.Status == TaskStatusEnum.Completed),
                TotalCount = tasks.Count
            };
        }
    }
}
