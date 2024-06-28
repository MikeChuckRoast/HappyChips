using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyChips.Models
{
    public class ChipReads
    {
        public required string ChipId { get; set; }
        public DateTime LastRead { get; set; }
        public long TotalReads { get; set; }

        // Read-only property to get the time in seconds since LastRead
        public double SecondsSinceLastRead
        {
            get
            {
                TimeSpan timeSinceLastRead = DateTime.UtcNow - LastRead;
                return timeSinceLastRead.TotalSeconds;
            }
        }
    }
}
