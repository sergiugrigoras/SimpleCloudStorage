using System;
using System.Collections.Generic;

namespace SimpleCloudStorage.Models
{
    public partial class Note
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreationDate { get; set; }

        public virtual User User { get; set; }
    }
}
