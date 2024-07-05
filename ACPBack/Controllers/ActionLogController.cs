using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stage_api.Models;

namespace stage_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionLogController : ControllerBase
    {
        private readonly DbContext _context;

        public ActionLogController(DbContext context)
        {
            _context = context;
        }

        [HttpGet("All")]
        public ActionResult<IEnumerable<ActionLog>> GetAllActionLogs()
        {
            var actionLogs = _context.ActionLogs
                .Include("PerformedBy")
                .ToList();

            if (actionLogs == null || actionLogs.Count == 0)
            {
                actionLogs = new List<ActionLog>();
            }

            return actionLogs;
        }
    }
}
