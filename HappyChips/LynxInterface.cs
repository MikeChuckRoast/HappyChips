using HappyChips.Models;
using System.Net.Sockets;
using System.Text;

namespace HappyChips
{
    internal class LynxInterface
    {

        private UdpClient _udpClient = new UdpClient();
        private string _lynxHostname;
        private int _lynxPort;
        private StreamWriter? _writer = null;
        private double lastRead = 0;

        public LynxInterface(string lynxHostname, int lynxPort, string logFilePath)
        {
            _lynxHostname = lynxHostname;
            _lynxPort = lynxPort;

            // Logging
            if (!logFilePath.Equals(""))
            {
                _writer = new StreamWriter(logFilePath, true); // 'true' to append to the file
            }
        }

        ~LynxInterface()
        {
            if (_writer != null)
            {
                _writer.Close();
            }
        }

        public void Close()
        {
            _udpClient.Close();
            if (_writer != null)
            {
                _writer.Close();
            }
        }

        public int SendMessageViaUdp(ChipReadDetail chipReadDetail)
        {
            var messageBytes = ReportDataToMessageBytes(chipReadDetail);

            if (_writer != null && _writer.BaseStream.CanWrite)
            {
                _writer.Write(ReportDataToString(chipReadDetail));
                _writer.Flush();
            }

            return _udpClient.Send(messageBytes, messageBytes.Length, _lynxHostname, _lynxPort);
        }

        private byte[] ReportDataToMessageBytes(ChipReadDetail chipReadDetail)
        {
            // Get the EPC from the tag report data
            string epc = chipReadDetail.ChipId;
            string time = DateTime.Now.ToString("HH:mm:ss.fff"); // Current time in HH:MM:SS.XXX format
            string message = $"{(char)0x01}S,{time},{epc}\r\n"; // Format message

            // Convert the message to a byte array
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            return messageBytes;
        }

        private string ReportDataToString(ChipReadDetail chipReadDetail)
        {
            // Time format that is being sent to lynx
            string time = DateTime.Now.ToString("HH:mm:ss.fff"); // Current time in HH:MM:SS.XXX format
            string epc = chipReadDetail.ChipId;
            // Additional details
            // Provide current time in seconds (for easy plotting) and calculate difference since last read
            var currentTime = DateTime.Now.TimeOfDay.TotalSeconds;
            var currentTimeString = currentTime.ToString("F3");
            var deltaTime = currentTime - lastRead;
            var deltaTimeString = deltaTime.ToString("F3");
            lastRead = currentTime;
            // Include antenna id
            string antennaId = chipReadDetail.AntennaId.ToString();
            string message = $"{time},{epc},{currentTimeString},{deltaTimeString},{antennaId}\r\n"; // Format message
            return message;
        }
    }
}
