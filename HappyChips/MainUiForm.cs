using HappyChips.Models;
using HappyChips.Properties;
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

        private LynxInterface? _lynxInterface;
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
        private int rightClickedRowIndex = -1;

        public MainUiForm()
        {
            InitializeComponent();

            // Load settings
            readerAddressTextBox.Text = Properties.Settings.Default.ReaderAddress;
            lynxAddressTextBox.Text = Properties.Settings.Default.LynxAddress;
            lynxPortTextBox.Text = Properties.Settings.Default.LynxPort;
            maskValueTextBox.Text = Properties.Settings.Default.ChipMask;
            transmitPowerCheckbox.Checked = Properties.Settings.Default.SetTransmitPower;
            transmitPowerTextBox.Text = Properties.Settings.Default.TransmitPower;

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

        private void MainUiForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Save settings
            Properties.Settings.Default.ReaderAddress = readerAddressTextBox.Text;
            Properties.Settings.Default.LynxAddress = lynxAddressTextBox.Text;
            Properties.Settings.Default.LynxPort = lynxPortTextBox.Text;
            Properties.Settings.Default.ChipMask = maskValueTextBox.Text;
            Properties.Settings.Default.SetTransmitPower = transmitPowerCheckbox.Checked;
            Properties.Settings.Default.TransmitPower = transmitPowerTextBox.Text;
            Properties.Settings.Default.Save();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (_reading)
            {
                addMessage("Already reading...");
                return;
            }

            addMessage("Starting...");

            // Create Lynx interface
            _lynxInterface = new LynxInterface(lynxAddressTextBox.Text, int.Parse(lynxPortTextBox.Text));

            // Connect to reader
            _reader = new RfidReader(readerAddressTextBox.Text, out ENUM_ConnectionAttemptStatusType status);
            if (status != ENUM_ConnectionAttemptStatusType.Success)
            {
                addMessage("Error connecting to reader: " + RfidReader.ConnectionAttemptStatusEnumToString(status));
                return;
            }

            // Start reading
            ClearChips();
            delegateRoAccessReport reportDelegate = new delegateRoAccessReport(OnChipRead);
            var (success, message) = _reader.ConfigureReader(reportDelegate, transmitPowerCheckbox.Checked, ushort.Parse(transmitPowerTextBox.Text));
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

            // Close lynx
            _lynxInterface?.Close();

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
            transmitPowerCheckbox.Enabled = !_reading;
            transmitPowerTextBox.Enabled = !_reading;
        }

        private void ClearChips()
        {
            _chipReads = new ConcurrentDictionary<string, ChipReads>();
        }

        private void OnChipRead(MSG_RO_ACCESS_REPORT msg)
        {
            if (msg.TagReportData == null)
            {
                return;
            }
            foreach (var tagReportData in msg.TagReportData)
            {
                if (!chipMatchesMask(tagReportData))
                    continue;

                // Log chip read for display in UI
                addChipRead(tagReportData);

                // Send message to Lynx
                _lynxInterface?.SendMessageViaUdp(tagReportData);
            }
        }

        private bool chipMatchesMask(PARAM_TagReportData tagReportData)
        {
            // Check if the chip matches the mask
            if (maskValueTextBox.Text.Length == 0)
            {
                return true;
            }
            string mask = maskValueTextBox.Text;
            string epc = RfidReader.GetEpcHexString(tagReportData.EPCParameter);
            return epc.Contains(mask);
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
            // sort chipReadsList by SecondsSinceLastRead
            var chipReadsList = new BindingList<ChipReads>(CurrentChipReadsList.OrderBy(c => c.SecondsSinceLastRead).ToList());
            chipDataGrid.DataSource = chipReadsList;
        }

        private void copyChipIPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickedRowIndex >= 0 && rightClickedRowIndex < chipDataGrid.Rows.Count)
            {
                // Assuming the ChipId is bound directly to a column in the DataGridView
                string chipId = chipDataGrid.Rows[rightClickedRowIndex].Cells["ChipId"].Value.ToString();
                if (!string.IsNullOrEmpty(chipId))
                    Clipboard.SetText(chipId); // Copy ChipId to clipboard
            }
        }

        private void ChipDataGrid_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int rowIndex = chipDataGrid.HitTest(e.X, e.Y).RowIndex;
                if (rowIndex != -1) // Ensure the click is on a row
                {
                    chipDataGrid.ClearSelection();
                    chipDataGrid.Rows[rowIndex].Selected = true; // Select the row
                    rightClickedRowIndex = rowIndex; // Store the right-clicked row index

                    // Show the context menu at the mouse position
                    contextMenuStrip1.Show(chipDataGrid, e.Location);
                }
            }
        }
    }
}
