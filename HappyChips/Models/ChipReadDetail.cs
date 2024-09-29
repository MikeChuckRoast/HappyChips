using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyChips.Models
{
    public class ChipReadDetail
    {
        public required string ChipId { get; set; }
        public DateTime LastRead { get; set; }
        public uint TagSeenCount { get; set; }
        public uint AntennaId { get; set; }
    }
}
