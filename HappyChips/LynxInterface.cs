using Org.LLRP.LTK.LLRPV1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HappyChips
{
    internal class LynxInterface
    {

        private UdpClient _udpClient = new UdpClient();
        private string _lynxHostname;
        private int _lynxPort;

        public LynxInterface(string lynxHostname, int lynxPort)
        {
            _lynxHostname = lynxHostname;
            _lynxPort = lynxPort;
        }

        public void Close()
        {
            _udpClient.Close();
        }

        public int SendMessageViaUdp(PARAM_TagReportData tagReportData)
        {
            var messageBytes = ReportDataToMessageBytes(tagReportData);
            return _udpClient.Send(messageBytes, messageBytes.Length, _lynxHostname, _lynxPort);
        }

        private byte[] ReportDataToMessageBytes(PARAM_TagReportData tagReportData)
        {
            // Get the EPC from the tag report data
            string epc = RfidReader.GetEpcHexString(tagReportData.EPCParameter);
            string time = DateTime.Now.ToString("HH:mm:ss.fff"); // Current time in HH:MM:SS.XXX format
            string message = $"{(char)0x01}S,{time},{epc}\r\n"; // Format message

            // Convert the message to a byte array
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            return messageBytes;
        }
    }
}
