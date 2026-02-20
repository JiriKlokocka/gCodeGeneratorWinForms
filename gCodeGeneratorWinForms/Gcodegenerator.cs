using System.Text;

namespace gCodeGeneratorWinForms
{
    // ─── PARAMETERS CLASS ────────────────────────────────────────────────────
    

    // ─── G-CODE GENERATOR CLASS ──────────────────────────────────────────────
    internal class GCodeGenerator
    {
        private readonly TurningParameters p;

        public GCodeGenerator(TurningParameters parameters, ProgramParameters programParameters)
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

            double rightRadiusFraction = Math.Abs(Math.Round(p.RightSideRadius / (mainSteps + 1), 4));

            // Determine at which depth arcs start being written (leftOffset / rightOffset)
            double leftOffset = 0;
            double rightOffset = 0;

            for (int i = 0; i <= mainSteps; i++)
            {
                double act = i * cut;
                if (depth - act < p.LeftSideRadius)
                {
                    leftOffset = Math.Abs(act - depth) + cut;
                    break;
                }
            }
            for (int i = 0; i <= mainSteps; i++)
            {
                double act = i * cut;
                if (depth - act < Math.Abs(p.RightSideRadius))
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
            W($";-) leftRad:{p.LeftSideRadius})");
            W($";-) rightOffset :{rightOffset})");
            W($";-) rightRadius:{p.RightSideRadius})");
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
                if (!p.RightSideRadiusIsInner)
                {
                    var arcXStart = initialRadius - (actDepth + ((i + 1) * rightRadiusFraction));
                    var arcXEnd = initialRadius - actDepth;
                    var arcZEnd = -((i + 1) * rightRadiusFraction);
                    //TODO: Handle bigger arc radii
                    if (!p.RightSideIsChamfer)
                    {
                        W(";-) Right Outer Radius");
                        W($"g1 x{R(arcXStart)}");
                        W("g1 z0");
                        if(p.RightSideRadiusIsPositive)
                        {

                            W($"G02 z{R(arcZEnd)} x{R(arcXEnd)} I{R(((i + 1) * rightRadiusFraction))} K0");
                        }
                        else
                        {
                            W($"G03 z{R(arcZEnd)} x{R(arcXEnd)} I0 K{R(-((i + 1) * rightRadiusFraction))}");
                        }
                    }
                    else
                    {
                        W(";-) Right Outer Chamfer");
                        W($"g1 x{R(arcXStart)}");
                        W("g1 z0");
                        W($"G01 z{R(arcZEnd)} x{R(arcXEnd)}");
                    }
                }
                // ══════════════════════════════════════════════════════════════
                // RIGHT SIDE — INNER RADIUS or CHAMFER
                // ══════════════════════════════════════════════════════════════
                else if(p.RightSideRadiusIsInner) 
                {
                    double rightInnerRadius = Math.Abs(p.RightSideRadius);

                    if (depth - actDepth < rightInnerRadius)
                    {
                        W(";-) Right Inner Radius or Chamfer");
                        W($"g1 x{R(initialRadius)}");
                        W($"g1 z{R(-(rightOffset - (n * cut)))}");
                        W($"g1 x{R(initialRadius - (depth - rightInnerRadius))}"); // arc start point

                        if (p.RightSideIsChamfer)
                        {
                            W($"G01 z{R(-rightInnerRadius)} x{R(initialRadius - actDepth)}");
                        }
                        else
                        {
                            if (p.RightSideRadiusIsPositive)
                            {
                                W($"G03 z{R(-rightInnerRadius)} x{R(initialRadius - actDepth)} I{R((rightOffset - (n * cut)) - rightInnerRadius)} K0");
                            }
                            else
                            {
                                W($"G02 z{R(-rightInnerRadius)} x{R(initialRadius - actDepth)} I0 K{R((rightOffset - (n * cut)) - rightInnerRadius)}");
                            }
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

                if (depth - actDepth < p.LeftSideRadius)
                {
                    var leftArcStartPoint = -(p.Length - p.LeftSideRadius);
                    //leftArcStartPoint += (((mainSteps - i) * (cut))); // Shift arc start point for better toolpath on bigger radii
                    W($"g1 z{R(leftArcStartPoint )}"); // arc start point
              
                    if (p.LeftSideIsChamfer)
                    {
                        W($"G01 z{R(-(p.Length - leftOffset + (m * cut)))} x{R(initialRadius - (depth - p.LeftSideRadius))}");
                    }
                    else
                    {
                        if(p.LeftSideRadiusIsPositive)
                        {
                            W($"G03 z{R(-(p.Length - leftOffset + (m * cut)))} x{R(initialRadius - (depth - p.LeftSideRadius))} I0 K{R(-(actDepth - (depth - p.LeftSideRadius)))}");
                        }
                        else
                        {
                            W($"G02 z{R(-(p.Length - leftOffset + (m * cut)))} x{R(initialRadius - (depth - p.LeftSideRadius))} I{R(actDepth - (depth - p.LeftSideRadius))} K0");
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
                W($"g0 x{R(initialRadius + p.Clearance)}");
                W("g0 z0");
                W($"g0 x{R(initialRadius + p.Clearance)}");
            }

            // ── End of program ────────────────────────────────────────────
            W("");
            W($"g0 z0{p.Clearance} x{R(initialRadius + p.Clearance)}");
            W("M5"); // Spindle stop

            // Replace commas with dots for G-code compatibility
            return sb.ToString().Replace(',', '.');
        }
    }
}

/*if (((i == mainSteps) || (p.LeftSideRadius < cut)) && p.LastCutTest == true)
          {
              // Last cut OR leftRadius smaller than cut step
              if (!p.LeftSideIsChamfer && p.LeftSideRadius > 0)
              {
                  W($"G02 z{R(-p.Length)} x{R(initialRadius - (depth - p.LeftSideRadius))} I{R(p.LeftSideRadius)} K0");
              }
              else
              {
                  W($"G01 z{R(-p.Length)} x-{R(depth - p.LeftSideRadius)}");
              }
          }
          else
          {
}*/