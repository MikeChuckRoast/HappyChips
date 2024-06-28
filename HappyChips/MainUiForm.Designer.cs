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
            ((System.ComponentModel.ISupportInitialize)chipDataGrid).BeginInit();
            SuspendLayout();
            // 
            // chipDataGrid
            // 
            chipDataGrid.AllowUserToAddRows = false;
            chipDataGrid.AllowUserToDeleteRows = false;
            chipDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            chipDataGrid.Location = new Point(12, 252);
            chipDataGrid.Name = "chipDataGrid";
            chipDataGrid.ReadOnly = true;
            chipDataGrid.Size = new Size(417, 397);
            chipDataGrid.TabIndex = 0;
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
            maskValueTextBox.Location = new Point(214, 214);
            maskValueTextBox.Name = "maskValueTextBox";
            maskValueTextBox.Size = new Size(122, 23);
            maskValueTextBox.TabIndex = 3;
            // 
            // maskLabel
            // 
            maskLabel.AutoSize = true;
            maskLabel.Location = new Point(105, 214);
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
            // 
            // lynxPortTextBox
            // 
            lynxPortTextBox.Location = new Point(286, 47);
            lynxPortTextBox.Name = "lynxPortTextBox";
            lynxPortTextBox.Size = new Size(100, 23);
            lynxPortTextBox.TabIndex = 8;
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
            messagesTextBox.Location = new Point(66, 149);
            messagesTextBox.Multiline = true;
            messagesTextBox.Name = "messagesTextBox";
            messagesTextBox.Size = new Size(320, 59);
            messagesTextBox.TabIndex = 12;
            // 
            // MainUiForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(441, 661);
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
            Name = "MainUiForm";
            Text = "Happy Chips";
            ((System.ComponentModel.ISupportInitialize)chipDataGrid).EndInit();
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
    }
}
