using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fundoo.Notes.Domain.Entities
{

    public class Note
    {
        public Guid NoteId { get; set; }

        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Color { get; set; } = "#FFFFFF";

        public bool IsArchived { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsPinned { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }

}
