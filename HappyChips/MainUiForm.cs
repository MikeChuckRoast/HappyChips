using HappyChips.Models;
using Org.LLRP.LTK.LLRPV1;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace HappyChips
{
    public partial class MainUiForm : Form
    {

        private RfidReader? _reader;
        private bool _reading = false;
        private System.Windows.Forms.Timer refreshTimer;
        private ConcurrentDictionary<string, ChipReads> _chipReads = new ConcurrentDictionary<string, ChipReads>();
        public BindingList<ChipReads> CurrentChipReadsList
        {
            get
            {
                // This creates a new list from the values of the dictionary
                // every time the property is accessed, ensuring it's up-to-date.
                return new BindingList<ChipReads>(_chipReads.Values.ToList());
            }
        }

        public MainUiForm()
        {
            InitializeComponent();

            chipDataGrid.DataSource = CurrentChipReadsList;
            // Set the size of a specific column after the DataSource is set
            chipDataGrid.DataBindingComplete += (s, e) =>
            {
                chipDataGrid.Columns["LastRead"].Visible = false;
                if (chipDataGrid.Columns["ChipId"] != null)
                {
                    chipDataGrid.Columns["ChipId"].Width = 200; // Set the desired width
                }
                if (chipDataGrid.Columns["SecondsSinceLastRead"] != null)
                {
                    chipDataGrid.Columns["SecondsSinceLastRead"].DefaultCellStyle.Format = "0.0";
                }
            };

            // Initialize the timer
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 2000; // 2000 milliseconds = 2 seconds
            refreshTimer.Tick += RefreshTimer_Tick; // Event handler for the Tick event

        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (_reading)
            {
                addMessage("Already reading...");
                return;
            }

            addMessage("Starting...");

            // Connect to reader
            _reader = new RfidReader(readerAddressTextBox.Text, out ENUM_ConnectionAttemptStatusType status);
            if (status != ENUM_ConnectionAttemptStatusType.Success)
            {
                addMessage("Error connecting to reader: " + RfidReader.ConnectionAttemptStatusEnumToString(status));
                return;
            }

            // Start reading
            delegateRoAccessReport reportDelegate = new delegateRoAccessReport(OnChipRead);
            var (success, message) = _reader.ConfigureReader(reportDelegate);
            if (!success)
            {
                addMessage(message);
                return;
            }

            // Start or restart the timer
            refreshTimer.Start();

            _reading = true;
            setButtonStates();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            // Stop reading
            if (!_reading || _reader == null)
            {
                addMessage("Not reading...");
                return;
            }
            addMessage("Stopping...");

            var (success, message) = _reader.StopReading();
            if (!success)
            {
                addMessage(message);
            }
            // Stop the timer
            refreshTimer.Stop();

            _reading = false;
            setButtonStates();
        }

        private void addMessage(string message)
        {
            if (messagesTextBox.InvokeRequired)
            {
                messagesTextBox.Invoke(new MethodInvoker(delegate { addMessage(message); }));
            }
            else
            {
                messagesTextBox.Text += message + Environment.NewLine;
            }
        }

        private void setButtonStates()
        {
            startButton.Enabled = !_reading;
            startButton.BackColor = _reading ? System.Drawing.Color.Gray : System.Drawing.Color.SpringGreen;
            stopButton.Enabled = _reading;
            stopButton.BackColor = _reading ? System.Drawing.Color.Red : System.Drawing.Color.Gray;
        }

        private void OnChipRead(MSG_RO_ACCESS_REPORT msg)
        {
            if (msg.TagReportData == null)
            {
                return;
            }
            foreach (var tagReportData in msg.TagReportData)
            {
                string epc = RfidReader.GetEpcHexString(tagReportData.EPCParameter);

                string time = DateTime.Now.ToString("HH:mm:ss.fff"); // Current time in HH:MM:SS.XXX format
                string message = $"{(char)0x01}S,{time},{epc}\r\n"; // Format message

                addChipRead(tagReportData);

                //addMessage(message);

                // Convert the message to a byte array
                //byte[] messageBytes = Encoding.ASCII.GetBytes(message);

                // Send the message via UDP
                //var bytes = udpClient.Send(messageBytes, messageBytes.Length, udpHost, udpPort);
            }
        }

        private void addChipRead(PARAM_TagReportData tagReportData)
        {
            // Chip EPC ID
            var chipId = RfidReader.GetEpcHexString(tagReportData.EPCParameter);

            // Last read time
            DateTime lastRead;
            if (tagReportData.LastSeenTimestampUTC != null)
            {
                var lastReadUs = tagReportData.LastSeenTimestampUTC.Microseconds;
                lastRead = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(lastReadUs / 1000);
            }
            else
            {
                lastRead = DateTime.UtcNow;
            }

            // Read count
            long readCount = 1;
            if (tagReportData.TagSeenCount != null)
                readCount = tagReportData.TagSeenCount.TagCount;

            if (_chipReads.ContainsKey(chipId))
            {
                _chipReads[chipId].TotalReads = _chipReads[chipId].TotalReads + readCount;
                _chipReads[chipId].LastRead = lastRead;
            }
            else
            {
                _chipReads.TryAdd(chipId, new ChipReads { ChipId = chipId, LastRead = lastRead, TotalReads = readCount });
            }
        }

        private void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            RefreshChipReadsDataGrid();
        }

        private void RefreshChipReadsDataGrid()
        {
            chipDataGrid.DataSource = CurrentChipReadsList;
        }

    }
}
