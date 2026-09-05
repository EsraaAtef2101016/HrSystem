using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities
{

    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;

        // Self-referencing relationship for manager assignment
        public Guid? ManagerId { get; set; }
        public User? Manager { get; set; }
        public ICollection<User> DirectReports { get; set; } = new List<User>();

        public EmployeeParticipation? Participation { get; set; }
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();



        private User() { }


        public static User Create(string name, string email, UserRole role, Guid? managerId = null)
        {
           
            return new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                Role = role,
                ManagerId = managerId,
                IsActive = true
            };
        }


        public void SetPasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

            PasswordHash = passwordHash;
        }


    }
}
