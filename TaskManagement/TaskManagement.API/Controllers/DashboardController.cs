using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            ArgumentNullException.ThrowIfNull(dashboardService);
            _dashboardService = dashboardService;
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out var userId))
                throw new UnauthorizedAccessException("User identity claim is missing or invalid");
            return userId;
        }

        private string GetCurrentUserRole() =>
            User.FindFirstValue(ClaimTypes.Role) ?? "User";

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboard = await _dashboardService.GetDashboardAsync(GetCurrentUserId(), GetCurrentUserRole());
            return Ok(dashboard);
        }
    }
}