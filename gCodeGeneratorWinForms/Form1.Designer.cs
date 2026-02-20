namespace gCodeGeneratorWinForms
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            splitMain = new SplitContainer();
            panelInputs = new Panel();
            splitRight = new SplitContainer();
            panelViewer = new Panel();
            txtGCode = new RichTextBox();
            btnSave = new Button();
            txtLength = new TextBox();
            txtInitialDiameter = new TextBox();
            txtTargetDiameter = new TextBox();
            txtCut = new TextBox();
            txtRoughFeed = new TextBox();
            txtFinishFeed = new TextBox();
            txtLeftRadius = new TextBox();
            lblMaxRightRadius = new Label();
            lblMaxLeftRadius = new Label();
            txtRightRadius = new TextBox();
            txtClear = new TextBox();
            txtFileName = new TextBox();
            txtIoSenderPath = new TextBox();
            chkOpenInIoSender = new CheckBox();
            chkLeftChamfer = new CheckBox();
            chkRightChamfer = new CheckBox();
            chkAutoRadiuses = new CheckBox();
            chkLastCutTest = new CheckBox();
            chkShowArrows = new CheckBox();
            chkSymmetricDisplay = new CheckBox();
            chkShowMaterial = new CheckBox();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitRight).BeginInit();
            splitRight.Panel1.SuspendLayout();
            splitRight.Panel2.SuspendLayout();
            splitRight.SuspendLayout();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.Location = new Point(0, 0);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(panelInputs);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(splitRight);
            splitMain.Size = new Size(1373, 819);
            splitMain.SplitterDistance = 480;
            splitMain.TabIndex = 0;
            // 
            // panelInputs
            // 
            panelInputs.AutoScroll = true;
            panelInputs.BackColor = Color.FromArgb(45, 45, 48);
            panelInputs.Dock = DockStyle.Fill;
            panelInputs.Location = new Point(0, 0);
            panelInputs.Name = "panelInputs";
            panelInputs.Padding = new Padding(8);
            panelInputs.Size = new Size(480, 819);
            panelInputs.TabIndex = 0;
            // 
            // splitRight
            // 
            splitRight.Dock = DockStyle.Fill;
            splitRight.Location = new Point(0, 0);
            splitRight.Name = "splitRight";
            splitRight.Orientation = Orientation.Horizontal;
            // 
            // splitRight.Panel1
            // 
            splitRight.Panel1.Controls.Add(panelViewer);
            // 
            // splitRight.Panel2
            // 
            splitRight.Panel2.Controls.Add(txtGCode);
            splitRight.Size = new Size(921, 819);
            splitRight.SplitterDistance = 409;
            splitRight.TabIndex = 0;
            // 
            // panelViewer
            // 
            panelViewer.BackColor = Color.FromArgb(30, 30, 30);
            panelViewer.BorderStyle = BorderStyle.FixedSingle;
            panelViewer.Dock = DockStyle.Fill;
            panelViewer.Location = new Point(0, 0);
            panelViewer.Name = "panelViewer";
            panelViewer.Size = new Size(921, 409);
            panelViewer.TabIndex = 0;
            panelViewer.Paint += panelViewer_Paint;
            // 
            // txtGCode
            // 
            txtGCode.BackColor = Color.FromArgb(20, 20, 20);
            txtGCode.Dock = DockStyle.Fill;
            txtGCode.Font = new Font("Consolas", 9F);
            txtGCode.ForeColor = Color.FromArgb(180, 230, 180);
            txtGCode.Location = new Point(0, 0);
            txtGCode.Name = "txtGCode";
            txtGCode.ReadOnly = true;
            txtGCode.Size = new Size(921, 406);
            txtGCode.TabIndex = 0;
            txtGCode.Text = "";
            txtGCode.WordWrap = false;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(0, 0);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 0;
            // 
            // txtLength
            // 
            txtLength.Location = new Point(0, 0);
            txtLength.Name = "txtLength";
            txtLength.Size = new Size(100, 23);
            txtLength.TabIndex = 0;
            // 
            // txtInitialDiameter
            // 
            txtInitialDiameter.Location = new Point(0, 0);
            txtInitialDiameter.Name = "txtInitialDiameter";
            txtInitialDiameter.Size = new Size(100, 23);
            txtInitialDiameter.TabIndex = 0;
            // 
            // txtTargetDiameter
            // 
            txtTargetDiameter.Location = new Point(0, 0);
            txtTargetDiameter.Name = "txtTargetDiameter";
            txtTargetDiameter.Size = new Size(100, 23);
            txtTargetDiameter.TabIndex = 0;
            // 
            // txtCut
            // 
            txtCut.Location = new Point(0, 0);
            txtCut.Name = "txtCut";
            txtCut.Size = new Size(100, 23);
            txtCut.TabIndex = 0;
            // 
            // txtRoughFeed
            // 
            txtRoughFeed.Location = new Point(0, 0);
            txtRoughFeed.Name = "txtRoughFeed";
            txtRoughFeed.Size = new Size(100, 23);
            txtRoughFeed.TabIndex = 0;
            // 
            // txtFinishFeed
            // 
            txtFinishFeed.Location = new Point(0, 0);
            txtFinishFeed.Name = "txtFinishFeed";
            txtFinishFeed.Size = new Size(100, 23);
            txtFinishFeed.TabIndex = 0;
            // 
            // txtLeftRadius
            // 
            txtLeftRadius.Location = new Point(0, 0);
            txtLeftRadius.Name = "txtLeftRadius";
            txtLeftRadius.Size = new Size(100, 23);
            txtLeftRadius.TabIndex = 0;
            // 
            // lblMaxRightRadius
            // 
            lblMaxRightRadius.Location = new Point(0, 0);
            lblMaxRightRadius.Name = "lblMaxRightRadius";
            lblMaxRightRadius.Size = new Size(100, 23);
            lblMaxRightRadius.TabIndex = 0;
            // 
            // lblMaxLeftRadius
            // 
            lblMaxLeftRadius.Location = new Point(0, 0);
            lblMaxLeftRadius.Name = "lblMaxLeftRadius";
            lblMaxLeftRadius.Size = new Size(100, 23);
            lblMaxLeftRadius.TabIndex = 0;
            // 
            // txtRightRadius
            // 
            txtRightRadius.Location = new Point(0, 0);
            txtRightRadius.Name = "txtRightRadius";
            txtRightRadius.Size = new Size(100, 23);
            txtRightRadius.TabIndex = 0;
            // 
            // txtClear
            // 
            txtClear.Location = new Point(0, 0);
            txtClear.Name = "txtClear";
            txtClear.Size = new Size(100, 23);
            txtClear.TabIndex = 0;
            // 
            // txtFileName
            // 
            txtFileName.Location = new Point(0, 0);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(100, 23);
            txtFileName.TabIndex = 0;
            // 
            // chkLeftChamfer
            // 
            chkLeftChamfer.Location = new Point(0, 0);
            chkLeftChamfer.Name = "chkLeftChamfer";
            chkLeftChamfer.Size = new Size(104, 24);
            chkLeftChamfer.TabIndex = 0;
            // 
            // chkRightChamfer
            // 
            chkRightChamfer.Location = new Point(0, 0);
            chkRightChamfer.Name = "chkRightChamfer";
            chkRightChamfer.Size = new Size(104, 24);
            chkRightChamfer.TabIndex = 0;
            // 
            // chkAutoRadiuses
            // 
            chkAutoRadiuses.Location = new Point(0, 0);
            chkAutoRadiuses.Name = "chkAutoRadiuses";
            chkAutoRadiuses.Size = new Size(104, 24);
            chkAutoRadiuses.TabIndex = 0;
            // 
            // chkShowArrows
            // 
            chkShowArrows.Location = new Point(0, 0);
            chkShowArrows.Name = "chkShowArrows";
            chkShowArrows.Size = new Size(104, 24);
            chkShowArrows.TabIndex = 0;
            // 
            // chkSymmetricDisplay
            // 
            chkSymmetricDisplay.Location = new Point(0, 0);
            chkSymmetricDisplay.Name = "chkSymmetricDisplay";
            chkSymmetricDisplay.Size = new Size(104, 24);
            chkSymmetricDisplay.TabIndex = 0;
            //
            // chkShowMaterial
            //
            chkShowMaterial.Location = new Point(0, 0);
            chkShowMaterial.Name = "chkShowMaterial";
            chkShowMaterial.Size = new Size(104, 24);
            chkShowMaterial.TabIndex = 0;
            //
            // chkLastCutTest
            //
            chkLastCutTest.Location = new Point(0, 0);
            chkLastCutTest.Name = "chkLastCutTest";
            chkLastCutTest.Size = new Size(104, 24);
            chkLastCutTest.TabIndex = 0;
            //
            // txtIoSenderPath
            //
            txtIoSenderPath.Location = new Point(0, 0);
            txtIoSenderPath.Name = "txtIoSenderPath";
            txtIoSenderPath.Size = new Size(100, 23);
            txtIoSenderPath.TabIndex = 0;
            //
            // chkOpenInIoSender
            //
            chkOpenInIoSender.Location = new Point(0, 0);
            chkOpenInIoSender.Name = "chkOpenInIoSender";
            chkOpenInIoSender.Size = new Size(104, 24);
            chkOpenInIoSender.TabIndex = 0;
            //
            // Form1
            // 
            BackColor = Color.FromArgb(37, 37, 38);
            ClientSize = new Size(1373, 819);
            Controls.Add(splitMain);
            Name = "Form1";
            Text = "G-Code Generator — Turning";
            Load += Form1_Load;
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            splitRight.Panel1.ResumeLayout(false);
            splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitRight).EndInit();
            splitRight.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // ── Control declarations ──────────────────────────────────────────────
        private SplitContainer splitMain, splitRight;
        private Panel panelInputs, panelViewer;
        private RichTextBox txtGCode;
        private Button btnSave;
        private TextBox txtLength, txtInitialDiameter, txtTargetDiameter;
        private TextBox txtCut, txtRoughFeed, txtFinishFeed;
        private TextBox txtLeftRadius, txtRightRadius, txtClear, txtFileName, txtIoSenderPath;
        private CheckBox chkLeftChamfer, chkRightChamfer, chkAutoRadiuses, chkShowArrows, chkLastCutTest, chkSymmetricDisplay, chkShowMaterial, chkOpenInIoSender;
        private Label lblMaxLeftRadius, lblMaxRightRadius;
    }
}