using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HrSystem.Domain.Entities
{
    public class PublicHoliday
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public string Name { get; set; } = string.Empty;
    
    
        private PublicHoliday() 
        { 
            Name = string.Empty;
        }

        private PublicHoliday(Guid id, DateOnly date, string name)
        {
            Id = id;
            Date = date;
            Name = name;
        }

        
        public static PublicHoliday Create(DateOnly date, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Holiday name is required.", nameof(name));

            return new PublicHoliday(Guid.NewGuid(), date, name);
        }

        public void UpdateDetails(DateOnly newDate, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Holiday name is required.", nameof(newName));

            Date = newDate;
            Name = newName;
        }
    }

}
