using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.Notification.Application
{
    public class SendAccessTokenEmailRequest
    {
        public string RecipientEmail { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
    }
}
