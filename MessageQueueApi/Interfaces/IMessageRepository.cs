using MessageQueueApi.Models;
using MessageQueueApi.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueApi.Interfaces
{
    public interface IMessageRepository
    {
        public Task<bool> AddMessage(Message message);
        public Task<bool> Handle(Guid id);
        public Task<Message> GetUnhandledMessage(MessageType type);
    }
}
