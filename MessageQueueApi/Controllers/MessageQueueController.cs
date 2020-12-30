using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageQueueApi.DTOs;
using MessageQueueApi.Interfaces;
using MessageQueueApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MessageQueueApi.Models.Enums;
namespace MessageQueueApi.Controllers
{
    [ApiController]
    [Route("api/queue/")]
    public class MessageQueueController : ControllerBase
    {
        private readonly IMessageRepository messages;

        public MessageQueueController(IMessageRepository messages)
        {
            this.messages = messages;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddMessageAsync(MessageDto mes)
        {
            var message = new Message
            {
                Type = mes.Type,
                Text = mes.Text
            };
            if(await messages.AddMessage(message)) return Ok("Added new message");
            return BadRequest("Failed to add message");
        }
        [HttpGet("handled/{id}")]
        public async Task<IActionResult> HandleMessageByIdAsync(Guid id)
        {
            if (await messages.Handle(id)) return Ok($"Handled message by id {id}");

            return BadRequest("Failed to handle message");
        }
        [HttpGet("retrieve/email")]
        public async Task<MessageDto> GetMessagesByEmailAsync()
        {
            var message = await messages.GetUnhandledMessage(MessageType.Email);
            if (message == null) return null;
            return new MessageDto
            {
                Id = message.Id,
                Handled = message.Handled,
                Text = message.Text,
                Type = message.Type
            };
        }
        [HttpGet("retrieve/log")]
        public async Task<MessageDto> GetMessagesByLogAsync()
        {
            var message = await messages.GetUnhandledMessage(MessageType.Log);
            if (message == null) return null;
            return new MessageDto
            {
                Id = message.Id,
                Handled = message.Handled,
                Text = message.Text,
                Type = message.Type
            };
        }
    }
}
