using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.Notes.Application.Interfaces;

public sealed class UserDirectoryUnavailableException : Exception
{
    public UserDirectoryUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
