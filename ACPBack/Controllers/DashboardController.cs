using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace stage_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DbContext _context;

        public DashboardController(DbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetStats()
        {
            try
            {
                var totalUsers = _context.Users.Count();
                var totalConversions = _context.ValidationRequests.Count();
                var pendingConversions = _context.ValidationRequests.Where(request => request.IsValidated == false).Count();
                var approvedConversions = _context.ValidationRequests.Where(request => request.IsValidated == true && request.IsApproved == true).Count();
                var rejectedConversions = _context.ValidationRequests.Where(request => request.IsValidated == true && request.IsApproved == false).Count();
                return Ok(new
                {
                    totalUsers,
                    totalConversions,
                    pendingConversions,
                    approvedConversions,
                    rejectedConversions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard stats.", error = ex.Message });
            }
        }

    }
}
