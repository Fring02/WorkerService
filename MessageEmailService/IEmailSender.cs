using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MessageEmailService
{
    public interface IEmailSender
    {
        Task SendEmail(Message message);
    }
}
