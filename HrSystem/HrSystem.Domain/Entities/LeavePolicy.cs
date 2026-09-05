using HrSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrSystem.Domain.Entities
{
    public class LeavePolicy
    {
        public Guid Id { get; set; }
        public LeaveType LeaveType { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int AnnualAllowance { get; set; }
        public int MaxConsecutiveDays { get; set; }
        public int MinNoticeDays { get; set; }
        public int BackdateDays { get; set; } // Specific to Sick Leave
        public int Version { get; set; } = 1;
    }
}
