﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Models
{
    public class RecordFile
    {
        public string FullName { get; set; }
        public long FileSize { get; set; }
        public DateTime Time { get; set; }
    }
}
