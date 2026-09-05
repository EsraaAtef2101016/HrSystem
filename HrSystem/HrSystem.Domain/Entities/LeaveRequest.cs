using HrSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrSystem.Domain.Entities
{
    public class LeaveRequest
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public User Employee { get; set; } = null!;

        public LeaveType LeaveType { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        public int ChargedDays { get; set; }
        public string? RejectionReason { get; set; }

        // Policy Snapshot fields to preserve rules at the time of submission
        public int PolicyVersionSnapshot { get; set; }
        public decimal PolicyAllowanceSnapshot { get; set; }
        public Guid LeavePolicyId { get; set; }
        public LeavePolicy LeavePolicy { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
