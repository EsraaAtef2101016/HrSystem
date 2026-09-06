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

        public LeaveType LeaveType { get;private set; }
        public int Year { get;private set; }
        public int InitialAllowance { get; set; }
        public int UsedDays { get;private  set; }
        public int ReservedDays { get;private set; } // Days blocked by Pending/Approved requests
        public int RemainingDays => InitialAllowance - (UsedDays + ReservedDays);
        protected LeaveBalance() { }
        public LeaveBalance(Guid employeeId, LeaveType leaveType, int year, int initialAllowance)
        {
            Id = Guid.NewGuid();
            EmployeeId = employeeId;
            LeaveType = leaveType;
            Year = year;
            InitialAllowance = initialAllowance;
            UsedDays = 0;
            ReservedDays = 0;
        }


        public void ReserveDays(int days)
        {
            if (days <= 0)
                throw new ArgumentException("Days to reserve must be greater than zero.", nameof(days));

            if (days > RemainingDays)
                throw new InvalidOperationException("Insufficient leave balance to fulfill the request.");

             ReservedDays += days;
        }
        public void UseDays(int days)
        {
            if (days <= 0)
                throw new ArgumentException("Days to reserve must be greater than zero.", nameof(days));

            if (days > RemainingDays)
                throw new InvalidOperationException("Insufficient leave balance to fulfill the request.");

            UsedDays += days;
        }

        public void ReleaseReservedDays(int days)
        {
            if (days <= 0)
                throw new ArgumentException("Days to release must be greater than zero.", nameof(days));

            if (days > ReservedDays)
                throw new InvalidOperationException("Cannot release more days than currently reserved.");

            ReservedDays -= days;
        }
         public void ReleaseUsedDays(int days)
        {
            if (days <= 0)
                throw new ArgumentException("Days to release must be greater than zero.", nameof(days));

            if (days > UsedDays)
                throw new InvalidOperationException("Cannot release more days than currently reserved.");

            UsedDays -= days;
        }
        
        
        public void CommitReservedToUsed(int days)
        {
            if (days <= 0)
                throw new ArgumentException("Days to commit must be greater than zero.", nameof(days));

            if (days > ReservedDays)
                throw new InvalidOperationException("Cannot commit more days than currently reserved.");

            ReservedDays -= days;
            UsedDays += days;
        }

        public void AdjustAllowance(int newAllowance)
        {
            if (newAllowance < (UsedDays + ReservedDays))
                throw new InvalidOperationException("New allowance cannot be less than the total used and reserved days.");

            InitialAllowance = newAllowance;

        }
        
        public void UpdateReservedDays(int oldDays, int newDays)
        {
            if (oldDays < 0 || newDays < 0)
                throw new ArgumentException("Days cannot be negative.");

            // Temporarily release the old reservation to calculate accurate availability
            ReservedDays -= oldDays;

            if (newDays > RemainingDays)
            {
                // Rollback if the new amount exceeds what is available
                ReservedDays += oldDays;
                throw new InvalidOperationException("Insufficient leave balance for the updated request.");
            }

            ReservedDays += newDays;
        }
    }
}
