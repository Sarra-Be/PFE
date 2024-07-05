using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using SendGrid;
using stage_api.DTO;
using stage_api.Models;
using System.Diagnostics;
using System.Drawing.Printing;
using stage_api.configuration;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace stage_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidationRequestController : ControllerBase
    {
        private readonly DbContext _context;
        private readonly ApplicationSettings _appSettings;


        public ValidationRequestController(DbContext context, IOptions<ApplicationSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        // POST: api/ValidationRequest
        [HttpPost]
        public ActionResult<ValidationRequest> CreateValidationRequestAsync(CreateValidationRequestDto validationRequestDto)
        {
            // Retrieve the user by Id
            var requestedBy = _context.Users.FirstOrDefault(u => u.Id == validationRequestDto.RequestById);
            if (requestedBy == null)
            {
                return NotFound("User not found");
            }

            // Create ValidationRequest entity
            var validationRequest = new ValidationRequest
            {
                FileName = validationRequestDto.FileName,
                TargetTableName = validationRequestDto.TargetTableName,
                AttributeMappingStr = validationRequestDto.AttributeMappingStr,
                FileJsonStrContent = validationRequestDto.FileJsonStrContent,
                RequestedBy = requestedBy,
                CreationDate = DateTime.Now,
                IsValidated = false
            };

            if (validationRequestDto.IsAdmin)
            {
                validationRequest.IsValidated = true;
                validationRequest.IsApproved = true;
                WriteDataToTargetTable(validationRequestDto.TargetTableName, validationRequestDto.AttributeMappingStr, validationRequestDto.FileJsonStrContent);
            }

            _context.ValidationRequests.Add(validationRequest);
            _context.SaveChanges();

            var log = new ActionLog
            {
                Name = "Create Validation Request",
                PerformedBy = requestedBy,
                CreatedAt = DateTime.Now,
            };
            _context.ActionLogs.Add(log);
            _context.SaveChanges();

            var message = "Validation reqeust sent";
            return Ok(new { message });
        }

        // PUT: api/ValidationRequest/5/Reject
        [HttpPut("{id}/Reject")]
        public async Task<IActionResult> RejectValidationRequestAsync(int id)
        {
            var validationRequest = _context.ValidationRequests
                .Include("RequestedBy")
                .Where(vr => vr.Id == id)
                .ToList().First();

            if (validationRequest == null)
            {
                return NotFound();
            }

            // Update validation request status to rejected
            validationRequest.IsValidated = true;
            validationRequest.IsApproved = false;
            validationRequest.ValidationDate = DateTime.Now;

            _context.Entry(validationRequest).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();

                var performedBy = _context.Users.FirstOrDefault(u => u.UserName == "admin");
                var log = new ActionLog
                {
                    Name = "Reject file conversion request",
                    PerformedBy = performedBy,
                    CreatedAt = DateTime.Now,
                };
                _context.ActionLogs.Add(log);
                _context.SaveChanges();

                var client = new SendGridClient(_appSettings.SendGrid_API_Key);
                var from_email = new EmailAddress(_appSettings.SendGrid_Sender_Email, "ACP Notifications");
                var subject = "Your file import request has been rejected.";
                var to_email = new EmailAddress(validationRequest.RequestedBy.Email, validationRequest.RequestedBy.FullName);
                var plainTextContent = $"Your request to import file {validationRequest.FileName} within the {validationRequest.TargetTableName} table has been rejected.";
                var htmlContent = $"<strong>Your request to import file {validationRequest.FileName} within the {validationRequest.TargetTableName} table has been rejected.</strong>";
                var msg = MailHelper.CreateSingleEmail(from_email, to_email, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ValidationRequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/ValidationRequest/5/Approve
        [HttpPut("{id}/Approve")]
        public async Task<IActionResult> ApproveValidationRequestAsync(int id)
        {
            var validationRequest = _context.ValidationRequests
                .Include("RequestedBy")
                .Where(vr => vr.Id == id)
                .ToList().First();

            if (validationRequest == null)
            {
                return NotFound();
            }

            // Update validation request status to approved
            validationRequest.IsValidated = true;
            validationRequest.IsApproved = true;
            validationRequest.ValidationDate = DateTime.Now;

            _context.Entry(validationRequest).State = EntityState.Modified;

            try
            {
                _context.SaveChanges();

                WriteDataToTargetTable(validationRequest.TargetTableName, validationRequest.AttributeMappingStr, validationRequest.FileJsonStrContent);

                var performedBy = _context.Users.FirstOrDefault(u => u.UserName == "admin");
                var log = new ActionLog
                {
                    Name = "Approve file conversion request",
                    PerformedBy = performedBy,
                    CreatedAt = DateTime.Now,
                };
                _context.ActionLogs.Add(log);
                _context.SaveChanges();

                var client = new SendGridClient(_appSettings.SendGrid_API_Key);
                var from_email = new EmailAddress(_appSettings.SendGrid_Sender_Email, "ACP Notifications");
                var subject = "Your file import request has been approved.";
                var to_email = new EmailAddress(validationRequest.RequestedBy.Email, validationRequest.RequestedBy.FullName);
                var plainTextContent = $"Your request to import file {validationRequest.FileName} within the {validationRequest.TargetTableName} table has been approved.";
                var htmlContent = $"<strong>Your request to import file {validationRequest.FileName} within the {validationRequest.TargetTableName} table has been approved.</strong>";
                var msg = MailHelper.CreateSingleEmail(from_email, to_email, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ValidationRequestExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // GET: api/ValidationRequest
        [HttpGet("All")]
        public ActionResult<IEnumerable<ValidationRequest>> GetAllValidationRequests()
        {
            var validationRequests = _context.ValidationRequests
                .Include("RequestedBy")
                .ToList();

            if (validationRequests == null || validationRequests.Count == 0)
            {
                validationRequests = new List<ValidationRequest>();
            }

            return validationRequests;
        }

        // GET: api/ValidationRequest/User/5
        [HttpGet("User/{userId}")]
        public ActionResult<IEnumerable<ValidationRequest>> GetValidationRequestsForUser(string userId)
        {
            var validationRequests = _context.ValidationRequests
                .Where(vr => vr.RequestedBy.Id == userId)
                .Include("RequestedBy")
                .ToList();
            if (validationRequests == null || validationRequests.Count == 0)
            {
                validationRequests = new List<ValidationRequest>();
            }

            return validationRequests;
        }

        private void WriteDataToTargetTable(string targetTableName, string attributeMappingStr, string fileJsonStrContent)
        {
            Dictionary<string, string> attributeMapping = JsonConvert.DeserializeObject<Dictionary<string, string>>(attributeMappingStr);
            List<Dictionary<string, object>> fileContent = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(fileJsonStrContent);

            string connectionString = "Server=(localdb)\\mssqllocaldb;Database=stage_api.configuration;Trusted_Connection=True;MultipleActiveResultSets=true";

            using (var connection = new SqlConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                foreach (var data in fileContent)
                {

                    Dictionary<string, object> modifiedData = new Dictionary<string, object>();

                    foreach (var element in data)
                    {
                        var mapping = attributeMapping.FirstOrDefault(el => el.Value == element.Key);

                        if (mapping.Key != null)
                        {
                            modifiedData.Add(mapping.Key, element.Value);
                        }
                    }

                    if (ValueExists(connection, targetTableName, modifiedData))
                    {
                        // Skip insertion if the value already exists
                        continue;
                    }

                    modifiedData.Remove("Id");
                    var id = GenerateRandomNumber();

                    // Add the Id to the dictionary
                    modifiedData.Add("Id", id);

                    var values = modifiedData.Values.Select(value => $"'{value}'");


                    // Construct SQL command to insert data into the specified table
                    string sqlCommand = $"INSERT INTO {targetTableName} ({string.Join(",", modifiedData.Keys)}) VALUES ({string.Join(",", values)})";

                    // Create and execute the command
                    using (var command = new SqlCommand(sqlCommand, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private int GenerateRandomNumber()
        {
            Random rnd = new Random();
            return rnd.Next(1, 100000000); // Adjust the range as needed
        }

        private bool ValueExists(SqlConnection connection, string tableName, Dictionary<string, object> data)
        {
            // Construct a SQL command to check if the value exists
            string sqlCommand = $"SELECT COUNT(*) FROM {tableName} WHERE ";

            // Construct WHERE clause to check each key-value pair
            var whereClause = new List<string>();
            foreach (var pair in data)
            {
                whereClause.Add($"{pair.Key} = '{pair.Value}'");
            }
            sqlCommand += string.Join(" AND ", whereClause);

            // Execute the command to check if the value exists
            using (var command = new SqlCommand(sqlCommand, connection))
            {
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private bool ValidationRequestExists(int id)
        {
            return _context.ValidationRequests.Any(e => e.Id == id);
        }
    }
}
