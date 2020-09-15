using System;
using System.Collections.Generic;

namespace SimpleCloudStorage.Models
{
    public partial class User
    {
        public User()
        {
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int HomeDirId { get; set; }
        public string UserAccountId { get; set; }

        public virtual FileSystemObject HomeDir { get; set; }
    }
}
