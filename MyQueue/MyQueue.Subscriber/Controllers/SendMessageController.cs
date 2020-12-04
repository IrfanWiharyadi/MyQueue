using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyQueue.Publisher.Services;

namespace MyQueue.Publisher.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SendMessageController : ControllerBase
    {

        private readonly ILogger<SendMessageController> _logger;
        private readonly IMessageService _messageService;

        public SendMessageController(ILogger<SendMessageController> logger, IMessageService messageService)
        {
            _logger = logger;
            _messageService = messageService;
        }

        [HttpPost]
        public async Task<string> SendMessage([FromQuery]string message)
        {
            if(await _messageService.PublishAsync(message))
                return $"Success:{message}";
            else
                return $"Error";
        }
    }
}
