using System.Text;

namespace gCodeGeneratorWinForms
{
    // ─── PARAMETERS CLASS ────────────────────────────────────────────────────
    internal class TurningParameters
    {
        public double Length { get; set; } = 20.0;
        public double InitialDiameter { get; set; } = 20.0;
        public double TargetDiameter { get; set; } = 10.0;
        public double Cut { get; set; } = 0.5;
        public double RoughFeed { get; set; } = 200;
        public double FinishFeed { get; set; } = 150;

        public double LeftRadius { get; set; } = 5.0;  // Cannot be greater than depth
        public bool LeftChamfer { get; set; } = false; // true = chamfer instead of arc

        // RightRadius: positive = outer radius, negative = inner radius
        // if positive  → cannot be greater than targetDiameter / 2
        // if negative  → absolute value cannot be greater than depth
        public double RightRadius { get; set; } = 5.0;
        public bool RightChamfer { get; set; } = false;

        public int LastCutTest { get; set; } = 1;
        public double Clear { get; set; } = 5.0;
        public string FileName { get; set; } = @"C:\Mach3\GCode\_myFile.gcode";
    }

    // ─── G-CODE GENERATOR CLASS ──────────────────────────────────────────────
    internal class GCodeGenerator
    {
        private readonly TurningParameters p;

        public GCodeGenerator(TurningParameters parameters)
        {
            p = parameters;
        }

        // Rounds all G-code coordinates to 4 decimal places to avoid floating point noise
        private static string R(double value) => Math.Round(value, 4).ToString("0.####");

        public string Generate()
        {
            // ─── DERIVED VALUES ───────────────────────────────────────────────
            double initialRadius = p.InitialDiameter / 2.0;
            double depth = Math.Abs((p.InitialDiameter - p.TargetDiameter) / 2.0);
            double cut = p.Cut;

            int mainSteps = (int)Math.Abs(Math.Truncate(depth / cut));
            double rest = depth - (mainSteps * cut);

            // Distribute the rest evenly across all steps
            cut = Math.Round(cut + (rest / mainSteps), 5);
            rest = 0;

            double rightRadiusFraction = Math.Abs(Math.Round(p.RightRadius / (mainSteps + 1), 4));

            // Determine at which depth arcs start being written (leftOffset / rightOffset)
            double leftOffset = 0;
            double rightOffset = 0;

            for (int i = 0; i <= mainSteps; i++)
            {
                double act = i * cut;
                if (depth - act < p.LeftRadius)
                {
                    leftOffset = Math.Abs(act - depth) + cut;
                    break;
                }
            }
            for (int i = 0; i <= mainSteps; i++)
            {
                double act = i * cut;
                if (depth - act < Math.Abs(p.RightRadius))
                {
                    rightOffset = Math.Abs(act - depth) + cut;
                    break;
                }
            }

            // ─── BUILD G-CODE ─────────────────────────────────────────────────
            var sb = new StringBuilder();
            void W(string line) => sb.AppendLine(line);

            W("M3 S500");   // Spindle start CW

            W($";-) rest: {rest}");
            W($";-) cut: {cut}");
            W($";-) leftOffset:{leftOffset}");
            W($";-) leftRad:{p.LeftRadius})");
            W($";-) rightOffset :{rightOffset})");
            W($";-) rightRadius:{p.RightRadius})");
            W("G18 G8 G21");

            int m = 1;  // left-radius step counter
            int n = 1;  // right inner-radius step counter

            for (int i = 0; i <= mainSteps; i++)
            {
                double actDepth = i * cut;

                W($";-) CYCLE:{i}/{mainSteps} DEPTH:{actDepth}/{depth}");

                // Feed rate — finish on last pass
                W(i == mainSteps ? $"f{p.FinishFeed}" : $"f{p.RoughFeed}");

                // ══════════════════════════════════════════════════════════════
                // RIGHT SIDE — OUTER RADIUS or CHAMFER
                // ══════════════════════════════════════════════════════════════
                if (p.RightRadius > 0)
                {
                    if (!p.RightChamfer)
                    {
                        W(";-) Right Outer Radius");
                        W($"g1 x{R(initialRadius - (actDepth + ((i + 1) * rightRadiusFraction)))}");
                        W("g1 z0");
                        W($"G03 z{R(-((i + 1) * rightRadiusFraction))} x{R(initialRadius - actDepth)} I0 K{R(-((i + 1) * rightRadiusFraction))}");
                    }
                    else
                    {
                        W(";-) Right Outer Chamfer");
                        W($"g1 x{R(initialRadius - (actDepth + ((i + 1) * rightRadiusFraction)))}");
                        W("g1 z0");
                        W($"G01 z{R(-((i + 1) * rightRadiusFraction))} x{R(initialRadius - actDepth)}");
                    }
                }
                // ══════════════════════════════════════════════════════════════
                // RIGHT SIDE — INNER RADIUS or CHAMFER
                // ══════════════════════════════════════════════════════════════
                else if (p.RightRadius < 0)
                {
                    double rightInnerRadius = Math.Abs(p.RightRadius);

                    if (depth - actDepth < rightInnerRadius)
                    {
                        W(";-) Right Inner Radius or Chamfer");
                        W($"g1 x{R(initialRadius)}");
                        W($"g1 z{R(-(rightOffset - (n * cut)))}");
                        W($"g1 x{R(initialRadius - (depth - rightInnerRadius))}"); // arc start point

                        if (!p.RightChamfer)
                        {
                            W($"G02 z{R(-rightInnerRadius)} x{R(initialRadius - actDepth)} I0 K{R((rightOffset - (n * cut)) - rightInnerRadius)}");
                        }
                        else
                        {
                            W($"G01 z{R(-rightInnerRadius)} x{R(initialRadius - actDepth)}");
                        }
                        n++;
                    }
                    else
                    {
                        W($"g1 x{R(initialRadius)}");
                        W($"g1 z{R(-rightOffset)}");
                    }

                    W($"g1 x{R(initialRadius - actDepth)}");
                }
                // ══════════════════════════════════════════════════════════════
                // NO RIGHT FEATURE — straight cuts before arcs
                // ══════════════════════════════════════════════════════════════
                else
                {
                    W($"g1 x{R(initialRadius - actDepth)}");
                }

                // ══════════════════════════════════════════════════════════════
                // LEFT SIDE — RADIUS or CHAMFER
                // ══════════════════════════════════════════════════════════════
                W(";-) Left Radius or Chamfer");

                if (depth - actDepth < p.LeftRadius)
                {
                    W($"g1 z{R(-(p.Length - p.LeftRadius))}"); // arc start point

                    if (((i == mainSteps) || (p.LeftRadius < cut)) && p.LastCutTest == 0)
                    {
                        // Last cut OR leftRadius smaller than cut step
                        if (!p.LeftChamfer && p.LeftRadius > 0)
                        {
                            W($"G02 z{R(-p.Length)} x{R(initialRadius - (depth - p.LeftRadius))} I{R(p.LeftRadius)} K0");
                        }
                        else
                        {
                            W($"G01 z{R(-p.Length)} x-{R(depth - p.LeftRadius)}");
                        }
                    }
                    else
                    {
                        if (!p.LeftChamfer && p.LeftRadius > 0)
                        {
                            W($"G02 z{R(-(p.Length - leftOffset + (m * cut)))} x{R(initialRadius - (depth - p.LeftRadius))} I{R(actDepth - (depth - p.LeftRadius))} K0");
                        }
                        else
                        {
                            W($"G01 z{R(-(p.Length - leftOffset + (m * cut)))} x{R(initialRadius - (depth - p.LeftRadius))}");
                        }
                    }

                    m++;
                }
                else
                {
                    W($"g1 x{R(initialRadius - actDepth)}");
                    W($"g1 z{R(-(p.Length - leftOffset))}");
                }

                // ── Retract & return to start ──────────────────────────────
                W($"g1 x{R(initialRadius)}");
                W($"g0 x{R(initialRadius + p.Clear)}");
                W("g0 z0");
                W($"g0 x{R(initialRadius + p.Clear)}");
            }

            // ── End of program ────────────────────────────────────────────
            W("");
            W($"g0 z0{p.Clear} x{R(initialRadius + p.Clear)}");
            W("M5"); // Spindle stop

            // Replace commas with dots for G-code compatibility
            return sb.ToString().Replace(',', '.');
        }
    }
}