using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SendGrid.Helpers.Mail;
using SendGrid;
using stage_api.configuration;
using stage_api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using stage_api.DTO;
using System.Diagnostics;
using System.Net.Mail;

namespace stage_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationUserController : ControllerBase
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationSettings _appSettings;
        private readonly DbContext _context;

        public ApplicationUserController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IOptions<ApplicationSettings> appSettings, DbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
            _context = context;
        }

        // GET: api/GetUsers
        [HttpGet]
        [Route("GetUsers")]
        public async Task<ActionResult<IEnumerable<ApplicationUser>>> getUsers()
        {
            if (_context.ApplicationUsers == null)
            {
                return NotFound();
            }
            return await _context.ApplicationUsers.ToListAsync();
        }

		[HttpPut]
		[Route("ActivateAccount/{id}")]
		public async Task<Object> ActivateAccount(string id)
		{
			try
			{
			    var user = _context.Users.FirstOrDefault(u => u.Id == id);
                user.LockoutEnabled = true;
                _context.SaveChanges();

				var client = new SendGridClient(_appSettings.SendGrid_API_Key);
				var from_email = new EmailAddress(_appSettings.SendGrid_Sender_Email, "ACP Notifications");
				var subject = "Your account has been activated.";
				var to_email = new EmailAddress(user.Email, user.FullName);
				var plainTextContent = $"Your account has been activated, you can now sign in to the platform.";
				var htmlContent = $"<strong>Your account has been activated, you can now sign in to the platform.</strong>";
				var msg = MailHelper.CreateSingleEmail(from_email, to_email, subject, plainTextContent, htmlContent);
				var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

				return Ok();

			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		[HttpPut]
		[Route("DeactivateAccount/{id}")]
		public async Task<Object> DeactivetAccount(string id)
		{
			try
			{
				var user = _context.Users.FirstOrDefault(u => u.Id == id);
				user.LockoutEnabled = false;
				_context.SaveChanges();
				return Ok();

			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		[HttpPost]
        [Route("Register")]
        //POST : /api/ApplicationUser/Register
        public async Task<Object> PostApplicationUser(ApplicationUserModel model)
        {
            model.Role = "USER";
            var applicationUser = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            try
            {
                var result = await _userManager.CreateAsync(applicationUser, model.Password);
                await _userManager.AddToRoleAsync(applicationUser, model.Role);

                var user = _context.Users.FirstOrDefault(u => u.Email == applicationUser.Email);
                user.LockoutEnabled = false; // par defaut désactivé
                _context.SaveChanges();

                var performedBy = _context.Users.FirstOrDefault(u => u.Id == applicationUser.Id);
                var log = new ActionLog
                {
                    Name = "Account creation",
                    PerformedBy = performedBy,
                    CreatedAt = DateTime.Now,
                };
                _context.ActionLogs.Add(log);
                _context.SaveChanges();

                return Ok(result);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpPost]
        [Route("Login")]
        //POST : /api/ApplicationUser/Login
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {

                if (!user.LockoutEnabled)
                {
					return BadRequest(new { message = "Your account is locked, please contact your administrator." });
				}

                //Get role assigned to the user
                var role = await _userManager.GetRolesAsync(user);
                IdentityOptions _options = new IdentityOptions();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim("UserID",user.Id.ToString()),
                        new Claim(_options.ClaimsIdentity.RoleClaimType,role.FirstOrDefault()),
                        new Claim("Fullname", user.FullName)
                    }),
                    Expires = DateTime.Now.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var token = tokenHandler.WriteToken(securityToken);
                return Ok(new { token });



            }
            else
                return BadRequest(new { message = "Username or password is incorrect. " });
        }

        [HttpPut]
        [Route("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromQuery] string updatedBy, [FromBody] ApplicationUserModel model)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            user.Email = model.Email;
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var message = "User updated successfully";

                var performedBy = _context.Users.FirstOrDefault(u => u.Id == updatedBy);
                var log = new ActionLog
                {
                    Name = "Updating account with email[" + user.Email + ']',
                    PerformedBy = performedBy,
                    CreatedAt = DateTime.Now,
                };
                _context.ActionLogs.Add(log);
                _context.SaveChanges();
                return Ok(new { message });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPost]
        [Route("RequestPasswordReset/{username}")]
        public async Task<IActionResult> PasswordReset(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return BadRequest(new { message = "Username does not exist." });
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("UserID", user.Id.ToString())
                }),
                Expires = DateTime.Now.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);
            var resetLink = $"http://localhost:4200/auth/reset-password?token={token}";

            var client = new SendGridClient(_appSettings.SendGrid_API_Key);
            var from = new EmailAddress(_appSettings.SendGrid_Sender_Email, "ACP Notifications");
            var subject = "Password Reset Request";
            var to = new EmailAddress(user.Email, user.FullName);
            var plainTextContent = $"Please use the following link to reset your password: {resetLink}";
            var htmlContent = $"<strong>Please use the following link to reset your password: <a href='{resetLink}'>Reset Password</a></strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
            Debug.WriteLine(from.Email);
            Debug.WriteLine(to.Email);
            Debug.WriteLine(response.StatusCode.ToString());
            Debug.WriteLine(response.Body.ToString());

            return Ok(new { message = "Password reset email sent." });
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken;
            try
            {
                var principal = tokenHandler.ValidateToken(model.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWT_Secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero // Override the default clock skew of 5 mins
                }, out validatedToken);

                var userId = principal.FindFirst("UserID")?.Value;
                if (userId == null)
                {
                    return BadRequest(new { message = "Invalid token." });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return BadRequest(new { message = "User not found." });
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

                if (result.Succeeded)
                {
                    var log = new ActionLog
                    {
                        Name = "Password reset",
                        PerformedBy = user,
                        CreatedAt = DateTime.Now,
                    };
                    _context.ActionLogs.Add(log);
                    _context.SaveChanges();

                    return Ok(new { message = "Password has been reset successfully." });
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid token.", error = ex.Message });
            }
        }
    }
}
