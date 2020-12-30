using MessageQueueApi.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueApi.DTOs
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public bool Handled { get; set; }
        public MessageType Type { get; set; }
        public string Text { get; set; }
    }
}
