using System;
using System.Collections.Generic;

namespace SimpleCloudStorage.Models
{
    public partial class File
    {

        public int Id { get; set; }
        public int FsoId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }

        public virtual FileSystemObject Fso { get; set; }
    }
}
