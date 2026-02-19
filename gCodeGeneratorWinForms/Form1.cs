using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace gCodeGeneratorWinForms
{
    public partial class Form1 : Form
    {
        // ─── TOOLPATH SEGMENTS FOR DRAWING ───────────────────────────────────
        private enum MoveType { Rapid, Feed, Arc }
        
        

        private TurningParameters parameters =  new TurningParameters();

        private class Segment
        {
            public MoveType Type;
            public PointF Start, End;
            public bool IsArc;
            public PointF Center;
            public float Radius;
            public float StartAngle, SweepAngle;
        }

        private List<Segment> _segments = new();
        private string _gcode = "";

        // Use InvariantCulture everywhere so decimal dot works regardless of locale (e.g. Czech)
        private static readonly CultureInfo CI = CultureInfo.InvariantCulture;

        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "gCodeGeneratorWinForms",
            "settings.json"
        );

        public Form1()
        {
            InitializeComponent();
        }

        // ─── SETTINGS PERSISTENCE ────────────────────────────────────────────
        private void SaveSettings()
        {
            try
            {
                string? dir = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                string json = System.Text.Json.JsonSerializer.Serialize(parameters, options);
                File.WriteAllText(SettingsPath, json, System.Text.Encoding.UTF8);
            }
            catch { /* Swallow silently — never show error dialog on close */ }
        }

        private TurningParameters? LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsPath)) return null;
                string json = File.ReadAllText(SettingsPath, System.Text.Encoding.UTF8);
                return System.Text.Json.JsonSerializer.Deserialize<TurningParameters>(json);
            }
            catch { return null; }
        }

        private void PopulateControls(TurningParameters p)
        {
            // Unsubscribe to suppress redundant UpdateAll() calls during population
            TextBox[] texts = { txtLength, txtInitialDiameter, txtTargetDiameter,
                                txtCut, txtRoughFeed, txtFinishFeed,
                                txtLeftRadius, txtRightRadius, txtClear, txtFileName };
            CheckBox[] checks = { chkLeftChamfer, chkRightChamfer, chkAutoRadii, chkShowArrows };

            foreach (var t in texts)  t.TextChanged    -= AnyInput_Changed;
            foreach (var c in checks) c.CheckedChanged -= AnyCheck_Changed;

            txtLength.Text          = p.Length.ToString(CI);
            txtInitialDiameter.Text = p.InitialDiameter.ToString(CI);
            txtTargetDiameter.Text  = p.TargetDiameter.ToString(CI);
            txtCut.Text             = p.Cut.ToString(CI);
            txtRoughFeed.Text       = p.RoughFeed.ToString(CI);
            txtFinishFeed.Text      = p.FinishFeed.ToString(CI);
            txtLeftRadius.Text      = p.LeftRadius.ToString(CI);
            txtRightRadius.Text     = p.RightRadius.ToString(CI);
            txtClear.Text           = p.Clear.ToString(CI);
            txtFileName.Text        = p.FileName;

            chkLeftChamfer.Checked  = p.LeftChamfer;
            chkRightChamfer.Checked = p.RightChamfer;
            chkAutoRadii.Checked    = p.AutoRadies;
            chkShowArrows.Checked   = p.ShowArrows;

            foreach (var t in texts)  t.TextChanged    += AnyInput_Changed;
            foreach (var c in checks) c.CheckedChanged += AnyCheck_Changed;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            SaveSettings();
        }

        // ─── FORM LOAD ────────────────────────────────────────────────────────
        private void Form1_Load(object sender, EventArgs e)
        {
            BuildInputPanel();

            TurningParameters? saved = LoadSettings();
            if (saved != null)
                PopulateControls(saved);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Set splitter distances after form is fully rendered
            splitMain.Panel1MinSize = 220;
            splitMain.Panel2MinSize = 400;
            splitMain.SplitterDistance = 220;

            splitRight.Panel1MinSize = 100;
            splitRight.Panel2MinSize = 150;
            splitRight.SplitterDistance = Math.Max(100, this.ClientSize.Height - 200);

            this.Resize += (s, ev) =>
            {
                int max = this.ClientSize.Height - splitRight.Panel2MinSize - splitRight.SplitterWidth;
                int dist = Math.Max(splitRight.Panel1MinSize, Math.Min(max, this.ClientSize.Height - 200));
                splitRight.SplitterDistance = dist;
            };

            // Trigger first draw now that panels are properly sized
            UpdateAll();
        }

        // ─── BUILD INPUT PANEL ────────────────────────────────────────────────
        private void BuildInputPanel()
        {
            int row = 8;
            int rowH = 42;

            void AddHeader(string text)
            {
                var lbl = new Label
                {
                    Text = text,
                    ForeColor = Color.FromArgb(100, 180, 255),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    Location = new Point(8, row),
                    Size = new Size(204, 20)
                };
                panelInputs.Controls.Add(lbl);
                row += 22;
            }

            void AddRow(string labelText, TextBox txt, string defaultVal, Label? secondLabel = null)
            {
                var lbl = new Label
                {
                    Text = labelText,
                    ForeColor = Color.FromArgb(200, 200, 200),
                    Font = new Font("Segoe UI", 8.5f),
                    Location = new Point(8, row),
                    //Size = new Size(204, 18),
                    AutoSize = true
                };
                if (secondLabel != null)
                {
                    secondLabel.ForeColor = Color.FromArgb(255, 150, 0);
                    secondLabel.Font = lbl.Font;
                    secondLabel.Location = new Point(lbl.Location.X + lbl.Size.Width + 6, lbl.Location.Y);
                    secondLabel.AutoSize = true;
                    panelInputs.Controls.Add(secondLabel);
                }
                txt.Text = defaultVal;
                txt.Location = new Point(8, row + 18);
                txt.Size = new Size(200, 22);
                txt.Font = new Font("Consolas", 9.5f);
                txt.BackColor = Color.FromArgb(30, 30, 30);
                txt.ForeColor = Color.White;
                txt.TextChanged += AnyInput_Changed;
                txt.BorderStyle = BorderStyle.FixedSingle;
                panelInputs.Controls.Add(lbl);
                panelInputs.Controls.Add(txt);
                
                row += rowH;
            }

            void AddCheck(CheckBox chk, string text)
            {
                chk.Text = text;
                chk.Location = new Point(8, row);
                chk.Size = new Size(204, 22);
                chk.ForeColor = Color.FromArgb(200, 200, 200);
                chk.Font = new Font("Segoe UI", 8.5f);
                chk.CheckedChanged += AnyCheck_Changed;
                panelInputs.Controls.Add(chk);
                row += 28;
            }

            // ─ Dimensions ────────────────────────────────────────────────────
            AddHeader("── Dimensions ──");
            AddRow("Length (mm)", txtLength, "20");
            AddRow("Initial Diameter (mm)", txtInitialDiameter, "20");
            AddRow("Target Diameter (mm)", txtTargetDiameter, "10");

            // ─ Cutting ───────────────────────────────────────────────────────
            AddHeader("── Cutting ──");
            AddRow("Cut per pass (mm)", txtCut, "0.5");
            AddRow("Rough Feed (mm/min)", txtRoughFeed, "200");
            AddRow("Finish Feed (mm/min)", txtFinishFeed, "150");

            // ─ Left Side ─────────────────────────────────────────────────────
            AddHeader("── Left Side ──");
            AddRow("Left Radius (mm)", txtLeftRadius, "5",lblMaxLeftRadius);
            AddCheck(chkLeftChamfer, "Left Chamfer (instead of arc)");

            // ─ Right Side ────────────────────────────────────────────────────
            AddHeader("── Right Side ──");
            AddRow("+ outer / - inner radius", txtRightRadius, "5", lblMaxRightRadius);
            AddCheck(chkRightChamfer, "Right Chamfer (instead of arc)");

            // ─ Other ─────────────────────────────────────────────────────────
            AddHeader("── Other ──");
            AddRow("Clearance (mm)", txtClear, "5");
            AddCheck(chkAutoRadii, "Auto left and right radies");
            AddCheck(chkShowArrows, "Show arrows in graphic");

            // ─ Output ────────────────────────────────────────────────────────
            AddHeader("── Output ──");
            AddRow("Output File Path", txtFileName, @"C:\Mach3\GCode\_myFile.gcode");

            // ─ Save Button ───────────────────────────────────────────────────
            row += 8;
            btnSave.Text = "💾  Save G-Code";
            btnSave.Location = new Point(8, row);
            btnSave.Size = new Size(204, 34);
            btnSave.BackColor = Color.FromArgb(0, 122, 204);
            btnSave.ForeColor = Color.White;
            btnSave.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Click += btnSave_Click;
            panelInputs.Controls.Add(btnSave);
        }

        // ─── LIVE UPDATE ─────────────────────────────────────────────────────
        private void AnyInput_Changed(object? sender, EventArgs e) => UpdateAll();
        private void AnyCheck_Changed(object? sender, EventArgs e) => UpdateAll();

        private void UpdateAll()
        {
            if (!TryReadParameters(out TurningParameters p)) return;
            try
            {
                var gen = new GCodeGenerator(p);
                _gcode = gen.Generate();

                txtGCode.Text = _gcode;
                txtGCode.SelectionStart = 0;
                txtGCode.ScrollToCaret();

                _segments = ParseGCode(_gcode);
                panelViewer.Invalidate();
            }
            catch (Exception ex)
            {
                txtGCode.Text = $"; Error: {ex.Message}";
            }
        }

        // ─── READ PARAMETERS — InvariantCulture so dot decimals work in Czech locale ──
        private bool TryReadParameters(out TurningParameters p)
        {
            p = new TurningParameters();
            try
            {
                p.Length = double.Parse(txtLength.Text, CI);
                //Check if initial diameter is bigger than target diameter
                if (double.Parse(txtInitialDiameter.Text, CI) >= p.TargetDiameter)
                {
                    p.InitialDiameter = double.Parse(txtInitialDiameter.Text, CI);
                    txtInitialDiameter.BackColor = this.BackColor;
                } else
                {
                    txtInitialDiameter.BackColor = Color.FromArgb(255, 255, 100, 100);
                }
                //Check if target diameter is smaller than initial diameter
                if (double.Parse(txtTargetDiameter.Text, CI) <= p.InitialDiameter)
                {
                    p.TargetDiameter = double.Parse(txtTargetDiameter.Text, CI);
                    txtTargetDiameter.BackColor = this.BackColor;
                }
                else
                {
                    txtTargetDiameter.BackColor = Color.FromArgb(255, 255, 100, 100);
                }

                if (double.Parse(txtCut.Text, CI) >= 0.01)
                {
                    p.Cut = double.Parse(txtCut.Text, CI);
                    txtCut.BackColor = this.BackColor;
                }
                else {                     
                    txtCut.BackColor = Color.FromArgb(255, 255, 100, 100);
                }


                p.RoughFeed = double.Parse(txtRoughFeed.Text, CI);
                p.FinishFeed = double.Parse(txtFinishFeed.Text, CI);
                p.LeftRadius = double.Parse(txtLeftRadius.Text, CI);
                p.LeftChamfer = chkLeftChamfer.Checked;
                p.RightRadius = double.Parse(txtRightRadius.Text, CI);
                p.RightChamfer = chkRightChamfer.Checked;
                p.AutoRadies = chkAutoRadii.Checked;
                p.ShowArrows = chkShowArrows.Checked;
                p.Clear = double.Parse(txtClear.Text, CI);
                p.FileName = txtFileName.Text;

                var maxLeftRadius = ((p.InitialDiameter - p.TargetDiameter) / 2);
                var maxRightRadiusPositive = (p.TargetDiameter / 2);
                var maxRightRadiusNegative = -((p.InitialDiameter - p.TargetDiameter) / 2);

                //For short parts, the radius cannot be larger than half the length, otherwise it would create a full circle or more
                if (maxLeftRadius > (p.Length/2)) {
                    maxLeftRadius = (p.Length / 2);
                }

                if (maxRightRadiusPositive > (p.Length / 2))
                {
                    maxRightRadiusPositive = (p.Length / 2);
                }

                //
              

                // If AutoRadies is enabled, set left and right radius to maximum possible values based on dimensions, and disable manual input
                if (p.AutoRadies)
                {
                    p.LeftRadius = maxLeftRadius;
                    
                    if(p.RightRadius >= 0)
                        if(maxRightRadiusPositive > maxLeftRadius)
                        {
                            p.RightRadius = maxLeftRadius;
                        }else
                        {
                            p.RightRadius = maxRightRadiusPositive;
                        }
                    else
                    {
                        p.RightRadius = maxRightRadiusNegative;
                    }
                    txtLeftRadius.Enabled = false;
                    txtRightRadius.Enabled = false;
                    txtLeftRadius.Text = p.LeftRadius.ToString(CI);
                    txtRightRadius.Text = p.RightRadius.ToString(CI);
                } else {                     
                    txtLeftRadius.Enabled = true;
                    txtRightRadius.Enabled = true;
                }

                lblMaxLeftRadius.Text = $"(max: {maxLeftRadius:0.###})";   
                lblMaxRightRadius.Text = $"(max: {maxRightRadiusPositive:0.###} / {maxRightRadiusNegative:0.###})";
                parameters = p;
                return true;
            }
            catch { return false; }
        }

        // ─── SAVE BUTTON ─────────────────────────────────────────────────────
        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_gcode)) return;
            try
            {
                if (!TryReadParameters(out TurningParameters p)) return;
                string? dir = Path.GetDirectoryName(p.FileName);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(p.FileName, _gcode, System.Text.Encoding.ASCII);
                MessageBox.Show($"Saved to:\n{p.FileName}", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── G-CODE PARSER ───────────────────────────────────────────────────
        private static List<Segment> ParseGCode(string gcode)
        {
            var segments = new List<Segment>();
            float x = 0, z = 0;
            bool isRapid = false;

            foreach (string rawLine in gcode.Split('\n'))
            {
                string line = rawLine.Trim().ToUpper();
                if (line.StartsWith(";") || string.IsNullOrEmpty(line)) continue;

                // Detect rapid / feed mode — check full command carefully
                string cmd = line.Split(' ')[0]; // get just the G/M code part
                if (cmd == "G0" || cmd == "G00") isRapid = true;
                if (cmd == "G1" || cmd == "G01") isRapid = false;

                float newX = x, newZ = z;
                if (TryGetVal(line, 'X', out float px)) newX = px;
                if (TryGetVal(line, 'Z', out float pz)) newZ = pz;

                bool isArcCW = line.StartsWith("G02");
                bool isArcCCW = line.StartsWith("G03");

                if (isArcCW || isArcCCW)
                {
                    TryGetVal(line, 'I', out float iOff);
                    TryGetVal(line, 'K', out float kOff);

                    float cx = x + iOff;
                    float cz = z + kOff;
                    float r = (float)Math.Sqrt((x - cx) * (x - cx) + (z - cz) * (z - cz));

                    float startAngle = (float)(Math.Atan2(z - cz, x - cx) * 180.0 / Math.PI);
                    float endAngle = (float)(Math.Atan2(newZ - cz, newX - cx) * 180.0 / Math.PI);

                    
                    
                    float sweep = endAngle - startAngle;

                    //Original Claude code
                    //if (isArcCW && sweep > 0) sweep -= 360;
                    //if (isArcCCW && sweep < 0) sweep += 360;

                    //New Code
                    if (sweep < 0)
                    {
                        sweep = startAngle - endAngle;
                        sweep -= 180;
                    }

                    segments.Add(new Segment
                    {
                        Type = MoveType.Arc,
                        Start = new PointF(x, z),
                        End = new PointF(newX, newZ),
                        IsArc = true,
                        Center = new PointF(cx, cz),
                        Radius = r,
                        StartAngle = startAngle,
                        SweepAngle = sweep
                    });
                }
                else if (newX != x || newZ != z)
                {
                    segments.Add(new Segment
                    {
                        Type = isRapid ? MoveType.Rapid : MoveType.Feed,
                        Start = new PointF(x, z),
                        End = new PointF(newX, newZ)
                    });
                }

                x = newX;
                z = newZ;
            }
            return segments;
        }

        private static bool TryGetVal(string line, char axis, out float val)
        {
            val = 0;
            var m = Regex.Match(line, $@"{axis}(-?\d+\.?\d*)");
            if (m.Success)
            {
                val = float.Parse(m.Groups[1].Value, CI);
                return true;
            }
            return false;
        }

        // ─── 2D VIEWER PAINT ─────────────────────────────────────────────────
        private void panelViewer_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.FromArgb(30, 30, 30));

            if (_segments.Count == 0) return;

            // Z is horizontal → use Start.Y/End.Y for horizontal bounds
            // X is vertical   → use Start.X/End.X for vertical bounds
            float minZ = _segments.Min(s => Math.Min(s.Start.Y, s.End.Y));
            float maxZ = _segments.Max(s => Math.Max(s.Start.Y, s.End.Y));
            float minX = _segments.Min(s => Math.Min(s.Start.X, s.End.X));
            float maxX = _segments.Max(s => Math.Max(s.Start.X, s.End.X));

            float padX = panelViewer.Width * 0.08f;
            float padY = panelViewer.Height * 0.08f;
            float scaleX = (panelViewer.Width - 2 * padX) / Math.Max(maxZ - minZ, 0.001f);
            float scaleY = (panelViewer.Height - 2 * padY) / Math.Max(maxX - minX, 0.001f);
            float scale = Math.Min(scaleX, scaleY);

            // Center horizontally, pin to bottom vertically
            float offsetX = padX + (panelViewer.Width - 2 * padX - (maxZ - minZ) * scale) / 2;

            // Lathe convention (matching ioSender):
            // Z=0 at right, negative Z goes left  → invert Z horizontally
            // X=0 at top,   positive X goes down  → X maps directly to screen Y
            PointF ToScreen(PointF p) => new PointF(
                (p.Y - maxZ) * scale + (panelViewer.Width - padX),   // Z: 0 at right, neg goes left
                (p.X - minX) * scale + padY                            // X: 0 at top, grows downward
            );

            // Grid
            using var gridPen = new Pen(Color.FromArgb(50, 50, 50), 1);
            for (float gz = (float)Math.Floor(minZ); gz <= maxZ + 1; gz++)
                g.DrawLine(gridPen, ToScreen(new PointF(minX, gz)), ToScreen(new PointF(maxX, gz)));
            for (float gx = (float)Math.Floor(minX); gx <= maxX + 1; gx++)
                g.DrawLine(gridPen, ToScreen(new PointF(gx, minZ)), ToScreen(new PointF(gx, maxZ)));

            // Centerline — X=0 horizontal line at top (lathe center axis)
            using var axisPen = new Pen(Color.FromArgb(120, 120, 120), 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
            g.DrawLine(axisPen, ToScreen(new PointF(0, minZ - 1)), ToScreen(new PointF(0, maxZ + 1)));

            // ── Thick axis lines (behind toolpath) ───────────────────────────
            using var stockPen = new Pen(Color.FromArgb(55, 255, 255, 255), 1f);
            
            using var xAxisPen = new Pen(Color.FromArgb(55, 255, 255, 0), 3f);
            using var zAxisPen = new Pen(Color.FromArgb(55, 255, 255, 0), 3f);
            // X=0 centerline — horizontal line across full width
            g.DrawLine(xAxisPen, ToScreen(new PointF(0, minZ - 2)), ToScreen(new PointF(0, maxZ + 2)));
            // Z=0 vertical line — full height
            g.DrawLine(zAxisPen, ToScreen(new PointF(minX - 1, 0)), ToScreen(new PointF(maxX + 1, 0)));
            
            //g.DrawRectangle(stockPen, ToScreen(new PointF(0, 0)).X, ToScreen(new PointF(0, 0)).Y, 50,50);
            
            //using (Brush brush = new SolidBrush(Color.FromArgb(55, 255, 255, 255)))
            //{
            //    g.FillRectangle(brush, ToScreen(new PointF(maxX + 1, 0)).X - 30, ToScreen(new PointF(0, 0)).Y, 50, 50);
            //}

            // ── Toolpath ─────────────────────────────────────────────────────
            var arrow = new System.Drawing.Drawing2D.AdjustableArrowCap(4, 4);
            float[] dashValues = { 2, 2 };

            using var rapidPen = new Pen(Color.FromArgb(200, 80, 80), 1.0f);
            if(parameters.ShowArrows) rapidPen.CustomEndCap = arrow;
            
            rapidPen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
            rapidPen.DashPattern = dashValues;
            using var feedPen = new Pen(Color.FromArgb(80, 160, 255), 1.5f);
            if (parameters.ShowArrows) feedPen.CustomEndCap = arrow;
            using var arcPen = new Pen(Color.FromArgb(80, 220, 160), 1.5f);

            if(_segments.Count > 1500)
            {
                MessageBox.Show($"To many steps to display ({_segments.Count})", "Aborted", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            } else
            {
                foreach (var seg in _segments)
                {
                    if (seg.IsArc)
                    {
                        // Convert arc to polyline — avoids GDI+ angle/flip issues
                        int steps = 32;
                        double startRad = seg.StartAngle * Math.PI / 180.0;
                        double sweepRad = seg.SweepAngle * Math.PI / 180.0;
                        PointF? prev = null;

                        for (int s = 0; s <= steps; s++)
                        {
                            double angle = startRad + sweepRad * s / steps;
                            float px = seg.Center.X + seg.Radius * (float)Math.Cos(angle);
                            float pz = seg.Center.Y + seg.Radius * (float)Math.Sin(angle);
                            PointF cur = ToScreen(new PointF(px, pz));

                            if (s == steps)
                            { 
                                if (parameters.ShowArrows) arcPen.CustomEndCap = arrow; 
                            }
                            else
                            {
                                arcPen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;
                            }
                                



                            if (prev.HasValue)
                                g.DrawLine(arcPen, prev.Value, cur);
                            prev = cur;
                        }
                    }
                    else
                    {
                        var pen = seg.Type == MoveType.Rapid ? rapidPen : feedPen;
                        g.DrawLine(pen, ToScreen(seg.Start), ToScreen(seg.End));
                    }
                }
            }
            

            // Legend
            using var font = new Font("Arial", 8);
            g.DrawString("■ Rapid", font, new SolidBrush(Color.FromArgb(200, 80, 80)), 5, 5);
            g.DrawString("■ Feed", font, new SolidBrush(Color.FromArgb(80, 160, 255)), 5, 20);
            g.DrawString("■ Arc", font, new SolidBrush(Color.FromArgb(80, 220, 160)), 5, 35);
        }
    }
}