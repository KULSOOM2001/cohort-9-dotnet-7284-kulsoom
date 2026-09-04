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
            int? userId = currentUserRole == "Admin" ? null : currentUserId;

            var counts = await _taskRepository.GetStatusCountsAsync(userId);

            return new DashboardDto
            {
                PendingCount = counts.GetValueOrDefault(TaskStatusEnum.Pending),
                InProgressCount = counts.GetValueOrDefault(TaskStatusEnum.InProgress),
                CompletedCount = counts.GetValueOrDefault(TaskStatusEnum.Completed),
                TotalCount = counts.Values.Sum()
            };
        }
    }
}