using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using stage_api.Models;
using System.Data;

namespace stage_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private UserManager<ApplicationUser> _UserManager;
        public UserProfileController(UserManager<ApplicationUser> userManager)
        {
            _UserManager = userManager;
        }

        [HttpGet]
        [Authorize]
        //Get : /api/UserProfile
        public async Task<Object> GetUserProfile()
        {
            string userId = User.Claims.First(c => c.Type == "UserID").Value;
            var user = await _UserManager.FindByIdAsync(userId);
            return new
            {
                user.FullName,
                user.Email,
                user.UserName,
                user.PhoneNumber
            };
        }

        [HttpGet]
        [Authorize(Roles = "Agent")]
        [Route("ForAgent")]
        public string GetForAgent()
        {
            return "web method for Agent";
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        [Route("ForCustomer")]
        public string GetForCustomer()
        {
            return "web method for customer";
        }
    }
}
