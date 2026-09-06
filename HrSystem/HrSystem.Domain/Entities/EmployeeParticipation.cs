using System;

namespace HrSystem.Domain.Entities
{
    public class EmployeeParticipation
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public User Employee { get; set; } = null!;

        public bool IsOptedIn { get; set; } = true;
        public DateTime? LastOptOutDate { get; set; }
        public DateTime? CooldownEndDate { get; set; }

        public string? LastForceChangeReason { get; set; }
        public DateTime? LastForceChangeDate { get; set; }

        public EmployeeParticipation() { }

        public EmployeeParticipation(Guid employeeId, bool defaultOptIn = true)
        {
            Id = Guid.NewGuid();
            EmployeeId = employeeId;
            IsOptedIn = defaultOptIn;
        }

        public void SetLastForceChange(string reason, DateTime date)
        {
            LastForceChangeReason = reason;
            LastForceChangeDate = date;
            CooldownEndDate = null;
        }

        public void UpdateOptInStatus(bool isOptedIn, DateTime currentDate)
        {
            IsOptedIn = isOptedIn;
            LastOptOutDate = currentDate;
        }
        public void OptOut( int cooldownDays)
        {
            IsOptedIn = false;
            LastOptOutDate =  DateTime.UtcNow;
            cooldownDaysSet(cooldownDays);
        }
        public void OptIn()
        {
            IsOptedIn = true;
            LastOptOutDate = DateTime.UtcNow;
        }
        public void cooldownDaysSet( int cooldownDays)
        {    DateTime currentDate = DateTime.UtcNow;
             CooldownEndDate = currentDate.AddDays(cooldownDays);
        }
    }
}