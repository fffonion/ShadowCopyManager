namespace ShadowCopyManager
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            createButton = new Button();
            diskComboBox = new ComboBox();
            shadowCopiesDataGridView = new DataGridView();
            refreshButton = new Button();
            currentMaxSizeLabel = new Label();
            usedSpaceLabel = new Label();
            usageProgressBar = new ProgressBar();
            groupBox1 = new GroupBox();
            setMaxSizeButton = new Button();
            maxSizeTextBox = new TextBox();
            maxSizeLabel = new Label();
            groupBox2 = new GroupBox();
            label1 = new Label();
            ((System.ComponentModel.ISupportInitialize)shadowCopiesDataGridView).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // createButton
            // 
            createButton.Location = new Point(10, 22);
            createButton.Margin = new Padding(3, 4, 3, 4);
            createButton.Name = "createButton";
            createButton.Size = new Size(147, 26);
            createButton.TabIndex = 0;
            createButton.Text = "Create Shadow Copy";
            createButton.UseVisualStyleBackColor = true;
            createButton.Click += CreateButton_Click;
            // 
            // diskComboBox
            // 
            diskComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            diskComboBox.Font = new Font("Microsoft YaHei UI", 15.000001F, FontStyle.Regular, GraphicsUnit.Point, 134);
            diskComboBox.FormattingEnabled = true;
            diskComboBox.Location = new Point(9, 9);
            diskComboBox.Margin = new Padding(3, 4, 3, 4);
            diskComboBox.Name = "diskComboBox";
            diskComboBox.Size = new Size(332, 35);
            diskComboBox.TabIndex = 1;
            diskComboBox.SelectedIndexChanged += DiskComboBox_SelectedIndexChanged;
            // 
            // shadowCopiesDataGridView
            // 
            shadowCopiesDataGridView.AllowUserToAddRows = false;
            shadowCopiesDataGridView.AllowUserToDeleteRows = false;
            shadowCopiesDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            shadowCopiesDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            shadowCopiesDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            shadowCopiesDataGridView.ColumnHeadersVisible = false;
            shadowCopiesDataGridView.Location = new Point(6, 290);
            shadowCopiesDataGridView.Name = "shadowCopiesDataGridView";
            shadowCopiesDataGridView.ReadOnly = true;
            shadowCopiesDataGridView.RowHeadersVisible = false;
            shadowCopiesDataGridView.Size = new Size(414, 288);
            shadowCopiesDataGridView.TabIndex = 2;
            shadowCopiesDataGridView.CellContentClick += ShadowCopiesDataGridView_CellContentClick;
            // 
            // refreshButton
            // 
            refreshButton.Location = new Point(346, 16);
            refreshButton.Margin = new Padding(3, 4, 3, 4);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(75, 26);
            refreshButton.TabIndex = 3;
            refreshButton.Text = "Refresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += RefreshButton_Click;
            // 
            // currentMaxSizeLabel
            // 
            currentMaxSizeLabel.AutoSize = true;
            currentMaxSizeLabel.Location = new Point(10, 31);
            currentMaxSizeLabel.Name = "currentMaxSizeLabel";
            currentMaxSizeLabel.Size = new Size(124, 17);
            currentMaxSizeLabel.TabIndex = 7;
            currentMaxSizeLabel.Text = "Current Max Size: --";
            // 
            // usedSpaceLabel
            // 
            usedSpaceLabel.AutoSize = true;
            usedSpaceLabel.Location = new Point(242, 31);
            usedSpaceLabel.Name = "usedSpaceLabel";
            usedSpaceLabel.Size = new Size(94, 17);
            usedSpaceLabel.TabIndex = 8;
            usedSpaceLabel.Text = "Used Space: --";
            // 
            // usageProgressBar
            // 
            usageProgressBar.Location = new Point(8, 63);
            usageProgressBar.Margin = new Padding(2);
            usageProgressBar.Name = "usageProgressBar";
            usageProgressBar.Size = new Size(396, 15);
            usageProgressBar.TabIndex = 9;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(usedSpaceLabel);
            groupBox1.Controls.Add(usageProgressBar);
            groupBox1.Controls.Add(currentMaxSizeLabel);
            groupBox1.Location = new Point(9, 63);
            groupBox1.Margin = new Padding(2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(2);
            groupBox1.Size = new Size(412, 95);
            groupBox1.TabIndex = 10;
            groupBox1.TabStop = false;
            groupBox1.Text = "Usage";
            // 
            // setMaxSizeButton
            // 
            setMaxSizeButton.Location = new Point(10, 59);
            setMaxSizeButton.Margin = new Padding(3, 4, 3, 4);
            setMaxSizeButton.Name = "setMaxSizeButton";
            setMaxSizeButton.Size = new Size(126, 26);
            setMaxSizeButton.TabIndex = 6;
            setMaxSizeButton.Text = "Resize Storage";
            setMaxSizeButton.UseVisualStyleBackColor = true;
            setMaxSizeButton.Click += SetMaxSizeButton_Click;
            // 
            // maxSizeTextBox
            // 
            maxSizeTextBox.Location = new Point(271, 64);
            maxSizeTextBox.Margin = new Padding(3, 4, 3, 4);
            maxSizeTextBox.Name = "maxSizeTextBox";
            maxSizeTextBox.Size = new Size(134, 23);
            maxSizeTextBox.TabIndex = 5;
            // 
            // maxSizeLabel
            // 
            maxSizeLabel.AutoSize = true;
            maxSizeLabel.Location = new Point(144, 66);
            maxSizeLabel.Name = "maxSizeLabel";
            maxSizeLabel.Size = new Size(123, 17);
            maxSizeLabel.TabIndex = 4;
            maxSizeLabel.Text = "Max Size (e.g. 10G):";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(maxSizeLabel);
            groupBox2.Controls.Add(maxSizeTextBox);
            groupBox2.Controls.Add(setMaxSizeButton);
            groupBox2.Controls.Add(createButton);
            groupBox2.Location = new Point(9, 170);
            groupBox2.Margin = new Padding(2);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(2);
            groupBox2.Size = new Size(412, 94);
            groupBox2.TabIndex = 11;
            groupBox2.TabStop = false;
            groupBox2.Text = "Operation";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 270);
            label1.Name = "label1";
            label1.Size = new Size(125, 17);
            label1.TabIndex = 12;
            label1.Text = "Volume Shadow List";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(429, 586);
            Controls.Add(label1);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(refreshButton);
            Controls.Add(shadowCopiesDataGridView);
            Controls.Add(diskComboBox);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Shadow Copy Manager";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)shadowCopiesDataGridView).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        private Button createButton;
        private ComboBox diskComboBox;
        private DataGridView shadowCopiesDataGridView;
        private Button refreshButton;
        private Label currentMaxSizeLabel;
        private Label usedSpaceLabel;
        private ProgressBar usageProgressBar;

        #endregion

        private GroupBox groupBox1;
        private Button setMaxSizeButton;
        private TextBox maxSizeTextBox;
        private Label maxSizeLabel;
        private GroupBox groupBox2;
        private Label label1;
    }
}