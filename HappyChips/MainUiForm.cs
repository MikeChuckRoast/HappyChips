using HappyChips.Models;
using HappyChips.Properties;

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
        private IGenericRfidReader? _reader;
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
            transmitPowerNumber.Text = Properties.Settings.Default.TransmitPower;

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
            Properties.Settings.Default.TransmitPower = transmitPowerNumber.Text;
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
            _lynxInterface = new LynxInterface(lynxAddressTextBox.Text, int.Parse(lynxPortTextBox.Text), Properties.Settings.Default.LogFile);

            // Connect to reader
            if (_reader == null)
            {
                _reader = new ZebraReader(readerAddressTextBox.Text, new DelegateChipRead(OnChipRead), out string status);
                if (!status.Equals(""))
                {
                    addMessage("Error connecting to reader: " + status);
                    _lynxInterface?.Close();
                    _lynxInterface = null;
                    return;
                }
            }

            // Start reading
            ClearChips();
            var (success, message) = _reader.StartReader(transmitPowerCheckbox.Checked, ushort.Parse(transmitPowerNumber.Text));
            if (!success)
            {
                addMessage(message);
                _lynxInterface?.Close();
                _lynxInterface = null;
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

            var (success, message) = _reader.StopReader();
            if (!success)
            {
                addMessage(message);
            }

            // Close lynx
            _lynxInterface?.Close();
            _lynxInterface = null;

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
            transmitPowerNumber.Enabled = !_reading;
        }

        private void ClearChips()
        {
            _chipReads = new ConcurrentDictionary<string, ChipReads>();
        }

        private void OnChipRead(ChipReadDetail chipReadDetail)
        {
            if (!chipMatchesMask(chipReadDetail.ChipId))
                return;

            // Log chip read for display in UI
            addChipRead(chipReadDetail);

            // Send message to Lynx
            _lynxInterface?.SendMessageViaUdp(chipReadDetail);
        }

        private bool chipMatchesMask(string chipId)
        {
            // Check if the chip matches the mask
            if (maskValueTextBox.Text.Length == 0)
            {
                return true;
            }
            string mask = maskValueTextBox.Text;
            return chipId.Contains(mask);
        }   

        private void addChipRead(ChipReadDetail chipReadDetail)       
        {
            //// Chip EPC ID
            //var chipId = RfidReader.GetEpcHexString(tagReportData.EPCParameter);

            //// Last read time
            //DateTime lastRead;
            //if (tagReportData.LastSeenTimestampUTC != null)
            //{
            //    var lastReadUs = tagReportData.LastSeenTimestampUTC.Microseconds;
            //    lastRead = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(lastReadUs / 1000);
            //}
            //else
            //{
            //    lastRead = DateTime.UtcNow;
            //}

            if (_chipReads.ContainsKey(chipReadDetail.ChipId))
            {
                _chipReads[chipReadDetail.ChipId].TotalReads = _chipReads[chipReadDetail.ChipId].TotalReads + chipReadDetail.TagSeenCount;
                _chipReads[chipReadDetail.ChipId].LastRead = chipReadDetail.LastRead;
            }
            else
            {
                _chipReads.TryAdd(chipReadDetail.ChipId, new ChipReads { ChipId = chipReadDetail.ChipId, LastRead = chipReadDetail.LastRead, TotalReads = chipReadDetail.TagSeenCount });
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
