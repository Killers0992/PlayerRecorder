﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayerRecorder.Structs
{
    [ProtoContract]
    public class DelayData : IEventType
    {
        public float DelayTime { get; set; }
    }
}
