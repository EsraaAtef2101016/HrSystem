using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrSystem.Domain.Entities
{
    public class GlobalPolicy
    {
        public Guid Id { get; private set; }
        public bool IsSelfOptOutAllowed { get; private set; }
        public int CooldownDays { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public GlobalPolicy(bool isSelfOptOutAllowed, int cooldownDays)
        {
            Id = Guid.NewGuid();
            UpdateSettings(isSelfOptOutAllowed, cooldownDays);
        }

        public void UpdateSettings(bool isSelfOptOutAllowed, int cooldownDays)
        {
            if (cooldownDays < 0)
                throw new ArgumentException("Cooldown days cannot be a negative number.", nameof(cooldownDays));

            IsSelfOptOutAllowed = isSelfOptOutAllowed;
            CooldownDays = cooldownDays;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
