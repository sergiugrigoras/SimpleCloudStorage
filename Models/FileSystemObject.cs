using System;
using System.Collections.Generic;

namespace SimpleCloudStorage.Models
{
    public partial class FileSystemObject
    {
        public FileSystemObject()
        {
            InverseParent = new HashSet<FileSystemObject>();
            Users = new HashSet<User>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? ParentId { get; set; }
        public bool IsFolder { get; set; }

        public virtual FileSystemObject Parent { get; set; }
        public virtual ICollection<FileSystemObject> InverseParent { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
