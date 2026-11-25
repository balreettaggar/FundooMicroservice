using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Fundoo.Notification.Application;

public class PasswordResetRequestedEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();

    public string RecipientEmail { get; set; } = string.Empty;

    public string ResetToken { get; set; } = string.Empty;

    public DateTimeOffset OccurredAtUtc { get; set; }
        = DateTimeOffset.UtcNow;
}

