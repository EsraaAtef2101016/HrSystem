using HrSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrSystem.Domain.Entities
{
    public class LeaveBalance
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public User Employee { get; set; } = null!;

        public LeaveType LeaveType { get; set; }
        public int Year { get; set; }
        public int InitialAllowance { get; set; }
        public int UsedDays { get; set; }
        public int ReservedDays { get; set; } // Days blocked by Pending/Approved requests
    }
}
