﻿using System;
using System.Collections.Generic;

namespace SimpleCloudStorage.Models
{
    public partial class Share
    {
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public int FsoId { get; set; }
        public DateTime SharedDate { get; set; }

        public virtual FileSystemObject Fso { get; set; }
        public virtual User FromUser { get; set; }
        public virtual User ToUser { get; set; }
    }
}
