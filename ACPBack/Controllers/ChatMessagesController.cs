using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using stage_api.DTO;
using stage_api.Models;

namespace stage_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatMessagesController : ControllerBase
    {
        private readonly DbContext _context;

        public ChatMessagesController(DbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetChatMessages()
        {
            try
            {
                var messages = _context
                    .ChatMessages
                    .Select(m => new
                    {
                        Id = m.Id,
                        Message = m.Message,
                        OwnerId = m.Owner.Id,
                        CreationDate = m.CreationDate
                    })
                    .ToList();
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving chat messages.", error = ex.Message });
            }
        }

        // endpoint = api
        // POST: api/ChatMessages
        [HttpPost]
        public IActionResult PostChatMessage([FromBody] ChatMessageDto messageDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = _context.Users.FirstOrDefault(u => u.Id == messageDto.OwnerId);
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {messageDto.OwnerId} not found." });
                }

                ChatMessage newMessage = new ChatMessage()
                {
                    Message = messageDto.Message,
                    Owner = user,
                    CreationDate = DateTime.Now // Set creation date
                };

                _context.ChatMessages.Add(newMessage);
                _context.SaveChanges();

                // Return the created ChatMessage instance
                return CreatedAtAction(nameof(GetChatMessages), new { id = newMessage.Id }, newMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating a chat message.", error = ex.Message });
            }
        }
    }
}
