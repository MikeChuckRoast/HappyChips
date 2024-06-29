using System.Windows.Forms;

namespace HappyChips
{
    partial class MainUiForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainUiForm));
            chipDataGrid = new DataGridView();
            startButton = new Button();
            stopButton = new Button();
            maskValueTextBox = new TextBox();
            maskLabel = new Label();
            readerAddressTextBox = new TextBox();
            lynxAddressTextBox = new TextBox();
            lynxPortTextBox = new TextBox();
            label2 = new Label();
            label3 = new Label();
            messagesTextBox = new TextBox();
            portLabel = new Label();
            contextMenuStrip1 = new ContextMenuStrip(components);
            copyChipIPToolStripMenuItem = new ToolStripMenuItem();
            transmitPowerCheckbox = new CheckBox();
            transmitPowerTextBox = new TextBox();
            ((System.ComponentModel.ISupportInitialize)chipDataGrid).BeginInit();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // chipDataGrid
            // 
            chipDataGrid.AllowUserToAddRows = false;
            chipDataGrid.AllowUserToDeleteRows = false;
            chipDataGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chipDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            chipDataGrid.Location = new Point(12, 275);
            chipDataGrid.Name = "chipDataGrid";
            chipDataGrid.ReadOnly = true;
            chipDataGrid.Size = new Size(417, 409);
            chipDataGrid.TabIndex = 0;
            chipDataGrid.MouseClick += ChipDataGrid_MouseClick;
            // 
            // startButton
            // 
            startButton.BackColor = Color.SpringGreen;
            startButton.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            startButton.Location = new Point(66, 90);
            startButton.Name = "startButton";
            startButton.Size = new Size(119, 49);
            startButton.TabIndex = 1;
            startButton.Text = "Start Reader";
            startButton.UseVisualStyleBackColor = false;
            startButton.Click += startButton_Click;
            // 
            // stopButton
            // 
            stopButton.BackColor = SystemColors.ControlDark;
            stopButton.Font = new Font("Segoe UI", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            stopButton.ForeColor = Color.White;
            stopButton.Location = new Point(250, 90);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(122, 49);
            stopButton.TabIndex = 2;
            stopButton.Text = "Stop Reader";
            stopButton.UseVisualStyleBackColor = false;
            stopButton.Click += stopButton_Click;
            // 
            // maskValueTextBox
            // 
            maskValueTextBox.Location = new Point(208, 237);
            maskValueTextBox.Name = "maskValueTextBox";
            maskValueTextBox.Size = new Size(122, 23);
            maskValueTextBox.TabIndex = 3;
            // 
            // maskLabel
            // 
            maskLabel.AutoSize = true;
            maskLabel.Location = new Point(108, 240);
            maskLabel.Name = "maskLabel";
            maskLabel.Size = new Size(94, 15);
            maskLabel.TabIndex = 4;
            maskLabel.Text = "Chip Mask Value";
            // 
            // readerAddressTextBox
            // 
            readerAddressTextBox.Location = new Point(170, 15);
            readerAddressTextBox.Name = "readerAddressTextBox";
            readerAddressTextBox.Size = new Size(100, 23);
            readerAddressTextBox.TabIndex = 5;
            readerAddressTextBox.Text = "FX960077E70B";
            // 
            // lynxAddressTextBox
            // 
            lynxAddressTextBox.Location = new Point(170, 47);
            lynxAddressTextBox.Name = "lynxAddressTextBox";
            lynxAddressTextBox.Size = new Size(100, 23);
            lynxAddressTextBox.TabIndex = 7;
            lynxAddressTextBox.Text = "127.0.0.1";
            // 
            // lynxPortTextBox
            // 
            lynxPortTextBox.Location = new Point(336, 47);
            lynxPortTextBox.Name = "lynxPortTextBox";
            lynxPortTextBox.Size = new Size(50, 23);
            lynxPortTextBox.TabIndex = 8;
            lynxPortTextBox.Text = "5086";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(33, 50);
            label2.Name = "label2";
            label2.Size = new Size(89, 15);
            label2.TabIndex = 10;
            label2.Text = "Lynx IP Address";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(33, 18);
            label3.Name = "label3";
            label3.Size = new Size(131, 15);
            label3.TabIndex = 11;
            label3.Text = "Reader Name / Address";
            // 
            // messagesTextBox
            // 
            messagesTextBox.Location = new Point(12, 172);
            messagesTextBox.Multiline = true;
            messagesTextBox.Name = "messagesTextBox";
            messagesTextBox.Size = new Size(417, 59);
            messagesTextBox.TabIndex = 12;
            // 
            // portLabel
            // 
            portLabel.AutoSize = true;
            portLabel.Location = new Point(301, 51);
            portLabel.Name = "portLabel";
            portLabel.Size = new Size(29, 15);
            portLabel.TabIndex = 13;
            portLabel.Text = "Port";
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { copyChipIPToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(145, 26);
            // 
            // copyChipIPToolStripMenuItem
            // 
            copyChipIPToolStripMenuItem.Name = "copyChipIPToolStripMenuItem";
            copyChipIPToolStripMenuItem.Size = new Size(144, 22);
            copyChipIPToolStripMenuItem.Text = "Copy Chip ID";
            copyChipIPToolStripMenuItem.Click += copyChipIPToolStripMenuItem_Click;
            // 
            // transmitPowerCheckbox
            // 
            transmitPowerCheckbox.AutoSize = true;
            transmitPowerCheckbox.Location = new Point(113, 147);
            transmitPowerCheckbox.Name = "transmitPowerCheckbox";
            transmitPowerCheckbox.Size = new Size(140, 19);
            transmitPowerCheckbox.TabIndex = 14;
            transmitPowerCheckbox.Text = "Set Transmitter Power";
            transmitPowerCheckbox.UseVisualStyleBackColor = true;
            // 
            // transmitPowerTextBox
            // 
            transmitPowerTextBox.Location = new Point(250, 145);
            transmitPowerTextBox.Name = "transmitPowerTextBox";
            transmitPowerTextBox.Size = new Size(70, 23);
            transmitPowerTextBox.TabIndex = 15;
            // 
            // MainUiForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(441, 696);
            Controls.Add(transmitPowerTextBox);
            Controls.Add(transmitPowerCheckbox);
            Controls.Add(portLabel);
            Controls.Add(messagesTextBox);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(lynxPortTextBox);
            Controls.Add(lynxAddressTextBox);
            Controls.Add(readerAddressTextBox);
            Controls.Add(maskLabel);
            Controls.Add(maskValueTextBox);
            Controls.Add(stopButton);
            Controls.Add(startButton);
            Controls.Add(chipDataGrid);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainUiForm";
            Text = "Happy Chips";
            FormClosing += MainUiForm_FormClosing;
            ((System.ComponentModel.ISupportInitialize)chipDataGrid).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView chipDataGrid;
        private Button startButton;
        private Button stopButton;
        private TextBox maskValueTextBox;
        private Label maskLabel;
        private TextBox readerAddressTextBox;
        private TextBox lynxAddressTextBox;
        private TextBox lynxPortTextBox;
        private Label label2;
        private Label label3;
        private TextBox messagesTextBox;
        private Label portLabel;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem copyChipIPToolStripMenuItem;
        private CheckBox transmitPowerCheckbox;
        private TextBox transmitPowerTextBox;
    }
}
