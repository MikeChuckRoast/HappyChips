using HappyChips.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HappyChips
{
    internal interface IGenericRfidReader
    {
        (bool, string) StartReader(bool setTransmitPower = false, int transmitPower = 30);
        (bool, string) StopReader();
    }
}
