﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zvs.Entities
{
    public partial class QueuedDeviceCommandBase : QueuedCommand
    {
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }
    }
}