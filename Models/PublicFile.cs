using System;
using System.Collections.Generic;

namespace SimpleCloudStorage.Models
{
    public partial class PublicFile
    {
        public int FromUserId { get; set; }
        public int FsoId { get; set; }
        public string PublicId { get; set; }
        public DateTime SharedDate { get; set; }
        public virtual FileSystemObject Fso { get; set; }
        public virtual User FromUser { get; set; }
    }
}
