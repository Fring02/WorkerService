using MessageQueueApi.Data;
using MessageQueueApi.Interfaces;
using MessageQueueApi.Models;
using MessageQueueApi.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageQueueApi.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private DataContext context;

        public MessageRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<bool> AddMessage(Message message)
        {
            context.Messages.Add(message);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetUnhandledMessage(MessageType type)
        {
            return await context.Messages.FirstOrDefaultAsync(m => m.Handled == false && m.Type.Equals(type));
        }

        public async Task<bool> Handle(Guid id)
        {
            var message = await context.Messages.FindAsync(id);
            message.Handled = true;
            message.Type = MessageType.Log;
            return await context.SaveChangesAsync() > 0;
        }
    }
}
